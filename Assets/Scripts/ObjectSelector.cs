using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    protected GameObject selectedObject;
    protected Vector3 initialScale;
    protected Quaternion initialRotation;

    protected bool SelectObject()
    {
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
                    initialRotation = selectedObject.transform.rotation;
                    Debug.Log("選択されたオブジェクト: " + selectedObject.name);
                    return true;
                }
            }
        }
        return false;
    }
}
