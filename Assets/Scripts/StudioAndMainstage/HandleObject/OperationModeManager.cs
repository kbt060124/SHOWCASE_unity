using UnityEngine;

public class OperationModeManager : MonoBehaviour
{
    public static OperationModeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum OperationMode
    {
        None,
        Resize,
        Rotate,
        AxisDragAndDropXY,
        AxisDragAndDropXZ
    }

    private OperationMode currentMode = OperationMode.AxisDragAndDropXZ;
    private bool canMove = true;

    public void ToggleMode(OperationMode mode)
    {
        if (currentMode == mode)
        {
            SetMode(OperationMode.AxisDragAndDropXZ);
        }
        else
        {
            SetMode(mode);
        }
    }

    public void SetMode(OperationMode mode)
    {
        currentMode = mode;
        canMove = (mode == OperationMode.AxisDragAndDropXY || mode == OperationMode.AxisDragAndDropXZ);
        Debug.Log("現在のモード: " + currentMode + ", 移動可能: " + (canMove ? "はい" : "いいえ"));

        ButtonController buttonController = FindObjectOfType<ButtonController>();
        if (buttonController != null)
        {
            buttonController.UpdateButtonState(buttonController.resizeButton, buttonController.resizeNormalSprite, buttonController.resizeSelectedSprite, mode == OperationMode.Resize);
            buttonController.UpdateButtonState(buttonController.rotateButton, buttonController.rotateNormalSprite, buttonController.rotateSelectedSprite, mode == OperationMode.Rotate);
            buttonController.UpdateButtonState(buttonController.axisButton, buttonController.axisNormalSprite, buttonController.axisSelectedSprite, mode == OperationMode.AxisDragAndDropXY);
        }
    }

    public bool IsCurrentMode(OperationMode mode)
    {
        return currentMode == mode;
    }

    public OperationMode GetCurrentMode()
    {
        return currentMode;
    }

    public bool CanMove()
    {
        return canMove;
    }

    public bool IsXYMode()
    {
        return currentMode == OperationMode.AxisDragAndDropXY;
    }

    public bool IsAnyNonMoveOperationActive()
    {
        return currentMode == OperationMode.Resize || currentMode == OperationMode.Rotate;
    }
}
