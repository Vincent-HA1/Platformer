using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class SaveSlotManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] List<SaveSlot> saveSlots;
    [SerializeField] GameObject saveSlotsScreen;
    [SerializeField] Button startButton;

    public Action<int> LoadSaveSlot;

    InputHandler inputHandler;

    // Start is called before the first frame update
    void Start()
    {
        inputHandler = FindObjectOfType<InputHandler>();
        startButton.onClick.AddListener(OpenSaveSlots);
        BindEvents();
    }

    void BindEvents()
    {
        for (int i = 0; i < saveSlots.Count; i++)
        {
            saveSlots[i].SaveSlotClicked += SaveSlotSelected;
            saveSlots[i].SaveSlotDeleted += SaveSlotDeleted;
            saveSlots[i].InitialiseSaveSlot(i);
        }
    }

    void SaveSlotSelected(SaveSlot saveSlotClicked)
    {
        int saveSlotIndex = saveSlots.IndexOf(saveSlotClicked);
        LoadSaveSlot?.Invoke(saveSlotIndex);
    }

    void SaveSlotDeleted(SaveSlot saveSlotDeleted)
    {
        int saveSlotIndex = saveSlots.IndexOf(saveSlotDeleted);
        SaveSystem.DeleteSave(saveSlotIndex);
        //Reinitialise the UI
        saveSlotDeleted.InitialiseSaveSlot(saveSlotIndex);
    }


    void OpenSaveSlots()
    {
        saveSlotsScreen.SetActive(true);
        EventSystem.current.SetSelectedGameObject(saveSlots[0].gameObject);
    }

    void CloseSaveSlots()
    {
        saveSlotsScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (saveSlotsScreen.activeInHierarchy && inputHandler.cancelPressed)
        {
            CloseSaveSlots();
        }
    }
}
