using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource[] audioSource;
    public AudioClip[] BGM;
    public AudioClip[] SFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponents<AudioSource>();
    }

    public void PlayBGM(int n)
    {
        audioSource[0].Stop();
        audioSource[0].PlayOneShot(BGM[n]);
    }
    public void PlaySFX(int n)
    {
        audioSource[1].PlayOneShot(SFX[n]);
    }
    public void ClearAllSFX()
    {
        audioSource[1].Stop();
    }
}
