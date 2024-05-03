using System.Collections.Generic;
using System.Linq;

namespace Solution
{
    public static class CellDataConverter
    {
        public static Cell[,] Convert(CellData[,] cellData)
        {
            var columns = cellData.GetLength(0);
            var rows = cellData.GetLength(1);

            var result = new Cell[columns, rows];


            for (var i = 0; i < columns; i++)
            {
                for (var j = 0; j < rows; j++)
                {
                    result[i, j] = new Cell(EnumToValues(cellData[i, j].CellContentList));
                }
            }

            return result;
        }

        private static IEnumerable<int> EnumToValues(IEnumerable<ColorInfo.ColorEnum> sourceList) => sourceList.Select(colorEnum => (int) colorEnum).ToList();
    }
}