using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    [SerializeField] private GameObject moveButtonPrefab;
    [SerializeField] private GameObject moveUIParent;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private GameObject attackButton;
    [SerializeField] private GameObject moveButton;
    private SpriteRenderer playerSpriteRenderer;

    Animator anim;

    // 상하좌우 방향 벡터 미리 정의
    private Vector2[] directions = new Vector2[] {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    // 현재 활성화된 이동 버튼 목록
    private List<GameObject> activeMoveButtons = new List<GameObject>();
    private void Start()
    {
        anim = GetComponent<Animator>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }
    // 이동 버튼을 눌렀을 때 호출: 이동 가능한 칸들을 표시
    public void ShowMoveOptions()
    {
        Debug.Log("함수 작동");
        ClearButtons(); // 기존에 표시된 버튼들 제거

        // 4방향 반복
        foreach (Vector2 dir in directions)
        {
            Debug.Log("반복문 작동");
            // 이동 목표 좌표 계산
            Vector2 targetPos = (Vector2)transform.position + dir;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(targetPos);
            Debug.Log(transform.position);

            // 해당 위치에 장애물이 없는지 체크 (OverlapCircle: 충돌 판정)
            if (!Physics2D.OverlapCircle(targetPos, 0.1f, obstacleMask))
            {
                // 이동 버튼 생성: targetPos 위치에 프리팹을 생성하고 부모에 넣기
                GameObject moveBtn = Instantiate(moveButtonPrefab, screenPos, Quaternion.identity, moveUIParent.transform);

                // 버튼 클릭 시 해당 위치로 이동하도록 리스너 추가
                moveBtn.GetComponent<Button>().onClick.AddListener(() => MoveTo(targetPos));

                // 버튼 리스트에 추가 (나중에 삭제할 때 필요)
                activeMoveButtons.Add(moveBtn);
            }
        }
    }

    // 클릭된 칸으로 이동
    void MoveTo(Vector2 destination)
    {
        if(transform.position.x - destination.x > 0)
        {
            playerSpriteRenderer.flipX = true;
        }
        else
        {
            playerSpriteRenderer.flipX = false;
        }
        ClearButtons(); // 버튼 지우기
        StartCoroutine(SmoothMove(destination)); // 부드럽게 이동 시작
    }

    // 부드럽게 이동하는 코루틴 함수
    IEnumerator SmoothMove(Vector2 target)
    {
        anim.SetFloat("Speed", 0);
        // 목표 위치에 도달할 때까지 반복
        while ((Vector2)transform.position != target)
        {
            anim.SetFloat("Speed", 1.0f);
            // 한 프레임에 조금씩 이동
            transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.01f); // 다음 프레임까지 대기
        }
        anim.SetFloat("Speed", 0);
        attackButton.SetActive(true);
        moveButton.SetActive(true);
    }

    // 기존에 생성된 버튼 모두 삭제
    void ClearButtons()
    {
        foreach (GameObject btn in activeMoveButtons)
        {
            Destroy(btn); // 버튼 오브젝트 삭제
        }
        activeMoveButtons.Clear(); // 리스트 비우기
        attackButton.SetActive(false);
        moveButton.SetActive(false);
    }

    public void Attack()
    {
        anim.SetTrigger("Attack");
        ClearButtons();
        Invoke("OnAttackEnd", 0.7f);
    }

    public void OnAttackEnd()
    {
        attackButton.SetActive(true);
        moveButton.SetActive(true);
    }
}
