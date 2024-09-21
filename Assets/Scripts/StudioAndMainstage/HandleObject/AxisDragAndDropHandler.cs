using UnityEngine;
using UnityEngine.EventSystems;

public class AxisDragAndDropHandler : ObjectSelector
{
    private const float Y_MOVEMENT_THRESHOLD = 0.8f;

    private Plane xzPlane;
    private Vector3 initialPosition;
    private float initialY;

    private CanvasManager canvasManager;

    private struct RoomBoundaries
    {
        public GameObject Right, Left, Back, Front, Ceiling, Floor;
    }
    private RoomBoundaries roomBoundaries;

    //------------------------------------------------
    // 初期化と設定
    //------------------------------------------------

    void Start()
    {
        InitializeComponents();
        SetInitialMode();
    }

    private void InitializeComponents()
    {
        xzPlane = new Plane(Vector3.up, Vector3.zero);
        roomBoundaries.Right = GameObject.FindGameObjectWithTag("WallRight");
        roomBoundaries.Left = GameObject.FindGameObjectWithTag("WallLeft");
        roomBoundaries.Back = GameObject.FindGameObjectWithTag("WallBack");
        roomBoundaries.Front = GameObject.FindGameObjectWithTag("WallFront");
        roomBoundaries.Ceiling = GameObject.FindGameObjectWithTag("Ceiling");
        roomBoundaries.Floor = GameObject.FindGameObjectWithTag("Floor");

        canvasManager = FindObjectOfType<CanvasManager>();
        if (canvasManager == null)
        {
            Debug.LogError("CanvasManagerが見つかりません。");
        }
    }

    private void SetInitialMode()
    {
        OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.AxisDragAndDropXZ);
        Debug.Log("初期状態：XZ軸モードに設定しました");
    }

    //------------------------------------------------
    // モード切り替えと更新
    //------------------------------------------------

    public void ToggleAxisMode()
    {
        if (canvasManager != null && canvasManager.isMainstageActive) return;

        OperationModeManager.OperationMode currentMode = OperationModeManager.Instance.GetCurrentMode();
        OperationModeManager.OperationMode newMode = currentMode == OperationModeManager.OperationMode.AxisDragAndDropXY
            ? OperationModeManager.OperationMode.AxisDragAndDropXZ
            : OperationModeManager.OperationMode.AxisDragAndDropXY;

        OperationModeManager.Instance.SetMode(newMode);
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool isAxisModeXY = OperationModeManager.Instance.IsCurrentMode(OperationModeManager.OperationMode.AxisDragAndDropXY);
        if (buttonController != null)
        {
            buttonController.UpdateButtonState(buttonController.axisButton, buttonController.axisNormalSprite, buttonController.axisSelectedSprite, isAxisModeXY);
        }
        else
        {
            Debug.LogWarning("ButtonController is not assigned in AxisDragAndDropHandler.");
        }
    }

    //------------------------------------------------
    // メインループと操作処理
    //------------------------------------------------

    void Update()
    {
        if (ShouldSkipUpdate()) return;

        HandleObjectSelection();
        HandleObjectMovement();
    }

    private bool ShouldSkipUpdate()
    {
        return (canvasManager != null && canvasManager.isMainstageActive) || !OperationModeManager.Instance.CanMove();
    }

    private void HandleObjectSelection()
    {
        if (SelectObject())
        {
            initialPosition = selectedObject.transform.position;
            initialY = selectedObject.transform.position.y;
        }
    }

    private void HandleObjectMovement()
    {
        if (selectedObject == null || !Input.GetMouseButton(0) || EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 newPosition = CalculateNewPosition(ray, OperationModeManager.Instance.IsXYMode());

        if (!IsColliding(newPosition))
        {
            selectedObject.transform.position = newPosition;
        }
    }

    //------------------------------------------------
    // 位置計算
    //------------------------------------------------

    private Vector3 CalculateNewPosition(Ray ray, bool isXYMode)
    {
        return isXYMode ? CalculateXYPosition(ray) : CalculateXZPosition(ray);
    }

    private Vector3 CalculateXYPosition(Ray ray)
    {
        Plane xyPlane = new Plane(Camera.main.transform.forward, selectedObject.transform.position);
        if (xyPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 newPosition = new Vector3(
                selectedObject.transform.position.x,
                hitPoint.y,
                selectedObject.transform.position.z
            );

            if (IsYMovementValid(newPosition.y))
            {
                return newPosition;
            }
        }
        return selectedObject.transform.position;
    }

    private Vector3 CalculateXZPosition(Ray ray)
    {
        // オブジェクトの現在のY座標を使用してxzPlaneを作成
        Plane objectXZPlane = new Plane(Vector3.up, new Vector3(0, selectedObject.transform.position.y, 0));

        if (objectXZPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            return new Vector3(hitPoint.x, selectedObject.transform.position.y, hitPoint.z);
        }
        return selectedObject.transform.position;
    }

    private bool IsYMovementValid(float newY)
    {
        float yDifference = Mathf.Abs(newY - selectedObject.transform.position.y);
        if (yDifference < Y_MOVEMENT_THRESHOLD)
        {
            return true;
        }
        Debug.Log($"Y座標の変更が大きすぎるため移動をキャンセル。差分: {yDifference:F3}, 閾値: {Y_MOVEMENT_THRESHOLD}");
        return false;
    }

    //------------------------------------------------
    // 衝突判定
    //------------------------------------------------

    private bool IsColliding(Vector3 newPosition)
    {
        if (canvasManager != null && canvasManager.isMainstageActive) return false;
        if (selectedObject == null) return false;

        Bounds objectBounds = GetObjectBounds(selectedObject, newPosition);

        if (IsCollidingWithWalls(objectBounds)) return true;
        if (IsCollidingWithFloor(objectBounds, newPosition)) return false;
        if (IsCollidingWithOtherObjects(objectBounds)) return true;

        return false;
    }

    private bool IsCollidingWithWalls(Bounds objectBounds)
    {
        return (roomBoundaries.Right != null && objectBounds.max.x > roomBoundaries.Right.transform.position.x) ||
               (roomBoundaries.Left != null && objectBounds.min.x < roomBoundaries.Left.transform.position.x) ||
               (roomBoundaries.Back != null && objectBounds.max.z > roomBoundaries.Back.transform.position.z) ||
               (roomBoundaries.Front != null && objectBounds.min.z < roomBoundaries.Front.transform.position.z) ||
               (roomBoundaries.Ceiling != null && objectBounds.max.y > roomBoundaries.Ceiling.transform.position.y);
    }

    private bool IsCollidingWithFloor(Bounds objectBounds, Vector3 newPosition)
    {
        if (roomBoundaries.Floor != null)
        {
            float floorY = roomBoundaries.Floor.transform.position.y;
            if (objectBounds.min.y < floorY)
            {
                float adjustment = floorY - objectBounds.min.y;
                selectedObject.transform.position = new Vector3(newPosition.x, newPosition.y + adjustment, newPosition.z);
                return true;
            }
        }
        return false;
    }

    private bool IsCollidingWithOtherObjects(Bounds objectBounds)
    {
        Collider[] hitColliders = Physics.OverlapBox(objectBounds.center, objectBounds.extents, Quaternion.identity);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != selectedObject && hitCollider.gameObject != roomBoundaries.Floor)
            {
                return true;
            }
        }
        return false;
    }

    private Bounds GetObjectBounds(GameObject obj, Vector3 position)
    {
        Bounds bounds = new Bounds(position, Vector3.zero);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 offset = position - obj.transform.position;
        bounds.center += offset;

        return bounds;
    }
}
