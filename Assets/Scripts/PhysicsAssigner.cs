using Autodesk.Fbx;
using UnityEngine;
using UnityEditor;
using System.IO;

public class PhysicsAssigner : MonoBehaviour
{
    public void AddPhysicsToChildren()
    {
        // "Objects"という名前のGameObjectを探す
        GameObject parentObject = GameObject.Find("Objects");
        if (parentObject == null)
        {
            Debug.LogError("\"Objects\"という名前のGameObjectが見つかりませんでした。");
            return;
        }

        // 子要素を取得し、物理判定を付与する
        foreach (Transform child in parentObject.transform)
        {
            if (child.gameObject.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false; // 重力を無効にして自由落下を防ぐ
                rb.mass = 10000f; // 質量を大きくして反発を減らす
                rb.drag = 100f; // 空気抵抗を増やして動きを抑える
                rb.angularDrag = 100f; // 回転の抵抗を増やして回転を抑える
                Debug.Log(child.gameObject.name + "にRigidbodyを追加し、useGravityをfalseに設定しました。");
            }
            else
            {
                Debug.Log(child.gameObject.name + "には既にRigidbodyが追加されています。");
            }

            if (child.gameObject.GetComponent<Collider>() == null)
            {
                child.gameObject.AddComponent<BoxCollider>(); // 必要に応じて他のColliderに変更可能
                Debug.Log(child.gameObject.name + "にBoxColliderを追加しました。");
            }
            else
            {
                Debug.Log(child.gameObject.name + "には既にColliderが追加されています。");
            }
        }
    }
}