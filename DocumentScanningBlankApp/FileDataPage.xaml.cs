using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DocumentScanningBlankApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileDataPage : Page
    {
        public FileDataPage()
        {
            this.InitializeComponent();
        public <ISeries[] Series { get; set; }
            = new ISeries[]
                  {
                      new LineSeries<double>
                          {
                              Values = new double[] { 2, 1, 3, 5, 3, 4, 6 },
                              Fill = null
                          }
                  };
        }
    }
}
