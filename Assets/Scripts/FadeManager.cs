using System.Collections;
using UnityEngine;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }
    public GameObject Fade;
    public bool FadeFinished = false;

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

    public IEnumerator FadeInOut(int type, float t)
    {
        FadeFinished = false;
        CanvasRenderer fade = Fade.GetComponent<CanvasRenderer>();
        float timecount = 0f;
        if (type == 0) // Fade in
        {
            fade.SetAlpha(1f);
            Fade.SetActive(true);
            while (timecount <= t)
            {
                fade.SetAlpha(1f - timecount / t);
                timecount += Time.deltaTime;
                yield return null;
            }
            Fade.SetActive(false);
        }
        else if (type == 1) // Fade out
        {
            fade.SetAlpha(0f);
            Fade.SetActive(true);
            while (timecount <= t)
            {
                fade.SetAlpha(timecount / t);
                timecount += Time.deltaTime;
                yield return null;
            }
        }
        FadeFinished = true;
        yield break;
    }
}
