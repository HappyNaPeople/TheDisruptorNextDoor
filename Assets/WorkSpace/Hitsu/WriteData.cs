using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;

public class WriteData
{
    private readonly string mainFolderName = $"GameData";
    private readonly string fileName = $"GameData_{DateTime.Today:yyyy-MM-dd}.xlsx";
    private string filePath;
    private FileInfo gameDatafile;
    public WriteData()
    {
        string folder = Path.Combine(Application.persistentDataPath, mainFolderName);
        string source = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        filePath = Path.Combine(folder, fileName);

        gameDatafile = new FileInfo(filePath);

        Debug.Log("•Û‘¶˜HŒa: " + filePath);
    }

}


