using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    private bool inventoryOpen = false;
    public GameObject suspectPanel;
    public GameObject suspectTab;
    public GameObject proofTab;
    public bool InventoryOpen => inventoryOpen;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }
    }
    private void OpenInventory()
    {
        ChangeCursorState(false);
        inventoryOpen = true;
        suspectPanel.SetActive(true);
    }

    private void CloseInventory()
    {
        ChangeCursorState(true);
        inventoryOpen = false;
        suspectPanel.SetActive(false);
    }

    public void OnProofTabClicked()
    {
        proofTab.SetActive(true);
        suspectTab.SetActive(false);
    }

    public void OnSuspectsClicked()
    {
        suspectTab.SetActive(false);
        suspectTab.SetActive(true);
    }

    private void ChangeCursorState(bool lockCursor)
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }
}
