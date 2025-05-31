using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuspectScript : MonoBehaviour
{
    public Image icon;
    public void AddSuspect(SuspectScript newSuspect)
    {
        Suspect = newSuspect;
        SuspectScript.sprite = newSuspect.icon;
    }

}
