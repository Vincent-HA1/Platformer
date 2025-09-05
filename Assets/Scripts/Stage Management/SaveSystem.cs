using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/* Static class used to handle saving the game */
public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/savefile.json";

    /* Save the stage to the save file */
    public static void Save(StageSave stageSave)
    {
        SaveData data = Load();
        //If there are no stages saved, initialise the list of saves
        if(data == null)
        {
            data = new SaveData(new List<StageSave>());
        }
        //add the stage save
        int listIndex = data.stagesSaved.FindIndex(s => s.stageName == stageSave.stageName);
        if(listIndex != -1)
        {
            //If the stage save is already there, then overwrite it.
            data.stagesSaved[listIndex] = stageSave;
        }
        else
        {
            //add it
            data.stagesSaved.Add(stageSave);
        }
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static SaveData Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null; // No save file found
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save deleted");
        }
    }
}
[Serializable]
public class SaveData
{
    public List<StageSave> stagesSaved;

    public SaveData(List<StageSave> stagesSaved)
    {
        this.stagesSaved = stagesSaved;
    }
}