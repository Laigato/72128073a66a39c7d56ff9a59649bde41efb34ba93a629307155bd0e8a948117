using UnityEngine;

[CreateAssetMenu(fileName = nameof(SaveConfiguration), menuName = "Matcher/" + nameof(SaveConfiguration))]
public class SaveConfiguration : ScriptableObject
{
    public string nameFile;
    public string nameClass;
}