using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DocumentScanningBlankApp
{
    using System;

    using DocumentScanningBlankApp.Events;
    using Windows.ApplicationModel.Activation;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class. 
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>


        public App()
        {
            this.InitializeComponent();
            _documentScannedEvent = new DocumentScannedEvent();
            _getPreviousFileData = new GetHistoricalDataTask();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();

        }
        private static DocumentScannedEvent _documentScannedEvent;
        private static GetHistoricalDataTask _getPreviousFileData;

        public static Window m_window;
       
    }
}
