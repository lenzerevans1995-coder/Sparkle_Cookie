// Sparkle Cookie - custom code.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    /// Self-contained: no dependency on the licensed kit's runtime. Loads the demo
    /// tile prefabs from Resources and wires HUD by hierarchy path, so the scene
    /// needs no manual inspector wiring.
    /// </summary>
    public class SparkleBoard : MonoBehaviour
    {
        [Header("Board")]
        public int rows = 8;
        public int columns = 8;
        [Range(2, 6)] public int colorCount = 6;
        public int minMatch = 2;
        public int startMoves = 25;

        [Header("Sparkle Surge meter")]
        public float meterCapacity = 80f;
        public float meterComboWindow = 2.5f;
        public float meterDrainPerSecond = 5f;

        // Demo animal tile prefabs (in a Resources folder), one per color index.
        private static readonly string[] TilePrefabResPaths =
        {
            "Elements UI/Game Items/Tiles/Tile - Bunny - Pink",
            "Elements UI/Game Items/Tiles/Tile - Cat - Orange",
            "Elements UI/Game Items/Tiles/Tile - Panda",
            "Elements UI/Game Items/Tiles/Tile - Pig",
            "Elements UI/Game Items/Tiles/Tile - Frog",
            "Elements UI/Game Items/Tiles/Tile - Owl",
        };

        private RectTransform table;
        private TMP_Text scoreText;
        private TMP_Text movesText;
        private Image meterFill;

        private GameObject[] tilePrefabs;
        private Sprite[] animalSprites;
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
                var glg = tableGO.GetComponent<GridLayoutGroup>();
                if (glg != null && glg.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    columns = glg.constraintCount;
                    rows = Mathf.Max(1, table.childCount / Mathf.Max(1, columns));
                    if (rows < 1) rows = columns;
                }
            }

            var scoreGO = GameObject.Find("Canvas/Top Bar/Score/Text - Score Amount");
            if (scoreGO != null) scoreText = scoreGO.GetComponent<TMP_Text>();

            var movesGO = GameObject.Find("Canvas/Top Bar/Moves/Text - Amount");
            if (movesGO != null) movesText = movesGO.GetComponent<TMP_Text>();

            var barGO = GameObject.Find("Canvas/Top Bar/Progress Bar (Half Radial)/Bar (Animation)");
            if (barGO != null)
            {
                meterFill = barGO.GetComponent<Image>();
                var an = barGO.GetComponent<Animator>();
                if (an != null) an.enabled = false; // we drive fillAmount ourselves
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

            if (rows < 4) rows = 8;
            moves = startMoves;
            GenerateGrid();
            BuildCells();
            RefreshSprites();
            UpdateHud();
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

        private void OnDestroy() { meter.Burst -= OnSparkleBurst; }

        private void Update()
        {
            meter.Tick(Time.deltaTime);
            if (meterFill != null) meterFill.fillAmount = meter.Normalized;
        }

        // ----- Model -----

        [Range(0f, 0.8f)]
        [Tooltip("Chance a new tile copies a neighbour's animal, forming clusters so taps are responsive.")]
        public float clusterBias = 0.5f;

        private void GenerateGrid()
        {
            grid = new int[rows, columns];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    grid[r, c] = PickClustered(r, c);
        }

        // Random animal, but biased to match an already-placed neighbour so the
        // board has plenty of 2+ clusters to tap.
        private int PickClustered(int r, int c)
        {
            if (c > 0 && grid[r, c - 1] >= 0 && Random.value < clusterBias) return grid[r, c - 1];
            if (r > 0 && grid[r - 1, c] >= 0 && Random.value < clusterBias) return grid[r - 1, c];
            return Random.Range(0, colorCount);
        }

        public void OnTileClicked(int r, int c)
        {
            if (busy || grid == null) return;
            if (r < 0 || r >= rows || c < 0 || c >= columns) return;

            var group = new List<Vector2Int>();
            FloodFill(r, c, grid[r, c], group, new bool[rows, columns]);
            if (group.Count < minMatch)
            {
                Debug.Log("[SparkleBoard] tap (" + r + "," + c + ") registered - group of " +
                          group.Count + " is too small (need " + minMatch + "+ adjacent).");
                return;
            }
            Debug.Log("[SparkleBoard] tap (" + r + "," + c + ") cleared " + group.Count + " animals.");

            busy = true;
            int gained = group.Count * group.Count * 10;
            score += gained;
            foreach (var p in group) grid[p.x, p.y] = -1;
            if (moves > 0) moves -= 1;

            meter.AddMatch(group.Count);
            SparkleCookie.Core.SparkleHooks.RaiseMatchResolved(group.Count, gained);

            Collapse();
            RefreshSprites();
            UpdateHud();
            busy = false;
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
                    // Bias refills to match the tile below so fresh clusters keep forming.
                    if (r + 1 < rows && grid[r + 1, c] >= 0 && Random.value < clusterBias)
                        grid[r, c] = grid[r + 1, c];
                    else
                        grid[r, c] = Random.Range(0, colorCount);
                }
            }
        }

        private void OnSparkleBurst()
        {
            // Sparkle Party: clear the most abundant animal on the board.
            var counts = new int[colorCount];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    if (grid[r, c] >= 0 && grid[r, c] < colorCount) counts[grid[r, c]]++;

            int best = 0;
            for (int i = 1; i < colorCount; i++) if (counts[i] > counts[best]) best = i;

            int cleared = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                    if (grid[r, c] == best) { grid[r, c] = -1; cleared++; }

            score += cleared * 15;
            Collapse();
            RefreshSprites();
            UpdateHud();
        }

        // ----- View -----

        // Build the persistent cell GameObjects once (one per grid slot, in
        // row-major sibling order so the GridLayoutGroup positions them exactly).
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
                    view.Init(this, r, c);
                    cells[r, c] = view;
                }
            }
            built = true;
        }

        // Update every cell's animal to match the model — no destroy/instantiate.
        private void RefreshSprites()
        {
            if (cells == null) return;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    int t = Mathf.Clamp(grid[r, c], 0, colorCount - 1);
                    var sprite = animalSprites != null && t < animalSprites.Length ? animalSprites[t] : null;
                    if (cells[r, c] != null) cells[r, c].SetType(t, sprite);
                }
            }
        }

        private void UpdateHud()
        {
            if (scoreText != null) scoreText.text = score.ToString();
            if (movesText != null) movesText.text = moves.ToString();
        }

        private void CheckEnd()
        {
            if (moves <= 0)
            {
                // MVP: out of moves -> return to Home. (Win/Lose popups: see roadmap.)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
            }
        }
    }
}
