namespace DocumentScanningBlankApp.Events;

using iText.Kernel.Pdf;
using System.IO;
using System.Linq;

using PdfDocument = iText.Kernel.Pdf.PdfDocument;

public class MergeFilesTask
{
    public static string FilePath { get; set; } = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["MergedDirectory"] is not null ? (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["MergedDirectory"] : @"M:\Scanned\MergedFiles";

    public static void MergeFiles(ScannedDocumentModel file)
    {
        var filePath = $@"{MergeFilesTask.FilePath}\{file.ExtensionlessFileName} (merged).pdf";
        var mergedPdf = new PdfDocument(new PdfWriter(filePath));
        var allFiles = file.Children.ToList();
        allFiles.Add(file);
        foreach (var oldDocument in allFiles)
        {
            var fileInfo = new FileInfo(oldDocument.FullPath);
            var pdfDoc = new PdfDocument(new PdfReader(oldDocument.FullPath));

            pdfDoc.CopyPagesTo(1, pdfDoc.GetNumberOfPages(), mergedPdf);
            pdfDoc.Close();
        }
        mergedPdf.Close();
    }
}