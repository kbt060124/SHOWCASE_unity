using UnityEngine;
using UnityEngine.EventSystems;

public class AxisDragAndDropHandler : ObjectSelector
{

    private Plane xzPlane;
    private Vector3 initialPosition;
    private float initialY;
    private bool isDragging = false;
    private Vector3 dragStartPosition;
    private float dragThreshold = 5f;

    private CanvasManager canvasManager;

    private struct RoomBoundaries
    {
        public GameObject Right, Left, Back, Front, Ceiling, Floor;
    }
    private RoomBoundaries roomBoundaries;

    private Vector3 roomMin;
    private Vector3 roomMax;

    //------------------------------------------------
    // 初期化と設定
    //------------------------------------------------

    void Start()
    {
        InitializeComponents();
        SetInitialMode();
        CalculateRoomBounds();
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

    private void CalculateRoomBounds()
    {
        if (roomBoundaries.Floor == null || roomBoundaries.Back == null || roomBoundaries.Front == null ||
            roomBoundaries.Ceiling == null || roomBoundaries.Left == null || roomBoundaries.Right == null)
        {
            Debug.LogError("部屋の境界を計算できません。コンポーネントが不足しています。");
            return;
        }

        // 壁の厚さを計算
        float floorThickness = roomBoundaries.Floor.GetComponent<Renderer>().bounds.size.y;
        float wallBackThickness = roomBoundaries.Back.GetComponent<Renderer>().bounds.size.z;
        float wallFrontThickness = roomBoundaries.Front.GetComponent<Renderer>().bounds.size.z;
        float ceilingThickness = roomBoundaries.Ceiling.GetComponent<Renderer>().bounds.size.y;
        float wallLeftThickness = roomBoundaries.Left.GetComponent<Renderer>().bounds.size.x;
        float wallRightThickness = roomBoundaries.Right.GetComponent<Renderer>().bounds.size.x;

        // Y軸の余裕を設定（部屋の高さの10%とします）
        float roomHeight = roomBoundaries.Ceiling.GetComponent<Renderer>().bounds.min.y - roomBoundaries.Floor.GetComponent<Renderer>().bounds.max.y;
        float yAxisMargin = roomHeight * 0.15f;

        // roomMinとroomMaxを計算
        roomMin = new Vector3(
            roomBoundaries.Left.GetComponent<Renderer>().bounds.max.x + wallLeftThickness,
            roomBoundaries.Floor.GetComponent<Renderer>().bounds.max.y + floorThickness - yAxisMargin, // Y軸の下限
            roomBoundaries.Front.GetComponent<Renderer>().bounds.max.z + wallFrontThickness
        );

        roomMax = new Vector3(
            roomBoundaries.Right.GetComponent<Renderer>().bounds.min.x - wallRightThickness,
            roomBoundaries.Ceiling.GetComponent<Renderer>().bounds.min.y - ceilingThickness, // Y軸の上限
            roomBoundaries.Back.GetComponent<Renderer>().bounds.min.z - wallBackThickness
        );
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
        if (selectedObject == null || EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            dragStartPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 dragDelta = Input.mousePosition - dragStartPosition;
            if (dragDelta.magnitude > dragThreshold)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 newPosition = CalculateNewPosition(ray, OperationModeManager.Instance.IsXYMode());

                // IsCollidingチェックを削除し、直接新しい位置を設定
                selectedObject.transform.position = newPosition;
            }
        }
    }

    //------------------------------------------------
    // 位置計算
    //------------------------------------------------

    private Vector3 CalculateNewPosition(Ray ray, bool isXYMode)
    {
        Vector3 newPosition = isXYMode ? CalculateXYPosition(ray) : CalculateXZPosition(ray);
        return ClampPositionToRoom(newPosition);
    }

    private Vector3 CalculateXYPosition(Ray ray)
    {
        Plane xyPlane = new Plane(Camera.main.transform.forward, selectedObject.transform.position);
        if (xyPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            return new Vector3(
                selectedObject.transform.position.x,
                hitPoint.y,
                selectedObject.transform.position.z
            );
        }
        return selectedObject.transform.position;
    }

    private Vector3 CalculateXZPosition(Ray ray)
    {
        // オブジェクトの現在のY座標を使用してxzPlaneを作る
        Plane objectXZPlane = new Plane(Vector3.up, new Vector3(0, selectedObject.transform.position.y, 0));

        if (objectXZPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            return new Vector3(hitPoint.x, selectedObject.transform.position.y, hitPoint.z);
        }
        return selectedObject.transform.position;
    }

    private Vector3 ClampPositionToRoom(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, roomMin.x, roomMax.x),
            Mathf.Clamp(position.y, roomMin.y, roomMax.y),
            Mathf.Clamp(position.z, roomMin.z, roomMax.z)
        );
    }
}
