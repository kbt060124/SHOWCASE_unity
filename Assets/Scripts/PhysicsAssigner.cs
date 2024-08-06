using UnityEngine;

public class PhysicsAssigner : MonoBehaviour
{
    public void AddPhysicsToChildren()
    {
        GameObject objectsContainer = GameObject.Find("Objects");
        if (objectsContainer == null)
        {
            Debug.LogError("'Objects'コンテナが見つかりません。");
            return;
        }

        Debug.Log($"'Objects'コンテナの子オブジェクト数: {objectsContainer.transform.childCount}");

        int addedPhysicsCount = 0;
        foreach (Transform child in objectsContainer.transform)
        {
            if (AddPhysicsToObject(child.gameObject))
            {
                addedPhysicsCount++;
            }
        }

        Debug.Log($"物理判定が追加されたオブジェクト数: {addedPhysicsCount}");
    }

    private bool AddPhysicsToObject(GameObject obj)
    {
        bool physicsAdded = false;

        if (obj.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 10000f;
            rb.drag = 100f;
            rb.angularDrag = 100f;
            Debug.Log($"{obj.name}にRigidbodyを追加しました。");
            physicsAdded = true;
        }

        if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>();
            Debug.Log($"{obj.name}にBoxColliderを追加しました。");
            physicsAdded = true;
        }

        return physicsAdded;
    }
}
