using UnityEngine;
using System.Collections;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject startText;
    [SerializeField]
    private GameObject startButtons;
    private bool active = true;
    private bool KeyPressed = false;

    void Start()
    {
        StartCoroutine(StartText());
        StartCoroutine(AwaitFirstInput());
    }

    IEnumerator StartText()
    {
        while (!KeyPressed)
        {
            active = !active;                  
            startText.SetActive(active);        
            yield return new WaitForSeconds(0.8f);
        }
    }

    IEnumerator AwaitFirstInput()
    {
        while (!Input.anyKey)
        {
            yield return null;
        }
        startText.SetActive(false);
        startButtons.SetActive(true);
        KeyPressed = true;
    }

    IEnumerator FadeOut()
    {
        yield return FadeManager.Instance.FadeInOut(1, 1.5f);
        SceneLoader.Instance.LoadScene("PlayScene");
        yield break;
    }

    public void StartGame()
    {
        StartCoroutine(FadeOut());
    }

    public void OpenOption()
    {
        
    }

    public void OffGame()
    {
        Application.Quit();
        Debug.Log("��������");
    }
}
