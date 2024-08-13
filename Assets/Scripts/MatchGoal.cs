using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchGoal : MonoBehaviour
{
    public static MatchGoal Instance
    { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _matches;

    public int CurrentMatches
    { get; private set; }

    public int CurrentMatchesTarget
    {  get; private set; }

    public event Action OnGoalEvent;

    private void Awake()
    {
        Instance = this;
    }

    public void SetGoal(int _targetAmount)
    {
        CurrentMatchesTarget = _targetAmount;
    }

    public void Add()
    {
        CurrentMatches++;
        _matches.text = CurrentMatches.ToString();

        if (CurrentMatches >= CurrentMatchesTarget)
        {
            try
            {
                OnGoalEvent?.Invoke();
            }
            catch (Exception _err) 
            { Debug.LogException(_err); }
        }
    }

    public void ResetProgress()
    {
        CurrentMatches = default;
        _matches.text = CurrentMatches.ToString();
    }
}
