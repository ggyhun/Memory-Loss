using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
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
    public void LoadScene(string name)
    {
        StartCoroutine(LoadSceneAsyncRoutine(name));
    }

    IEnumerator LoadSceneAsyncRoutine(string name)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);

        while (!asyncLoad.isDone)
        {
            Debug.Log("Loading progress: " + asyncLoad.progress);
            yield return null;
        }
        Debug.Log("Scene loaded successfully!");
    }
}