using UnityEngine;

public class Card : MonoBehaviour
{
    private CardDataSession _session;

    internal void Initialize(CardDataSession _toCardDataSession)
    {
        _session = _toCardDataSession;
    }

    public void Check()
    {
    }
}