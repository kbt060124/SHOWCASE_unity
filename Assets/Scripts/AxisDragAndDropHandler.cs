using UnityEngine;

public class AxisDragAndDropHandler : ObjectSelector
{
    private bool isXZMode = true;
    private Plane xzPlane;
    private float initialY;
    private Vector3 screenPoint;

    void Start()
    {
        xzPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void Update()
    {
        if (SelectObject())
        {
            initialY = selectedObject.transform.position.y;
            screenPoint = Camera.main.WorldToScreenPoint(selectedObject.transform.position);
        }

        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            if (isXZMode)
            {
                MoveOnXZPlane();
            }
            else
            {
                MoveOnYAxis();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedObject = null;
        }
    }

    void MoveOnXZPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (xzPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 newPosition = ray.GetPoint(rayDistance);
            newPosition.y = initialY;
            selectedObject.transform.position = newPosition;
        }
    }

    void MoveOnYAxis()
    {
        float screenX = screenPoint.x;
        float screenY = Input.mousePosition.y;
        float screenZ = screenPoint.z;

        Vector3 currentScreenPoint = new Vector3(screenX, screenY, screenZ);
        Vector3 currentPosition = Camera.main.ScreenToWorldPoint(currentScreenPoint);
        selectedObject.transform.position = currentPosition;
    }

    public void ToggleMode()
    {
        isXZMode = !isXZMode;
        Debug.Log(isXZMode ? "XZ軸モードに切り替えました" : "Y軸モードに切り替えました");
    }
}