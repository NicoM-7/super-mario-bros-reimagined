using UnityEngine;

public class TripleJumpSMB : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("IsTripleJumping", false);
    }
}
