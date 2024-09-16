using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void ChangeToSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void ChangeToWarehouse()
    {
        SceneManager.LoadScene("Warehouse");
    }
}