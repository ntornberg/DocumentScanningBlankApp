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
    using Microsoft.UI.Xaml.Navigation;
    using System.Collections.Specialized;
    using System.Drawing;
    using PdfImageViewer = Windows.Data.Pdf;
    using System.Drawing.Imaging;

    using Windows.Storage;

    using ABI.Microsoft.UI.Xaml.Media.Imaging;

    using Microsoft.UI.Xaml.Media;

    using Image = Microsoft.UI.Xaml.Controls.Image;
    using Windows.Storage.Streams;
    using Windows.Graphics.Imaging;

    using ABI.Microsoft.UI.Text;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileProcessingPage : Page
    {
        private static ObservableCollection<ScannedDocumentModel> _rootNodes = new();
        private static string _parentFileName { get; set; }

        private static ScannedDocumentModel _parentNode { get; set; }

        private static int _childCount { get; set; }

        public FileProcessingPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.treeView.ItemsSource = _rootNodes;
            this.Loaded += FileProcessingPage_Loaded;

            ScannedFileData.IncomingFiles.CollectionChanged += this.IncomingFiles_CollectionChanged;
            /*var watcher = new FileSystemWatcher();
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName;
            watcher.Path = @"C:\Users\ntornberg\OneDrive - Metal Exchange Corporation\Documents\newdocs\Scanned\East\Need_To_Name";
            watcher.Created += OnFileCreated;
            watcher.Changed += OnFileCreated;
            watcher.EnableRaisingEvents = true;*/

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

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

            if (!await this.PromptKeepFileAsync(file))
            {
                return;
            }

            if (await this.PromptIsNewDocumentAsync())
            {
                var userInput = await this.PromptUserInputAsync();
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
            else
            {
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


                _childCount++;
            }
        }

        private async Task<bool> PromptKeepFileAsync(FileInfo fileInfo)
        {

            var taskCompletionSource = new TaskCompletionSource<bool>();

            DispatcherQueue.TryEnqueue(
                    async () =>
                        {
                            Image imageControl = new Image();
                            StorageFile pdfFile = await StorageFile.GetFileFromPathAsync(fileInfo.FullName);
                            PdfImageViewer.PdfDocument pdfDocument =
                                await PdfImageViewer.PdfDocument.LoadFromFileAsync(pdfFile);

                            var pageIndex = 0;
                            var pdfPage = pdfDocument.GetPage((uint)pageIndex);

                            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                            {
                                await pdfPage.RenderToStreamAsync(stream);
                                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                                SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                                var bitmapSource = new Microsoft.UI.Xaml.Media.Imaging.SoftwareBitmapSource();

                                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8
                                    || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                                {
                                    softwareBitmap = SoftwareBitmap.Convert(
                                        softwareBitmap,
                                        BitmapPixelFormat.Bgra8,
                                        BitmapAlphaMode.Premultiplied);
                                }

                                await bitmapSource.SetBitmapAsync(softwareBitmap);

                                imageControl.Source = bitmapSource;
                            }

                            var messageText = new TextBlock()
                                                  {
                                                      Text = $"A new file has been created. Do you want to keep it?\n\n"
                                                             + $"File Size: {fileInfo.Length * 0.000001} MB\n"
                                                             + $"Page Count: {new PdfDocument(new PdfReader(fileInfo.FullName)).GetNumberOfPages()}\n",
                                                  };
                            StackPanel stackPanel = new StackPanel();
                            stackPanel.Children.Add(messageText);
                            stackPanel.Children.Add(imageControl);
                            
                            var dialog = new ContentDialog();
                            dialog.XamlRoot = XamlRoot;
                            dialog.Title = "File Action";
                            dialog.Content = stackPanel;
                            dialog.PrimaryButtonText = "Yes";
                            dialog.SecondaryButtonText = "No";

                            var results = await dialog.ShowAsync();

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
                            Content = "Is this a new document?",
                            PrimaryButtonText = "Yes",
                            SecondaryButtonText = "No"
                        };

                        var results = await dialog.ShowAsync();

                        var result = results == ContentDialogResult.Primary;
                        taskCompletionSource.SetResult(result);
                    });


            return await taskCompletionSource.Task;
        }

        private async Task<string> PromptUserInputAsync()
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
                            CloseButtonText = "Cancel"
                        };

                        var result = await dialog.ShowAsync();
                        if (result == ContentDialogResult.Primary)
                        {
                            var textBox = dialog.Content as TextBox;
                            taskCompletionSource.SetResult(textBox?.Text);
                        }
                    });
            return await taskCompletionSource.Task;
        }

        private async void MergeButton_OnClick(object sender, RoutedEventArgs e)
        {
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
                        MergeFilesTask.MergeFiles(_parentNode);
                    }
                    else
                    {
                        // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                        // Do nothing.
                    }
                });
        }
    }
}


