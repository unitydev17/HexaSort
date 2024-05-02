using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class PickableStack : MonoBehaviour
{
    [Header("Configuration")] public bool IsPicked;
    public bool IsPlaced;
    [SerializeField] private LayerMask cellLayer;

    [Header("Debug")] private Vector3 _startPos;
    private Vector3 offset => new Vector3(0, .5f, 2);
    private Collider _collider => GetComponent<Collider>();
    [SerializeField] private List<HexagonController> hexagons = new List<HexagonController>();
    private Camera _camera;
    private Transform _tr;
    private CellController _prevCellBelow;

    private void Awake()
    {
        _camera = Camera.main;
        _tr = transform;
        _startPos = _tr.position;
    }

    private IEnumerator Start()
    {
        GameManager.instance.LevelEndedEvent += DestroySelf;

        yield return null;

        foreach (Transform hex in _tr)
        {
            hexagons.Add(hex.GetComponent<HexagonController>());
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!IsPicked) return;
            IsPicked = false;

            var cell = GetCellBelow();

            if (cell != null)
            {
                GoToCell(cell);
                _collider.enabled = false;
            }
            else
            {
                GetReleased();
                _prevCellBelow = null;
            }
        }

        if (!gameObject.activeInHierarchy) return;
        if (!IsPicked) return;

        FollowMousePos();
        MarkCell();
    }

    private void MarkCell()
    {
        var cellBelow = GetCellBelow();
        if (cellBelow is null)
        {
            if (_prevCellBelow is null) return;
            _prevCellBelow.ToggleCellObject(out _);
            _prevCellBelow = null;
            return;
        }

        if (cellBelow == _prevCellBelow) return;
        cellBelow.ToggleCellObject(out _);

        if (_prevCellBelow is null) _prevCellBelow = cellBelow;
        else
        {
            _prevCellBelow.ToggleCellObject(out _);
            _prevCellBelow = cellBelow;
        }
    }

    private void FollowMousePos()
    {
        var mousePosition =
            _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));

        // Update the position of the selected object to the mouse position
        _tr.position = new Vector3(mousePosition.x + offset.x, _tr.position.y, mousePosition.z + offset.z);
    }

    private void GoToCell(CellController targetCell)
    {
        targetCell.UpdateHexagonsList(hexagons);

        for (var i = 0; i < hexagons.Count; i++)
        {
            var hex = hexagons[i].transform;

            hex.transform.DOLocalMove(new Vector3(0, i * GridManager.instance.VERTICAL_PLACEMENT_OFFSET, 0), 0.3f);
        }

        InputManager.instance.SetBlockPicking(shouldBlock: false);
        InputManager.instance.TriggerStackPlacedOnGridEvent(this);
        targetCell.SetOccupied(true);
        targetCell.StartCoroutine(targetCell.ControlTransfer(.4f));

        DestroySelf();
    }

    private void DestroySelf()
    {
        Destroy(gameObject, .1f);
        GameManager.instance.LevelEndedEvent -= DestroySelf;
    }

    #region GETTERS

    private CellController GetCellBelow()
    {
        var ray = new Ray(_tr.position, -_tr.up);

        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);

        if (Physics.Raycast(ray, out var hit, 100, cellLayer))
        {
            if (hit.collider.transform.parent.parent.TryGetComponent(out CellController cell))
            {
                if (cell.isOccupied) return null;
                return cell;
            }
        }

        return null;
    }

    public void GetPicked()
    {
        IsPicked = true;
    }

    public void GetReleased()
    {
        IsPicked = false;
        InputManager.instance.SetBlockPicking(false);
        _tr.DOMove(_startPos, .5f);
    }

    public void GetPlaced(Vector3 cellPosition)
    {
        cellPosition.y += 0.1f;
        _tr.position = cellPosition;
        IsPicked = true;
    }

    #endregion
}