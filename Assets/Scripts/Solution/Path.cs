using UnityEngine;

public struct Path
{
    public Vector2Int from;
    public Vector2Int to;
    public int count;

    public Path(Vector2Int from, Vector2Int to, int count)
    {
        this.from = from;
        this.to = to;
        this.count = count;
    }


    public override string ToString()
    {
        return $"from {from} to {to} = {count})";
    }
}