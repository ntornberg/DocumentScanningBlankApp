namespace DocumentScanningBlankApp.Events;

using DocumentScanningBlankApp.Data;
using System.Collections.ObjectModel;
using System.IO;

using DocumentScanningBlankApp.StupidHacks;

public class DocumentScannedEvent
{
    private FileSystemWatcher watcher;

    public DocumentScannedEvent()
    {
        watcher = new FileSystemWatcher();
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.FileName;
        watcher.Path = @"C:\Users\ntornberg\OneDrive - Metal Exchange Corporation\Documents\newdocs\Scanned\East\Need_To_Name";
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
