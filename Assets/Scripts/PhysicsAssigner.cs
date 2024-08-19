using UnityEngine;
using System.Collections.Generic; // この行を追加

public class PhysicsAssigner : MonoBehaviour
{
    public void AddPhysicsToChildren()
    {
        string[] targetTags = { "SceneObject", "Item", "Shelf" };
        List<GameObject> allObjects = new List<GameObject>();

        foreach (string tag in targetTags)
        {
            allObjects.AddRange(GameObject.FindGameObjectsWithTag(tag));
        }

        int addedPhysicsCount = 0;
        foreach (GameObject obj in allObjects)
        {
            if (AddPhysicsToObject(obj))
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
            rb.isKinematic = true;
            rb.mass = 1f;
            rb.drag = 10f;
            rb.angularDrag = 10f;
            Debug.Log($"{obj.name}にRigidbodyを追加しました。");
            physicsAdded = true;
        }

        if (obj.CompareTag("Shelf"))
        {
            AddShelfCollider(obj);
        }
        else if (obj.GetComponent<Collider>() == null)
        {
            obj.AddComponent<BoxCollider>();
            Debug.Log($"{obj.name}にBoxColliderを追加しました。");
            physicsAdded = true;
        }

        return physicsAdded;
    }

    private void AddShelfCollider(GameObject shelf)
    {
        // 既存のColliderを削除
        Collider[] existingColliders = shelf.GetComponents<Collider>();
        foreach (Collider collider in existingColliders)
        {
            DestroyImmediate(collider);
        }

        // MeshFilterコンポーネントを取得
        MeshFilter meshFilter = shelf.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            // MeshColliderを追加
            MeshCollider meshCollider = shelf.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = true;
            Debug.Log($"{shelf.name}にMeshColliderを追加しました。");
        }
        else
        {
            // MeshFilterがない場合はBoxColliderを追加
            shelf.AddComponent<BoxCollider>();
            Debug.Log($"{shelf.name}にBoxColliderを追加しました。");
        }
    }
}