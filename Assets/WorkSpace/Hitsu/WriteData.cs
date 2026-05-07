using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections;

using System.IO;
using UnityEngine;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using static UnityEngine.Rendering.ReloadAttribute;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;

enum GameSaveIn
{
    Null, GameTitle, InGame, Release, Error
}

struct Pair
{
    public int row {  get; private set; }
    public int col { get; private set; }

    public Pair(int tX, int tY)
    {
        row = tX;
        col = tY;
    }
}

static class DataCell
{
    // Game Start
    public static readonly Pair gameSaveIn = new Pair(0, 7);

    // GameTitle End
    public static readonly Pair player01_trapStart = new Pair(4, 1);
    public static readonly Pair player01_trapEnd = new Pair(5, 4);

    public static readonly Pair player02_trapStart = new Pair(12, 1);
    public static readonly Pair player02_trapEnd = new Pair(13, 4);

    // GameSet
    public static readonly Pair player01_runRecord = new Pair(7, 1);
    public static readonly Pair player01_timeRecord = new Pair(8, 1);

    public static readonly Pair player02_runRecord = new Pair(15, 1);
    public static readonly Pair player02_timeRecord = new Pair(16, 1);

    // Release Init
    public static readonly Pair player01_record = new Pair(2, 1);
    public static readonly Pair player02_record = new Pair(10, 1);

    // Release End
    public static readonly Pair timer_min = new Pair(0, 1);
    public static readonly Pair timer_sec = new Pair(0, 3);


}

public class WriteData : MonoBehaviour
{
    [Header("Data path")]
    //private const string mainFolderName = "GameData";
    //private string fileName = $"GameData_{DateTime.Today:yyyy-MM-dd}.xlsx";
    //private string filePath;
    //private FileInfo gameDatafile;
    //private int gameCount;
    //private string sheetName => $"Game_{gameCount:D3}";

    private const string folderPath = "GameData";
    private const string dataBasic = "GameData_000.csv";

    private int gameCount = 1;
    private string gameDataName => $"Game_{gameCount:D3}.csv";
    private string filePath;


    [Header("Timer")]
    private const float timerStart = 0.0f;
    private float timer = timerStart;

    //public void WriteDataInit()
    //{


    //    //string folder = Path.Combine(Application.persistentDataPath, mainFolderName);

    //    //string source = Path.Combine(Application.streamingAssetsPath, mainFolderName, "GameData_Basic.xlsx");

    //    //if (!Directory.Exists(folder))
    //    //{
    //    //    Directory.CreateDirectory(folder);
    //    //}
    //    //filePath = Path.Combine(folder, fileName);
    //    ////gameDatafile = new FileInfo(filePath);
    //    ////if (!File.Exists(filePath))
    //    ////{
    //    ////    File.Copy(source, filePath);
    //    ////}

    //    //gameDatafile = new FileInfo(filePath);
    //    //if (!File.Exists(filePath))
    //    //{
    //    //    FileInfo sourceFile = new FileInfo(source);

    //    //    using (ExcelPackage package = new ExcelPackage(sourceFile))
    //    //    {
    //    //        package.SaveAs(gameDatafile);
    //    //    }

    //    //    using (ExcelPackage package = new ExcelPackage(gameDatafile))
    //    //    {
    //    //        package.Save();

    //    //    }

    //    //}
    //}

    public void WriteDataInit()
    {
        string folder = Path.Combine(Application.persistentDataPath, folderPath);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string source = Path.Combine(Application.streamingAssetsPath, folderPath, "GameData_000.csv");
        if (!File.Exists(source))
        {
            Debug.LogError("No GameData Files");

            return;
        }

        Debug.Log("CSV Found");

        while (gameCount < 999)
        {
            string target = Path.Combine(folder, gameDataName);
            if (File.Exists(target)) gameCount++;
            else
            {
                File.Copy(source, target);
                Debug.Log("CSV Copied");

                filePath = target;
                break;
            }

        }

    }
    private Coroutine timerProcess;

