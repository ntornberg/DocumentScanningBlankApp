namespace DocumentScanningBlankApp.Events;

using DocumentScanningBlankApp.Data;
using DocumentScanningBlankApp.StupidHacks;
using System.IO;

public class DocumentScannedEvent
{
    private static FileSystemWatcher watcher;

    private static string filePath = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["OutputDirectory"] is not null ? (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["OutputDirectory"]: @"M:\Scanned\East";

    public static string FilePath
    {
        get => filePath;
        set{
            filePath = value;
            watcher.Path = value;
        }
    }

    public DocumentScannedEvent()
    {
        watcher = new FileSystemWatcher();
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.FileName; 
        watcher.Path = FilePath;
        watcher.Created += OnFileCreated;
        watcher.Changed += OnFileCreated;
        watcher.EnableRaisingEvents = true;
        ScannedFileData.IncomingFiles ??= new AddOnlyObservableCollection<FileInfo>();
    }

    private static void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Created)
        {
            return;
        }

        ScannedFileData.IncomingFiles.Add(new FileInfo(e.FullPath));
    }
}
