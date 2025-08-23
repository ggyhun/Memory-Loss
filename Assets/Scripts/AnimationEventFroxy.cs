using UnityEngine;

// 자식 Sprite(Animator 달린 오브젝트)에 붙이기
public class AnimationEventProxy : MonoBehaviour
{
    // Animation Event 창에서 이 함수 이름으로 이벤트를 추가
    public void AnimEvent_ActionEnd()
    {
        // 부모 Empty에 붙은 싱글톤 PlayerAnimator 호출
        PlayerAnimator.Instance.AnimEvent_ActionEnd();
    }
    
    // 부모 Empty에 붙은 싱글톤이 아닌 EnemyAnimator 호출
    public void AnimEvent_EnemyActionEnd()
    {
        EnemyAnimator enemyAnimator = GetComponentInParent<EnemyAnimator>();
        if (enemyAnimator != null)
        {
            enemyAnimator.AnimEvent_EnemyActionEnd();
        }
    }
}