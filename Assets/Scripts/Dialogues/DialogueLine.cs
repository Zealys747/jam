using UnityEngine;

[System.Serializable]

public class DialogueLine
{
    public string CharacterName;
    public string CharacterLine;
    public Sprite sprite;

    public float delayBefore = 0f;
    public float typewriterSpeed = 40f;
    public float autoAdvanceDelay = 2f;

}
