using UnityEngine;
using System.Collections.Generic; // この行を追加

public class ObjectSelector : MonoBehaviour
{
    protected GameObject selectedObject;
    protected Vector3 initialScale;
    protected Quaternion initialRotation;

    private void SetKinematicState(GameObject obj, bool isKinematic)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = isKinematic;
        }
    }

    private void UpdateKinematicStates()
    {
        // 対象とするタグのリスト
        string[] targetTags = { "SceneObject", "Item", "Shelf" };

        List<GameObject> allObjects = new List<GameObject>();

        // 各タグに対してオブジェクトを取得し、リストに追加
        foreach (string tag in targetTags)
        {
            allObjects.AddRange(GameObject.FindGameObjectsWithTag(tag));
        }

        foreach (GameObject obj in allObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (obj == selectedObject)
                {
                    rb.isKinematic = false;
                    rb.detectCollisions = true;
                }
                else
                {
                    rb.isKinematic = true;
                    rb.detectCollisions = true;
                }
            }
        }
    }

    protected virtual bool SelectObject()
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
                    UpdateKinematicStates();
                    return true;
                }
            }
        }
        return false;
    }
}
