using System.IO;
using System.Threading.Tasks;

namespace DocumentScanningBlankApp.Events;

using DocumentScanningBlankApp.Data;
using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;

public class GetHistoricalDataTask
{
    private static List<FileInfo> files = new();

    private static List<(string date, double fileSize)> fileData = new();

    private static string filePath =
        (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["SortedFilesPath"] is not null
            ? (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["SortedFilesPath"]
            : @"M:\Sorted"; //TODO:  C:\Users\ntornberg\OneDrive - Metal Exchange Corporation\Documents\newdocs\Sorted

    public static string FilePath
    {
        get => filePath;
        set
        {
            filePath = value;
            GetHistoricalData();
        }
    }


    public async Task Run()
    {
        await GetHistoricalData();
    }

    private static async Task GetHistoricalData()
    {
        ScannedFileData.PreviouslyScannedFiles.Clear();
        AppSettings.totalPageCount = 0;
        await ProcessDirectory(new DirectoryInfo(filePath));
    }

    private static async Task ProcessDirectory(DirectoryInfo root)
    {
        Parallel.ForEach(root.GetFiles(), file => // Using parallel processing here
        {
            var date = file.LastWriteTime.ToShortDateString();
            var fileSize = file.Length / (1000000.0);

            using (var document = new PdfDocument(new PdfReader(file.FullName)))
            {
                AppSettings.totalPageCount += document.GetNumberOfPages();
            }

            lock (ScannedFileData.PreviouslyScannedFiles) // Lock the shared resource
            {
                if (ScannedFileData.PreviouslyScannedFiles.ContainsKey(date))
                {
                    ScannedFileData.PreviouslyScannedFiles[date] += fileSize;
                }
                else
                {
                    ScannedFileData.PreviouslyScannedFiles[date] = fileSize;
                }
            }
        });

        // Recursively process all subdirectories
        foreach (var directory in root.GetDirectories())
        {
            ProcessDirectory(directory);
        }

        // Order the dictionary once after processing
        ScannedFileData.PreviouslyScannedFiles = ScannedFileData.PreviouslyScannedFiles.OrderBy(x => DateTime.Parse(x.Key)).ToDictionary(x => x.Key, x => x.Value);
    }

}