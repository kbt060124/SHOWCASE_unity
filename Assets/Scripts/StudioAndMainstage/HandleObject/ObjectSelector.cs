using UnityEngine;
using System.Collections.Generic;
using UnityFx.Outline;
using UnityEngine.EventSystems;

public class ObjectSelector : MonoBehaviour
{
    protected GameObject selectedObject;
    protected Vector3 initialScale;
    protected Quaternion initialRotation;

    [SerializeField]
    private OutlineResources outlineResources; // インスペクターでアサインする

    [SerializeField]
    protected ButtonController buttonController;

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
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0))
        {
            // UIの要素がクリックされた場合は、オブジェクト選択を行わない
            if (IsPointerOverUIObject())
            {
                return false;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector3)Input.mousePosition);
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
                        // ButtonControllerのObjectSelected関数を呼び出す
                        if (buttonController != null)
                        {
                            buttonController.ObjectSelected();
                        }
                        else
                        {
                            Debug.LogWarning("ButtonController is not assigned in ObjectSelector.");
                        }

                        return true;
                    }
                }
                else
                {
                    DeselectObject();
                    return true;
                }
            }
            else
            {
                DeselectObject();
                return true;
            }
        }
        return false;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<FixedJoystick>() != null)
            {
                DeselectObject();
                return true;
            }
        }

        return Input.touchCount > 0 ?
            EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) :
            EventSystem.current.IsPointerOverGameObject();
    }

    private void DeselectObject()
    {
        if (selectedObject != null)
        {
            RemoveOutline(selectedObject);
        }
        selectedObject = null;
        Debug.Log("選択が解除されました");
        UpdateKinematicStates();

        // EditButtonが押されたかどうかをチェック
        if (buttonController != null && !IsEditButtonPressed())
        {
            buttonController.ObjectDeselected();
        }
        else
        {
            Debug.LogWarning("ButtonController is not assigned in ObjectSelector or EditButton was pressed.");
        }
    }

    // EditButtonが押されたかどうかをチェックする新しいメソッド
    private bool IsEditButtonPressed()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return EventSystem.current.currentSelectedGameObject.CompareTag("EditButton");
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

    private void Start()
    {
        if (buttonController == null)
        {
            buttonController = FindObjectOfType<ButtonController>();
            if (buttonController == null)
            {
                Debug.LogError("ButtonController not found in the scene. Please assign it manually.");
            }
        }

        // 初期状態ではオブジェクトが選択されていないので、ObjectDeselectedを呼び出す
        if (buttonController != null)
        {
            buttonController.ObjectDeselected();
        }
    }
}
