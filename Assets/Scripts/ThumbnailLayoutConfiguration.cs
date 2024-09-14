using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ThumbnailLayoutConfig", menuName = "ScriptableObjects/ThumbnailLayoutConfiguration", order = 1)]
public class ThumbnailLayoutConfiguration : ScriptableObject
{
    public Vector2 cellSize = new Vector2(80, 80);
    public Vector2 spacing = new Vector2(40, 10);
    public GridLayoutGroup.Corner startCorner = GridLayoutGroup.Corner.UpperLeft;
    public GridLayoutGroup.Axis startAxis = GridLayoutGroup.Axis.Horizontal;
    public TextAnchor childAlignment = TextAnchor.MiddleCenter;
    public GridLayoutGroup.Constraint constraint = GridLayoutGroup.Constraint.Flexible;
}
