using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private void Start()
    {
        BoardNavigator.Instance.ResetNavigation();
        BoardNavigator.Instance.OnClickBoardEvent += OnBoardSelected;
    }

    #region Event Callback

    private void OnBoardSelected(BoardConfiguration _configuration)
    {
        PlayerPrefs.SetString(BoardConfiguration.PREF_KEY, _configuration.name);
        SceneManager.LoadScene("MatchGeneratorScene");
    }

    #endregion
}
