// Sparkle Cookie - custom code.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SparkleCookie.Board
{
    /// <summary>
    /// HEADLINE GAMEPLAY: a custom tap-to-blast match game that drives the GUI
    /// pack's demo Game-Portrait board (its GridLayoutGroup "Table", cell size and
    /// spacing untouched), using the demo's animal Tile prefabs for the look.
    ///
    /// Tap any group of 2+ connected same animals to clear them; tiles fall and
    /// refill. Every clear charges the Sparkle Surge meter; when full, the animals
    /// throw a "Sparkle Party" that clears the most abundant animal on the board.
    ///
    /// Input is polled directly + raycast here (not via UI Button events), which is
    /// reliable under Unity 6 with the Input System package installed.
    /// </summary>
    public class SparkleBoard : MonoBehaviour
    {
        [Header("Board")]
        public int rows = 8;
        public int columns = 8;
        [Range(2, 6)] public int colorCount = 6;
        public int minMatch = 2;
        public int startMoves = 25;
        [Range(0f, 0.8f)]
        [Tooltip("Chance a new tile copies a neighbour's animal, forming tappable clusters.")]
        public float clusterBias = 0.5f;

        [Header("Sparkle Surge meter")]
        public float meterCapacity = 80f;
        public float meterComboWindow = 2.5f;
        public float meterDrainPerSecond = 5f;

        private static readonly string[] TilePrefabResPaths =
        {
            "Elements UI/Game Items/Tiles/Tile - Bunny - Pink",
            "Elements UI/Game Items/Tiles/Tile - Cat - Orange",
            "Elements UI/Game Items/Tiles/Tile - Panda",
            "Elements UI/Game Items/Tiles/Tile - Pig",
            "Elements UI/Game Items/Tiles/Tile - Frog",
            "Elements UI/Game Items/Tiles/Tile - Owl",
        };
        private const string SparkleResPath = "Elements UI/Items (Pre-Built)/Sparkles/Sparkle 1";

        private RectTransform table;
        private Transform canvasTransform;
        private GraphicRaycaster raycaster;
        private EventSystem eventSystem;
        private TMP_Text scoreText;
        private TMP_Text movesText;
        private Image meterFill;

        private GameObject[] tilePrefabs;
        private Sprite[] animalSprites;
        private GameObject sparklePrefab;
        private SparkleTileView[,] cells;
        private int[,] grid;
        private int score;
        private int moves;
        private bool busy;
        private bool built;

        private readonly SparkleMeter.SparkleMeter meter = new SparkleMeter.SparkleMeter();

        /// <summary>The Sparkle Surge meter — the seam companion powers plug into.</summary>
        public SparkleMeter.SparkleMeter Meter { get { return meter; } }

        private void Awake()
        {
            var tableGO = GameObject.Find("Canvas/Table");
            if (tableGO != null)
            {
                table = tableGO.GetComponent<RectTransform>();
                canvasTransform = table.parent;
                var canvas = table.GetComponentInParent<Canvas>();
                if (canvas != null) raycaster = canvas.GetComponent<GraphicRaycaster>();
                var glg = tableGO.GetComponent<GridLayoutGroup>();
                if (glg != null && glg.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    columns = glg.constraintCount;
                    rows = Mathf.Max(1, table.childCount / Mathf.Max(1, columns));
                }
            }
            eventSystem = FindObjectOfType<EventSystem>();

            var scoreGO = GameObject.Find("Canvas/Top Bar/Score/Text - Score Amount");
            if (scoreGO != null) scoreText = scoreGO.GetComponent<TMP_Text>();
            var movesGO = GameObject.Find("Canvas/Top Bar/Moves/Text - Amount");
            if (movesGO != null) movesText = movesGO.GetComponent<TMP_Text>();
            var barGO = GameObject.Find("Canvas/Top Bar/Progress Bar (Half Radial)/Bar (Animation)");
            if (barGO != null)
            {
                meterFill = barGO.GetComponent<Image>();
                var an = barGO.GetComponent<Animator>();
                if (an != null) an.enabled = false;
            }
        }

        private void Start()
        {
            meter.Capacity = meterCapacity;
            meter.ComboWindow = meterComboWindow;
            meter.DrainPerSecond = meterDrainPerSecond;
            meter.Burst += OnSparkleBurst;

            tilePrefabs = new GameObject[colorCount];
            animalSprites = new Sprite[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                tilePrefabs[i] = Resources.Load<GameObject>(TilePrefabResPaths[i]);
                animalSprites[i] = ExtractAnimalSprite(tilePrefabs[i]);
            }
            sparklePrefab = Resources.Load<GameObject>(SparkleResPath);

            if (rows < 4) rows = 8;
            moves = startMoves;
            GenerateGrid();
            BuildCells();
            RefreshSprites(false);
            UpdateHud();
        }

        private void OnDestroy() { meter.Burst -= OnSparkleBurst; }

        private void Update()
        {
            meter.Tick(Time.deltaTime);
            if (meterFill != null) meterFill.fillAmount = meter.Normalized;

            if (busy) return;
            Vector2 pos;
            if (TryGetPointerDown(out pos)) TryTap(pos);
        }

        // Reads "pointer pressed this frame" from whichever input backend is active
        // (new Input System package and/or legacy Input Manager).
        private bool TryGetPointerDown(out Vector2 pos)
        {
            pos = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                pos = mouse.position.ReadValue();
                return true;
            }
            var ts = Touchscreen.current;
            if (ts != null && ts.primaryTouch.press.wasPressedThisFrame)
            {
                pos = ts.primaryTouch.position.ReadValue();
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0)) { pos = Input.mousePosition; return true; }
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            { pos = Input.GetTouch(0).position; return true; }
#endif
            return false;
        }

        private void TryTap(Vector2 screenPos)
        {
            if (raycaster == null || eventSystem == null) return;
            var ped = new PointerEventData(eventSystem) { position = screenPos };
            var results = new List<RaycastResult>();
            raycaster.Raycast(ped, results);
            foreach (var rr in results)
            {
                var view = rr.gameObject.GetComponentInParent<SparkleTileView>();
                if (view != null) { OnTileClicked(view.Row, view.Col); return; }
            }
        }

        private static Sprite ExtractAnimalSprite(GameObject tilePrefab)
        {
            if (tilePrefab == null) return null;
            var item = tilePrefab.transform.Find("Button/Item");
            if (item != null)
            {
                var img = item.GetComponent<Image>();
                if (img != null) return img.sprite;
            }
            return null;
        }

        // ----- Model -----

        private void GenerateGrid()
        {
            grid = new int[rows, columns];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    grid[r, c] = PickClustered(r, c);
        }

        private int PickClustered(int r, int c)
        {
            if (c > 0 && grid[r, c - 1] >= 0 && Random.value < clusterBias) return grid[r, c - 1];
            if (r > 0 && grid[r - 1, c] >= 0 && Random.value < clusterBias) return grid[r - 1, c];
            return Random.Range(0, colorCount);
        }

        public void OnTileClicked(int r, int c)
        {
            if (busy || grid == null) return;
            if (r < 0 || r >= rows || c < 0 || c >= columns || grid[r, c] < 0) return;

            var group = new List<Vector2Int>();
            FloodFill(r, c, grid[r, c], group, new bool[rows, columns]);
            if (group.Count < minMatch)
            {
                Debug.Log("[SparkleBoard] tap (" + r + "," + c + ") - group of " + group.Count +
                          " too small (need " + minMatch + "+).");
                return;
            }

            busy = true;
            int gained = group.Count * group.Count * 10;
            score += gained;
            foreach (var p in group)
            {
                if (cells[p.x, p.y] != null) SpawnSparkle(cells[p.x, p.y].transform.position);
                grid[p.x, p.y] = -1;
            }
            if (moves > 0) moves -= 1;

            meter.AddMatch(group.Count);
            SparkleCookie.Core.SparkleHooks.RaiseMatchResolved(group.Count, gained);

            Collapse();
            RefreshSprites(true);
            UpdateHud();
            busy = false;
            Debug.Log("[SparkleBoard] cleared " + group.Count + " animals at (" + r + "," + c + ").");
            CheckEnd();
        }

        private void FloodFill(int r, int c, int type, List<Vector2Int> outList, bool[,] seen)
        {
            if (type < 0) return;
            if (r < 0 || r >= rows || c < 0 || c >= columns) return;
            if (seen[r, c] || grid[r, c] != type) return;
            seen[r, c] = true;
            outList.Add(new Vector2Int(r, c));
            FloodFill(r + 1, c, type, outList, seen);
            FloodFill(r - 1, c, type, outList, seen);
            FloodFill(r, c + 1, type, outList, seen);
            FloodFill(r, c - 1, type, outList, seen);
        }

        private void Collapse()
        {
            for (int c = 0; c < columns; c++)
            {
                int write = rows - 1;
                for (int r = rows - 1; r >= 0; r--)
                {
                    if (grid[r, c] != -1)
                    {
                        grid[write, c] = grid[r, c];
                        if (write != r) grid[r, c] = -1;
                        write--;
                    }
                }
                for (int r = write; r >= 0; r--)
                {
                    if (r + 1 < rows && grid[r + 1, c] >= 0 && Random.value < clusterBias)
                        grid[r, c] = grid[r + 1, c];
                    else
                        grid[r, c] = Random.Range(0, colorCount);
                }
            }
        }

        private void OnSparkleBurst()
        {
            var counts = new int[colorCount];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    if (grid[r, c] >= 0 && grid[r, c] < colorCount) counts[grid[r, c]]++;

            int best = 0;
            for (int i = 1; i < colorCount; i++) if (counts[i] > counts[best]) best = i;

            int cleared = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    if (grid[r, c] == best)
                    {
                        if (cells[r, c] != null) SpawnSparkle(cells[r, c].transform.position);
                        grid[r, c] = -1;
                        cleared++;
                    }

            score += cleared * 15;
            Collapse();
            RefreshSprites(true);
            UpdateHud();
        }

        // ----- View / VFX -----

        private void BuildCells()
        {
            if (table == null || built) return;
            for (int i = table.childCount - 1; i >= 0; i--)
                DestroyImmediate(table.GetChild(i).gameObject);

            var template = tilePrefabs != null && tilePrefabs.Length > 0 ? tilePrefabs[0] : null;
            cells = new SparkleTileView[rows, columns];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    GameObject go = template != null
                        ? Instantiate(template, table)
                        : new GameObject("Cell", typeof(RectTransform), typeof(Image));
                    go.name = "Cell_" + r + "_" + c;
                    var view = go.GetComponent<SparkleTileView>();
                    if (view == null) view = go.AddComponent<SparkleTileView>();
                    view.Init(r, c);
                    cells[r, c] = view;
                }
            }
            built = true;
        }

        private void RefreshSprites(bool popChanged)
        {
            if (cells == null) return;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int t = Mathf.Clamp(grid[r, c], 0, colorCount - 1);
                    var sprite = animalSprites != null && t < animalSprites.Length ? animalSprites[t] : null;
                    var cell = cells[r, c];
                    if (cell == null) continue;
                    bool changed = cell.Type != t;
                    cell.SetType(t, sprite, popChanged && changed);
                }
            }
        }

        private void SpawnSparkle(Vector3 worldPos)
        {
            if (sparklePrefab == null || canvasTransform == null) return;
            var fx = Instantiate(sparklePrefab, canvasTransform);
            fx.transform.position = worldPos;
            fx.transform.SetAsLastSibling();
            Destroy(fx, 0.7f);
        }

        private void UpdateHud()
        {
            if (scoreText != null) scoreText.text = score.ToString();
            if (movesText != null) movesText.text = moves.ToString();
        }

        private void CheckEnd()
        {
            if (moves <= 0)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
        }
    }
}
