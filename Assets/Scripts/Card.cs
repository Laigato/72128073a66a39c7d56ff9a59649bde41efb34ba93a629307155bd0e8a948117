using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;

    private CardDataSession _session;

    internal void Initialize(CardDataSession _toCardDataSession)
    {
        _session = _toCardDataSession;
        _renderer.material = _session.Material;
    }

    public void Check()
    {
    }
}