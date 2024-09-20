using UnityEngine;

public class ObjectRotator : ObjectSelector
{
    private Vector3 lastMousePosition;

    public void ToggleRotationMode()
    {
        OperationModeManager.Instance.ToggleMode(OperationModeManager.OperationMode.Rotate);
        Debug.Log("回転モード: " + (OperationModeManager.Instance.IsCurrentMode(OperationModeManager.OperationMode.Rotate) ? "オン" : "オフ"));
    }

    void Update()
    {
        if (!OperationModeManager.Instance.IsCurrentMode(OperationModeManager.OperationMode.Rotate)) return;

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
