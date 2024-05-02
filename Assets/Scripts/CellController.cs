using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour
{
    [Header("References")]
    public Transform HexStackParent;
    public GameObject opaqueMesh;
    public GameObject transparentMesh;

    [Header("Debug")]
    public bool IsAction;
    public bool isOccupied;
    public bool isOpen = true;
    [SerializeField] private Vector2 _coordinates = Vector2.zero;

    [Header("Hexagons Related")]
    [SerializeField]
    private List<HexagonController> hexagons = new List<HexagonController>();
    public List<ColorInfo.ColorEnum> contentInfo;
    public void Starter()
    {
        SetHexagonLists();
    }
    public IEnumerator ControlTransfer(float StartControlDelay)
    {
        //If There is Any Hex
        if (hexagons.Count > 0)
        {
            yield return new WaitForSeconds(StartControlDelay);

            //If There is opportunity to Blast
            if (IsThereBlast())
            {
                //Create Blast Hex List
                var selectedHexList = new List<HexagonController>();
                var topHexColor = hexagons[^1].GetColor();
                selectedHexList.Add(hexagons[^1]);

                for (var i = hexagons.Count - 2; i >= 0; i--)
                {
                    if (hexagons[i].GetColor() == topHexColor)
                    {
                        selectedHexList.Add(hexagons[i]);
                    }
                    else
                    {
                        break;
                    }
                }

                //Update GridPlan and CellData
                for (var i = 0; i < selectedHexList.Count; i++)
                {
                    hexagons.RemoveAt(hexagons.Count - 1);
                    var ThisGridClass = GridManager.instance.GridPlan[(int)GetCoordinates().x, (int)GetCoordinates().y];
                    ThisGridClass.CellContentList.RemoveAt(ThisGridClass.CellContentList.Count - 1);
                }

                //Blast Rope Group
                BlastSelectedHexList(selectedHexList);

                //Wait Blast Complete Time
                yield return new WaitForSeconds(0.36f);

                SetOccupied(hexagons.Count > 0);
                StartCoroutine(ControlTransfer(0));

                GameManager.instance.CheckFailStatus();
            }
            //If No Blast
            else
            {
                var TopRopeColor = hexagons[hexagons.Count - 1].GetColor();
                var SendOrTake = GridManager.TransferType.Take;
                var NeighboursCoordinateList = GridManager.instance.GetNeighboursCoordinates(GetCoordinates());
                var SelectedNeighbours = new List<Vector2>();
                var SelectedNeighbour = Vector2.zero;

                //Control All Finded Neighbours Cells
                for (var i = 0; i < NeighboursCoordinateList.Count; i++)
                {
                    var NeighbourPosX = (int)NeighboursCoordinateList[i].x;
                    var NeighbourPosY = (int)NeighboursCoordinateList[i].y;
                    var ControlNeighbourGrid = GridManager.instance.GridPlan[NeighbourPosX, NeighbourPosY];
                    var ControlNeighbourGridPart = GridManager.instance.GridPlan[NeighbourPosX, NeighbourPosY].CellObject.GetComponent<CellController>();

                    //If Cell Open And Have a Hexagon
                    if (ControlNeighbourGrid.isOpen && ControlNeighbourGrid.CellContentList.Count > 0)
                    {
                        //If Hexagon Colors Matched
                        if (TopRopeColor == ControlNeighbourGridPart.hexagons[ControlNeighbourGridPart.hexagons.Count - 1].GetColor())
                        {
                            SelectedNeighbours.Add(new Vector2(NeighbourPosX, NeighbourPosY));
                        }
                    }
                }

                //If There Is Possible Moves
                if (SelectedNeighbours.Count > 0)
                {
                    //Set Selected Neighbours To First Finded
                    SelectedNeighbour = SelectedNeighbours[0];

                    //Check Selected Neighbours Pure Status
                    for (var i = 0; i < SelectedNeighbours.Count; i++)
                    {
                        if (GridManager.instance.GridPlan[(int)SelectedNeighbours[i].x, (int)SelectedNeighbours[i].y].CellObject.GetComponent<CellController>().IsPure() && !IsPure())
                        {
                            SendOrTake = GridManager.TransferType.Send;
                            SelectedNeighbour = SelectedNeighbours[i];
                            break;
                        }
                    }

                    //If Transfer Type is "Take" and There is Other Color Rope, Control Second Color Transfer is Possible
                    if (SendOrTake == GridManager.TransferType.Take)
                    {
                        /*
                        bool IsThereOtherColor = false;

                        for (int i = 0; i < NeighboursCoordinateList.Count; i++)
                        {
                            int NeighbourPosX = (int)NeighboursCoordinateList[i].x;
                            int NeighbourPosY = (int)NeighboursCoordinateList[i].y;
                            CellData ControlNeighbourGrid = GridManager.instance.I.GridPlan[NeighbourPosX, NeighbourPosY];
                            GridPart ControlNeighbourGridPart = GridManager.instance.I.GridPlan[NeighbourPosX, NeighbourPosY].CellObject.GetComponent<GridPart>();

                            //If Grid Open And Have a Rope
                            if (ControlNeighbourGrid.IsGridOpen && ControlNeighbourGrid.CellContentList.Count > 0)
                            {
                                //If Rope Colors Matched
                                if (topHexColor == ControlNeighbourGridPart.hexagons[ControlNeighbourGridPart.hexagons.Count - 1].GetComponent<RopePart>().HexColor)
                                {
                                    SelectedNeighbours.Add(new Vector2(NeighbourPosX, NeighbourPosY));
                                }
                            }
                        }
                        */
                    }

                    var SelectedGridPart = GridManager.instance.GridPlan[(int)SelectedNeighbour.x, (int)SelectedNeighbour.y].CellObject.GetComponent<CellController>();
                    var SelectedGridClass = GridManager.instance.GridPlan[(int)SelectedNeighbour.x, (int)SelectedNeighbour.y];
                    var ThisGridClass = GridManager.instance.GridPlan[(int)GetCoordinates().x, (int)GetCoordinates().y];

                    //Change Action Situations
                    IsAction = true;
                    SelectedGridPart.IsAction = true;

                    //Take
                    if (SendOrTake == GridManager.TransferType.Take)
                    {
                        //Create Take Rope List
                        var WillTakeRopeList = new List<HexagonController>();
                        for (var i = SelectedGridPart.hexagons.Count - 1; i >= 0; i--)
                        {
                            if (SelectedGridPart.hexagons[i].GetColor() == TopRopeColor)
                            {
                                WillTakeRopeList.Add(SelectedGridPart.hexagons[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        //Update Grid Classes
                        for (var i = 0; i < WillTakeRopeList.Count; i++)
                        {
                            ThisGridClass.CellContentList.Add(SelectedGridClass.CellContentList[SelectedGridClass.CellContentList.Count - 1]);
                            SelectedGridClass.CellContentList.RemoveAt(SelectedGridClass.CellContentList.Count - 1);
                        }

                        //Move Rope Objects
                        for (var i = 0; i < WillTakeRopeList.Count; i++)
                        {
                            WillTakeRopeList[i].transform.SetParent(HexStackParent);
                            WillTakeRopeList[i].transform.DOLocalMove(new Vector3(0, hexagons.Count * GridManager.instance.VERTICAL_PLACEMENT_OFFSET, 0), 0.3f);

                            hexagons.Add(WillTakeRopeList[i]);
                            SelectedGridPart.hexagons.RemoveAt(SelectedGridPart.hexagons.Count - 1);

                            yield return new WaitForSeconds(0.06f);
                        }

                        SetOccupied(hexagons.Count > 0);
                    }

                    //Send
                    else if (SendOrTake == GridManager.TransferType.Send)
                    {
                        //Create Send Rope List
                        var WillSendRopeList = new List<HexagonController>();
                        for (var i = hexagons.Count - 1; i >= 0; i--)
                        {
                            if (hexagons[i].GetColor() == TopRopeColor)
                            {
                                WillSendRopeList.Add(hexagons[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        //Update Grid Classes
                        for (var i = 0; i < WillSendRopeList.Count; i++)
                        {
                            SelectedGridClass.CellContentList.Add(ThisGridClass.CellContentList[ThisGridClass.CellContentList.Count - 1]);
                            ThisGridClass.CellContentList.RemoveAt(ThisGridClass.CellContentList.Count - 1);
                        }

                        //Move Hex Objects
                        for (var i = 0; i < WillSendRopeList.Count; i++)
                        {
                            WillSendRopeList[i].transform.SetParent(SelectedGridPart.HexStackParent);
                            WillSendRopeList[i].transform.DOLocalMove(new Vector3(0, SelectedGridPart.hexagons.Count * GridManager.instance.VERTICAL_PLACEMENT_OFFSET, 0), 0.3f);

                            SelectedGridPart.hexagons.Add(WillSendRopeList[i]);
                            hexagons.RemoveAt(hexagons.Count - 1);

                            yield return new WaitForSeconds(0.06f);
                        }

                        //If There Is No Hex In This Cell Set Occupation Status
                        SetOccupied(hexagons.Count > 0);
                    }

                    //Wait Transfer Complete Time
                    yield return new WaitForSeconds(0.36f);

                    StartCoroutine(ControlTransfer(0));
                    StartCoroutine(SelectedGridPart.ControlTransfer(0));
                }
                else
                {
                    IsAction = false;
                    GameManager.instance.CheckFailStatus();
                }
            }
        }
        else
        {
            IsAction = false;
            isOccupied = false;
            GameManager.instance.CheckFailStatus();
        }
        GameManager.instance.CheckFailStatus();
    }
    public bool IsThereBlast()
    {
        var performBlast = false;
        if (IsPure())
        {
            if (hexagons.Count >= GameManager.instance.BlastObjectiveAmount)
                performBlast = true;

        }
        //if (hexagons.Count > 1)
        //{
        //    int matchCount = 0;
        //    ColorInfo.ColorEnum TopRopeColor = hexagons[hexagons.Count - 1].GetColor();
        //    for (int i = hexagons.Count - 2; i >= 0; i--)
        //    {
        //        if (hexagons[i].GetColor() == TopRopeColor)
        //        {
        //            matchCount++;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    if (matchCount >= GameManager.instance.BlastObjectiveAmount)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        return performBlast;
    }

    private bool IsPure()
    {
        var TopRopeColor = hexagons[hexagons.Count - 1].GetColor();
        for (var i = hexagons.Count - 1; i >= 0; i--)
        {
            if (hexagons[i].GetColor() != TopRopeColor)
            {
                return false;
            }
        }

        return true;
    }
    public void BlastSelectedHexList(List<HexagonController> hexList)
    {
        for (var i = 0; i < hexList.Count; i++)
        {
            hexList[i].DestroySelf();
        }

        CanvasManager.instance.UpdateScoreText();
    }
    public void UpdateHexagonsList(List<HexagonController> hexes)
    {
        foreach (var hex in hexes)
        {
            hexagons.Add(hex);
            hex.transform.SetParent(HexStackParent);
            GridManager.instance.GridPlan[(int)_coordinates.x, (int)_coordinates.y].CellContentList.Add(hex.GetColor());
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
    public Vector3 GetCenter()
    {
        var centerPos = new Vector3(transform.position.x, transform.position.y + .2f, transform.position.z);
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