using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public static class HexHelper
    {
        public static IEnumerator<Vector2Int> GetNeighbourPos(Vector2Int pos)
        {
            var isEvenRow = pos.x % 2 == 0;

            var offsetsEvenRow = new[]
            {
                new Vector2Int(0, -1), // Top
                new Vector2Int(+1, -1), // Top Right
                new Vector2Int(+1, 0), // Bottom Right
                new Vector2Int(0, +1), // Bottom
                new Vector2Int(-1, 0), // Bottom Left
                new Vector2Int(-1, -1) // Top Left
            };

            var offsetsOddRow = new[]
            {
                new Vector2Int(0, -1), // Top
                new Vector2Int(+1, 0), // Top Right
                new Vector2Int(+1, +1), // Bottom Right
                new Vector2Int(0, +1), // Bottom
                new Vector2Int(-1, +1), // Bottom Left
                new Vector2Int(-1, 0) // Top Left
            };

            var offsets = isEvenRow ? offsetsEvenRow : offsetsOddRow;

            foreach (var offset in offsets)
            {
                var neighbourPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);
                yield return neighbourPos;
            }
        }
    }
}