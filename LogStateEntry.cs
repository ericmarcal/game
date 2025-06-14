using UnityEngine;

public class LogStateEntry : StateMachineBehaviour
{
    [Tooltip("Nome descritivo para este estado que aparecerá no log. Ex: Attack1_Side_State")]
    public string stateNameForLog;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Removido: Debug.Log($"ANIMATOR ENTROU NO ESTADO: >>> {stateNameForLog} <<<");
        // Removido: Debug.Log($"ANIMATOR ENTROU NO ESTADO hash: {stateInfo.fullPathHash}");
    }

    // override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //    // Removido
    // }
}