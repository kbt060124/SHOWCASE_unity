using UnityEngine;
using System.Collections.Generic; // この行を追加
using UnityFx.Outline;

public class ObjectSelector : MonoBehaviour
{
    protected GameObject selectedObject;
    protected Vector3 initialScale;
    protected Quaternion initialRotation;

    [SerializeField]
    private OutlineResources outlineResources; // インスペクターでアサインする

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
                    rb.isKinematic = true;
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
                GameObject hitObject = hit.transform.gameObject;
                if (hitObject.CompareTag("Shelf") || hitObject.CompareTag("Item"))
                {
                    if (selectedObject != hitObject)
                    {
                        // 以前に選択されたオブジェクトのアウトラインを削除
                        if (selectedObject != null)
                        {
                            RemoveOutline(selectedObject);
                        }

                        selectedObject = hitObject;
                        initialScale = selectedObject.transform.localScale;
                        initialRotation = selectedObject.transform.rotation;
                        Debug.Log("選択されたオブジェクト: " + selectedObject.name);

                        // 新しく選択されたオブジェクトにアウトラインを追加
                        AddOutline(selectedObject);

                        UpdateKinematicStates();
                        return true;
                    }
                }
                else
                {
                    // "Shelf"か"Item"タグが付いていないオブジェクトにrayがあたった場合
                    if (selectedObject != null)
                    {
                        RemoveOutline(selectedObject);
                    }
                    selectedObject = null;
                    Debug.Log("選択が解除されました");
                    UpdateKinematicStates();
                    return true;
                }
            }
            else
            {
                // rayがどのオブジェクトにもあたらなかった場合
                if (selectedObject != null)
                {
                    RemoveOutline(selectedObject);
                }
                selectedObject = null;
                Debug.Log("選択が解除されました");
                UpdateKinematicStates();
                return true;
            }
        }
        return false;
    }

    private void AddOutline(GameObject obj)
    {
        if (outlineResources == null)
        {
            Debug.LogError("OutlineResources is not set. Cannot add outline.");
            return;
        }

        OutlineBehaviour outlineBehaviour = obj.GetComponent<OutlineBehaviour>();
        if (outlineBehaviour == null)
        {
            outlineBehaviour = obj.AddComponent<OutlineBehaviour>();
        }

        outlineBehaviour.OutlineResources = outlineResources;
        outlineBehaviour.OutlineColor = Color.green;
        outlineBehaviour.OutlineWidth = 10;
        outlineBehaviour.OutlineIntensity = 10;
    }

    private void RemoveOutline(GameObject obj)
    {
        OutlineBehaviour outlineBehaviour = obj.GetComponent<OutlineBehaviour>();
        if (outlineBehaviour != null)
        {
            Destroy(outlineBehaviour);
        }
    }

    private void Awake()
    {
        if (outlineResources == null)
        {
            outlineResources = Resources.Load<OutlineResources>("DefaultOutlineResources");
            if (outlineResources == null)
            {
                Debug.LogError("DefaultOutlineResources not found. Please create and assign an OutlineResources asset.");
            }
        }
    }
}
