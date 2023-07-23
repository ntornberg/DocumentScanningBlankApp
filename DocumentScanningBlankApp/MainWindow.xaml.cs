using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DocumentScanningBlankApp
{
    using DocumentScanningBlankApp.Data;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public double InfoBadgeOpacity { get; set; }
        public int FileScannedNotificationCount { get; set; }
        public MainWindow()
        {

            this.InitializeComponent();
            this.NavMenu.ItemInvoked += NavMenu_ItemInvoked;
            ScannedFileData.IncomingFiles.CollectionChanged += this.IncomingFiles_CollectionChanged;
        }

        private void IncomingFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
            DispatcherQueue.TryEnqueue(
                async () =>
                    {
                        if (contentFrame.SourcePageType != typeof(FileProcessingPage))
                        {
                            this.InfoBadgeOpacity = 1;
                            this.FileScannedNotificationCount++;
                        }
                        else
                        {
                            this.InfoBadgeOpacity = 0;
                            this.FileScannedNotification.Value = 0;
                        }
                        this.FileScannedNotification.Visibility = Visibility.Visible;
                        this.FileScannedNotification.Value = ScannedFileData.IncomingFiles.Count;
                    });
        }

        private void NavMenu_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var tag = args.InvokedItemContainer.Tag as string;

            // Handle menu item selection based on the tag value
            switch (tag)
            {
                case "FileProcessingPage":
                    this.contentFrame.Navigate(typeof(FileProcessingPage));
                    DispatcherQueue.TryEnqueue(
                        async () =>
                            {
                                this.InfoBadgeOpacity = 0;
                                this.FileScannedNotification.Value = 0;
                                                               this.FileScannedNotification.Visibility = Visibility.Collapsed;
                            });
                                break;
                case "FileData":
                    this.contentFrame.Navigate(typeof(FileDataPage));
                    // Handle Menu Item2 selection
                    // ...
                    break;
                case "Settings":
                    // Navigate to settings page
                    this.contentFrame.Navigate(typeof(SettingsPage));
                    break;

            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
