using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AsyncSceneLoader : MonoBehaviour
{

    public void LoadSceneAsync(int sceneNumber)
    {
        StartCoroutine(LoadSceneAsyncRoutine(sceneNumber));
    }

    IEnumerator LoadSceneAsyncRoutine(int sceneNumber)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNumber);

        // 로딩이 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            // 로딩 진행 상황을 UI 등에 표시할 수 있습니다.
            Debug.Log("Loading progress: " + asyncLoad.progress);
            yield return null;
        }

        Debug.Log("Scene loaded successfully!");
    }
}