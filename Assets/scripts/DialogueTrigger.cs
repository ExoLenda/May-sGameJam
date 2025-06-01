using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueCharachter
{
    public string name;
    public Sprite icon;
}


[System.Serializable]
public class DialogueLine
{
    public DialogueCharachter charachter;
    [TextArea(3,10)]
    public string line;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
}
