using UnityEngine;

public class ObjectResizer : MonoBehaviour
{
    private bool isResizeModeActive = false;
    private GameObject selectedObject;
    private Vector3 initialScale;
    private float currentScaleFactor = 1f;

    public void ToggleResizeMode()
    {
        isResizeModeActive = !isResizeModeActive;
        Debug.Log("リサイズモード: " + (isResizeModeActive ? "オン" : "オフ"));
    }

    void Update()
    {
        if (!isResizeModeActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (selectedObject != hit.transform.gameObject)
                {
                    selectedObject = hit.transform.gameObject;
                    initialScale = selectedObject.transform.localScale;
                    currentScaleFactor = 1f;
                    Debug.Log("選択されたオブジェクト: " + selectedObject.name);
                }
            }
        }

        if (selectedObject != null)
        {
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
}
