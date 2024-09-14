using UnityEngine;

public class ObjectRotator : ObjectSelector
{
    private bool isRotationModeActive = false;
    private Vector3 lastMousePosition;

    // 回転モードを有効/無効にするメソッド
    public void ToggleRotationMode()
    {
        isRotationModeActive = !isRotationModeActive;
        if (isRotationModeActive)
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.Rotate);
        }
        else
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.None);
        }
        Debug.Log("回転モード: " + (isRotationModeActive ? "オン" : "オフ"));
    }

    void Update()
    {
        if (!isRotationModeActive || !OperationModeManager.Instance.IsCurrentMode(OperationModeManager.OperationMode.Rotate)) return;

        if (SelectObject())
        {
            lastMousePosition = Input.mousePosition;
        }

        if (selectedObject != null && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            
            // オブジェクトの中心を軸にY軸回転を適用
            selectedObject.transform.RotateAround(
                selectedObject.transform.position,
                Vector3.up,
                -delta.x * 0.5f
            );
            
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            selectedObject = null;
        }
    }
}
