using System.Collections.Generic;
using System.Linq;
using Solution;
using UnityEngine;

public class GridManager : MonoSingleton<GridManager>
{
    [Header("References")] [SerializeField]
    private GameObject CellPrefab;

    [SerializeField] private HexagonController hexagonBlockPrefab;
    public Material BlockMaterial;
    public ColorPack colorPack;
    public LayerMask CellLayer;

    [Header("Configuration")] [SerializeField]
    private int _gridSizeX = 0;

    [SerializeField] private int _gridSizeY = 0;

    [Header("Debug")] public CellData[,] GridPlan;
    private const float CELL_HORIZONTAL_OFFSET = 0.75f;
    private const float CELL_VERTICAL_OFFSET = 0.8660254f;
    public float VERTICAL_PLACEMENT_OFFSET = 0.2f;
    [SerializeField] private List<StartInfo> startInfos;

    private readonly HashSet<Vector2Int> _distinctPath = new HashSet<Vector2Int>();


    [Space(125)] public GridInfoAssigner CurrentGridInfo;

    public void Start()
    {
        startInfos = new ();

        foreach (var info in InfoManager.instance.GetCurrentInfo().startInfos)
        {
            startInfos.Add(info);
        }

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (GridPlan != null)
        {
            DestroyPreviousGrid();
        }

        GridPlan = new CellData[_gridSizeX, _gridSizeY];
        for (var x = 0; x < _gridSizeX; x++)
        {
            for (var y = 0; y < _gridSizeY; y++)
            {
                GridPlan[x, y] = gameObject.AddComponent<CellData>();

                GridPlan[x, y].PosX = x;
                GridPlan[x, y].PosY = y;

                if (ContainsInStartInfo(x, y, out var index))
                {
                    GridPlan[x, y].isOpen = startInfos[index].isOpen;
                    var CE = startInfos[index].ContentInfo.ToList();
                    GridPlan[x, y].CellContentList = CE;
                }
                else
                {
                    GridPlan[x, y].isOpen = true;
                    GridPlan[x, y].CellContentList = new ();
                }

                if (GridPlan[x, y].isOpen)
                {
                    var cloneCellGO = Instantiate(CellPrefab, Vector3.zero, CellPrefab.transform.rotation, transform);
                    cloneCellGO.transform.position =
                        new Vector3(x * CELL_HORIZONTAL_OFFSET, 0,
                            -(x % 2 * (CELL_VERTICAL_OFFSET / 2) + y * CELL_VERTICAL_OFFSET));

                    GridPlan[x, y].CellObject = cloneCellGO;
                    GridPlan[x, y].CellController = cloneCellGO.GetComponent<CellController>();

                    cloneCellGO.name = x + "," + y;
                    GridPlan[x, y].CellController.SetCoordinates(x, y);
                }

                if (GridPlan[x, y].CellContentList.Count != 0 && GridPlan[x, y].isOpen)
                {
                    var cellParent = GridPlan[x, y].CellObject.GetComponent<CellController>();
                    for (var i = 0; i < GridPlan[x, y].CellContentList.Count; i++)
                    {
                        var color = GridPlan[x, y].CellContentList[i];
                        var mat = new Material(BlockMaterial)
                        {
                            color = colorPack.HexagonColorInfo[colorPack.GetColorEnumIndex(color)].HexColor
                        };


                        SpawnHexagon(i,
                            cellParent.transform.position,
                            cellParent.HexStackParent,
                            mat,
                            color);
                    }

                    cellParent.SetOccupied(true);
                }

                if (GridPlan[x, y].isOpen)
                    GridPlan[x, y].CellObject.GetComponent<CellController>().Starter();
            }
        }
    }

    private void DestroyPreviousGrid()
    {
        for (var x = 0; x < GridPlan.GetLength(0); x++)
        {
            for (var y = 0; y < GridPlan.GetLength(1); y++)
            {
                Destroy(GridPlan[x, y].CellObject);
            }
        }
    }

    public void SpawnHexagon(int index, Vector3 gridPos, Transform parent, Material mat, ColorInfo.ColorEnum color)
    {
        var verticalPos = (index + 1) * VERTICAL_PLACEMENT_OFFSET;
        var spawnPos = gridPos + new Vector3(0, verticalPos, 0);

        var cloneBlock = Instantiate(hexagonBlockPrefab, spawnPos, Quaternion.identity, parent);

        cloneBlock.Initialize(color, mat);
    }

    private bool ContainsInStartInfo(int x, int y, out int index)
    {
        for (var i = 0; i < startInfos.Count; i++)
        {
            var startInfo = startInfos[i];
            if (startInfo.Coordinates.x == x && startInfo.Coordinates.y == y)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }


    [System.Serializable]
    public class StartInfo
    {
        public Vector2Int Coordinates;
        public List<ColorInfo.ColorEnum> ContentInfo;
        public bool isOpen;
    }

    public async void ManageTransfers()
    {
        do
        {
            var cells = CellDataConverter.Convert(GridPlan);
            var solver = new Solver(cells);
            var path = solver.Solve();

            _distinctPath.Clear();

            // move
            foreach (var pair in path)
            {
                var from = GridPlan[pair.from.x, pair.from.y].CellController;
                var to = GridPlan[pair.to.x, pair.to.y].CellController;

                _distinctPath.Add(pair.from);
                _distinctPath.Add(pair.to);

                await from.Move(to, pair.count);
            }

            // check blasts
            foreach (var cell in _distinctPath.Select(pos => GridPlan[pos.x, pos.y].CellController))
            {
                await cell.TryBlast();
            }
        } while (_distinctPath.Count > 0);

        // validate fail state
        GameManager.instance.CheckFailStatus();
    }
}