using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
//remember to later have an ambience sound option
//volume
//save refrences?  

[System.Serializable]
public class SongData
{
    public string Path;
    public string SongName;
}

[RequireComponent(typeof(AudioSource), typeof(MusicPlayer))]
public class RandomMusicPlayer : MonoBehaviour
{

    [System.Serializable]
    public enum Page
    {
        Start,
        Onboarding,
        LoadSongs,
        MusicPlayer,
        Settings,
        Error
    }

    [System.Serializable]
    public enum PlayType
    {
        Random,
        Sequential,
        Repeat
    }
    private PlayType ChangeSongType = PlayType.Random; // Default play type

    public TMP_InputField pathInputField;
    public UnityEngine.UI.Slider progressSlider;
    public TMP_Dropdown songDropdown;
    public UnityEngine.UI.Button favButton;
    public TMP_Text currentSongDisplay;
    [SerializeField] private List<SongData> songPaths = new List<SongData>();
    [SerializeField] private List<SongData> recentSongs = new List<SongData>();
    [SerializeField] private List<string> favoriteSongs = new List<string>();
    public string selectedPath = "C:/Music";
    public string selectedSongName;
    public int selectedSongPlace = 0; //tracks place in the list, DO NOT SAVE AND LOAD IT WILL BE UPDATED
    private AudioSource audioSource;
    private bool isLoadingSong = false;
    private int songsListenedTo = 0;
    [SerializeField] private Page currentPage = Page.Start;
    [SerializeField] private Page lastPage;

    //all of these are parents to the ui
    private GameObject LoadingUI; private GameObject MusicPlayerUI; private GameObject SettingsUI; private GameObject OnboardingUI; private GameObject StartUI; private GameObject ErrorUI;


    void Awake()
    {
        Application.targetFrameRate = 30;
        audioSource = GetComponent<AudioSource>();
        pathInputField = GameObject.FindGameObjectWithTag("Input").GetComponent<TMP_InputField>();
        progressSlider = GameObject.Find("ProgressSlider").GetComponent<UnityEngine.UI.Slider>();
        currentSongDisplay = GameObject.Find("SongText").GetComponent<TMP_Text>();
        favButton = GameObject.Find("FavoriteButton").GetComponent<UnityEngine.UI.Button>();

        StartUI = GameObject.Find("StartUI");
        LoadingUI = GameObject.Find("LoadingUI");
        MusicPlayerUI = GameObject.Find("MusicPlayerUI");
        SettingsUI = GameObject.Find("SettingsUI");
        OnboardingUI = GameObject.Find("OnboardingUI");
        ErrorUI = GameObject.Find("ErrorUI");

        StartUI.SetActive(true);
        LoadingUI.SetActive(false);
        MusicPlayerUI.SetActive(false);
        SettingsUI.SetActive(false);
        OnboardingUI.SetActive(false);
        ErrorUI.SetActive(false);
        lastPage = currentPage;

    }


    //Check if a save exists and load it
    void Start()
    {
        LoadOnStart();
        audioSource.Pause();
    }

