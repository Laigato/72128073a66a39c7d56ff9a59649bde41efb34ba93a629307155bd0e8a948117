using UnityEngine;

[CreateAssetMenu(fileName = nameof(BoardConfiguration), menuName = "Matcher/" + nameof(BoardConfiguration))]
public class BoardConfiguration : ScriptableObject
{
    [SerializeField, Min(1)] private int _width;
    [SerializeField, Min(1)] private int _height;

    public int Width
    {
        get { return _width; }
    }

    public int Height
    {
        get { return _height; }
    }

    public const string PREF_KEY = "BoardKey";
}
