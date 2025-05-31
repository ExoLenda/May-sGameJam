using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuspectScript : MonoBehaviour
{
    public Image icon;

    // Her SuspectScript'in referans vereceði bir baþka SuspectScript
    public SuspectScript Suspect { get; private set; }

    public void AddSuspect(SuspectScript newSuspect)
    {
        Suspect = newSuspect;
        if (newSuspect != null && newSuspect.icon != null)
        {
            icon.sprite = newSuspect.icon.sprite;
        }
    }
}
