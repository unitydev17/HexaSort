using System.Collections.Generic;
using System.Linq;

public class Cell
{
    public readonly List<int> items;

    public Cell(IEnumerable<int> items)
    {
        this.items = items.ToList();
    }
}