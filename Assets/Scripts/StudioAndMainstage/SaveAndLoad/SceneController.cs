using UnityEngine;

public class SceneController : MonoBehaviour
{
    public SaveManager saveManager;

    private void Awake()
    {
        // シーンが変わっても破壊されないようにする
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // アプリケーション起動時に自動的にシーンを読み込む
        saveManager.LoadScene();
    }

    // アプリケーション終了時に自動的にシーンを保存する（オプション）
    private void OnApplicationQuit()
    {
        saveManager.SaveScene();
    }
}
