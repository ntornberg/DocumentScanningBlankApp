using System.Collections.ObjectModel;

namespace DocumentScanningBlankApp.Data;

using System.IO;

using DocumentScanningBlankApp.StupidHacks;

public class ScannedFileData
{
    public static AddOnlyObservableCollection<FileInfo> IncomingFiles { get; set; }
}