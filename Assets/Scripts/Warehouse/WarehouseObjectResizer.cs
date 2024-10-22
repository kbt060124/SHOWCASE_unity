using UnityEngine;

public class WarehouseObjectResizer : MonoBehaviour
{
    private GameObject targetObject;
    private Vector3 initialScale;
    private Vector3 initialPosition;
    private float currentScaleFactor = 1f;
    private Camera mainCamera;
    private float initialDistance;

    [SerializeField] private float sensitivity = 0.001f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetTargetObject(GameObject obj)
    {
        targetObject = obj;
        initialScale = obj.transform.localScale;
        initialPosition = obj.transform.position;
        currentScaleFactor = 1f;
        Debug.Log($"新しいターゲットオブジェクトが設定されました: {obj.name}");
    }

    void Update()
    {
        if (targetObject == null || mainCamera == null) return;

        float scaleDelta = 0f;
        bool isResizing = false;

        #if UNITY_EDITOR || UNITY_STANDALONE
        // PC用のマウスホイールによるリサイズ
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            scaleDelta = scrollDelta * sensitivity;
            isResizing = true;
        }
        #else
        // モバイル用のピンチリサイズ
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
            {
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                scaleDelta = (currentMagnitude - prevMagnitude) * sensitivity;
                isResizing = true;
            }
        }
        #endif

        if (scaleDelta != 0)
        {
            ResizeObject(scaleDelta);
        }

        // WarehouseObjectRotatorのIsResizingプロパティを更新
        WarehouseObjectRotator rotator = GetComponent<WarehouseObjectRotator>();
        if (rotator != null)
        {
            rotator.IsResizing = isResizing;
        }
    }

    private void ResizeObject(float scaleDelta)
    {
        float newScaleFactor = currentScaleFactor * (1 + scaleDelta);
        newScaleFactor = Mathf.Clamp(newScaleFactor, 0.1f, 10f);

        // スケール変更を徐々に適用
        currentScaleFactor = Mathf.Lerp(currentScaleFactor, newScaleFactor, 0.1f);

        Vector3 objectCenter = targetObject.transform.position;
        Vector3 newScale = initialScale * currentScaleFactor;

        // スケールを適用
        targetObject.transform.localScale = newScale;

        // オブジェクトの中心位置を維持
        targetObject.transform.position = objectCenter;

        Debug.Log($"オブジェクトのスケールを変更: {newScale}, 位置: {objectCenter}");
    }
}
