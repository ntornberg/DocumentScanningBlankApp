using Microsoft.UI.Xaml.Controls;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DocumentScanningBlankApp
{
    using DocumentScanningBlankApp.ViewModels;

    using Microsoft.UI.Xaml;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileDataPage : Page
    {
        public FileDataPage()
        {
            this.InitializeComponent();
        }

        private void Weight_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var output = sender as TextBox;
           RandomGaugeViewModel.itemWeight = double.Parse(output.Text);
        }
    }
}

