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

    // �����¿� ���� ���� �̸� ����
    private Vector2[] directions = new Vector2[] {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    // ���� Ȱ��ȭ�� �̵� ��ư ���
    private List<GameObject> activeMoveButtons = new List<GameObject>();
    private void Start()
    {
        anim = GetComponent<Animator>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }
    // �̵� ��ư�� ������ �� ȣ��: �̵� ������ ĭ���� ǥ��
    public void ShowMoveOptions()
    {
        Debug.Log("�Լ� �۵�");
        ClearButtons(); // ������ ǥ�õ� ��ư�� ����

        // 4���� �ݺ�
        foreach (Vector2 dir in directions)
        {
            Debug.Log("�ݺ��� �۵�");
            // �̵� ��ǥ ��ǥ ���
            Vector2 targetPos = (Vector2)transform.position + dir;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(targetPos);
            Debug.Log(transform.position);

            // �ش� ��ġ�� ��ֹ��� ������ üũ (OverlapCircle: �浹 ����)
            if (!Physics2D.OverlapCircle(targetPos, 0.1f, obstacleMask))
            {
                // �̵� ��ư ����: targetPos ��ġ�� �������� �����ϰ� �θ� �ֱ�
                GameObject moveBtn = Instantiate(moveButtonPrefab, screenPos, Quaternion.identity, moveUIParent.transform);

                // ��ư Ŭ�� �� �ش� ��ġ�� �̵��ϵ��� ������ �߰�
                moveBtn.GetComponent<Button>().onClick.AddListener(() => MoveTo(targetPos));

                // ��ư ����Ʈ�� �߰� (���߿� ������ �� �ʿ�)
                activeMoveButtons.Add(moveBtn);
            }
        }
    }

    // Ŭ���� ĭ���� �̵�
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
        ClearButtons(); // ��ư �����
        StartCoroutine(SmoothMove(destination)); // �ε巴�� �̵� ����
    }

    // �ε巴�� �̵��ϴ� �ڷ�ƾ �Լ�
    IEnumerator SmoothMove(Vector2 target)
    {
        anim.SetFloat("Speed", 0);
        // ��ǥ ��ġ�� ������ ������ �ݺ�
        while ((Vector2)transform.position != target)
        {
            anim.SetFloat("Speed", 1.0f);
            // �� �����ӿ� ���ݾ� �̵�
            transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(0.01f); // ���� �����ӱ��� ���
        }
        anim.SetFloat("Speed", 0);
        attackButton.SetActive(true);
        moveButton.SetActive(true);
    }

    // ������ ������ ��ư ��� ����
    void ClearButtons()
    {
        foreach (GameObject btn in activeMoveButtons)
        {
            Destroy(btn); // ��ư ������Ʈ ����
        }
        activeMoveButtons.Clear(); // ����Ʈ ����
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
