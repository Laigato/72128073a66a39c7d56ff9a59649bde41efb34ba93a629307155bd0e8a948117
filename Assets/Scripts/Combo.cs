using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Combo : MonoBehaviour
{
    public static Combo Instance
    {  get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _combo;

    [Header("Settings")]
    /// Value that determines how much a chain combo affects something. The higher the chain, the higher the chain weight value become. Could be used for scoring
    [SerializeField, Min(1)] private float _chainWeightPerLink;

    public event Action<int> OnComboUpdatedEvent;
    
    public int CurrentChain
    { get; private set; }

    public float CurrentChainWeight
    { get; private set; }   

    private void Awake()
    {
        Instance = this;
    }

    public void Chain()
    {
        CurrentChain++;
        CurrentChainWeight = Mathf.Pow(_chainWeightPerLink, 1 - CurrentChain);
        _combo.text = CurrentChain.ToString();
        try
        {
            OnComboUpdatedEvent?.Invoke(CurrentChain);
        }
        catch (Exception _err)
        { Debug.LogError(_err); }
    }

    public void ChainReset()
    {
        CurrentChain = default;
        CurrentChainWeight = Mathf.Pow(_chainWeightPerLink, 1 - CurrentChain);
        _combo.text = CurrentChain.ToString();
        try
        {
            OnComboUpdatedEvent?.Invoke(CurrentChain);
        }
        catch (Exception _err)
        { Debug.LogError(_err); }
    }

}
