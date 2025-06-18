using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueControl : MonoBehaviour
{
    public enum Idioma { pt, eng, spa } // Enum corrigido

    public static DialogueControl instance;

    [Header("Configurações")]
    public Idioma language = Idioma.pt; // Propriedade adicionada

    [Header("Componentes")]
    public GameObject dialogueObj;
    public Text speechText;
    public float typingSpeed = 0.05f;

    private bool _isShowing;
    public bool isShowing => _isShowing;

    private void Awake()
    {
        instance = this;
    }

    public void Speech(string[] txt)
    {
        if (!_isShowing)
        {
            dialogueObj.SetActive(true);
            StartCoroutine(TypeSentence(txt));
            _isShowing = true;
        }
    }

    IEnumerator TypeSentence(string[] sentences)
    {
        foreach (string sentence in sentences)
        {
            speechText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                speechText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
        }
        dialogueObj.SetActive(false);
        _isShowing = false;
    }
}