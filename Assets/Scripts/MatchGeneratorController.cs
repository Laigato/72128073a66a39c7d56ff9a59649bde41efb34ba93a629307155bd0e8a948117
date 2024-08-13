using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class MatchGeneratorController : MonoBehaviour
{
    [SerializeField] private SaveConfiguration _saveConfigurationForMatch;
    [SerializeField] private List<CardData> _cards = new();

    private const float HALF = 0.5f;

    private void Start()
    {
        SaveManager.Instance.DeleteSave(_saveConfigurationForMatch);
        StartCoroutine(CreatingSaveMatchCoroutine());
    }

    private IEnumerator CreatingSaveMatchCoroutine()
    {
        var _boardName = PlayerPrefs.GetString(BoardConfiguration.PREF_KEY);
        var _boardConfigurationOperation = Addressables.LoadAssetAsync<BoardConfiguration>(_boardName);

        //  Load BoardConfiguration, which determined from Main Menu
        yield return _boardConfigurationOperation;

        var _boardConfiguration = _boardConfigurationOperation.Result;
       
        //  Count how many cards in the board
        var _cardsCount = _boardConfiguration.Width * _boardConfiguration.Height;
        //  Count how many unique cards in the board (without duplicate card)
        var _cardsDataDistinct = Mathf.FloorToInt(_cardsCount * HALF);

        //  Create list of cards
        var _cardsInBoard = new List<SaveMatch.Card>();
        for (int i = 0; i < _cardsDataDistinct; i++)
        {
            var _card = _cards[i];
            var _cardSave1 = new SaveMatch.Card()
            {
                name = _card.name
            };
            var _cardSave2 = new SaveMatch.Card()
            {
                name = _card.name
            };

            _cardsInBoard.Add(_cardSave1);
            _cardsInBoard.Add(_cardSave2);
        }

        //  Shuffle and assign position for each card
        var _cardsInBoardShuffled = FisherYates(_cardsInBoard);
        for (int i = 0; i < _cardsInBoardShuffled.Count; i++) 
        {
            var x = i % _boardConfiguration.Width;
            var y = Mathf.FloorToInt(i / _boardConfiguration.Width);

            _cardsInBoardShuffled[i].x = x;
            _cardsInBoardShuffled[i].y = y;
        }

        var _saveMatch = new SaveMatch()
        {
            currentBoard = _boardName,
            cards = _cardsInBoardShuffled,
        };
        SaveManager.Instance.Save(_saveConfigurationForMatch, _saveMatch);
        SceneManager.LoadScene("MatchScene");
    }

    #region Utilities

    private static List<T> FisherYates<T>(List<T> _currentList)
    {
        int _size = _currentList.Count;
        for (int i = 0; i < _size; i++)
        {
            T _tempItem = _currentList[i];

            int _randomInt = UnityEngine.Random.Range(i, _size);

            _currentList[i] = _currentList[_randomInt];
            _currentList[_randomInt] = _tempItem;

        }

        return new List<T>(_currentList);
    }

    #endregion
}