    private IEnumerator TimerProcess()
    {
        while (true)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    private void TimerStart()
    {
        timer = timerStart;
        if (timerProcess != null) StopCoroutine(timerProcess);
        timerProcess = null;
        timerProcess = StartCoroutine(TimerProcess());
    }

    //public void WriteInGameTitleInit()
    //{
    //    //using (ExcelPackage package = new ExcelPackage(gameDatafile))
    //    //{
    //    //    gameCount = package.Workbook.Worksheets.Count;

    //    //    if (gameCount == 1)
    //    //    {
    //    //        var sheet = package.Workbook.Worksheets.Copy("Game_000", sheetName);
    //    //        package.Save();
    //    //        return;

    //    //    }

    //    //    var startSheet = package.Workbook.Worksheets[$"Game_{(gameCount - 1):D3}"];
    //    //    if (startSheet == null)
    //    //    {
    //    //        Debug.LogWarning("Not Target Sheet, check the data file");
    //    //    }

    //    //    if (startSheet.Cells[DataCell.gameSaveIn.row, DataCell.gameSaveIn.col].Text == GameSaveIn.Null.ToString())
    //    //    {
    //    //        gameCount--; 
    //    //    }
    //    //    else
    //    //    {
    //    //        var sheet = package.Workbook.Worksheets.Copy("Game_000", sheetName);
    //    //    }

    //    //    package.Save();

    //    //}

    //    //TimerStart();
    //    //Debug.Log(timer);

    //}

    private void LoadCSV(string path, out string[,] targetCsv)
    {
        string[] lines = File.ReadAllLines(path);

        int rowCount = lines.Length;
        int colCount = 0;

        foreach (string line in lines)
        {
            int cols = line.Split(',').Length;

            if (cols > colCount)
            {
                colCount = cols;
            }
        }

        string[,] data = new string[rowCount, colCount];

        for (int row = 0; row < rowCount; row++)
        {
            string[] cols = lines[row].Split(',');

            for (int col = 0; col < cols.Length; col++)
            {
                data[row, col] = cols[col];
            }
        }

        targetCsv = data;
    }

    private void SaveCSV(string[,] csvData)
    {
        List<string> lines = new List<string>();

        // 行
        for (int row = 0; row < csvData.GetLength(0); row++)
        {
            List<string> cols = new List<string>();

            // 列
            for (int col = 0; col < csvData.GetLength(1); col++)
            {
                string value = csvData[row, col];

                // null 防止
                if (string.IsNullOrEmpty(value))
                {
                    value = "";
                }

                cols.Add(value);
            }

            // 用逗号连接一整行
            lines.Add(string.Join(",", cols));
        }

        // 写入文件
        File.WriteAllLines(filePath, lines);

        Debug.Log("CSV Saved : " + filePath);
    }


    public void WriteInGameTitleInit()
    {
        TimerStart();
        Debug.Log("Time Start");
    }
    public void WriteInGameTitleEnd()
    {
        List<TrapName> player01TrapNames = GameManager.Instance.player01.hunter.backpack.trapsPack;
        List<TrapName> player02TrapNames = GameManager.Instance.player02.hunter.backpack.trapsPack;

        LoadCSV(filePath, out string[,] csvData);

        csvData[DataCell.gameSaveIn.row,DataCell.gameSaveIn.col] = GameSaveIn.GameTitle.ToString();

        int index = 0;
        for (int row = DataCell.player01_trapStart.row; row <= DataCell.player01_trapEnd.row &&
            index < player01TrapNames.Count; row++)
        {
            for (int col = DataCell.player01_trapStart.col; col <= DataCell.player01_trapEnd.col &&
                 index < player01TrapNames.Count;col++)
            {
                csvData[row, col] = player01TrapNames[index].ToString();
                index++;
            }
        }

        index = 0;

        for (int row = DataCell.player02_trapStart.row;row <= DataCell.player02_trapEnd.row &&
             index < player02TrapNames.Count;row++)
        {
            for (int col = DataCell.player02_trapStart.col;col <= DataCell.player02_trapEnd.col &&
                 index < player02TrapNames.Count;col++)
            {
                csvData[row, col] = player02TrapNames[index].ToString();

                index++;
            }
        }

        SaveCSV(csvData);
    }

    public void WriteInGameSet()
    {
        PlayerData player01Data = GameManager.Instance.player01.playerData;
        PlayerData player02Data = GameManager.Instance.player02.playerData;

        LoadCSV(filePath, out string[,] csvData);
        csvData[DataCell.gameSaveIn.row, DataCell.gameSaveIn.col] = GameSaveIn.InGame.ToString();

        csvData[DataCell.player01_runRecord.row, DataCell.player01_runRecord.col] = player01Data.passDistance.ToString("F2");
        csvData[DataCell.player01_timeRecord.row, DataCell.player01_timeRecord.col] = player01Data.passTime.ToString("F2");

        csvData[DataCell.player02_runRecord.row, DataCell.player02_runRecord.col] = player01Data.passDistance.ToString("F2");
        csvData[DataCell.player02_timeRecord.row, DataCell.player02_timeRecord.col] = player01Data.passTime.ToString("F2");

        SaveCSV(csvData);


        //using (ExcelPackage package = new ExcelPackage(gameDatafile))
        //{
        //    var startSheet = package.Workbook.Worksheets[sheetName];
        //    startSheet.Cells[DataCell.gameSaveIn.row, DataCell.gameSaveIn.col].Value = GameSaveIn.InGame.ToString();

        //    startSheet.Cells[DataCell.player01_runRecord.row, DataCell.player01_runRecord.col].Value = player01Data.passDistance.ToString("0.00");
        //    startSheet.Cells[DataCell.player01_timeRecord.row, DataCell.player01_timeRecord.col].Value = player01Data.passTime.ToString("0.00");
        //    startSheet.Cells[DataCell.player02_runRecord.row, DataCell.player02_runRecord.col].Value = player02Data.passDistance.ToString("0.00");
        //    startSheet.Cells[DataCell.player02_timeRecord.row, DataCell.player02_timeRecord.col].Value = player01Data.passTime.ToString("0.00");

        //    package.Save();
        //}
        //Debug.Log(timer);

    }

    public void WriteInReleaseInit(Winner winner)
    {
        string player01Record = winner == Winner.Player01 ? "Winner" : "Loser";
        string player02Record = winner == Winner.Player02 ? "Winner" : "Loser";

        LoadCSV(filePath, out string[,] csvData);

        csvData[DataCell.gameSaveIn.row, DataCell.gameSaveIn.col] = GameSaveIn.Release.ToString();

        csvData[DataCell.player01_record.row, DataCell.player01_record.col] = player01Record;
        csvData[DataCell.player02_record.row, DataCell.player02_record.col] = player02Record;

        SaveCSV(csvData);


        //using (ExcelPackage package = new ExcelPackage(gameDatafile))
        //{
        //    var startSheet = package.Workbook.Worksheets[sheetName];
        //    string player01Record = winner == Winner.Player01 ? "Winner" : "Loser";
        //    string player02Record = winner == Winner.Player02 ? "Winner" : "Loser";

        //    startSheet.Cells[DataCell.player01_record.row, DataCell.player01_record.col].Value = player01Record;
        //    startSheet.Cells[DataCell.player02_record.row, DataCell.player02_record.col].Value = player02Record;

        //    package.Save();

        //}
        //Debug.Log(timer);

    }
    public void WriteInReleaseEnd()
    {

        int totalSeconds = Mathf.FloorToInt(timer);

        int min = totalSeconds / 60;
        int sec = totalSeconds % 60;

        LoadCSV(filePath, out string[,] csvData);

        csvData[DataCell.timer_min.row, DataCell.timer_min.col] = min.ToString();
        csvData[DataCell.timer_sec.row, DataCell.timer_sec.col] = sec.ToString();



        SaveCSV(csvData);

        //using (ExcelPackage package = new ExcelPackage(gameDatafile))
        //{
        //    var startSheet = package.Workbook.Worksheets[sheetName];

        //    startSheet.Cells[DataCell.timer_min.row, DataCell.timer_min.col].Value = min;
        //    startSheet.Cells[DataCell.timer_sec.row, DataCell.timer_sec.col].Value = sec;

        //    package.Save();
        //}

        //Debug.Log(timer);
    }

}


