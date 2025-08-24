using UnityEngine;
using System.Collections;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject startText;
    [SerializeField]
    private GameObject startButtons;
    private bool active = true;

    void Start()
    {
        StartCoroutine(StartText());
    }

    IEnumerator StartText()
    {
        while (true)   // ���� �ݺ�
        {
            active = !active;                  
            startText.SetActive(active);        
            yield return new WaitForSeconds(0.8f);
            if (Input.anyKey)
            {
                startText.SetActive(false);
                startButtons.SetActive(true);
                yield break;
            }
        }
    }

    public void StartGame()
    {
        SceneLoader.Instance.LoadScene("PlayScene");
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
