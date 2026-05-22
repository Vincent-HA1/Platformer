using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class SaveSlot : MonoBehaviour
{
    public Action<SaveSlot> SaveSlotClicked;
    [Header("UI References")]
    [SerializeField] Image playerIcon;
    [SerializeField] TextMeshProUGUI stagesClearedText;
    [SerializeField] TextMeshProUGUI bigCoinsFoundText;
    [SerializeField] GameObject blankSaveIcon;
    [SerializeField] GameObject stagesClearedElement;
    [SerializeField] GameObject bigCoinsFoundElement;

    Button saveSlotButton;

    private void Awake()
    {
        saveSlotButton = GetComponent<Button>();
        saveSlotButton.onClick.AddListener(ClickSaveSlot);
    }

    void ClickSaveSlot()
    {
        SaveSlotClicked?.Invoke(this);
    }

    public void InitialiseSaveSlot(int slotIndex)
    {
        SaveData saveData = SaveSystem.Load(slotIndex);
        if (saveData != null)
        {
            ShowSaveInformation(saveData);
        }
    }

    void ShowSaveInformation(SaveData saveData)
    {
        blankSaveIcon.SetActive(false); //Hide the blank data icon
        //Find the save information
        int bigCoinsFound = saveData.stagesSaved.Sum(stage => stage.bigCoinsFound.Count(bigCoin => bigCoin == 1));
        string furthestStageCleared = saveData.stagesSaved.Count > 0 ? saveData.stagesSaved[saveData.stagesSaved.Count - 1].stageName : "-";
        //Set the UI elements
        bigCoinsFoundText.text = $"{bigCoinsFound.ToString()}/{SaveSystem.GetBigCoinsTotal().ToString()}";
        stagesClearedText.text = furthestStageCleared;
        stagesClearedElement.SetActive(true);
        bigCoinsFoundElement.SetActive(true);
        playerIcon.gameObject.SetActive(true);
    }

}
