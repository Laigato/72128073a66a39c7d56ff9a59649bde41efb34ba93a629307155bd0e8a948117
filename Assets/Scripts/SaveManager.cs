using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance
    { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public event Action<object> OnLoadEvent;

    public void Save(SaveConfiguration _configuration, object _toData)
    {
        var _JSON = JsonUtility.ToJson(_toData, true);
        
        using (StreamWriter _streamWriter = new StreamWriter(Application.persistentDataPath + $"/{_configuration.nameFile}.json"))
        {
            _streamWriter.WriteLine(_JSON);
        }
    }

    public object Load(SaveConfiguration _configuration)
    {
        var _path = Application.persistentDataPath + $"/{_configuration.nameFile}.json";
        if (IsExist(_configuration))
        {
            using (StreamReader _streamReader = new StreamReader(_path))
            {
                var _JSON = _streamReader.ReadToEnd();
                var _fileClass = Type.GetType(_configuration.nameClass, true);
                var _fileInstance = JsonUtility.FromJson(_JSON, _fileClass);
                try
                {
                    OnLoadEvent?.Invoke(_fileInstance);
                }
                catch (Exception _err)
                { Debug.LogError(_err); }

                return _fileInstance;
            }
        }

        return null;
    }

    public void DeleteSave(SaveConfiguration _configuration)
    {
        var _path = Application.persistentDataPath + $"/{_configuration.nameFile}.json";
        File.Delete(_path);
    }

    public bool IsExist(SaveConfiguration _configuration)
    {
        var _path = Application.persistentDataPath + $"/{_configuration.nameFile}.json";
        var _isPathExists = File.Exists(_path);
        return _isPathExists;
    }
}