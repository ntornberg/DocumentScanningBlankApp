namespace DocumentScanningBlankApp.Events;

using DocumentScanningBlankApp.Data;
using DocumentScanningBlankApp.StupidHacks;
using System.IO;

public class DocumentScannedEvent
{
    private FileSystemWatcher watcher;

    public DocumentScannedEvent()
    {
        watcher = new FileSystemWatcher();
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.FileName; 
        watcher.Path = @"M:\Scanned\East";
        watcher.Created += OnFileCreated;
        watcher.Changed += OnFileCreated;
        watcher.EnableRaisingEvents = true;
        ScannedFileData.IncomingFiles ??= new AddOnlyObservableCollection<FileInfo>();
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Created)
        {
            return;
        }

        ScannedFileData.IncomingFiles.Add(new FileInfo(e.FullPath));
    }
}
