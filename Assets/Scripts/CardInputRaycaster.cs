using System;
using UnityEngine;

public class CardInputRaycaster : MonoBehaviour
{
    public static CardInputRaycaster Instance
    { get; private set; }

    public bool canDetectInput;
    public event Action<Card> OnClickCard;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (canDetectInput is false) return;

        if (Input.GetMouseButtonDown(0))
        {
            var _camera = Camera.main;
            var _cameraRay = _camera.ScreenPointToRay(Input.mousePosition);
            var _isHitting = Physics.Raycast(_cameraRay, out RaycastHit _hit, Mathf.Infinity);
            if (_isHitting)
            {
                try
                {
                    OnClickCard?.Invoke(_hit.collider.GetComponent<Card>());
                }
                catch (Exception e)
                { Debug.LogError(e); }
            }
        }

        if (Input.GetMouseButtonDown(1))
        { }

    }
}
