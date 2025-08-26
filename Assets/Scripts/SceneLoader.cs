using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    bool isLoading;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void LoadScene(string name)
    {
        if (isLoading) return;           // 중복 호출 방지
        StartCoroutine(LoadSceneAsyncRoutine(name));
    }

    IEnumerator LoadSceneAsyncRoutine(string name)
    {
        isLoading = true;
        Time.timeScale = 1f;             // 혹시 이전 씬에서 0으로 둔 게 이어지지 않게

        var op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
        op.allowSceneActivation = false; // 활성화 이전까지 로딩만

        // 리소스 로딩(0.0 ~ 0.9)
        while (op.progress < 0.9f)
        {
            Debug.Log($"Loading: {op.progress:P0}");
            yield return null;
        }

        Debug.Log("Activation start");   // ← 여기까지 오면 로딩은 끝, 이제 씬 활성화만 남음
        op.allowSceneActivation = true;

        // 활성화 완료 대기 (여기서 멈춘다면 '새 씬의' Awake/Start 중 블로킹)
        float watchdog = 0f;
        while (!op.isDone)
        {
            watchdog += Time.unscaledDeltaTime;
            if (watchdog > 10f)
                Debug.LogWarning("Scene activation taking unusually long. Check for blocking code in Awake/Start.");
            yield return null;
        }

        Debug.Log("Scene loaded successfully!");
        isLoading = false;
    }
}
