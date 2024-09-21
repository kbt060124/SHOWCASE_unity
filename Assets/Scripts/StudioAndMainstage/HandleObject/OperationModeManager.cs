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

    public delegate void ModeChangedHandler(OperationMode oldMode, OperationMode newMode);
    public event ModeChangedHandler OnModeChanged;

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
        OperationMode oldMode = currentMode;
        currentMode = mode;
        canMove = (mode == OperationMode.AxisDragAndDropXY || mode == OperationMode.AxisDragAndDropXZ || mode == OperationMode.None);
        Debug.Log($"モード変更: {oldMode} -> {currentMode}" + ", 移動可能: " + (canMove ? "はい" : "いいえ"));

        OnModeChanged?.Invoke(oldMode, currentMode);
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
