using UnityEngine;

public class AnimationEndHandler : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.GetComponent<Player>().OnAttackEnd();
    }
}