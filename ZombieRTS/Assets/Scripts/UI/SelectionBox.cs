using UnityEngine;

public class SelectionBox : MonoBehaviour
{
    private Camera myCamera;
    [SerializeField] private RectTransform boxVisual;
    private Rect selectionBox;
    private Vector2 startPosition;
    private Vector2 endPosition;


    private void Start()
    {
        myCamera = Camera.main;
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    // Handles input for drawing and managing the selection box
    private void Update()
    {
        HandleMouseButtonDown();
        HandleMouseButton();
        HandleMouseButtonUp();
    }

    // Handles the initial mouse down event to start drawing the box
    private void HandleMouseButtonDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            selectionBox = new Rect();
        }
    }

    // Handles mouse button hold to update the visual representation of the selection box
    private void HandleMouseButton()
    {
        if (Input.GetMouseButton(0))
        {
            UpdateSelectionVisual();
        }
    }

    // Updates the visual elements of the selection box
    private void UpdateSelectionVisual()
    {
        if (boxVisual.rect.width > 0.9 || boxVisual.rect.height > 0.9)
        {
            UnitSelectionManager.Instance.DeselectAll(); // Updated to match the new method name
            SelectUnits();
        }

        endPosition = Input.mousePosition;
        DrawVisual();
        UpdateSelectionBox();
    }

    // Finalizes selection and resets on mouse button up
    private void HandleMouseButtonUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();
            ResetSelection();
        }
    }

    // Draws the visual representation of the selection box
    private void DrawVisual()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;
        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

        boxVisual.position = boxCenter;
        boxVisual.sizeDelta = boxSize;
    }

    // Updates the actual selection box used for determining selected units
    private void UpdateSelectionBox()
    {
        selectionBox.xMin = Mathf.Min(startPosition.x, Input.mousePosition.x);
        selectionBox.xMax = Mathf.Max(startPosition.x, Input.mousePosition.x);
        selectionBox.yMin = Mathf.Min(startPosition.y, Input.mousePosition.y);
        selectionBox.yMax = Mathf.Max(startPosition.y, Input.mousePosition.y);
    }

    // Selects units within the defined selection box
    private void SelectUnits()
    {
        foreach (GameObject unit in UnitSelectionManager.Instance.GetAllUnits())
        {
            if (selectionBox.Contains(myCamera.WorldToScreenPoint(unit.transform.position)))
            {
                UnitSelectionManager.Instance.AddToSelectedUnits(unit);
            }
        }
    }

    // Resets selection variables after selection is finalized
    private void ResetSelection()
    {
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }
}
