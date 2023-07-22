namespace DocumentScanningBlankApp.Events;

using System.IO;
using System.Reflection.PortableExecutable;
using Windows.Data.Pdf;
using Windows.UI.StartScreen;
using iText.Kernel.Pdf;
using iText.Layout;

using PdfDocument = iText.Kernel.Pdf.PdfDocument;

public class MergeFilesTask
{
    public void MergeFiles(ScannedDocumentModel file)
    {
        var filePath = $@"{AppSettings.MergedDirectoryPath}\{file.ExtensionlessFileName} (merged).pdf";
        var mergedPdf = new PdfDocument(new PdfWriter(filePath));
        foreach (var oldDocument in file.Children)
        {
            var fileInfo = new FileInfo(oldDocument.FullPath);

            using (var pdfDoc = new PdfDocument(new PdfReader(oldDocument.FullPath)))
            {
                pdfDoc.CopyPagesTo(1, pdfDoc.GetNumberOfPages(), mergedPdf);
                pdfDoc.Close();
            }
               
            
            

        }
    }
}