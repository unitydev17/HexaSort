using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Solver : MonoBehaviour
{
    public class Cell
    {
        public List<int> items;
        public bool Empty => items.Count == 0;

        public Cell(IEnumerable<int> items)
        {
            this.items = items.ToList();
        }
    }

    private Cell[,] _board;

    private Cell[,] CloneBoard(Cell[,] sampleBoard)
    {
        var rows = sampleBoard.GetLength(0);
        var columns = sampleBoard.GetLength(1);

        var result = new Cell[rows, columns];

        for (var i = 0; i < rows; i++)
        for (var j = 0; j < columns; j++)
            result[i, j] = new Cell(sampleBoard[i, j].items);

        return result;
    }


    private void Start()
    {
        Init();
        Solve();
    }

    private void Init()
    {
        _board = new[,]
        {
            {new Cell(new[] {2, 1}), new Cell(new int [0])},
            {new Cell(new[] {2, 1}), new Cell(new[] {1})}
        };
    }

    private void Solve()
    {
        var _positions = new List<Vector4>();
        FindPair(_positions);
    }

    private void FindPair(List<Vector4> positions)
    {
        var rows = _board.Length;
        for (var i = 0; i < rows; i++)
        {
            var columns = _board.GetLength(i);
            for (var j = 0; j < columns; j++)
            {
                
                
                
                
                var indexNeighbourRight = j + 1;
                if (indexNeighbourRight > columns - 1) Debug.Log("right neighbour DOESN'T EXIST");
                if (_board[i, indexNeighbourRight].Empty) Debug.Log("right neighbour is EMPTY");
                if (_board[i, j] == _board[i, indexNeighbourRight])
                {
                    Debug.Log("right neighbour is the SAME");
                    // updated board 
                    var newBoard = CloneBoard(_board);
                    newBoard[i, indexNeighbourRight].items.Add(_board[i, j].items.Last());
                    newBoard[i, j].items.RemoveAt(newBoard[i, j].items.Count - 1);
                    // save pair
                    var newPositions = new List<Vector4>(positions);
                    newPositions.Add(new Vector4(i, j, i, indexNeighbourRight));
                    FindPair(newPositions);
                }
                
                
                
                
                var indexNeighbourBottom = i + 1;
                if (indexNeighbourBottom > rows - 1) Debug.Log("bottom neighbour DOESN'T EXIST");
                if (_board[indexNeighbourBottom, j].Empty) Debug.Log("bottom neighbour is EMPTY");
                if (_board[i, j] == _board[i, indexNeighbourBottom])
                {
                    Debug.Log("bottom neighbour is the SAME");
                    // updated board 
                    var newBoard = CloneBoard(_board);
                    newBoard[indexNeighbourBottom, j].items.Add(_board[i, j].items.Last());
                    newBoard[i, j].items.RemoveAt(newBoard[i, j].items.Count - 1);
                    // save pair
                    var newPositions = new List<Vector4>(positions);
                    newPositions.Add(new Vector4(i, j, i, indexNeighbourBottom));
                    FindPair(newPositions);
                }
                
                
                
                
                

                var indexNeighbourLeft = j + 1;
                if (indexNeighbourLeft < 0) Debug.Log("left neighbour DOESN'T EXIST");
                if (_board[i, indexNeighbourLeft].Empty) Debug.Log("left neighbour is EMPTY");
                if (_board[i, j] == _board[i, indexNeighbourLeft])
                {
                    Debug.Log("left neighbour is the SAME");
                    // updated board 
                    var newBoard = CloneBoard(_board);
                    newBoard[i, indexNeighbourLeft].items.Add(_board[i, j].items.Last());
                    newBoard[i, j].items.RemoveAt(newBoard[i, j].items.Count - 1);
                    // save pair
                    var newPositions = new List<Vector4>(positions);
                    newPositions.Add(new Vector4(i, j, i, indexNeighbourLeft));
                    FindPair(newPositions);
                }
                
                
                
                
                
                
                var indexNeighbourTop = i - 1;
                if (indexNeighbourTop < 0) Debug.Log("top neighbour DOESN'T EXIST");
                if (_board[indexNeighbourTop, j].Empty) Debug.Log("top neighbour is EMPTY");
                if (_board[i, j] == _board[i, indexNeighbourTop])
                {
                    Debug.Log("top neighbour is the SAME");
                    // updated board 
                    var newBoard = CloneBoard(_board);
                    newBoard[indexNeighbourTop, j].items.Add(_board[i, j].items.Last());
                    newBoard[i, j].items.RemoveAt(newBoard[i, j].items.Count - 1);
                    // save pair
                    var newPositions = new List<Vector4>(positions);
                    newPositions.Add(new Vector4(i, j, i, indexNeighbourTop));
                    FindPair(newPositions);
                }
            }
        }

        DampPositions(positions);
    }


    private void DampPositions(List<Vector4> pos)
    {
        Debug.Log("-----------DUMP--------");
        foreach (var p in pos)
        {
            Debug.Log(pos);
        }

        Debug.Log($"count = {pos.Count}");
    }
}