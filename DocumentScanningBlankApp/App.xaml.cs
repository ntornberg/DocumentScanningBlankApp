using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DocumentScanningBlankApp
{
    using DocumentScanningBlankApp.Events;

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        private static DocumentScannedEvent _documentScannedEvent;
        private static GetHistoricalDataTask _getPreviousFileData;

        public static Window m_window;
        private static DispatcherTimer timer;
        public App()
        {
            this.InitializeComponent();
            _documentScannedEvent = new DocumentScannedEvent();
            _getPreviousFileData = new GetHistoricalDataTask();
            
        }
        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
            Task.Run(() => _getPreviousFileData.Run());
            m_window.Closed += OnWindowClosed;
            timer = new DispatcherTimer();

  
            timer.Interval = TimeSpan.FromSeconds(30); 

   
            timer.Tick += AutoSave;


            timer.Start();
        }

        private void AutoSave(object sender, object e)
        {
            var tempPath = Path.GetTempPath();
            var cacheDirectory = Path.Combine(tempPath, "ScannedDocumentAppCache");
            foreach (var file in Directory.GetFiles(cacheDirectory))
            {
                var newFilePath =
                    Path.Combine(@"M:\Scanned\East\NewScannedFiles(DO NOT DELETE)", Path.GetFileName(file));
                if (!File.Exists(newFilePath))
                {
                    File.Move(file,Path.Combine(@"M:\Scanned\East\NewScannedFiles(DO NOT DELETE)",Path.GetFileName(file)),true);
                }
            }
        }

        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            var tempPath = Path.GetTempPath();
            var cacheDirectory = Path.Combine(tempPath, "ScannedDocumentAppCache");
            foreach (var file in Directory.GetFiles(cacheDirectory))
            {
                var newFilePath =
                    Path.Combine(@"M:\Scanned\East\NewScannedFiles(DO NOT DELETE)", Path.GetFileName(file));
                if (File.Exists(newFilePath))
                {
                    File.Move(file,Path.Combine(@"M:\Scanned\East\NewScannedFiles(DO NOT DELETE)",Path.GetFileName(file)),true);
                }
            }
        }
    }
}
