using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using NativeFilePickerNamespace;

public class FileUpload : MonoBehaviour
{
    public Button pickFileButton;
    public Text fileNameText;
    private string selectedFilePath;

    void Start()
    {
        pickFileButton.onClick.AddListener(OnPickFileButtonClicked);
    }

    public void OnPickFileButtonClicked()
    {
        // string[] allowedFileTypes = new string[] { "public.fbx", "public.item", "public.png" };
        string[] allowedFileTypes = new string[] { "public.fbx" };
        NativeFilePicker.PickFile(OnFilePicked, allowedFileTypes);
    }

    void OnFilePicked(string path)
    {
        Debug.Log("OnFilePickedが呼び出されました。");
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("ファイルが選択されませんでした。");
            if (fileNameText != null)
            {
                fileNameText.text = "ファイルが選択されませんでした。";
            }
        }
        else
        {
            string fileName = System.IO.Path.GetFileName(path);
            selectedFilePath = path; // 選択したファイルのパスを保存
            Debug.Log("選択されたファイル: " + fileName);
            if (fileNameText != null)
            {
                fileNameText.text = fileName;
                StartCoroutine(UploadFileToLaravelAPI(selectedFilePath, fileName));
            }
        }
    }

    private IEnumerator UploadFileToLaravelAPI(string filePath, string fileName)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, fileName, "model/vnd.fbx");
        Debug.Log("Form contains file: " + form.data.Length + " bytes");

        using (UnityWebRequest www = UnityWebRequest.Post("http://13.114.102.118/api/store", form))
        {
            Debug.Log("Sending request to: " + www.url);
            Debug.Log("File name: " + fileName);
            Debug.Log("File size: " + fileData.Length + " bytes");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                Debug.LogError("Response: " + www.downloadHandler.text);
                Debug.Log("Form contains file: " + form.data.Length + " bytes");
            }
            else
            {
                Debug.Log("File upload complete!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
}
