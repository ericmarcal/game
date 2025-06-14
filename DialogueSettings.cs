using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Dialogue", menuName = "New Dialogue/Dialogue")]
public class DialogueSettings : ScriptableObject
{
    [Header("Editor Helper")]
    [Tooltip("Sprite do personagem que está falando.")]
    public Sprite speakerSprite;
    [Tooltip("A frase a ser adicionada.")]
    [TextArea]
    public string sentence;

    [Header("Dialogue Data")]
    public List<Sentences> dialogues = new List<Sentences>();
}

[System.Serializable]
public class Sentences
{
    public string actorName;
    public Sprite profile;
    public Languages sentence;
}

[System.Serializable]
public class Languages
{
    public string portuguese;
    public string english;
    public string spanish;
}

#if UNITY_EDITOR
[CustomEditor(typeof(DialogueSettings))]
public class BuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenha todos os campos que não são customizados primeiro
        DrawDefaultInspector();

        // Pega uma referência para o objeto que estamos editando
        DialogueSettings ds = (DialogueSettings)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Custom Dialogue Builder", EditorStyles.boldLabel);

        // O código do botão que estava causando o problema foi reescrito para ser mais seguro
        if (GUILayout.Button("Add Current Sentence to Dialogue List"))
        {
            if (!string.IsNullOrEmpty(ds.sentence))
            {
                // Prepara a nova sentença
                Languages lang = new Languages();
                lang.portuguese = ds.sentence;

                Sentences s = new Sentences();
                s.profile = ds.speakerSprite;
                s.sentence = lang;
                s.actorName = (ds.speakerSprite != null) ? ds.speakerSprite.name : "";

                // Permite que a ação seja desfeita com Ctrl+Z
                Undo.RecordObject(ds, "Add Dialogue Sentence");

                // Adiciona a sentença à lista
                ds.dialogues.Add(s);

                // Marca o objeto como "sujo", informando a Unity que ele precisa ser salvo
                EditorUtility.SetDirty(ds);

                // Limpa os campos do helper para a próxima adição
                ds.sentence = "";
                ds.speakerSprite = null;

                // Remove o foco da text area para evitar bugs de input
                GUI.FocusControl(null);
            }
            else
            {
                Debug.LogWarning("Cannot add an empty sentence.");
            }
        }
    }
}
#endif