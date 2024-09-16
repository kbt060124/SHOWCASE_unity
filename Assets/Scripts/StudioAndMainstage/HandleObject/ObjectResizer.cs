using UnityEngine;

public class ObjectResizer : ObjectSelector
{
    private bool isResizeModeActive = false;
    private float currentScaleFactor = 1f;
    private float initialDistance;

    public void ToggleResizeMode()
    {
        isResizeModeActive = !isResizeModeActive;
        if (isResizeModeActive)
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.Resize);
        }
        else
        {
            OperationModeManager.Instance.SetMode(OperationModeManager.OperationMode.None);
        }
        Debug.Log("リサイズモード: " + (isResizeModeActive ? "オン" : "オフ"));
    }

    void Update()
    {
        if (!isResizeModeActive || !OperationModeManager.Instance.IsCurrentMode(OperationModeManager.OperationMode.Resize)) return;

        if (SelectObject())
        {
            currentScaleFactor = 1f;
        }

        #if UNITY_EDITOR
        // エディター用のマウスホイールによるリサイズ
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            currentScaleFactor *= (1 + scrollDelta * 0.1f);
            currentScaleFactor = Mathf.Clamp(currentScaleFactor, 0.1f, 10f);
            selectedObject.transform.localScale = initialScale * currentScaleFactor;
        }
        #else
        // モバイル用のピンチリサイズ
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
            }
            else if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
                float scaleFactor = currentDistance / initialDistance;

                currentScaleFactor *= scaleFactor;
                currentScaleFactor = Mathf.Clamp(currentScaleFactor, 0.1f, 10f);
                selectedObject.transform.localScale = initialScale * currentScaleFactor;

                initialDistance = currentDistance;
            }
        }
        #endif
    }
}
