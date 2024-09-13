using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public Camera mainstageCamera;
    public Canvas mainstageCanvas;
    public Camera studioCamera;
    public Canvas studioCanvas;

    // TagsControllerへの参照を追加
    public TagsController tagsController;

    public void ActiveMainstage()
    {
        mainstageCamera.gameObject.SetActive(true);
        mainstageCanvas.gameObject.SetActive(true);
        studioCamera.gameObject.SetActive(false);
        studioCanvas.gameObject.SetActive(false);
        // RemoveTags()を呼び出す
        tagsController.RemoveTags();

        // mainstageCameraの位置と回転を設定
        mainstageCamera.transform.position = new Vector3(0f, 1.65f, -1.75f);
        mainstageCamera.transform.rotation = Quaternion.identity;
    }

    public void ActiveStudio()
    {
        mainstageCamera.gameObject.SetActive(false);
        mainstageCanvas.gameObject.SetActive(false);
        studioCamera.gameObject.SetActive(true);
        studioCanvas.gameObject.SetActive(true);
        // AddTags()を呼び出す
        tagsController.AddTags();

        studioCamera.transform.position = new Vector3(0f, 1.65f, -1.75f);
        studioCamera.transform.rotation = Quaternion.identity;
    }
}