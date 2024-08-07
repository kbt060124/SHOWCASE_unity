using UnityEngine;
using UnityEngine.UI; // UIを使用するために追加

public class ObjectDeleter : ObjectSelector
{
    private void Update()
    {
        SelectObject(); // 継続的にオブジェクトの選択を行う
    }

    public void DeleteSelectedObject()
    {
        if (selectedObject != null)
        {
            Debug.Log("オブジェクトを削除: " + selectedObject.name);
            Destroy(selectedObject);
            selectedObject = null;
        }
        else
        {
            Debug.Log("削除するオブジェクトが選択されていません");
        }
    }
}