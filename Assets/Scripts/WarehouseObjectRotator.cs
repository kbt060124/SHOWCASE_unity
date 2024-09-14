using UnityEngine;

public class WarehouseObjectRotator : MonoBehaviour
{
    private Vector3 lastMousePosition;
    private GameObject targetObject;
    private float rotationSpeed = 0.5f;
    private Camera mainCamera;
    private Vector3 objectCenter;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetTargetObject(GameObject obj)
    {
        targetObject = obj;
        objectCenter = GetObjectCenter(obj);
        Debug.Log($"新しいターゲットオブジェクトが設定されました: {obj.name}");
    }

    void Update()
    {
        if (targetObject == null || mainCamera == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            
            if (delta.magnitude > 0)
            {
                Vector3 axis = Vector3.Cross(mainCamera.transform.forward, delta.normalized);
                targetObject.transform.RotateAround(objectCenter, axis, -delta.magnitude * rotationSpeed);
            }
            
            lastMousePosition = Input.mousePosition;
        }
    }

    private Vector3 GetObjectCenter(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return obj.transform.position;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.center;
    }
}