    private void LoadOnStart()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, "songdata.txt");
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveSystem.Load(); // Use static method
            currentPage = Page.LoadSongs;

            if (recentSongs != null && recentSongs.Count > 0)
            {
                StartCoroutine(LoadAndPlaySong(recentSongs[recentSongs.Count - 1])); // Load the last played song

            }
        }
    }


    private void Update()
    {
        if (currentPage != lastPage)
        {
            UpdatePageHandler(); // Updates UI and also runs functions
        }

        UpdateSliderPos();

    }

    private void UpdateSliderPos()
    {
        if (audioSource.isPlaying && audioSource.clip != null)
        {
            float sliderValue = (audioSource.time / audioSource.clip.length) * 100f;
            progressSlider.value = sliderValue;
        }
        else if (audioSource.clip == null)
        {
            progressSlider.value = 0f; // Reset slider if song is null
        }
    }



    //the handler for enabling/disabling ui based on current page (also loading the songs and such)
    private void UpdatePageHandler()
    {
        lastPage = currentPage;
        switch (currentPage)
        {
            case Page.Start:
                ShowStartUI();
                break;
            case Page.Onboarding:
                ShowOnboardingUI();
                break;
            case Page.LoadSongs:
                ShowLoadSongsUI();
                LoadSongsHandler();
                break;
            case Page.MusicPlayer:
                ShowMusicPlayerUI();
                break;
            case Page.Settings:
                ShowSettingsUI();
                break;
            case Page.Error:
                ShowErrorUI();
                break;
            default:
                Debug.LogError("Unknown page: " + currentPage);
                break;
        }
    }

    private void ShowStartUI()
    {
        HideUI();
        StartUI.SetActive(true);
        songPaths.Clear();
        recentSongs.Clear();
    }

    private void ShowOnboardingUI()
    {
        HideUI();
        OnboardingUI.SetActive(true);
    }

    private void ShowLoadSongsUI()
    {
        HideUI();
        LoadingUI.SetActive(true);
    }

    private void LoadSongsHandler() //the load function for loading screen
    {
        if (isLoadingSong)
            return;

        isLoadingSong = true;

        string path = pathInputField.text;
        if (string.IsNullOrEmpty(path))
        {
            path = selectedPath;
        }

        try
        {
            // Get all MP3 files recursively from the directory and all subdirectories
            string[] files = Directory.GetFiles(path, "*.mp3", SearchOption.AllDirectories);

            Debug.Log($"Searching for MP3 files in: {path}");

            foreach (string file in files)
            {
                // Verify the file still exists (in case it was deleted during search)
                if (File.Exists(file))
                {
                    songPaths.Add(new SongData
                    {
                        Path = file,
                        SongName = Path.GetFileNameWithoutExtension(file)
                    });
                }
            }

            Debug.Log($"Found {songPaths.Count} MP3 files in directory and subdirectories");
        }
        catch (System.UnauthorizedAccessException ex)
        {
            Debug.LogError($"Access denied to some directories: {ex.Message}");
            currentPage = Page.Error;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading songs: {ex.Message}");
            currentPage = Page.Error;
        }
        finally
        {
            isLoadingSong = false;
        }
        currentPage = Page.MusicPlayer;
        Debug.Log("Songs loaded successfully. Transitioning to Music Player UI.");
    }

    private void UpdateSongListDropdown()
    {
        foreach (SongData song in songPaths)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(song.SongName);
            songDropdown.options.Add(option);
        }
    }

    private void ShowMusicPlayerUI()
    {
        HideUI();
        MusicPlayerUI.SetActive(true);
        UpdateSongListDropdown();
        // Implement music player UI logic here
    }

    private void ShowSettingsUI()
    {
        HideUI();
        SettingsUI.SetActive(true);
        // Implement settings UI logic here
    }

    private void ShowErrorUI()
    {
        HideUI();
        ErrorUI.SetActive(true);
    }

    private void HideUI()
    {
        StartUI.SetActive(false);
        LoadingUI.SetActive(false);
        MusicPlayerUI.SetActive(false);
        SettingsUI.SetActive(false);
        OnboardingUI.SetActive(false);
        ErrorUI.SetActive(false);
    }


    #region UI Handlers

    public void SliderValueChangedHandler()
    {
        if (audioSource.isPlaying && audioSource.clip != null)
        {
            float newTime = audioSource.clip.length * (progressSlider.value / 100f);
            audioSource.time = newTime;
        }
    }

    public void NextSongPressedHandler()
    {

    }

    public void LastSongPressedHandler()
    {
        // TODO: Implement logic to play the previous song
    }

    public void PauseButtonPressedHandler()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.UnPause();
        }
    }


    public void StartButtonPathSelected()
    {
        string path = pathInputField.text;
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            StartCoroutine(TurnStartButtonRed());
            return;
        }

        bool hasMp3 = false;
        try
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.mp3", SearchOption.AllDirectories))
            {
                hasMp3 = true;
                break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error checking for mp3 files: {ex.Message}");
        }

        if (!hasMp3)
        {
            StartCoroutine(TurnStartButtonRed());
            return;
        }

        currentPage = Page.LoadSongs;
        selectedPath = path;
        Debug.Log("Selected path: " + selectedPath);
    }

    private IEnumerator TurnStartButtonRed()
    {
        var startButton = GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>();
        var originalColor = startButton.image.color;
        startButton.image.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        startButton.image.color = originalColor;
    }

    public void DropDownSongSelected()
    {
        int selectedIndex = songDropdown.value;
        if (selectedIndex >= 0 && selectedIndex < songPaths.Count)
        {
            SongData selectedSong = songPaths[selectedIndex];
            StartCoroutine(LoadAndPlaySong(selectedSong));
        }
        else
        {
            Debug.LogWarning("Invalid song selection in dropdown.");
        }
    }

    public void PlayRandomSong()
    {
        if (songPaths.Count == 0)
        {
            Debug.LogWarning("No songs available to play!");
            if (currentSongDisplay != null)
                currentSongDisplay.text = "No songs available";
            return;
        }

        // Get a random song from the list
        int randomIndex = Random.Range(0, songPaths.Count);
        SongData selectedSong = songPaths[randomIndex];

        // Load the song as an AudioClip
        StartCoroutine(LoadAndPlaySong(selectedSong));
    }

    private IEnumerator LoadAndPlaySong(SongData song)
    {
        songsListenedTo++;
        string filePath = "file://" + song.Path;

        selectedSongPlace = songPaths.FindIndex(s => s.Path == song.Path);

        // Clean up previous audio clip to prevent memory leak
        if (audioSource.clip != null)
        {
            AudioClip oldClip = audioSource.clip;
            audioSource.clip = null;
            DestroyImmediate(oldClip);
        }

        // Manage recent songs list size to prevent unlimited growth
        recentSongs.Add(song);
        if (recentSongs.Count > 50) // Keep only last 50 songs
        {
            recentSongs.RemoveAt(0);
        }

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();

                // Update the display with the song name
                if (currentSongDisplay != null)
                {
                    currentSongDisplay.text = song.SongName;
                }
                selectedSongName = song.SongName;
                Debug.Log($"Now playing: {song.SongName}");

                // Cleanup memory every 5 songs to prevent buildup
                if (recentSongs.Count % 5 == 0)
                {
                    ForceMemoryCleanup();
                }
            }
            else
            {
                Debug.LogError($"Failed to load song: {song.Path}. Error: {www.error}");
                if (currentSongDisplay != null)
                    currentSongDisplay.text = "Failed to load song";
            }
        }
        CheckIfFavorite();
    }

    public void ForceMemoryCleanup()
    {
        // Force garbage collection
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();

        // Unload unused assets
        Resources.UnloadUnusedAssets();

        Debug.Log("Forced memory cleanup completed");
    }

    #endregion

    #region Favorites


    public void AddToFavoritesButton()
    {
        if (favoriteSongs.Contains(selectedSongName))
        {
            favoriteSongs.Remove(selectedSongName);
        }
        else if (selectedSongName != null)
        {
            favoriteSongs.Add(selectedSongName);
        }
        CheckIfFavorite();
    }

    private void CheckIfFavorite()
    {
        if (!string.IsNullOrEmpty(selectedSongName) && favoriteSongs.Contains(selectedSongName))
        {
            favButton.image.color = Color.magenta;
            Debug.Log("Song is favorited: " + selectedSongName);
        }
        else
        {
            favButton.image.color = Color.white;
            Debug.Log("Song is not favorited: " + selectedSongName);
        }
    }









    #endregion

    #region Save/Load

    // Add public save function
    public void SaveGame()
    {
        SaveSystem.Save();
        Debug.Log("Game saved successfully!");
    }

    public void DeleteSave()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, "songdata.txt");
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
    }

    public void Save(ref FavoritesData data)
    {
        data._FavoriteSongs = favoriteSongs;
        data._RecentSongs = recentSongs;
    }
    public void Save(ref ListensData data)
    {
        data._SongsListenedTo = songsListenedTo; // Fixed: use correct field name
    }
    public void Save(ref SettingsData data)
    {
        data._PlayType = ChangeSongType;
        data._Volume = audioSource.volume;
        data._SelectedPath = selectedPath;
    }

    public void Load(FavoritesData data)
    {
        if (data._FavoriteSongs != null)
            favoriteSongs = data._FavoriteSongs;
        if (data._RecentSongs != null)
            recentSongs = data._RecentSongs;
        if (songDropdown != null)
            songDropdown.ClearOptions();
    }
    public void Load(ListensData data)
    {
        songsListenedTo = data._SongsListenedTo;
    }
    public void Load(SettingsData data)
    {
        ChangeSongType = data._PlayType;

        if (audioSource != null) audioSource.volume = data._Volume;

        if (!string.IsNullOrEmpty(data._SelectedPath)) selectedPath = data._SelectedPath;

        
    }

    //Called from savesys when loading is done
    public void FinishedLoading()
    {
        Debug.Log("Finished loading songs and settings.");
        if (pathInputField != null) pathInputField.text = selectedPath; //updates text

    }




}

[System.Serializable]
public struct FavoritesData
{
    public List<string> _FavoriteSongs;
    public List<SongData> _RecentSongs;
}

[System.Serializable]
public struct ListensData
{
    public int _SongsListenedTo;
}

[System.Serializable]
public struct SettingsData
{
    public RandomMusicPlayer.PlayType _PlayType;
    public float _Volume;
    public string _SelectedPath;
}

#endregion 