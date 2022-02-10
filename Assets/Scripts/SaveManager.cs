using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager
{

    public static void UseSaveState(int slot)
    {
        string path = Application.dataPath + string.Format("/../saveSlot{0}.sav", slot);

        StreamReader reader = new StreamReader(path, true);

        reader.Close();
    }

    public static void ReadSaveState(int slot)
    {
        string path = Application.dataPath + string.Format("/../saveSlot{0}.sav", slot);

        if (!File.Exists(path)) { return; }
        StreamReader reader = new StreamReader(path, true);

        reader.Close();
    }

    public static void CreateSaveState(int slot)
    {
        string path = Application.dataPath + string.Format("/../saveSlot{0}.sav", slot);

        StreamWriter writer = new StreamWriter(path, false);

        writer.Close();
    }
}
