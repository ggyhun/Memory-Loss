using UnityEngine;
using System.Collections;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject startText;
    [SerializeField]
    private GameObject startButtons;
    private bool active = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(StartText());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StartText()
    {
        while (true)   // 무한 반복
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
}
