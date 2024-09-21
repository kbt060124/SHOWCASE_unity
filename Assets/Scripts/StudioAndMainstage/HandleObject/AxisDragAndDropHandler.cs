using UnityEngine;
using UnityEngine.EventSystems;

public class AxisDragAndDropHandler : ObjectSelector
{
    private bool isXZMode = true;
    private Plane xzPlane;
    private float initialY;
    private Vector3 screenPoint;
    private Vector3 initialPosition;
    private GameObject wallRight;
    private GameObject wallLeft;
    private GameObject wallBack;
    private GameObject wallFront;
    private GameObject ceiling;
    private GameObject floor;
    private CanvasManager canvasManager;

    void Start()
    {
        xzPlane = new Plane(Vector3.up, Vector3.zero);
        wallRight = GameObject.FindGameObjectWithTag("WallRight");
        wallLeft = GameObject.FindGameObjectWithTag("WallLeft");
        wallBack = GameObject.FindGameObjectWithTag("WallBack");
        wallFront = GameObject.FindGameObjectWithTag("WallFront");
        ceiling = GameObject.FindGameObjectWithTag("Ceiling");
        floor = GameObject.FindGameObjectWithTag("Floor");

        // 初期状態をXZ軸モードに設定
        OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.AxisDragAndDropXZ);
        Debug.Log("初期状態：XZ軸モードに設定しました");

        // CanvasManagerの参照を取得
        canvasManager = FindObjectOfType<CanvasManager>();
        if (canvasManager == null)
        {
            Debug.LogError("CanvasManagerが見つかりません。");
        }
    }

    public void ToggleAxisMode()
    {
        // Mainstageがアクティブな場合は何もしない
        if (canvasManager != null && canvasManager.isMainstageActive) return;

        OperationModeManager.OperationMode currentMode = OperationModeManager.Instance.GetCurrentMode();

        if (currentMode == OperationModeManager.OperationMode.AxisDragAndDropXY)
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.AxisDragAndDropXZ);
        }
        else
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.AxisDragAndDropXY);
        }
    }

    void Update()
    {
        if (canvasManager != null && canvasManager.isMainstageActive) return;

        if (!OperationModeManager.Instance.CanMove()) return;

        if (SelectObject())
        {
            screenPoint = Camera.main.WorldToScreenPoint(selectedObject.transform.position);
            initialPosition = selectedObject.transform.position;
            initialY = selectedObject.transform.position.y;
        }

        if (selectedObject != null && Input.GetMouseButton(0))
        {
            // UIの上でクリックされていないか確認
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return; // UI要素上でクリックされた場合は処理をスキップ
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (OperationModeManager.Instance.CanMove())
            {
                if (OperationModeManager.Instance.IsXYMode())
                {
                    // XY平面での移動（Y座標のみ操作可能）
                    Plane xyPlane = new Plane(Camera.main.transform.forward, selectedObject.transform.position);
                    if (xyPlane.Raycast(ray, out float distance))
                    {
                        Vector3 hitPoint = ray.GetPoint(distance);
                        Vector3 newPosition = new Vector3(
                            selectedObject.transform.position.x,
                            hitPoint.y,
                            selectedObject.transform.position.z
                        );
                        // Y座標の変更が一定以下の場合のみ適用
                        float yDifference = Mathf.Abs(newPosition.y - selectedObject.transform.position.y);
                        float yThreshold = 1f;
                        if (yDifference < yThreshold)
                        {
                            if (!IsColliding(newPosition))
                            {
                                selectedObject.transform.position = newPosition;
                            }
                            else
                            {
                                Debug.Log($"衝突のため移動をキャンセル。Y座標: {newPosition.y:F3}, 差分: {yDifference:F3}");
                            }
                        }
                        else
                        {
                            Debug.Log($"Y座標の変更が大きすぎるため移動をキャンセル。差分: {yDifference:F3}, 閾値: {yThreshold}");
                        }
                    }
                }
                else
                {
                    // XZ平面での移動（Y座標固定）
                    if (xzPlane.Raycast(ray, out float distance))
                    {
                        Vector3 hitPoint = ray.GetPoint(distance);
                        Vector3 newPosition = new Vector3(hitPoint.x, selectedObject.transform.position.y, hitPoint.z);
                        if (!IsColliding(newPosition))
                        {
                            selectedObject.transform.position = newPosition;
                        }
                    }
                }
            }
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
        // Mainstageがアクティブな場合は衝突していないとみなす
        if (canvasManager != null && canvasManager.isMainstageActive) return false;
        if (selectedObject == null) return false;

        Bounds objectBounds = GetObjectBounds(selectedObject, newPosition);

        // 部屋の境界との衝突チェック
        if (wallRight != null && objectBounds.max.x > wallRight.transform.position.x) return true;
        if (wallLeft != null && objectBounds.min.x < wallLeft.transform.position.x) return true;
        if (wallBack != null && objectBounds.max.z > wallBack.transform.position.z) return true;
        if (wallFront != null && objectBounds.min.z < wallFront.transform.position.z) return true; // 前壁との衝突チェックを追加
        if (ceiling != null && objectBounds.max.y > ceiling.transform.position.y) return true;

        // 床との衝突チェック（オブジェクトを床の上に配置）
        if (floor != null)
        {
            float floorY = floor.transform.position.y;
            if (objectBounds.min.y < floorY)
            {
                float adjustment = floorY - objectBounds.min.y;
                selectedObject.transform.position = new Vector3(newPosition.x, newPosition.y + adjustment, newPosition.z);
                return false; // 床に接地させたので、衝突としては扱わない
            }
        }

        // 他のオブジェクトとの衝突チェック
        Collider[] hitColliders = Physics.OverlapBox(objectBounds.center, objectBounds.extents, selectedObject.transform.rotation);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != selectedObject && hitCollider.gameObject != floor)
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

        // オブジェクトの現在位置と新しい位置の差分を計算
        Vector3 offset = position - obj.transform.position;
        bounds.center += offset;

        return bounds;
    }
}
