using Microsoft.UI.Xaml.Controls;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DocumentScanningBlankApp
{
    using DocumentScanningBlankApp.Data;
    using DocumentScanningBlankApp.Events;
    using DocumentScanningBlankApp.StupidHacks;
    using iText.Kernel.Pdf;
    using Microsoft.UI.Dispatching;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Media;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Windows.Graphics.Imaging;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Image = Microsoft.UI.Xaml.Controls.Image;
    using PdfImageViewer = Windows.Data.Pdf;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileProcessingPage : Page
    {
        private static ObservableCollection<ScannedDocumentModel> _rootNodes = new();
        private static string _parentFileName { get; set; }

        private static ScannedDocumentModel _parentNode { get; set; }

        private static int _childCount { get; set; }

        private static bool _isFileNameChanged { get; set; }

        private static string _previousFileName { get; set; }
        
        private static string _deletedFilePAth =
            (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["DeletedFilesPath"] is not null
                ? (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["DeletedFilesPath"]
                : @"M:\Deleted"; //TODO:  C:\Users\ntornberg\OneDrive - Metal Exchange Corporation\Documents\newdocs\Scanned\East\Need_To_Name\DeletedFiles


        public FileProcessingPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.treeView.ItemsSource = _rootNodes;
            this.Loaded += FileProcessingPage_Loaded;

            ScannedFileData.IncomingFiles.CollectionChanged += this.IncomingFiles_CollectionChanged;
        }

        private void FileProcessingPage_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.GetForCurrentThread().TryEnqueue(async () =>
                {
                    var scannedFiles = new AddOnlyObservableCollection<FileInfo>(ScannedFileData.IncomingFiles);
                    ScannedFileData.IncomingFiles.Clear();
                    foreach (var file in scannedFiles)
                    {
                        await this.OnFileCreated(file);
                    }
                });
        }



        private void IncomingFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            var scannedFiles = new AddOnlyObservableCollection<FileInfo>(ScannedFileData.IncomingFiles);
            ScannedFileData.IncomingFiles.Clear();


            foreach (var file in scannedFiles)
            {
                this.OnFileCreated(file);
            }

        }

        private async Task OnFileCreated(FileInfo file)
        {

            if (!await this.PromptKeepFileAsync(file).ConfigureAwait(true))
            {
                File.Move(file.FullName,$"{_deletedFilePAth}\\{file.Name}");
                return; // TODO: move file to delete folder. I should really make an abstraction for this 
            }

            using (var document = new PdfDocument(new PdfReader(file.FullName)))
            {
                AppSettings.todaysPageCount += document.GetNumberOfPages();
            }
            if (await this.PromptIsNewDocumentAsync().ConfigureAwait(true))
            {
                try
                {
                    var userInput = await this.PromptUserInputAsync(file).ConfigureAwait(true);
                    _childCount = 0;
                    _parentFileName = userInput;
                    var newPathName = Path.Combine(Path.GetDirectoryName(file.FullName), _parentFileName + ".pdf");
                    File.Move(file.FullName, newPathName);
                    _parentNode = new ScannedDocumentModel(new FileInfo(newPathName), true);
                    if (_rootNodes is null)
                    {
                        _rootNodes = new ObservableCollection<ScannedDocumentModel>();
                    }

                    DispatcherQueue.TryEnqueue(async () => { _rootNodes.Add(_parentNode); });
                    _childCount++;
                }
                catch (Exception e)
                {
                    throw new Exception("Error placing file", e);
                    Debug.Fail("Error placing file");
                }
            }
            else
            {
                try
                { //TODO: make sure files aren't the same name make this more robust
                    var newFileName = Path.Combine(
                        Path.GetDirectoryName(file.FullName),
                        _parentFileName + $" (pt {_childCount}).pdf");
                    File.Move(file.FullName, newFileName);

                    DispatcherQueue.TryEnqueue(
                        async () =>
                            {
                                var child = new FileInfo(newFileName);
                                _parentNode.FileSize += child.Length;
                                _parentNode.Children.Add(new ScannedDocumentModel(child, false));
                            });
                }
                catch (Exception e)
                {
                    throw new Exception("Error placing file", e);
                    Debug.Fail("Error placing file");
                }



                _childCount++;
            }
        }

        private async Task<ImageSource> RenderPdfPageAsync(string filePath, uint pageIndex)
        {
            var pdfFile = await StorageFile.GetFileFromPathAsync(filePath);
            var pdfDocument = await PdfImageViewer.PdfDocument.LoadFromFileAsync(pdfFile);
            var pdfPage = pdfDocument.GetPage(pageIndex);

            using var stream = new InMemoryRandomAccessStream();
            await pdfPage.RenderToStreamAsync(stream);

            var decoder = await BitmapDecoder.CreateAsync(stream);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(
                    softwareBitmap,
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied);
            }

            var bitmapSource = new Microsoft.UI.Xaml.Media.Imaging.SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(softwareBitmap);

            return bitmapSource;
        }

        private ContentDialog CreateContentDialog(FileInfo fileInfo, Image imageControl, int pageCount)
        {
            var messageText = new TextBlock
            {
                Text = $"A new file has been created. Do you want to keep it?\n\n"
                     + $"File Size: {fileInfo.Length * 0.000001} MB\n"
                     + $"Page Count: {pageCount}\n",
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(imageControl);

            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "File Action",
                Content = stackPanel,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
            };

            return dialog;
        }

        private async Task<bool> PromptKeepFileAsync(FileInfo fileInfo)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            DispatcherQueue.TryEnqueue(async () =>
            {
                var pageIndex = 0;
                var imageControl = new Image
                {
                    Source = await RenderPdfPageAsync(fileInfo.FullName, (uint)pageIndex)
                };

                var documentData = new PdfDocument(new PdfReader(fileInfo.FullName));
                var dialog = CreateContentDialog(fileInfo, imageControl, documentData.GetNumberOfPages());

                var results = await dialog.ShowAsync();
                documentData.Close();

                var result = results == ContentDialogResult.Primary;
                taskCompletionSource.SetResult(result);
            });

            return await taskCompletionSource.Task;
        }






        private async Task<bool> PromptIsNewDocumentAsync()
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            DispatcherQueue.TryEnqueue(
                async () =>
                    {
                        var dialog = new ContentDialog
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "File Action",
                            Content = "Is this a new Folder?",
                            PrimaryButtonText = "Yes",
                            SecondaryButtonText = "No"
                        };

                        var results = await dialog.ShowAsync();

                        var result = results == ContentDialogResult.Primary;
                        taskCompletionSource.SetResult(result);
                    });


            return await taskCompletionSource.Task;
        }

        private async Task<string> PromptUserInputAsync(FileInfo file)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();

            DispatcherQueue.TryEnqueue(
                async () =>
                    {
                        var dialog = new ContentDialog
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "Name Input",
                            Content = new TextBox(),
                            PrimaryButtonText = "OK",
                            SecondaryButtonText = "Name Later"
                        };

                        var result = await dialog.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            var textBox = dialog.Content as TextBox;
                            taskCompletionSource.SetResult(textBox?.Text);
                        }
                        else if (result == ContentDialogResult.Secondary)
                        {
                            taskCompletionSource.SetResult(Path.GetFileNameWithoutExtension(file.FullName));
                        }
                    });
            return await taskCompletionSource.Task;
        }

        private async void MergeButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var panel = button?.Parent as StackPanel;
            var dataContext = panel.DataContext as ScannedDocumentModel;

            DispatcherQueue.TryEnqueue(async () =>
                {
                    ContentDialog mergeFileDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Merge File?",
                        Content = "Are you sure you want to merge these files?",
                        PrimaryButtonText = "Merge",
                        CloseButtonText = "Cancel"
                    };

                    var result = await mergeFileDialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        MergeFilesTask.MergeFiles(_rootNodes.First(c => c.FileName.Equals(dataContext.FileName)));
                    }
                    else
                    {
                        
                    }
                });
        }

        private void FileName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _isFileNameChanged = true;
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {

            var textBox = sender as TextBox;
            if (textBox is null || textBox.Text.Equals(_previousFileName))
            {
                return;
            }

            var oldNode = _rootNodes.FirstOrDefault(node => node.FileName.Equals(_previousFileName));
            var newNode = oldNode; // Copy old node
            var newChildren = newNode.Children;
            var isCurrentFolder = _parentNode.Equals(oldNode);
            var newFileName = Path.Combine(
                Path.GetDirectoryName(newNode.FullPath),
                textBox.Text);
            File.Move(newNode.FullPath, newFileName);

            newNode = new ScannedDocumentModel(new FileInfo(newFileName), true);
            ICollection<ScannedDocumentModel> children = new List<ScannedDocumentModel>();

            foreach (var child in newChildren)
            {
                var match = Regex.Match(child.FileName, @"\(\w+\s\d+\)\.pdf");
                var newChildName = Path.Combine(
                    Path.GetDirectoryName(child.FullPath),
                    textBox.Text.Replace(".pdf", "") + " " + match.Value);
                File.Move(child.FullPath, newChildName);
                children.Add(new ScannedDocumentModel(new FileInfo(newChildName), false));

            }
            _rootNodes.Remove(oldNode);

            newNode.Children = children;
            if (isCurrentFolder)
            {
                _parentNode = newNode;
            }
            _rootNodes.Add(newNode);

        }

        private void FileParentName_OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = !args.NewText.EndsWith(".pdf");

        }

        private void FileName_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            _previousFileName = textBox.Text;
        }
    }
}






