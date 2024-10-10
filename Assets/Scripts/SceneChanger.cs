using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // 現在のシーンのリソースをアンロード
        yield return Resources.UnloadUnusedAssets();

        // ガベージコレクションを強制的に実行
        System.GC.Collect();

        // シーンの非同期読み込みを開始
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // シーンの読み込みが完了するまで待機
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 新しいシーンの読み込み後、再度未使用アセットをアンロード
        yield return Resources.UnloadUnusedAssets();

        // ガベージコレクションを再度実行
        System.GC.Collect();
    }

    public void ChangeToSampleScene()
    {
        StartCoroutine(LoadSceneCoroutine("SampleScene"));
    }

    public void ChangeToWarehouse()
    {
        StartCoroutine(LoadSceneCoroutine("Warehouse"));
    }
}