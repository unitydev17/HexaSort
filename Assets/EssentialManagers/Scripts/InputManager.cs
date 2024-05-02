using UnityEngine;

public class InputManager : MonoSingleton<InputManager>
{
    public event System.Action TouchStartEvent;
    public event System.Action TouchEndEvent;
    public event System.Action<PickableStack> StackPlacedOnGridEvent;

    public LayerMask PickableLayer;
    [SerializeField] private bool blockPicking;

    #region Essentials

    protected override void Awake()
    {
        base.Awake();
        _camera = Camera.main;
    }

    public void OnPointerDown()
    {
        TouchStartEvent?.Invoke();
    }

    public void OnPointerUp()
    {
        TouchEndEvent?.Invoke();
    }

    #endregion

    private Camera mainCamera;
    private bool isDragging = false;
    private PickableStack selectedPickable;
    private Vector3 offset;
    private Camera _camera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!GameManager.instance.isLevelActive) return;

        // Check for mouse input
        if (Input.GetMouseButtonDown(0))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, 300, PickableLayer))
            {
                if (hit.collider.TryGetComponent(out PickableStack pickable))
                {
                    if (blockPicking) return;
                    if (pickable.IsPicked) return;

                    pickable.GetPicked();

                    blockPicking = true;
                    isDragging = true;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedPickable != null)
            {
                // Calculate the cell position and desired Y-axis offset
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    var cellPosition = hit.point;

                    // Place the object on the grid with the calculated parameters
                    var pickableStack = selectedPickable.GetComponent<PickableStack>();
                    if (pickableStack != null)
                    {
                        pickableStack.GetPlaced(cellPosition);
                    }
                }

                selectedPickable = null;
                isDragging = false;
            }
        }

        // If dragging, move the selected object with the mouse
        if (isDragging && selectedPickable != null)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                selectedPickable.transform.position = new Vector3(hit.point.x, .2f, hit.point.z);
            }
        }
    }


    public void SetBlockPicking(bool shouldBlock)
    {
        blockPicking = shouldBlock;
    }

    public void TriggerStackPlacedOnGridEvent(PickableStack stack)
    {
        StackPlacedOnGridEvent?.Invoke(stack);
    }
}