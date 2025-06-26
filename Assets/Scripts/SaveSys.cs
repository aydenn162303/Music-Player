using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    // https://www.youtube.com/watch?v=1mf730eb5Wo about 6:00
    
    private static SaveData _saveData = new SaveData();

    [System.Serializable]

    public struct SaveData
    {
        public FavoritesData FavoritesData;
        public ListensData ListensData;
        public SettingsData SettingsData;
    }

    public static string SaveFileName()
    {
        string saveFile = Application.persistentDataPath + "/songdata" + ".txt";
        return saveFile;
    }

    public static void Save()
    {
        HandleSaveData();

        File.WriteAllText(SaveFileName(), JsonUtility.ToJson(_saveData, true));
    }

    private static void HandleSaveData()
    {
        MusicPlayer.Instance.RandomMusicPlayer.Save(ref _saveData.FavoritesData);
        MusicPlayer.Instance.RandomMusicPlayer.Save(ref _saveData.ListensData);
        MusicPlayer.Instance.RandomMusicPlayer.Save(ref _saveData.SettingsData);
    }


    public static void Load()
    {
        string path = SaveFileName();
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Save file not found at {path}. Skipping load.");
            return;
        }
        string saveContent = File.ReadAllText(path);
        _saveData = JsonUtility.FromJson<SaveData>(saveContent);
        HandleLoadData();
        MusicPlayer.Instance.RandomMusicPlayer.FinishedLoading(); // do things when loading is done
    }

    private static void HandleLoadData()
    {
        MusicPlayer.Instance.RandomMusicPlayer.Load(_saveData.FavoritesData);
        MusicPlayer.Instance.RandomMusicPlayer.Load(_saveData.ListensData);
        MusicPlayer.Instance.RandomMusicPlayer.Load(_saveData.SettingsData);
    }

}
