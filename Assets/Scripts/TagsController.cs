using UnityEngine;

public class TagsController : MonoBehaviour
{
    public static TagsController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RemoveTagsFromGroup(string groupName, string tagToRemove)
    {
        GameObject objectsParent = GameObject.Find("Objects");
        if (objectsParent != null)
        {
            Transform group = objectsParent.transform.Find(groupName);
            if (group != null)
            {
                RemoveTagsFromChildren(group, tagToRemove);
            }
            else
            {
                Debug.LogWarning($"{groupName}が見つかりません。");
            }
        }
        else
        {
            Debug.LogError("Objects親オブジェクトが見つかりません。");
        }
    }

    public void AddTagsToGroup(string groupName, string tagToAdd)
    {
        GameObject objectsParent = GameObject.Find("Objects");
        if (objectsParent != null)
        {
            Transform group = objectsParent.transform.Find(groupName);
            if (group != null)
            {
                AddTagsToChildren(group, tagToAdd);
            }
            else
            {
                Debug.LogWarning($"{groupName}が見つかりません。");
            }
        }
        else
        {
            Debug.LogError("Objects親オブジェクトが見つかりません。");
        }
    }

    private void RemoveTagsFromChildren(Transform parent, string tagToRemove)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tagToRemove))
            {
                child.tag = "Untagged";
            }
        }
    }

    private void AddTagsToChildren(Transform parent, string tagToAdd)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Untagged"))
            {
                child.tag = tagToAdd;
            }
        }
    }
}
