namespace DocumentScanningBlankApp.ViewModels;

using CommunityToolkit.Mvvm.Input;
using DocumentScanningBlankApp.Data;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

public class ScannedFileChartViewModel
{
    public ISeries[] Series { get; set; }

    public ScannedFileChartViewModel()
    {
        var columnSeries = new ColumnSeries<double>
        {
            Values = ScannedFileData.PreviouslyScannedFiles.Values.ToList(),
            Fill = new SolidColorPaint(SKColors.CornflowerBlue),
            IsVisible = true,

        };
        columnSeries.PointMeasured += OnPointMeasured;
        Series = new ISeries[] { columnSeries };
    }



    private static void OnPointMeasured(ChartPoint<double, RoundedRectangleGeometry, LabelGeometry> point)
    {
        var perPointDelay = 100; // milliseconds
        var delay = point.Context.Index * perPointDelay;
        var speed = (float)point.Context.Chart.AnimationsSpeed.TotalMilliseconds + delay;

        point.Visual?.SetTransition(
            new Animation(progress =>
                    {
                        var d = delay / speed;

                        return progress <= d
                                   ? 0
                                   : EasingFunctions.BuildCustomElasticOut(1.5f, 0.60f)((progress - d) / (1 - d));
                    },
                TimeSpan.FromMilliseconds(speed)));
    }

    public Axis[] XAxes { get; set; }
        = new Axis[]
              {
                  new Axis
                      {

                          Name = "Scanned Date",
                          NamePaint = new SolidColorPaint(SKColors.White)
                                          {
                                              StrokeThickness = 0,
                                              Style = SKPaintStyle.Fill,
                                              SKFontStyle = new SKFontStyle(SKFontStyleWeight.SemiBold,SKFontStyleWidth.Normal,SKFontStyleSlant.Upright),

                                          },

                          Labels = ScannedFileData.PreviouslyScannedFiles.Keys.ToList(),
                          LabelsPaint = new SolidColorPaint(SKColors.White)
                                            {
                                                StrokeThickness = 0,
                                                Style = SKPaintStyle.Fill,
                                                SKFontStyle = new SKFontStyle(SKFontStyleWeight.SemiBold,SKFontStyleWidth.Normal,SKFontStyleSlant.Upright),

                                            },
                          TextSize = 16,
                          
                          /*ZeroPaint = new SolidColorPaint(SKColors.Red)
                                          {
                                              StrokeThickness = 2,
                                              Style = SKPaintStyle.Fill,
                                              SKFontStyle = new SKFontStyle(SKFontStyleWeight.SemiBold,SKFontStyleWidth.Normal,SKFontStyleSlant.Upright),
                                              
                                          },*/
                          SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) { StrokeThickness = 2 }
                      }
              };

    public Axis[] YAxes { get; set; }
        = new Axis[]
              {
                  new Axis
                      {
                          Name = "File size (MB)",
                          NamePaint = new SolidColorPaint(SKColors.White)
                                          {
                                              StrokeThickness = 0,
                                              Style = SKPaintStyle.Fill,
                                              SKFontStyle = new SKFontStyle(SKFontStyleWeight.SemiBold,SKFontStyleWidth.Normal,SKFontStyleSlant.Upright),

                                          },
                          
                          //LabelsPaint = new SolidColorPaint(SKColors.Green),
                          TextSize = 16,
                          MinStep = 100,
                          ForceStepToMin = false,
                          LabelsPaint = new SolidColorPaint(SKColors.White)
                                            {
                                                StrokeThickness = 0,
                                                Style = SKPaintStyle.Fill,
                                                SKFontStyle = new SKFontStyle(SKFontStyleWeight.SemiBold,SKFontStyleWidth.Normal,SKFontStyleSlant.Upright),

                                            },
                          MinLimit = 0,
                          CustomSeparators = Enumerable.Range(0, 2000).Select(i => (double)i * 500).Concat(Enumerable.Range(0, 2000).Select(i => (double)i * 250).ToArray()),

                          SeparatorsPaint = new SolidColorPaint(SKColors.White)
                                                {
                                                    StrokeThickness = 2,


                                                },
                          ZeroPaint = new SolidColorPaint(SKColors.Red)
                                          {
                                              StrokeThickness = 2,
                                              Style = SKPaintStyle.Fill,
                                              SKFontStyle = new SKFontStyle(SKFontStyleWeight.SemiBold,SKFontStyleWidth.Normal,SKFontStyleSlant.Upright),

                                          },

                      }
              };

   
    
}



