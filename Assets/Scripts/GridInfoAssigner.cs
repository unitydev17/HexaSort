using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GridManager;

public class GridInfoAssigner : MonoBehaviour
{
    public List<StartInfo> startInfos = new();
    public string levelKey;

    private IEnumerator Start()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    public void AddToStartInfo(CellController selectedCell)
    {
        var coordinates = new Vector2Int((int)selectedCell.GetCoordinates().x, (int)selectedCell.GetCoordinates().y);
        if (startInfos.Any(info => info.Coordinates == coordinates)) return;

        if (startInfos == null) startInfos = new();

        StartInfo info = new();

        info.Coordinates = new Vector2Int((int)selectedCell.GetCoordinates().x, (int)selectedCell.GetCoordinates().y);
        info.ContentInfo = selectedCell.contentInfo;
        info.isOpen = selectedCell.isOpen;


        startInfos.Add(info);

        levelKey = "";
        var glyphs = "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ0123456789";
        var charAmount = Random.Range(15, 20); //set those to the minimum and maximum length of your string
        for (var i = 0; i < charAmount; i++)
        {
            levelKey += glyphs[Random.Range(0, glyphs.Length)];
        }
    }
}
