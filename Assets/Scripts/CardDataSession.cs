using UnityEngine;

public class CardDataSession
{
    internal Material Material
    { get; private set; }

    public CardData CardData
    { get; private set; }

    public bool IsFaceUp 
    { get; set; }

    internal CardDataSession(Material _materialShader, CardData _cardData)
    {
        Material = _materialShader;
        CardData = _cardData;
    }
}