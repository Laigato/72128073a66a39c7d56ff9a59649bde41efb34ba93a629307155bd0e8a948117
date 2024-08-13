using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MatchController : MonoBehaviour
{
    [SerializeField] private List<CardData> _cardDataList = new();

    [Header("References")]
    [SerializeField] private Transform _cardsParent;
    [SerializeField] private Card _cardPrefab;
    [SerializeField] private Material _cardMaterialPrefab;

    [Header("Settings")]
    [SerializeField] private SaveConfiguration _saveConfigurationForMatch;
    [Space]
    [SerializeField, Min(0)] private float spacing = 0f;
    [Space]
    [SerializeField, Min(0.1f)] private float _flipUpDuration;
    [SerializeField, Min(0.1f)] private float _flipDownDuration;
    [SerializeField, Min(0.1f)] private float _flipInterval;
    [Space]
    [SerializeField] private AudioClip _sfxFlip;
    [SerializeField] private AudioClip _sfxCorrect;
    [SerializeField] private AudioClip _sfxIncorrect;
    [SerializeField] private AudioClip _sfxComplete;

    private const float HALF = 0.5f;
    private const int TO_INDEX = -1;

    private const float ROTATION_Y_FACEUP = 180f;
    private const float ROTATION_X_FACEDOWN = 0f;

    private readonly Dictionary<string, CardData> _cardDataDictionary = new();

    private AudioSource _sfxSource;
    private Card _currentSelectedCard;
    private List<Card> _currentGeneratedCards;

    private void Awake()
    {
        _sfxSource = GetComponent<AudioSource>();

        //  Create Dictionary of CardData with itemName as Key for easy reference without search
        foreach (var _cardData in _cardDataList)
        {
            _cardDataDictionary.Add(_cardData.name, _cardData);
        }
    }

    private void Start()
    {
        var _isExists = SaveManager.Instance.IsExist(_saveConfigurationForMatch);
        if (_isExists is not true)
        {
            SceneManager.LoadScene("MainMenuScene");
            return;
        }

        var _saveMatch = SaveManager.Instance.Load(_saveConfigurationForMatch) as SaveMatch;

        SaveManager.Instance.OnLoadEvent += OnLoadFile;
        StartCoroutine(GenerateBoardFromSaveMatch(_saveMatch));
    }

    private IEnumerator GenerateBoardFromSaveMatch(SaveMatch _saveMatch)
    {
        var _operation = Addressables.LoadAssetAsync<BoardConfiguration>(_saveMatch.currentBoard);

        yield return _operation;

        var _configuration = _operation.Result;

        //  Fetch Board
        var _cardsCount = _configuration.Width * _configuration.Height;

        //  Note CardSize = 1
        var _worldWidth = _configuration.Width + (spacing * (_configuration.Width - 1));
        var _worldHeight = _configuration.Height + (spacing * (_configuration.Height - 1));

        //  Resize Camera based on board dimension
        var _ratio = Screen.width / (float)Screen.height;
        var _orthoWidth = _worldWidth / (_ratio - 1) * HALF;
        Camera.main.orthographicSize = _orthoWidth;

        //  Calculate unique cards (matches without duplicate)
        var _cardsDataDistinct = Mathf.FloorToInt(_cardsCount * HALF);

        var _boardSpacingCountForWidth = _configuration.Width - 1;
        var _boardSpacingCountForHeight = _configuration.Height - 1;
        var _boardDiameterWidth = (_configuration.Width + TO_INDEX) * HALF;
        var _boardDiameterHeight = (_configuration.Height + TO_INDEX) * HALF;

        var _cards = new List<Card>();
        var _cardTextureLoadTask = new List<Task>();

        for (int i = 0; i < _cardsCount; i++)
        {
            var _cardSave = _saveMatch.cards[i];
            var _cardData = _cardDataDictionary[_cardSave.name];
            var _card = Instantiate(_cardPrefab, _cardsParent);

            var _cardMaterial = Instantiate(_cardMaterialPrefab);
            var _cardSession = new CardDataSession(_cardMaterial, _cardData);

            //  Loading texture from Addressables using CardData, then assign BackTex
            var _processLoad = Addressables.LoadAssetAsync<Texture2D>(_cardData.itemTextureName);
            _processLoad.Completed += (_operation) =>
            {
                //  HACKME: Change to BackTex instead of FrontTex
                _cardMaterial.SetTexture("_BackTex", _operation.Result);
            };
            _cardTextureLoadTask.Add(_processLoad.Task);

            var x = i % _configuration.Width;
            var y = Mathf.FloorToInt(i / _configuration.Width);

            var _cardPosition = new Vector2(
                x - _boardDiameterWidth + ((x - (_boardSpacingCountForWidth * HALF)) * spacing),
                y - _boardDiameterHeight + ((y - (_boardSpacingCountForHeight * HALF)) * spacing));

            _card.Initialize(_cardSession);
            _card.Session.X = x;
            _card.Session.Y = y;
            _card.name = $"{_cardSave.name} {x}, {y}";
            _card.transform.position = _cardPosition;
            _cards.Add(_card);
        }

        _currentGeneratedCards = _cards;

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
        _sfxSource.PlayOneShot(_sfxFlip);

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
                _sfxSource.PlayOneShot(_sfxCorrect);

                //  Informs combo to chain
                Combo.Instance.Chain();
                Score.Instance.Add(Mathf.FloorToInt(Combo.Instance.CurrentChainWeight));

                MatchGoal.Instance.Add();

                yield break;
            }

            _sfxSource.PlayOneShot(_sfxIncorrect);
            
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

    private void OnLoadFile(object _toData)
    {
        foreach (Transform _item in _cardsParent)
        {
            Destroy(_item.gameObject);
        }
        _currentSelectedCard = default;
        _currentGeneratedCards.Clear();

        StartCoroutine(GenerateBoardFromSaveMatch(_toData as SaveMatch));
    }

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
        _sfxSource.PlayOneShot(_sfxComplete);
    }

    #endregion

    #region Unity Event Callback

    public void INPUT_Save(SaveConfiguration _configuration)
    {
        var _saveMatch = new SaveMatch();
        _saveMatch.currentBoard = PlayerPrefs.GetString(BoardConfiguration.PREF_KEY);
        _saveMatch.currentScore = Score.Instance.CurrentScore;
        _saveMatch.currentMatch = MatchGoal.Instance.CurrentMatches;
        _saveMatch.currentCombo = Combo.Instance.CurrentChain;
        
        for (int i = 0; i < _currentGeneratedCards.Count; i++)
        {
            var _currentCard = _currentGeneratedCards[i];
            var _currentSaveMatchCard = new SaveMatch.Card()
            {
                name = _currentCard.Session.CardData.name,
                x = _currentCard.Session.X,
                y = _currentCard.Session.Y,
                isFaceUp = _currentCard.Session.IsFaceUp
            };

            _saveMatch.cards.Add(_currentSaveMatchCard);
        }

        SaveManager.Instance.Save(_configuration, _saveMatch);
    }

    public void INPUT_Load(SaveConfiguration _configuration)
    {
        SaveManager.Instance.Load(_configuration);
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
