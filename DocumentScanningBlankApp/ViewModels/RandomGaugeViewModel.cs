using System;
using System.Collections.Generic;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Linq;

namespace DocumentScanningBlankApp.ViewModels;

public partial class RandomGaugeViewModel : ObservableObject
{
   
        public Paint paint { get; set; } = new SolidColorPaint(SKColors.White);
        

        public static double itemWeight { get; set; } = 0;
        public static string itemLabel
        {
            get => "Papers";
            set
            {
                itemLabel = value;
            } 
        }

        public RandomGaugeViewModel()
        {
            PaperCount = new ObservableValue { Value = 0 };
         

            Series = new GaugeBuilder()
                .WithLabelsPosition(PolarLabelsPosition.Start)
                .WithLabelsSize(30)
                .WithInnerRadius(75)
                .WithBackgroundInnerRadius(75)
                .WithBackground(new SolidColorPaint(new SKColor(100, 181, 246, 90)))
                .WithLabelsPosition(PolarLabelsPosition.ChartCenter)
                .AddValue(PaperCount, itemLabel,SKColors.CornflowerBlue,SKColors.White)
                .BuildSeries();
                
        }

        public ObservableValue PaperCount { get; set; }
       
        public IEnumerable<ISeries> Series { get; set; }

        [RelayCommand]
        public void DoRandomChange()
        {
            PaperCount.Value = itemWeight;
           
        }
   
      
}