/********************************************************************************************************

MasterGrab.MasterGrab.MapErrorOverlayControl
============================================

Redraws the borders of some countries with an error colour.

License
-------

To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring 
rights to this software to the public domain worldwide under the Creative Commons 0 license (legal text 
see License CC0.html file, also <http://creativecommons.org/publicdomain/zero/1.0/>). 

The author gives no warranty of any kind that the code is free of defects, merchantable, fit for a 
particular purpose or non-infringing. Use it at your own risk :-)

Written 2016-2022 in Switzerland & Singapore by Jürgpeter Huber 

Contact: https://github.com/PeterHuberSg/MasterGrab
********************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;


namespace MasterGrab {


  /// <summary>
  /// Redraws just the borders of some countries, usually because the mouse pointer is over them.
  /// </summary>
  public class MapErrorOverlayControl: CustomControlBase {

    #region Constructor
    //      -----------

    readonly MapControl mapControl;
    readonly HashSet<int> highlightedCountries;
    readonly DispatcherTimer dispatcherTimer;


    public MapErrorOverlayControl(MapControl mapControl) {
      this.mapControl = mapControl;
      highlightedCountries = new HashSet<int>();
      dispatcherTimer = new DispatcherTimer();
      dispatcherTimer.Tick += dispatcherTimer_Tick;
      dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void dispatcherTimer_Tick(object? sender, EventArgs e) {
      isHighlight = !isHighlight;
      InvalidateVisual();
    }
    #endregion


    #region Methods
    //      -------

    public void AddCountry(int Id) {
      highlightedCountries.Add(Id);
      InvalidateVisual();
      dispatcherTimer.Start();
    }


    public void RemoveCountry(int Id) {
      highlightedCountries.Remove(Id);
      InvalidateVisual();
      if (highlightedCountries.Count<=0) {
      }
      dispatcherTimer.Stop();
    }


    public void RemoveAllCountries() {
      highlightedCountries.Clear();
      InvalidateVisual();
      dispatcherTimer.Stop();
    }
    #endregion


    #region Graphics Override
    //      -----------------

    protected override Size MeasureContentOverride(Size constraint) {
      return constraint;
    }


    protected override Size ArrangeContentOverride(Rect arrangeRect) {
      return arrangeRect.Size;
    }


    readonly Brush highlightBrush = new SolidColorBrush(Color.FromArgb(0x80, 0xE0, 0xE0, 0xE0));
    bool isHighlight;


    protected override void OnRenderContent(DrawingContext drawingContext, Size renderContentSize) {
      if (mapControl.PixelMap!=null) {
        //other thread has created a map
        var brush = isHighlight ? highlightBrush : Brushes.Transparent;
        foreach (var countryId in highlightedCountries) {
          drawingContext.DrawGeometry(brush, null, mapControl.GeometryByCountry[countryId]);
        }
      }
    }
    #endregion
  }
}
