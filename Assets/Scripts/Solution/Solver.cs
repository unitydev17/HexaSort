using System.Collections.Generic;
using System.Linq;
using Solution;
using UnityEngine;

public class Solver
{
    private readonly Cell[,] _board;
    private readonly int _rows;
    private readonly int _columns;

    private List<Path> _bestPath = new List<Path>();
    private int _pathCount;
    private int _itemsTransferCount;

    public Solver(Cell[,] board)
    {
        _board = board;
        _columns = _board.GetLength(0);
        _rows = _board.GetLength(1);
    }

    public IEnumerable<Path> Solve()
    {
        var _positions = new List<Path>();
        FindPair(_board, _positions);

        // DampPositions(_bestPath);
        return _bestPath;
    }

    private static Cell[,] CloneBoard(Cell[,] sampleBoard)
    {
        var columns = sampleBoard.GetLength(0);
        var rows = sampleBoard.GetLength(1);

        var result = new Cell[columns, rows];

        for (var i = 0; i < columns; i++)
        {
            for (var j = 0; j < rows; j++)
            {
                result[i, j] = new Cell(sampleBoard[i, j].items);
            }
        }

        return result;
    }

    private void FindPair(Cell[,] board, List<Path> positions)
    {
        for (var i = 0; i < _columns; i++)
        {
            for (var j = 0; j < _rows; j++)
            {
                if (board[i, j].items.Count == 0) continue;

                var pos = new Vector2Int(i, j);

                // iterate against the hexagonal positions

                var iterator = HexHelper.GetNeighbourPos(pos);
                while (iterator.MoveNext())
                {
                    if (CheckNear(board, pos, iterator.Current, positions)) break;
                }
            }
        }


        // Selection criteria:
        // a) overall path steps to move
        // if (positions.Count <= _pathCount) return;
        // b) overall items to move

        var itemCounts = positions.Sum(p => p.count);
        if (itemCounts <= _itemsTransferCount) return;


        // _pathCount = positions.Count;
        _bestPath = positions;
        _itemsTransferCount = itemCounts;
    }


    private bool CheckNear(Cell[,] board, Vector2Int pos, Vector2Int nearPos, IEnumerable<Path> positions)
    {
        // check borders
        if (nearPos.x > _columns - 1) return false;
        if (nearPos.x < 0) return false;
        if (nearPos.y > _rows - 1) return false;
        if (nearPos.y < 0) return false;


        var neighbourItems = board[nearPos.x, nearPos.y].items;
        if (neighbourItems.Count == 0) return false;

        var items = board[pos.x, pos.y].items;
        if (items.Last() != neighbourItems.Last()) return false;

        // create updated board 
        var newBoard = CloneBoard(board);

        // copy values 
        var copyValue = items.Last();
        var countValue = 0;

        for (var k = items.Count - 1; k >= 0; k--)
        {
            if (items[k] != copyValue) break;
            countValue++;
            newBoard[nearPos.x, nearPos.y].items.Add(copyValue);
        }

        newBoard[pos.x, pos.y].items.RemoveRange(items.Count - countValue, countValue);


        // save move position
        var newPositions = new List<Path>(positions)
        {
            new Path(pos, nearPos, countValue)
        };

        FindPair(newBoard, newPositions);

        return true;
    }


    private static void DampPositions(IReadOnlyCollection<Path> path)
    {
        Debug.Log("-----------DUMP--------");
        if (path == null)
        {
            Debug.Log($"count = 0");
            return;
        }

        foreach (var p in path) Debug.Log(p);
        Debug.Log($"count = {path.Count}");
    }
}