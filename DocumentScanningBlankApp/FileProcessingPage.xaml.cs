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
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Navigation;
    using System.Collections.Specialized;

    using Windows.UI.Core;

    using DocumentScanningBlankApp.StupidHacks;

    using Microsoft.UI.Dispatching;

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

            if (!await this.PromptKeepFileAsync())
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

        private async Task<bool> PromptKeepFileAsync()
        {

            var taskCompletionSource = new TaskCompletionSource<bool>();
           
                DispatcherQueue.TryEnqueue(
                    async () =>
                        {
                    var dialog = new ContentDialog();
                        dialog.XamlRoot = XamlRoot;
                        dialog.Title = "File Action";
                        dialog.Content = "A new file has been created. Do you want to keep it?";
                        dialog.PrimaryButtonText = "Yes";
                        dialog.SecondaryButtonText = "No";

                        var results = await dialog.ShowAsync();

                        var result = results == ContentDialogResult.Primary;
                        taskCompletionSource.SetResult(result);
                    });


            return await taskCompletionSource.Task;
            /*var dialog = new ContentDialog
                             {
                                 Title = "File Action",
                                 Content = "A new file has been created. Do you want to keep it?",
                                 PrimaryButtonText = "Yes",
                                 SecondaryButtonText = "No"
                             };

            // Attach dialog to the appropriate XAML root
            dialog.XamlRoot = this.XamlRoot;

            var results = await dialog.ShowAsync();
            return results == ContentDialogResult.Primary;*/
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


