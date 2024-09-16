using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class ItemPanel : MonoBehaviour
{
    public Image thumbnailImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI memoText;

    public void UpdateThumbnail(Texture2D thumbnail)
    {
        if (thumbnailImage != null && thumbnail != null)
        {
            Sprite sprite = Sprite.Create(thumbnail, new Rect(0, 0, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f));
            thumbnailImage.sprite = sprite;
        }
    }

    public void UpdateInfo(ObjectInfo info)
    {
        if (info != null)
        {
            if (nameText != null) nameText.text = info.name;
            if (categoryText != null) categoryText.text = info.category;
            if (memoText != null) memoText.text = info.memo;
        }
        else
        {
            if (nameText != null) nameText.text = "";
            if (categoryText != null) categoryText.text = "";
            if (memoText != null) memoText.text = "";
        }
    }

    private Texture2D LoadTexture(string filePath)
    {
        if (File.Exists(filePath))
        {
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(File.ReadAllBytes(filePath)))
            {
                return texture;
            }
        }
        return null;
    }
}
