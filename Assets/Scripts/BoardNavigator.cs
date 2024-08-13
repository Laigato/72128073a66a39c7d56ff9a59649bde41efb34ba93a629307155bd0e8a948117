using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardNavigator : MonoBehaviour
{
    public static BoardNavigator Instance
    { get; private set; }

    [SerializeField] private List<BoardConfiguration> _boardConfigurations = new();
    [SerializeField] private TextMeshProUGUI _currentBoardDimension;

    public int CurrentBoardIndex
    { get; private set; }
    
    public BoardConfiguration CurrentBoard
    { 
        get { return _boardConfigurations[CurrentBoardIndex]; }
    }

    public event Action<BoardConfiguration> OnClickBoardEvent;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetNavigation()
    {
        CurrentBoardIndex = default;
        Refresh();
    }

    public void Refresh()
    {
        _currentBoardDimension.text = $"{CurrentBoard.Width}x{CurrentBoard.Height}";
    }

    public void Previous()
    {
        CurrentBoardIndex = Mathf.Max(CurrentBoardIndex - 1, default);
        Refresh();
    }

    public void Next()
    {
        CurrentBoardIndex = Mathf.Min(CurrentBoardIndex + 1, _boardConfigurations.Count - 1);
        Refresh();
    }

    public void Click()
    {
        try
        {
            OnClickBoardEvent?.Invoke(CurrentBoard);
        }
        catch (Exception _err)
        { Debug.LogError(_err); }
    }
}
