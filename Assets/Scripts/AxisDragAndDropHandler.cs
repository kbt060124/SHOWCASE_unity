using UnityEngine;

public class AxisDragAndDropHandler : ObjectSelector
{
    private bool isXZMode = true;
    private Plane xzPlane;
    private float initialY;
    private Vector3 screenPoint;
    private Vector3 initialPosition;

    void Start()
    {
        xzPlane = new Plane(Vector3.up, Vector3.zero);
    }

    public void ToggleAxisMode()
    {
        OperationModeManager.OperationMode currentMode = OperationModeManager.Instance.GetCurrentMode();

        if (currentMode != OperationModeManager.OperationMode.AxisDragAndDrop)
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.AxisDragAndDrop);
            isXZMode = true;
            Debug.Log("XZ軸モードをオンにしました");
        }
        else if (isXZMode)
        {
            isXZMode = false;
            Debug.Log("Y軸モードに切り替えました");
        }
        else
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.None);
            Debug.Log("ドラッグ＆ドロップモードをオフにしました");
        }
    }

    void Update()
    {
        if (OperationModeManager.Instance.GetCurrentMode() != OperationModeManager.OperationMode.AxisDragAndDrop) return;

        if (SelectObject())
        {
            initialY = selectedObject.transform.position.y;
            screenPoint = Camera.main.WorldToScreenPoint(selectedObject.transform.position);
            initialPosition = selectedObject.transform.position;
        }

        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            Vector3 newPosition = CalculateNewPosition();
            if (!IsColliding(newPosition))
            {
                selectedObject.transform.position = newPosition;
            }
        }

        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            selectedObject = null;
        }
    }

    Vector3 CalculateNewPosition()
    {
        return isXZMode ? CalculateXZPosition() : CalculateYPosition();
    }

    Vector3 CalculateXZPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (xzPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 newPosition = ray.GetPoint(rayDistance);
            newPosition.y = initialY;
            return newPosition;
        }
        return selectedObject.transform.position;
    }

    Vector3 CalculateYPosition()
    {
        float screenX = screenPoint.x;
        float screenY = Input.mousePosition.y;
        float screenZ = screenPoint.z;

        Vector3 currentScreenPoint = new Vector3(screenX, screenY, screenZ);
        return Camera.main.ScreenToWorldPoint(currentScreenPoint);
    }

    private bool IsColliding(Vector3 newPosition)
    {
        if (selectedObject == null) return false;

        Collider[] colliders = selectedObject.GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            if (collider is BoxCollider boxCollider)
            {
                Vector3 size = boxCollider.size;
                Vector3 center = newPosition + boxCollider.center;

                Collider[] hitColliders = Physics.OverlapBox(center, size / 2, selectedObject.transform.rotation);
                foreach (Collider hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != selectedObject)
                    {
                        return true;
                    }
                }
            }
            else
            {
                // BoxCollider以外のColliderの場合、簡易的な判定を行う
                Vector3 center = newPosition;
                Vector3 size = collider.bounds.size;

                Collider[] hitColliders = Physics.OverlapBox(center, size / 2, selectedObject.transform.rotation);
                foreach (Collider hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != selectedObject)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
