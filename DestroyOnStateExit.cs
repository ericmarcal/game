// DestroyOnStateExit.cs
using UnityEngine;

public class DestroyOnStateExit : StateMachineBehaviour
{
    // OnStateEnter é chamado quando a animação deste estado começa a tocar
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Agenda a destruição do GameObject do Animator
        // O tempo de delay é o comprimento exato da animação que está começando
        Destroy(animator.gameObject, stateInfo.length);
    }
}