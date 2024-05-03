using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CellController : MonoBehaviour
{
    [Header("References")] public Transform HexStackParent;
    public GameObject opaqueMesh;
    public GameObject transparentMesh;

    [Header("Debug")] public bool IsAction;
    public bool isOccupied;
    public bool isOpen = true;
    [SerializeField] private Vector2 _coordinates = Vector2.zero;

    [Header("Hexagons Related")] [SerializeField]
    private List<HexagonController> hexagons = new List<HexagonController>();

    public List<ColorInfo.ColorEnum> contentInfo;

    public void Starter()
    {
        SetHexagonLists();
    }

    public async Task Move(CellController destination, int count)
    {
        var toCellPos = destination.GetCoordinates();
        var toCell = GridManager.instance.GridPlan[(int) toCellPos.x, (int) toCellPos.y];

        var fromCellPos = GetCoordinates();
        var fromCell = GridManager.instance.GridPlan[(int) fromCellPos.x, (int) fromCellPos.y];

        var itemsToSend = new List<HexagonController>();
        for (var i = hexagons.Count - 1; i >= hexagons.Count - count; i--) itemsToSend.Add(hexagons[i]);

        //Update Grid Classes
        for (var i = 0; i < itemsToSend.Count; i++)
        {
            toCell.CellContentList.Add(fromCell.CellContentList[^1]);
            fromCell.CellContentList.RemoveAt(fromCell.CellContentList.Count - 1);
        }

        //Move Hex Objects
        var index = 0;
        foreach (var item in itemsToSend)
        {
            item.transform.SetParent(destination.HexStackParent);
            var delay = 0.1f * index++;
            item.transform.DOLocalMove(new Vector3(0, destination.hexagons.Count * GridManager.instance.VERTICAL_PLACEMENT_OFFSET, 0), 0.3f).SetDelay(delay);

            destination.hexagons.Add(item);
            hexagons.RemoveAt(hexagons.Count - 1);
        }

        //If There Is No Hex In This Cell Set Occupation Status
        SetOccupied(hexagons.Count > 0);

        await Task.Delay(300 + (count - 1) * 100);
    }

    public async Task TryBlast()
    {
        if (hexagons.Count == 0)
        {
            IsAction = false;
            isOccupied = false;
            return;
        }

        if (IsBlast())
        {
            var blastItems = GetBlastItems();

            var cellPos = GetCoordinates();
            var cell = GridManager.instance.GridPlan[(int) cellPos.x, (int) cellPos.y];

            // Update GridPlan and CellData
            for (var i = 0; i < blastItems.Count; i++)
            {
                hexagons.RemoveAt(hexagons.Count - 1);
                cell.CellContentList.RemoveAt(cell.CellContentList.Count - 1);
            }

            // Blast Items
            BlastSelectedHexList(blastItems);

            SetOccupied(hexagons.Count > 0);

            await Task.Delay(360);
        }
    }

    private List<HexagonController> GetBlastItems()
    {
        var blastItems = new List<HexagonController> {hexagons[^1]};

        var topColor = hexagons[^1].GetColor();
        for (var i = hexagons.Count - 2; i >= 0; i--)
        {
            if (hexagons[i].GetColor() != topColor) break;
            blastItems.Add(hexagons[i]);
        }

        return blastItems;
    }

    private bool IsBlast()
    {
        var topColor = hexagons[^1].GetColor();
        var count = 0;
        for (var i = hexagons.Count - 1; i >= 0; i--)
        {
            if (hexagons[i].GetColor() != topColor) break;
            count++;
        }

        return count >= GameManager.instance.BlastObjectiveAmount;
    }


    private void BlastSelectedHexList(IEnumerable<HexagonController> hexList)
    {
        foreach (var item in hexList)
        {
            item.DestroySelf();
        }

        CanvasManager.instance.UpdateScoreText();
    }

    public void UpdateHexagonsList(List<HexagonController> hexes)
    {
        foreach (var hex in hexes)
        {
            hexagons.Add(hex);
            hex.transform.SetParent(HexStackParent);
            GridManager.instance.GridPlan[(int) _coordinates.x, (int) _coordinates.y].CellContentList.Add(hex.GetColor());
        }
    }

    public void ToggleCellObject(out bool _isOpen)
    {
        var status = opaqueMesh.activeSelf;
        opaqueMesh.SetActive(!status);
        transparentMesh.SetActive(status);

        isOpen = opaqueMesh.activeSelf;
        _isOpen = isOpen;
    }

    #region Getters / Setters

    public void AddHex(HexagonController hex)
    {
        if (!hexagons.Contains(hex))
            hexagons.Add(hex);
    }

    // GETTERS
    public Vector2 GetCoordinates()
    {
        return _coordinates;
    }

    private Vector3 GetCenter()
    {
        var position = transform.position;
        var centerPos = new Vector3(position.x, position.y + .2f, position.z);
        return centerPos;
    }

    public Vector3 GetVerticalPosForHex()
    {
        var verticalOffset = (hexagons.Count - 1) * GridManager.instance.VERTICAL_PLACEMENT_OFFSET;
        var pos = new Vector3(0, verticalOffset, 0);

        return GetCenter() + pos;
    }

    public int GetHexListCount()
    {
        return hexagons.Count;
    }

    // SETTERS
    private void SetHexagonLists()
    {
        foreach (Transform hex in HexStackParent)
        {
            var hexagonController = hex.GetComponent<HexagonController>();
            hexagons.Add(hexagonController);
        }
    }

    public void SetCoordinates(float x, float y)
    {
        _coordinates.x = x;
        _coordinates.y = y;
    }

    public void SetOccupied(bool state)
    {
        isOccupied = state;
    }

    #endregion
}