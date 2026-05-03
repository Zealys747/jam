using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}
