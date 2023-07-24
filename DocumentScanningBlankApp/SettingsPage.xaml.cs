using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DocumentScanningBlankApp
{
    using Microsoft.UI;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Media;
    using System;
    using System.IO;
    using Windows.Storage;
    using Windows.Storage.AccessCache;
    using Windows.Storage.Pickers;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void MergedDirectory_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (Directory.Exists(textBox.Text))
            {
                localSettings.Values["MergedDirectory"] = textBox.Text;
            } //TODO: Do this for all of them

            /*else
            {
                DispatcherQueue.TryEnqueue(
                    async () =>
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

                            await taskCompletionSource.Task;
                        });*/

        }


        private void OutputDirectory_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (Directory.Exists(textBox.Text))
            {
                localSettings.Values["OutputDirectory"] = textBox.Text;
            }

        }

        private void DirectoryInput_OnLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (string.IsNullOrEmpty(textBox.Text) || !Directory.Exists(textBox.Text))
            {

                textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                switch (textBox.Name)
                {
                    case "PickScannedOutputTextBlock":
                        this.ToggleScannedOutputTeachingTip.IsOpen = true;
                        break;
                    case "PickMergeOutputTextBlock":
                        this.ToggleMergeOutputTeachingTip.IsOpen = true;
                        break;
                    case "PickSortedFilesTextBlock":
                        this.ToggleSortedFilesPathTeachingTip.IsOpen = true;
                        break;
                }

            }
            else
            {
                // The TextBox is not empty - reset the border color
                textBox.ClearValue(TextBox.BorderBrushProperty);
            }
        }

        private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            Button textBox = (Button)sender;
            switch (textBox.Name)
            {
                case "PickScannedOutputButton":
                    this.PickScannedOutputTextBlock.Text = "";
                    break;
                case "PickMergedOutputButton":
                    this.PickMergeOutputTextBlock.Text = "";
                    break;
                case "PickSortedFilesPathButton":
                    this.PickSortedFilesPathTextBlock.Text = "";
                    break;
            }


            // Create a folder picker
            var openPicker = new Windows.Storage.Pickers.FolderPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var window = App.m_window;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();

            if (folder == null)
            {
                return;
            }

            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            switch (textBox.Name)
            {
                case "PickScannedOutputButton":
                    this.PickScannedOutputTextBlock.Text = folder.Path;
                    break;
                case "PickMergedOutputButton":
                    this.PickMergeOutputTextBlock.Text = folder.Path;
                    break;
                case "PickSortedFilesPathButton":
                    this.PickSortedFilesPathTextBlock.Text = folder.Path;
                    break;
            }
        }

        private void PickSortedFilesPathTextBlock_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (Directory.Exists(textBox.Text))
            {
                localSettings.Values["SortedFilesPath"] = textBox.Text;
            }
        }
    }
}
