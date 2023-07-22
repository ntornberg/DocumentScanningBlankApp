using System.Collections.Generic;
using System;

namespace DocumentScanningBlankApp;

using iText.Kernel.Pdf;
using System.Collections.ObjectModel;
using System.IO;

public class ScannedDocumentModel
{
    public ScannedDocumentModel(FileInfo file,bool isParent)
    {
        FileName = file.Name;
        FileSize = file.Length;
        ScannedDate = file.CreationTime;
        Children = new ObservableCollection<ScannedDocumentModel>();
        IsParent = isParent;
        FileDirectory = file.DirectoryName;
        ExtensionlessFileName = Path.GetFileNameWithoutExtension(file.FullName);
        PageCount = new PdfDocument(new PdfReader(file.FullName)).GetNumberOfPages();
        FullPath = file.FullName;
    }

    public string FullPath
    {
        get; set;
    }
    public string FileName
    {
        get; set;
    }

    public int PageCount
    {
        get; set;
    }
    public string ExtensionlessFileName
    {
        get; set;
    }

    public string FileDirectory
    {
        get; set;
    }
    public double FileSize
    {
        get; set;
    }
    public DateTime ScannedDate
    {
        get; set;
    }
    public string Status
    {
        get; set;
    }
    public string Symbol
    {
        get; set;
    }
    public string SymbolName
    {
        get; set;
    }

    public bool IsParent
    {
        get; set;
    }

    public ICollection<ScannedDocumentModel> Children
    {
        get; set;
    }
}