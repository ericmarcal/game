using UnityEngine;

public class LogStateEntry : StateMachineBehaviour
{
    [Tooltip("Nome descritivo para este estado que aparecerá no log. Ex: Attack1_Side_State")]
    public string stateNameForLog;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}