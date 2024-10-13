using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoomMaterialPresetChanger : MonoBehaviour
{
    [System.Serializable]
    public class MaterialPreset
    {
        public Material wallRightMaterial;
        public Material wallLeftMaterial;
        public Material wallBackMaterial;
        public Material wallFrontMaterial;
        public Material ceilingMaterial;
        public Material floorMaterial;
    }

    public GameObject room;
    public Button changePresetButton;
    public List<MaterialPreset> presets;

    private int currentPresetIndex = 0;

    void Start()
    {
        changePresetButton.onClick.AddListener(ChangeToNextPreset);
        if (presets.Count > 0)
        {
            ApplyPreset(presets[0]);
        }
    }

    void ChangeToNextPreset()
    {
        currentPresetIndex = (currentPresetIndex + 1) % presets.Count;
        ApplyPreset(presets[currentPresetIndex]);
    }

    void ApplyPreset(MaterialPreset preset)
    {
        ChangeMaterial("WallRight", preset.wallRightMaterial);
        ChangeMaterial("WallLeft", preset.wallLeftMaterial);
        ChangeMaterial("WallBack", preset.wallBackMaterial);
        ChangeMaterial("WallFront", preset.wallFrontMaterial);
        ChangeMaterial("Ceiling", preset.ceilingMaterial);
        ChangeMaterial("Floor", preset.floorMaterial);
    }

    void ChangeMaterial(string objectName, Material newMaterial)
    {
        Transform child = room.transform.Find(objectName);
        if (child != null)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = newMaterial;
            }
            else
            {
                Debug.LogWarning($"{objectName}にRendererコンポーネントが見つかりません。");
            }
        }
        else
        {
            Debug.LogWarning($"{objectName}が見つかりません。");
        }
    }
}
