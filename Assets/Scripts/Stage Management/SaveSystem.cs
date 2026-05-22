using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/* Static class used to handle saving the game */
public static class SaveSystem
{
    public static int currentSaveSlotUsed;
    private static int bigCoinsTotal = 17;
    //private static string savePath = Application.persistentDataPath + "/savefile.json";

    /* Save the stage to the save file */
    public static void Save(StageSave stageSave)
    {
        SaveData data = Load(currentSaveSlotUsed);
        string savePath = GetSavePath(currentSaveSlotUsed);
        //If there are no stages saved, initialise the list of saves
        if (data == null)
        {
            data = new SaveData(new List<StageSave>(), 0, 0);
        }
        //Add the new stage save (from the stage just cleared)
        int listIndex = data.stagesSaved.FindIndex(s => s.stageName == stageSave.stageName);
        if (listIndex != -1)
        {
            //If the stage save is already there, then overwrite it.
            data.stagesSaved[listIndex] = stageSave;
        }
        else
        {
            //Otherwise, add it as a new stage
            data.stagesSaved.Add(stageSave);
        }
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static void SaveLastStageEntered(int lastStageEntered, int lastWorldEntered)
    {
        SaveData data = Load(currentSaveSlotUsed);
        string savePath = GetSavePath(currentSaveSlotUsed);
        //If there are no stages saved, initialise the list of saves
        if (data == null)
        {
            data = new SaveData(new List<StageSave>(), 0, 0);
        }
        data.lastStageEntered = lastStageEntered;
        data.lastWorldEntered = lastWorldEntered;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static SaveData Load(int saveSlot)
    {
        string savePath = GetSavePath(saveSlot);
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null; // No save file found
    }

    public static void DeleteSave(int saveSlot)
    {
        string savePath = GetSavePath(saveSlot);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save deleted");
        }
    }

    public static void SetCurrentSaveSlot(int saveSlot)
    {
        currentSaveSlotUsed = saveSlot;
    }

    public static int GetBigCoinsTotal()
    {
        return bigCoinsTotal;
    }

    // Change save path with the save slot index
    private static string GetSavePath(int saveSlot)
    {
        return Application.persistentDataPath + $"/savefile_{saveSlot}.json";
    }


}
[Serializable]
public class SaveData
{
    public List<StageSave> stagesSaved;
    public int lastStageEntered;
    public int lastWorldEntered;

    public SaveData(List<StageSave> stagesSaved, int lastStageEntered, int lastWorldEntered)
    {
        this.stagesSaved = stagesSaved;
        this.lastStageEntered = lastStageEntered;
        this.lastWorldEntered = lastWorldEntered;
    }
}