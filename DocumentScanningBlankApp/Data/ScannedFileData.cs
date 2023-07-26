namespace DocumentScanningBlankApp.Data;

using DocumentScanningBlankApp.StupidHacks;
using System.Collections.Generic;
using System.IO;

public class ScannedFileData
{
    public static AddOnlyObservableCollection<FileInfo> IncomingFiles { get; set; }
    public static Dictionary<string, double> PreviouslyScannedFiles { get; set; } = new Dictionary<string, double>();

}