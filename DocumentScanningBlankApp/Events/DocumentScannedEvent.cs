using System.Diagnostics;

namespace DocumentScanningBlankApp.Events;

using DocumentScanningBlankApp.Data;
using DocumentScanningBlankApp.StupidHacks;
using System.IO;

public class DocumentScannedEvent
{
    private static FileSystemWatcher watcher;   

    private static string _filePath =
        (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["OutputDirectory"] is not null
            ? (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["OutputDirectory"]
            : @"M:\Scanned\East"; //TODO: rename to  C:\Users\ntornberg\OneDrive - Metal Exchange Corporation\Documents\newdocs\Scanned\East

    public static string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            watcher.Path = value;
        }
    }

    public DocumentScannedEvent()
    {
        watcher = new FileSystemWatcher();
        watcher.IncludeSubdirectories = false;
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName;;
        watcher.Path = FilePath;
     
        watcher.EnableRaisingEvents = true;
        watcher.Created += OnFileCreated;
        watcher.Changed += OnFileCreated;
        watcher.Error += WatcherOnError;
        ScannedFileData.IncomingFiles ??= new AddOnlyObservableCollection<FileInfo>();
    }

    private void WatcherOnError(object sender, ErrorEventArgs e)
    {
        Debug.Fail(e.ToString());
    }

    private static void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Created)
        {
            return;
        }

        var tempPath = Path.GetTempPath();
        
        var cacheDirectory = Path.Combine(tempPath, "ScannedDocumentAppCache");
        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }
        var tempFilePath = Path.Combine(cacheDirectory, e.Name);
        var maxAttempts = 5;
        for (int attempts = 0; attempts <= maxAttempts; attempts++)
        {
            try
            {
                File.Copy(e.FullPath, tempFilePath);
                break;
            }
            catch (IOException)
            {
                if (attempts == maxAttempts)
                {
                    throw;
                }
                System.Threading.Thread.Sleep(200);
            }
        }


        ScannedFileData.IncomingFiles.Add(new FileInfo(tempFilePath));
    }
}
