using System.IO;

namespace DocumentScanningBlankApp.Events;

using System;
using System.Collections.Generic;
using System.Linq;

using DocumentScanningBlankApp.Data;
using iText.Commons.Utils;

public class GetHistoricalDataTask
{
    private static List<FileInfo> files = new();

    private static List<(string date, double fileSize)> fileData = new();
    private static string filePath = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["SortedFilesPath"] is not null ? (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["SortedFilesPath"] : @"M:\Sorted"; //TODO: Write this better man

    public static string FilePath
    {
        get => filePath;
        set
        {
            filePath = value;
            GetHistoricalData();
        }
    }

    public GetHistoricalDataTask()
    {
        GetHistoricalData();

    }

    private static void GetHistoricalData()
    {
        files.Clear();
        var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
        

        SearchDirs(new DirectoryInfo(filePath));

        foreach (var fileItem in files)
        {
            var date = fileItem.LastWriteTime.ToShortDateString();
            var fileSize = fileItem.Length / (1000000.0);
            fileData.Add((date, fileSize));
        }

        AddHistoricalData();
    }

    private static void SearchDirs(DirectoryInfo root)
    {
        foreach (var file in root.GetFiles())
        {

            files.Add(file);
        }

        // Recursively print files from all subdirectories
        foreach (var directory in root.GetDirectories())
        {
            SearchDirs(directory);
        }
    }

    private static void AddHistoricalData()
    {
        foreach (var file in fileData)
        {
            if (ScannedFileData.PreviouslyScannedFiles.ContainsKey(file.Item1))
            {
                ScannedFileData.PreviouslyScannedFiles[file.Item1] += file.Item2;
            }
            else
            {
                ScannedFileData.PreviouslyScannedFiles[file.Item1] = file.Item2;
            }

        }
        ScannedFileData.PreviouslyScannedFiles = ScannedFileData.PreviouslyScannedFiles.OrderBy(x => DateTime.Parse(x.Key)).ToDictionary(x => x.Key, x => x.Value);
    }
}