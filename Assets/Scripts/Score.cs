using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public static Score Instance
    { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _score;

    public int CurrentScore
    { get; private set; }

    private void Awake()
    {
        Instance = this; 
    }

    public void Add(int _toAmount)
    {
        CurrentScore += _toAmount;
        _score.text = CurrentScore.ToString();
    }

    public void ResetScore()
    {
        CurrentScore = default;
        _score.text = CurrentScore.ToString();
    }
}