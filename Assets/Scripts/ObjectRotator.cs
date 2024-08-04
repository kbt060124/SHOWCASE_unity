using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    private bool isRotationModeActive = false;
    private GameObject selectedObject;
    private Vector3 lastMousePosition;

    public void ToggleRotationMode()
    {
        isRotationModeActive = !isRotationModeActive;
        Debug.Log("回転モード: " + (isRotationModeActive ? "オン" : "オフ"));
    }

    void Update()
    {
        if (!isRotationModeActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                selectedObject = hit.transform.gameObject;
                lastMousePosition = Input.mousePosition;
                Debug.Log("選択されたオブジェクト: " + selectedObject.name);
            }
        }

        if (selectedObject != null && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            selectedObject.transform.Rotate(Vector3.up, -delta.x * 0.5f, Space.World);
            selectedObject.transform.Rotate(Vector3.right, delta.y * 0.5f, Space.World);
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedObject = null;
        }
    }
}