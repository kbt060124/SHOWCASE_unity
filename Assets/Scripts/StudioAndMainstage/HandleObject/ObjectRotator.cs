using UnityEngine;

public class ObjectRotator : ObjectSelector
{
    private Vector3 lastMousePosition;
    private ButtonController buttonController;

    private void Start()
    {
        buttonController = FindObjectOfType<ButtonController>();
    }

    public void ToggleRotationMode()
    {
        OperationModeManager.Instance.ToggleMode(OperationModeManager.OperationMode.Rotate);
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        bool isRotateMode = OperationModeManager.Instance.IsCurrentMode(OperationModeManager.OperationMode.Rotate);
        buttonController.UpdateButtonState(buttonController.rotateButton, buttonController.rotateNormalSprite, buttonController.rotateSelectedSprite, isRotateMode);
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
