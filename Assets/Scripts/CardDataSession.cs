using UnityEngine;

public class CardDataSession
{
    internal Material Material
    { get; private set; }

    internal CardDataSession(Material _materialShader)
    {
        Material = _materialShader;
    }
}