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
        AxisDragAndDrop
    }

    private OperationMode currentMode = OperationMode.None;

    public void SetMode(OperationMode mode)
    {
        if (currentMode != mode)
        {
            currentMode = mode;
            Debug.Log("現在のモード: " + currentMode);
        }
    }

    public void SetModeToNone()
    {
        SetMode(OperationMode.None);
    }

    public bool IsCurrentMode(OperationMode mode)
    {
        return currentMode == mode;
    }

    public OperationMode GetCurrentMode()
    {
        return currentMode;
    }
}