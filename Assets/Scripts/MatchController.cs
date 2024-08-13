using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MatchController : MonoBehaviour
{
    [SerializeField] private BoardConfiguration _testBoard;
    [SerializeField] private List<CardData> _cardDataList = new();

    [Header("References")]
    [SerializeField] private Transform _cardsParent;
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private Material _cardMaterialPrefab;

    [Header("Settings")]
    [Space]
    [SerializeField, Min(0)] private float spacing = 0f;
    [Space]
    [SerializeField, Min(0.1f)] private float _flipUpDuration;
    [SerializeField, Min(0.1f)] private float _flipDownDuration;
    [SerializeField, Min(0.1f)] private float _flipInterval;

    private const float HALF = 0.5f;
    private const int TO_INDEX = -1;

    private const float ROTATION_Y_FACEUP = 180f;
    private const float ROTATION_X_FACEDOWN = 0f;

    private Card _currentSelectedCard;

    private void Start()
    {
        //  Fetch Board
        var _cardsCount = _testBoard.Width * _testBoard.Height;

        //  Note CardSize = 1
        var _worldWidth = _testBoard.Width + (spacing * (_testBoard.Width - 1));
        var _worldHeight = _testBoard.Height + (spacing * (_testBoard.Height - 1));

        //  Resize Camera based on board dimension
        var _ratio = Screen.width / (float)Screen.height;
        var _orthoWidth = _worldWidth / (_ratio - 1) * HALF;
        Camera.main.orthographicSize = _orthoWidth;

        //  Generate Card
        var _cardsDataDistinct = Mathf.FloorToInt(_cardsCount * HALF);

        //  Shuffle CardData.
        //  Create Save

        //  Load Save

        //  Generate Card
        var _cards = new List<Card>();
        var _cardTextureLoadTask = new List<Task>();
        for (int i = 0; i < _cardsDataDistinct; i++)
        {
            var _cardData = _cardDataList[i];
            var _card1 = Instantiate(_cardPrefab, _cardsParent);
            var _card2 = Instantiate(_cardPrefab, _cardsParent);

            var _cardMaterial = Instantiate(_cardMaterialPrefab);
            var _cardSession1 = new CardDataSession(_cardMaterial, _cardData);
            var _cardSession2 = new CardDataSession(_cardMaterial, _cardData);

            //  Loading texture from Addressables using CardData, then assign FrontTex
            var _processLoad = Addressables.LoadAssetAsync<Texture2D>(_cardData.itemTextureName);
            _processLoad.Completed += (_operation) =>
            {
                //  HACKME: Change to BackTex instead of FrontTex
                _cardMaterial.SetTexture("_BackTex", _operation.Result);
            };
            _cardTextureLoadTask.Add(_processLoad.Task);

            _card1.Initialize(_cardSession1);
            _card2.Initialize(_cardSession2);
            _cards.Add(_card1);
            _cards.Add(_card2);
        }

        //  Shuffle Generated Cards
        var _cardsShuffled = FisherYates(_cards);

        var _boardSpacingCountForWidth = _testBoard.Width - 1;
        var _boardSpacingCountForHeight = _testBoard.Height - 1;
        var _boardDiameterWidth = (_testBoard.Width + TO_INDEX) * HALF;
        var _boardDiameterHeight = (_testBoard.Height + TO_INDEX) * HALF;

        //  Rearrange card position & initialize
        for (int i = 0; i < _cards.Count; i++)
        {
            var x = i % _testBoard.Width;
            var y = Mathf.FloorToInt(i / _testBoard.Width);

            var _card = _cardsShuffled[i];
            var _cardPosition = new Vector2(
                x - _boardDiameterWidth + ((x - (_boardSpacingCountForWidth * HALF)) * spacing),
                y - _boardDiameterHeight + ((y - (_boardSpacingCountForHeight * HALF)) * spacing));

            _card.name = $"Card {x}, {y}";
            _card.transform.position = _cardPosition;
        }

        //  Spawn Cards. Amount = Board.Width * Board.Height
        //  Insert CardData to Cards

        StartCoroutine(StartingCoroutine(_cardsDataDistinct, _cardTextureLoadTask));
    }


    private IEnumerator StartingCoroutine(int _targetMatchCount, List<Task> _tasks)
    {
        var _task = Task.WhenAll(_tasks);
        do
        {
            yield return null;
        }
        while (_task.IsCompleted is false);

        StartMatching(_targetMatchCount);
    }

    private void StartMatching(int _targetMatchCount)
    {
        CardInputRaycaster.Instance.OnClickCard += OnOpenCard;
        CardInputRaycaster.Instance.canDetectInput = true;

        Combo.Instance.OnComboUpdatedEvent += OnComboUpdated;
        Combo.Instance.ChainReset();

        Score.Instance.ResetScore();

        MatchGoal.Instance.OnGoalEvent += OnMatchComplete;
        MatchGoal.Instance.SetGoal(_targetMatchCount);
        MatchGoal.Instance.ResetProgress();
    }

    private IEnumerator OnOpenCardCoroutine(Card _toCard)
    {
        var _cacheCurrentSelectedCard = _currentSelectedCard;
        var _hasCache = _cacheCurrentSelectedCard is not null;

        _currentSelectedCard = _toCard;
        
        //  Flip Up
        var _flipCoroutine = StartCoroutine(LerpCoroutine(_flipUpDuration, ROTATION_X_FACEDOWN, ROTATION_Y_FACEUP, (_value) =>
        {
            _toCard.Session.IsFaceUp = true;
            _toCard.transform.rotation = Quaternion.Euler(0, _value, 0);
        }));

        if (_hasCache)
        {
            _currentSelectedCard = null;

            //  NOTE: Possible Bug
            //  Wait flip animation to complete before matching
            yield return _flipCoroutine;

            var _isMatch = _toCard.Session.CardData.itemName == _cacheCurrentSelectedCard.Session.CardData.itemName;
            if (_isMatch)
            {
                //  Informs combo to chain
                Combo.Instance.Chain();
                Score.Instance.Add(Mathf.FloorToInt(Combo.Instance.CurrentChainWeight));

                MatchGoal.Instance.Add();

                yield break;
            }

            //  Not a match, flip back to face down
            yield return new WaitForSeconds(_flipInterval);

            yield return StartCoroutine(LerpCoroutine(_flipDownDuration, ROTATION_Y_FACEUP, ROTATION_X_FACEDOWN, (_value) =>
            {
                _cacheCurrentSelectedCard.transform.rotation = Quaternion.Euler(0, _value, 0);
                _toCard.transform.rotation = Quaternion.Euler(0, _value, 0);
            }));

            _cacheCurrentSelectedCard.Session.IsFaceUp = false;
            _toCard.Session.IsFaceUp = false;
            
            //  Reset combo when fail once
            Combo.Instance.ChainReset();
        }

    }

    #region Private Event Callbacks

    private void OnOpenCard(Card _toCard)
    {
        //  Prevent flipping already face up card/ currently facing up.
        if (_toCard.Session.IsFaceUp) return;

        StartCoroutine(OnOpenCardCoroutine(_toCard));
    }

    private void OnComboUpdated(int _toLinkCount)
    {

    }

    private void OnMatchComplete()
    {
        Debug.Log("Complete");
    }

    #endregion

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

    private IEnumerator LerpCoroutine(float _duration, float _valueInitial, float _valueTarget, Action<float> _callback)
    {
        var _timer = 0f;
        var _percentage = 0f;

        _callback(_valueInitial);

        do
        {
            yield return null;

            _timer += Time.deltaTime;
            _percentage = _timer / _duration;

            _callback(Mathf.Lerp(_valueInitial, _valueTarget, _percentage));
        }
        while (_timer < _duration);

        _callback(_valueTarget);
    }

    #endregion
}
