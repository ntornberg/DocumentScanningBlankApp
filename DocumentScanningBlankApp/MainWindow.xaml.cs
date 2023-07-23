using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DocumentScanningBlankApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public MainWindow()
        {

            this.InitializeComponent();

            this.NavMenu.ItemInvoked += NavMenu_ItemInvoked;
        }

        private void NavMenu_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var tag = args.InvokedItemContainer.Tag as string;

            // Handle menu item selection based on the tag value
            switch (tag)
            {
                case "FileProcessingPage":
                    this.contentFrame.Navigate(typeof(FileProcessingPage));
                    break;
                case "SamplePage2":
                    // Handle Menu Item2 selection
                    // ...
                    break;
                case "Settings":
                    // Navigate to settings page
                    this.contentFrame.Navigate(typeof(SettingsPage));
                    break;

            }

        }

    }
}
