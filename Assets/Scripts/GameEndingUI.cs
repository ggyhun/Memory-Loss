using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class GameEndingUI : MonoBehaviour
{
    public TextMeshProUGUI EndingText;
    public GameObject Fade;
    public GameObject TextPanelUI;
    public GameObject LetterUI;
    string[] TextData = {
        "1",
        "끝없는 동굴을 헤맨 기억이 있다.",
        "이것도 잘못된 기억일까?",
        "",
        "내가 어쩌다 이곳까지 왔는지 모르겠다.",
        "애초에, 내가 마법을 쓸 수 있었나?",
        "",
        "까만 세상을 영원히 걸었던 것만 같다.",
        "점점... 눈 앞이 밝아진다.",
        "",
        "2",
        "눈을 떠보니 낯선 병원 침대다.",
        "",
        "- 내가 왜 여기 있는거지?",
        "",
        "잘 기억이 나지 않는다.",
        "나는 분명히 마법을 썼는데...",
        "",
        "그때 내 눈에 편지 한 통이 들어왔다.",
        "",
        "3",
        "아들에게",
        "",
        "사랑하는 우리 아들",
        "네가 눈을 뜨지 못한지",
        "벌써 2개월이 됐구나.",
        "",
        "친구를 잃은 슬픔에",
        "네가 너에게 저지른 일이",
        "엄마는 너무 마음이 아프단다.",
        "",
        "아들아, 네 죄가 아니다.",
        "그 일은 사고였단다.",
        "",
        "부디 네가 다시 일어나",
        "나에게 안겨주길",
        "",
        "엄마가",
        "",
        "2",
        "편지를 다 읽자 머리가 깨질듯이 아파왔다.",
        "",
        "나는 동굴을 탈출한 마법사가 아니었나.",
        "그렇다면 내가 지금까지 겪은 일들은 뭐지?",
        "",
        "순식간에 많은 정보가 머릿속으로 들어왔다.",
        "",
        "친구와 함께 귀가하면서 길을 걷던 일,",
        "코너를 돌자마자 차량이 우리를 덮치던 불빛,",
        "나를 밀치고 저 멀리 날아간 친구의 시체",
        "",
        "웅성 거리는 주변...",
        "그리고 끊긴 내 기억.",
        "",
        "2",
        "그때, 병실을 들어오던 간호사가 나를 보고 소리를 질렀다.",
        "- 앗! 12번 환자가 깨어났습니다!",
        "",
        "얼마 있지 않아, 익숙한 얼굴의 여성이 달려와 나를 안았다.",
        "",
        "눈물을 흘리는 그녀.",
        "",
        "내 입에서 자연스럽게 말이 흘러나왔다.",
        "- 엄마...",
        "",
        "나는 그녀를 기억하려 몸을 꽉 껴안았다.",
        "",
        "4",
        "END",
        ""
    };
    string DisplayingText;
    bool IsAwaitingInput = false;
    bool WalkingSFXPlaying = true;
    int TextDataLine = 0;
    int TextDataColumn = 0;
    void Awake()
    {
        AudioManager.Instance.PlayBGM(0);
        StartCoroutine(WalkingSFX());
        StartCoroutine(StartEndingScene());
    }

    void Start()
    {
        StartCoroutine(FadeManager.Instance.FadeInOut(0, 1f));
    }
    
    IEnumerator WalkingSFX()
    {
        while (WalkingSFXPlaying)
        {
            AudioManager.Instance.PlaySFX(0);
            yield return new WaitForSeconds(1.5f);
        }
        yield break;
    }
    IEnumerator StartEndingScene()
    {
        while (TextDataLine < TextData.Length)
        {
            if (IsAwaitingInput == false)
            {
                if (!Input.GetKey(KeyCode.Space))
                {
                    yield return new WaitForSeconds(0.05f);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                while (!Input.GetKeyUp(KeyCode.Space))
                    yield return null;
                IsAwaitingInput = false;
            }
            else
            {
                yield return null;
                continue;
            }
            SceneUpdate();
        }
        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
        yield return FadeManager.Instance.FadeInOut(1, 1f);
        SceneLoader.Instance.LoadScene("GameOver");
        yield break;
    }
    IEnumerator BackgroundFadeIn()
    {
        float timecount = 0f;
        CanvasRenderer fade = Fade.GetComponent<CanvasRenderer>();
        while (timecount <= 2f)
        {
            fade.SetAlpha(1f - timecount / 2f);
            timecount += Time.deltaTime;
            yield return null;
        }
        Fade.SetActive(false);
        yield break;
    }
    void SceneUpdate()
    {
        int mode = TextUpdate();
        if (mode == 1)
        {
            TextPanelUI.SetActive(false);
            LetterUI.SetActive(false);
            Fade.SetActive(true);
            Fade.GetComponent<CanvasRenderer>().SetAlpha(1f);
            EndingText.rectTransform.anchoredPosition = new Vector3(50, 500, 0);
            EndingText.fontSize = 64;
            EndingText.color = new Color(0xff, 0xff, 0xff);
        }
        else if (mode == 2)
        {
            WalkingSFXPlaying = false;
            AudioManager.Instance.ClearAllSFX();
            TextPanelUI.SetActive(true);
            LetterUI.SetActive(false);
            if (Fade.activeSelf)
                StartCoroutine(BackgroundFadeIn());
            EndingText.rectTransform.anchoredPosition = new Vector3(150, 400, 0);
            EndingText.fontSize = 58;
            EndingText.color = new Color(0x00, 0x00, 0x00);
        }
        else if (mode == 3)
        {
            TextPanelUI.SetActive(true);
            LetterUI.SetActive(true);
            EndingText.rectTransform.anchoredPosition = new Vector3(650, 350, 0);
            EndingText.fontSize = 42;
            EndingText.color = new Color(0x00, 0x00, 0x00);
        }
        else if (mode == 4)
        {
            TextPanelUI.SetActive(false);
            LetterUI.SetActive(false);
            Fade.SetActive(true);
            Fade.GetComponent<CanvasRenderer>().SetAlpha(1f);
            EndingText.rectTransform.anchoredPosition = new Vector3(900, 0, 0);
            EndingText.fontSize = 108;
            EndingText.color = new Color(0xff, 0xff, 0xff);
        }
    }
    int TextUpdate()
    {
        if (TextData[TextDataLine].Length == 0)
        {
            TextDataLine++;
            DisplayingText += "\n";
            IsAwaitingInput = true;
        }
        else if (TextData[TextDataLine].Length == 1)
        {
            TextDataLine++;
            DisplayingText = string.Empty;
            return int.Parse(TextData[TextDataLine - 1]);
        }
        else if (TextDataColumn >= TextData[TextDataLine].Length)
        {
            TextDataLine++;
            TextDataColumn = 0;
            DisplayingText += "\n";
        }
        else
        {
            DisplayingText += TextData[TextDataLine][TextDataColumn];
            TextDataColumn++;
        }
        EndingText.text = DisplayingText;
        return 0;
    }
}
