using UnityEngine;

public class TagsController : MonoBehaviour
{
    public GameObject objectsParent;

    private void Start()
    {
        // オブジェクトの参照を取得
        objectsParent = GameObject.Find("Objects");
    }

    public void RemoveTags()
    {
        RemoveTagsFromChildren(objectsParent.transform.Find("Items"), "Item");
        RemoveTagsFromChildren(objectsParent.transform.Find("Shelves"), "Shelf");
    }

    public void AddTags()
    {
        AddTagsToChildren(objectsParent.transform.Find("Items"), "Item");
        AddTagsToChildren(objectsParent.transform.Find("Shelves"), "Shelf");
    }

    private void RemoveTagsFromChildren(Transform parent, string tag)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                child.tag = "Untagged";
            }
        }
    }

    private void AddTagsToChildren(Transform parent, string tag)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            child.tag = tag;
        }
    }
}
