using UnityEngine;

[CreateAssetMenu(fileName = nameof(CardData), menuName = "Matcher/" + nameof(CardData))]
public class CardData : ScriptableObject
{
    public string itemName;
    public string itemTextureName;

}
