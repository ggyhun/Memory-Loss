using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFlipbookOnce : MonoBehaviour
{
    [Header("Frames")]
    public Sprite[] frames;                 // 프리팹(Asset)에 프레임들을 꼭 넣기!
    public float fps = 12f;
    public bool destroyOnEnd = true;

    [Header("Render")]
    public string sortingLayer = "Effects"; // 존재하지 않으면 Default로 경고 후 대체
    public int orderInLayer = 10;
    public Vector3 worldOffset = Vector3.zero;

    [Header("Options")]
    public bool playOnEnable = true;
    public bool useUnscaledTime = true;     // Time.timeScale=0이어도 재생되게

    private SpriteRenderer sr;
    private Coroutine co;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = true;
        sr.color = Color.white;

        // SortingLayer 존재 검증
        int layerId = SortingLayer.NameToID(sortingLayer);
        if (layerId == 0 && sortingLayer != "Default")
        {
            Debug.LogWarning($"[{name}] SortingLayer '{sortingLayer}' not found. Use 'Default'.", this);
        }
        else
        {
            sr.sortingLayerID = layerId;
        }
        sr.sortingOrder = orderInLayer;

        if (worldOffset != Vector3.zero)
            transform.position += worldOffset;
    }

    private void OnEnable()
    {
        if (playOnEnable) Play();
    }

    public void Play()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        Debug.Log($"[{name}] Flipbook Play (frames={frames?.Length ?? 0})", this);

        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"[{name}] No frames assigned.", this);
            yield break;
        }

        float dt = 1f / Mathf.Max(1f, fps);

        // 첫 프레임 즉시 노출 (timeScale=0 이어도 보이게)
        sr.sprite = frames[0];

        for (int i = 1; i < frames.Length; i++)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(dt);
            else                 yield return new WaitForSeconds(dt);

            sr.sprite = frames[i];
        }

        if (destroyOnEnd) Destroy(gameObject);
    }

    [ContextMenu("Sort Frames By Name")]
    private void SortByName()
    {
        System.Array.Sort(frames, (a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
    }
}
