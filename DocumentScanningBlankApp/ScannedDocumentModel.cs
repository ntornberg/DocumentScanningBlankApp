using System;
using System.Collections.Generic;

namespace DocumentScanningBlankApp;

using iText.Kernel.Pdf;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

public class ScannedDocumentModel
{
    public ScannedDocumentModel(FileInfo file, bool isParent)
    {
        using (var document = new PdfDocument(new PdfReader(file.FullName)))
        {
            FileName = file.Name;
            FileSize = file.Length;
            ScannedDate = file.CreationTime;
            Children = new ObservableCollection<ScannedDocumentModel>();
            IsParent = isParent;
            FileDirectory = file.DirectoryName;
            ExtensionlessFileName = Path.GetFileNameWithoutExtension(file.FullName);
            PageCount = document.GetNumberOfPages();
            FullPath = file.FullName;
        }
    }
    private int _pageCount;
    private double _fileSize;
    public bool IsParent
    {
        get; set;
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
        get => this.IsParent ? Children.Select(child => child._pageCount).Sum() + this._pageCount : this._pageCount;
        set => this._pageCount = value;
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
        get => this.IsParent ? Children.Select(child => child._fileSize).Sum() + this._fileSize : this._fileSize;
        set => this._fileSize = value * 0.000001;
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


    public ICollection<ScannedDocumentModel> Children
    {
        get; set;
    }
}