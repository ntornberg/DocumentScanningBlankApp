using System.IO;

namespace DocumentScanningBlankApp.Events;

using System;
using System.Collections.Generic;

using DocumentScanningBlankApp.Data;

public class GetHistoricalDataTask
{
    private static List<FileInfo> files = new();

    private static List<(string date, double fileSize)> fileData = new();

    public GetHistoricalDataTask()
    {
        var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
        var path = settings.Values["SortedFilesPath"] as string;
        if (path == null)
        {
            path = @"M:\Sorted";
        }

        SearchDirs(new DirectoryInfo(path));

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
    }
}