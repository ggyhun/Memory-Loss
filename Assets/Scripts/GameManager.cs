using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator GameOver()
    {
        yield return FadeManager.Instance.FadeInOut(1, 2f);
        SceneLoader.Instance.LoadScene("GameOver");
    }

    public IEnumerator GameClear()
    {
        CameraFollow cameraFollow = GameObject.Find("Main Camera").AddComponent<CameraFollow>();
        yield return StartCoroutine(cameraFollow.InGameZoomInOut(5, 2, 3f));
        yield return FadeManager.Instance.FadeInOut(1, 2f);
        GameRecorder.Instance.IsGameCleared = true;
        SceneLoader.Instance.LoadScene("GameEnding");
    }
}
