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

        // �ε��� �Ϸ�� ������ ���
        while (!asyncLoad.isDone)
        {
            // �ε� ���� ��Ȳ�� UI � ǥ���� �� �ֽ��ϴ�.
            Debug.Log("Loading progress: " + asyncLoad.progress);
            yield return null;
        }

        Debug.Log("Scene loaded successfully!");
    }
}