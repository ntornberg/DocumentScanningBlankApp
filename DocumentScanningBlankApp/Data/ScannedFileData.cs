using System.Collections.ObjectModel;

namespace DocumentScanningBlankApp.Data;

public class ScannedFileData
{
    public static ObservableCollection<ScannedDocumentModel> RootNodes { get; set; }
}