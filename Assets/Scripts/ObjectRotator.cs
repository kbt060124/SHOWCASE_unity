using UnityEngine;

public class ObjectRotator : ObjectSelector
{
    private bool isRotationModeActive = false;
    private Vector3 lastMousePosition;

    // 回転モードを有効/無効にするメソッド
    public void ToggleRotationMode()
    {
        isRotationModeActive = !isRotationModeActive;
        Debug.Log("リサイズモード: " + (isRotationModeActive ? "オン" : "オフ"));
    }

    void Update()
    {
        if (!isRotationModeActive) return;

        if (SelectObject() || Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (selectedObject != null && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            
            // オブジェクトのローカル座標系で回転を適用
            selectedObject.transform.Rotate(Vector3.up, -delta.x * 0.5f, Space.Self);
            selectedObject.transform.Rotate(Vector3.right, delta.y * 0.5f, Space.Self);
            
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedObject = null;
        }
    }
}
