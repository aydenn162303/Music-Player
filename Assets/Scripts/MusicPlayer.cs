using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    
    [SerializeField] private RandomMusicPlayer randomMusicPlayer;
    public RandomMusicPlayer RandomMusicPlayer => randomMusicPlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (randomMusicPlayer == null)
                randomMusicPlayer = GetComponent<RandomMusicPlayer>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
