/********************************************************************************************************

MasterGrab.MasterGrab.CustomControlBase
=======================================

Base class for custom controls, allowing the control to draw and to add any other WPF controls as 
children.

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


//http://tech.pro/tutorial/856/wpf-tutorial-using-a-visual-collection
//http://wpftutorial.net/HowToCreateACustomControl.html

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace MasterGrab {


  /// <summary>
  /// Base class for custom controls. Since it inherits from Control, it provides standard properties like Font and 
  /// Background. However, those properties are not doing anything. CustomControlBase adds the code for Padding, Border 
  /// and Background. Inheritors should not override MeasureOverride, ArrangeOverride and OnRender but override 
  /// MeasureContentOverride, ArrangeContentOverride and OnRenderContent.
  /// </summary>
  public abstract class CustomControlBase: Control {

    #region Properties
    //      ----------
    #if DEBUG
    /// <summary>
    /// Should for debugging the layout data like Margin, Padding, measurement size, arrange size, render size and actual size get
    /// displayed over actual content ?
    /// Default: false
    /// </summary>
    public bool IsShowLayoutData {
      get => (bool)GetValue(IsShowLayoutDataProperty);
      set => SetValue(IsShowLayoutDataProperty, value);
    }

    // DependencyProperty definition for IsShowLayoutData
    public static readonly DependencyProperty IsShowLayoutDataProperty =
        DependencyProperty.Register("IsShowLayoutData", typeof(bool), typeof(CustomControlBase), new UIPropertyMetadata(false));
    #endif


    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// static constructor
    /// </summary>
    static CustomControlBase() {
      HorizontalAlignmentProperty.OverrideMetadata(typeof(CustomControlBase), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
      VerticalAlignmentProperty.OverrideMetadata(typeof(CustomControlBase), new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
      BorderThicknessProperty.OverrideMetadata(typeof(CustomControlBase), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
      BorderBrushProperty.OverrideMetadata(typeof(CustomControlBase), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));
      PaddingProperty.OverrideMetadata(typeof(CustomControlBase), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
      BackgroundProperty.OverrideMetadata(typeof(CustomControlBase), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));
      HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CustomControlBase), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
      VerticalContentAlignmentProperty.OverrideMetadata(typeof(CustomControlBase), new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
    }


    /// <summary>
    /// Default Constructor
    /// </summary>
    public CustomControlBase() {
      visualCollection = new VisualCollection(this);

    }
    #endregion


    #region Public Methods
    //      --------------

    private List<Visual>? children; //use LogicalChildren to get the children
    private VisualCollection visualCollection;


    /// <summary>
    /// Adding a Visual derived class to visualCollection will add it to the visual tree of the CustomControl
    /// </summary>
    protected void AddChild(Visual child) {
      //add child to logical tree
      children ??= new List<Visual>();
      children.Add(child);
      //tell child who is its parent
      AddLogicalChild(child); //equivalent to Child.Parent = this;
      //add child to visual tree
      visualCollection ??= new VisualCollection(this);//this line ensures that all children are part of visual tree
      visualCollection.Add(child);
    }


    /// <summary>
    /// Removes the child from the visual tree
    /// </summary>
    protected void RemoveChild(Visual child) {
      children!.Remove(child);
      RemoveLogicalChild(child);
      visualCollection.Remove(child);
    }
    #endregion


    #region Overwrites
    //      -----------

    //Provides access to the logical children (Logical tree)
    protected override IEnumerator LogicalChildren => children!.GetEnumerator();


    /// <summary>
    /// Number of Visuals added as children to this control by inheriting class.
    /// </summary>
    protected sealed override int VisualChildrenCount => visualCollection.Count;


    /// <summary>
    /// Returns a Visual added by inheriting class.
    /// </summary>
    protected sealed override Visual GetVisualChild(int index) {
      return visualCollection[index];
    }


    /// <summary>
    /// Inheritors: Override MeasureContentOverride instead of MeasureOverride
    /// </summary>
    /// <remarks>
    /// MeasureOverride removes the space needed for Border and Padding, then calls MeasureContentOverride with the remaining space
    /// </remarks>
    protected sealed override Size MeasureOverride(Size constraint) {
      return MeasureOverrideTraced(constraint);
    }


    /// <summary>
    /// Can be used to trace the sealed MeasureOverride
    /// </summary>
    protected virtual Size MeasureOverrideTraced(Size constraint) {
      return doMeasureOverride(constraint);
    }


    /// <summary>
    /// Size required by class inheriting when returning from MeasureContentOverride().
    /// </summary>
    protected Size MeasureContentSize { get; private set; }


    #if DEBUG
    Size measureConstraintSize;
    Size measureRequestedSize;
    #endif


    private Size doMeasureOverride(Size constraint) {
      /*
      MeasureOverride() gets called by FrameworkElement.MeasurementCore() which gets called by UIElement.Measure() 
      UIElement.Measure(availableSize) does the following before calling MeasurementCore():
      + throw exception isNan(availableSize), both for width and height
      + if UIElement is collapsed, MeasurementCore() will not get called
      + if the availableSize has not changed, MeasurementCore() will not get called, unless InvalidateMeasure() was called before
      + before calling MeasurementCore() , InvalidateArrange() gets called
      FrameworkElement.MeasurementCore(availableSize) does the following before calling MeasureOverride():
      + removing Margin from availableSize
      + taking care of height, minHeight and maxHeight and the same width properties
      after calling MeasureOverride(), MeasurementCore() adjusts the value returned by MeasureOverride() to calculate DesiredSize:
      + DesiredSize must be within MinWidth and MaxWidth
      + add margin to DesiredSize
      after calling FrameworkElement.MeasurementCore(), UIElement.Measure() executes:
      + throw exception if return width or height is Infinite
      + throw exception if return width or height isNaN 
      */

      #if DEBUG
      measureConstraintSize = constraint;
      //curious if the following ever occurs ?
      if (!double.IsNaN(Width)) {
        if (constraint.Width!=Width)
          throw new ArgumentException("MeasurementOverride should set constraint.Width = FrameworkElement.Width if Width is a number.");
      }
      if (!double.IsNaN(Height)) {
        if (constraint.Height!=Height)
          throw new ArgumentException("MeasurementOverride should set constraint.Height = FrameworkElement.Height if Height is a number.");
      }
      #else
      Size measureRequestedSize;
      #endif

      //remove first space for border and padding
      MeasureContentSize = MeasureContentOverride(removeBorderPaddingSpace(constraint, out var borderPaddingWith, out var borderPaddingHeight));

      measureRequestedSize = new Size(borderPaddingWith + MeasureContentSize.Width, borderPaddingHeight + MeasureContentSize.Height);
      return measureRequestedSize;
    }


    private Size removeBorderPaddingSpace(Size availableSize, out double borderPaddingWith, out double borderPaddingHeight) {
      borderPaddingWith = BorderThickness.Left + Padding.Left + Padding.Right + BorderThickness.Right;
      var remainingContentWidth = Math.Max(0, availableSize.Width - borderPaddingWith);
      borderPaddingHeight = BorderThickness.Top + Padding.Top + Padding.Bottom + BorderThickness.Bottom;
      var remainingContentHeight = Math.Max(0, availableSize.Height - borderPaddingHeight);
      return new Size(remainingContentWidth, remainingContentHeight);
    }


    /// <summary>
    /// Inheritors: Override MeasureContentOverride instead of MeasureOverride. MeasureContentOverride provides the size left
    /// after applying Border and Padding. Can be between 0 and infinite, but not negative.
    /// </summary>
    protected abstract Size MeasureContentOverride(Size constraint);


    /// <summary>
    /// Inheritors: Override ArrangeContentOverride instead of ArrangeOverride
    /// </summary>
    /// <remarks>
    /// ArrangeOverride removes the space needed for Border and Padding, then calls ArrangeContentOverride with the remaining space
    /// </remarks>
    protected sealed override Size ArrangeOverride(Size arrangeBounds) {
      return ArrangeOverrideTraced(arrangeBounds);
    }


    /// <summary>
    /// Can be used to trace the sealed MeasureOverride
    /// </summary>
    protected virtual Size ArrangeOverrideTraced(Size arrangeBounds) {
      return doArrangeOverride(arrangeBounds);
    }


    #if DEBUG
    Size arrangeConstraintSize;
    Size arrangeRequestedSize;
    #endif
    Size arrangeContentSize;


        private Size doArrangeOverride(Size arrangeBounds) {
      /*
      ArrangeOverride() gets called by FrameworkElement.ArrangeCore() which gets called by UIElement.Arrange() 

      UIElement.Arrange(finalRect) does the following before calling ArrangeCore():
      + throw exception if Width or Height are infinite or NAN
      + does not call ArrangeCore if UIElement is collapsed
      + call Measure again if MeasureDirty
      + does not call ArrangeCore if size has not changes since last call, unless ArrangeValid has been invalidated

      FrameworkElement.ArrangeCore(finalRect) does the following before calling ArrangeOverride():
      + arrangeSize = finalRect
      + remove margin from arrangeSize
      + if (arrangeSize<DesiredSize){
      +   arrangeSize = DesiredSize; NeedsClipBounds = true
      + }
      + if (not stretched){
      +   arrangeSize = DesiredSize; //NeedsClipBounds doesn't change
      + }
      + Size MaxSize = new Size(MaxWidth, MaxHeight)
      + if (arrangeSize<MaxSize){
      +   arrangeSize = MaxSize; NeedsClipBounds = true
      + }
      + arrangeSize can be bigger than finalRect. In that case NeedsClipBounds is true
      + RenderSize = ArrangeOverride(arrangeSize)

      after calling ArrangeOverride(), ArrangeCore() adjusts the value returned by ArrangeOverride():
      + if (RenderSize>MaxSize) NeedsClipBounds=true
      + if (RenderSize>finalRect-Margin) NeedsClipBounds=true
      + update VisualTransform and VisualOffset to cater for Alignments based on DesiredSize and available space

      after calling FrameworkElement.ArrangeCore(), UIElement.Arrange() executes:
      + clips FrameworkElement to finalRect from UIElement.Arrange()
      + call OnRender(drawingContext) if size has changed or RenderValid is invalidated
      */

#if DEBUG
      arrangeConstraintSize = arrangeBounds;
      #else
      Size arrangeRequestedSize;
      #endif

      //the source code of FrameworkElement states that arrangeBounds should always be bigger or equal than DesiredSize. If the available
      //space is smaller, DesiredSize is given, but later only the part of CustomControlBase shown that fits into the available space.
      if (arrangeBounds.Width<DesiredSize.Width-Margin.Left - Margin.Right || arrangeBounds.Height<DesiredSize.Height - Margin.Top - Margin.Bottom) {
        throw new ArgumentException("doArrangeOverride(): arrangeBounds " + arrangeBounds + " should be equal or bigger than DesiredSize " +
          (DesiredSize.Width - Margin.Left - Margin.Right) + ", " + (DesiredSize.Height - Margin.Top - Margin.Bottom) + ".");
      }

      var lessBorderPaddingSize = 
        removeBorderPaddingSpace(arrangeBounds, out var borderPaddingWith, out var borderPaddingHeight);
      var arrangeSize = lessBorderPaddingSize;

      var offsetX = Padding.Left + BorderThickness.Left;
      var offsetY = Padding.Top + BorderThickness.Top;
      arrangeContentSize = ArrangeContentOverride(new Rect(new Point(offsetX, offsetY), arrangeSize));
      arrangeRequestedSize = new Size(borderPaddingWith + arrangeContentSize.Width, borderPaddingHeight + arrangeContentSize.Height);

      return arrangeRequestedSize;
    }


    /// <summary>
    /// Inheritors: Override ArrangeContentOverride instead of ArrangeOverride. ArrangeContentOverride provides the remaining size 
    /// after applying Border and Padding. Can between infinite and 0, but not negative.<para/>
    /// Note that the parameter is a rectangle, not a size, because there might be an offset when ContentAlignment is not stretched. Use
    /// ContentArrange() instead of Arrange().
    /// </summary>
    protected abstract Size ArrangeContentOverride(Rect arrangeRect);


    /// <summary>
    /// Should all the available width be used for this Control or only the width it really needs ? The control
    /// knows how to use additional space (=expandable).
    /// </summary>
    protected bool IsSizingWidthToExpandableContent() {
      if (!double.IsNaN(Width) || HorizontalAlignment==HorizontalAlignment.Stretch) {
        //width is defined or control is stretched
        //if content stretched, then it should use all space, otherwise just use space as needed
        return HorizontalContentAlignment!=HorizontalAlignment.Stretch;
      }
      //width is not defined and control is not stretched
      //content should use needed space
      return true;
    }


    /// <summary>
    /// Should all the available height be used for this Control or only the height it really needs ? The control
    /// knows how to use additional space (=expandable).
    /// </summary>
    protected bool IsSizingHeightToExpandableContent() {
      if (!double.IsNaN(Height) || VerticalAlignment==VerticalAlignment.Stretch) {
        //height is defined or control is stretched
        //if content stretched, then it should use all space, otherwise just use space as needed
        return VerticalContentAlignment!=VerticalAlignment.Stretch;
      }
      //height is not defined and control is not stretched
      //content should use only needed space
      return true;
    }


    /// <summary>
    /// Should all the available width be used for this Control or only the width it really needs ? The control
    /// does not know how to use additional space (=fixed).
    /// </summary>
    protected bool IsSizingWidthToFixedContent() {
      if (!double.IsNaN(Width) || HorizontalAlignment==HorizontalAlignment.Stretch) {
        //width is defined or control is stretched
        //content should use all available space space
        return false;
      }
      //width is not defined and control is not stretched
      //content should use only needed space
      return true;
    }


    /// <summary>
    /// Should all the available height be used for this Control or only the height it really needs ? The control
    /// does not know how to use additional space (=fixed).
    /// </summary>
    protected bool IsSizingHeightToFixedContent() {
      if (!double.IsNaN(Height) || VerticalAlignment==VerticalAlignment.Stretch) {
        //height is defined or control is stretched
        //content should use all available space space
        return false;
      }
      //height is not defined and control is not stretched
      //content should use only needed space
      return true;
    }


    #if DEBUG
    static Brush? layoutDataBrush;
    #endif


    /// <summary>
    /// Draws Border and Background. Inheritors should overwrite OnRenderContent instead
    /// </summary>
    protected sealed override void OnRender(DrawingContext drawingContext) {
      OnRenderTraced(drawingContext);
    }


    /// <summary>
    /// Can be used to trace the sealed MeasureOverride
    /// </summary>
    protected virtual void OnRenderTraced(DrawingContext drawingContext) {
      doOnRender(drawingContext);
    }


    private void doOnRender(DrawingContext drawingContext) {
      if (Background!=null) {
        //draw background
        drawingContext.DrawRectangle(Background, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
      }

      //draw Border over background
      var borderWidthThickness = Math.Min(RenderSize.Width, BorderThickness.Left + BorderThickness.Right);
      var borderHeightThickness = Math.Min(RenderSize.Height, BorderThickness.Top + BorderThickness.Bottom);
      if (borderWidthThickness>0 || borderHeightThickness>0) {
        //enough space to draw at least some border
        if (BorderBrush!=null && BorderThickness.Left + BorderThickness.Top + BorderThickness.Right + BorderThickness.Bottom>0) {
          if (borderWidthThickness>=RenderSize.Width || borderHeightThickness>=RenderSize.Height) {
            //only space enough for part of the border
            drawingContext.DrawRectangle(BorderBrush, null, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
          } else {
            var borderGeometry = new GeometryGroup();
            borderGeometry.Children.Add(new RectangleGeometry(new Rect(0, 0, RenderSize.Width, RenderSize.Height)));
            borderGeometry.Children.Add(new RectangleGeometry(
              new Rect(BorderThickness.Left, BorderThickness.Top, RenderSize.Width-borderWidthThickness, RenderSize.Height-borderHeightThickness)));
            drawingContext.DrawGeometry(BorderBrush, null, borderGeometry);
          }
        }
      }

      if (RenderSize.Width>BorderThickness.Left + BorderThickness.Right + Padding.Left + Padding.Left &&
          RenderSize.Height>BorderThickness.Top + BorderThickness.Bottom + Padding.Top + Padding.Bottom) {
        //enough space to draw some content
        drawingContext.PushTransform(new TranslateTransform(BorderThickness.Left + Padding.Left, BorderThickness.Top + Padding.Top));
        drawingContext.PushClip(new RectangleGeometry(new Rect(arrangeContentSize)));
        OnRenderContent(drawingContext, arrangeContentSize);
        drawingContext.Pop();
        drawingContext.Pop();

        #if DEBUG
        if (IsShowLayoutData) {
          //write layout data over content
          layoutDataBrush ??= new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
          var layoutDataString =
            "Alignment H: " + HorizontalAlignment + ", V: " + VerticalAlignment + Environment.NewLine +
            "Margin        L:" + format5(Margin.Left) + ", T: " + format5(Margin.Top) + ", R: " + format5(Margin.Right) + ", B: " + format5(Margin.Bottom) + Environment.NewLine +
            "Border        L:" + format5(BorderThickness.Left) + ", T: " + format5(BorderThickness.Top) + ", R: " + format5(BorderThickness.Right) + ", B: " + format5(BorderThickness.Bottom) + Environment.NewLine +
            "Padding       L:" + format5(Padding.Left) + ", T: " + format5(Padding.Top) + ", R: " + format5(Padding.Right) + ", B: " + format5(Padding.Bottom) + Environment.NewLine +
            "                Width   Height" + Environment.NewLine +
            "Size:        " + format8(Width) + ", " + format8(Height) + Environment.NewLine +
            "MConstraint: " + format8(measureConstraintSize.Width) + ", " + format8(measureConstraintSize.Height) + Environment.NewLine +
            "MeasContent: " + format8(MeasureContentSize.Width) + ", " + format8(MeasureContentSize.Height) + Environment.NewLine +
            "MRequested:  " + format8(measureRequestedSize.Width) + ", " + format8(measureRequestedSize.Height) + Environment.NewLine +
            "Desired:     " + format8(DesiredSize.Width) + ", " + format8(DesiredSize.Height) + Environment.NewLine +
            "AConstraint: " + format8(arrangeConstraintSize.Width) + ", " + format8(arrangeConstraintSize.Height) + Environment.NewLine +
            "AranContent: " + format8(arrangeContentSize.Width) + ", " + format8(arrangeContentSize.Height) + Environment.NewLine +
            "ARequested:  " + format8(arrangeRequestedSize.Width) + ", " + format8(arrangeRequestedSize.Height) + Environment.NewLine +
            "Render:      " + format8(RenderSize.Width) + ", " + format8(RenderSize.Height) + Environment.NewLine +
            "Actual:      " + format8(ActualWidth) + ", " + format8(ActualHeight);

          var formattedText = new FormattedText(layoutDataString,
            Thread.CurrentThread.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            12, layoutDataBrush, 
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
          drawingContext.DrawText(formattedText, new Point(BorderThickness.Left + Padding.Left, BorderThickness.Top + Padding.Top));
        }
        #endif
      }
    }


    private static string format8(double number) {
      return number.ToString("0").PadLeft(8);
    }


    private static string format5(double number) {
      return number.ToString("0").PadLeft(5);
    }


    /// <summary>
    /// Inheritors: Override OnRenderContent instead of OnRender. the drawingContext is adjusted for 
    /// Border and Padding.
    /// </summary>
    protected virtual void OnRenderContent(DrawingContext drawingContext, Size renderContentSize) {
    }


    protected override Geometry? GetLayoutClip(Size layoutSlotSize) {
      return ClipToBounds ? new RectangleGeometry(new Rect(layoutSlotSize)) : null;
    }
    #endregion
  }


  public static class CustomControlBaseExtensions {

    /// <summary>
    /// Calls Arrange using the offset in arrangeRect and x,y. Offsets x and y cannot be NAN. Width and Height must be a positive number.
    /// </summary>
    public static void ArrangeBorderPadding(this FrameworkElement frameworkElement, Rect arrangeRect, double x, double y, double width, double height) {
      if (double.IsNaN(x)) {
        #if DEBUG
        System.Diagnostics.Debugger.Break();
        #endif
        throw new ArgumentException($"Custom Control ArrangeContentOverride(): Offset X {x} cannot be NAN (not a number).");
      }
      if (double.IsNaN(y)) {
        #if DEBUG
        System.Diagnostics.Debugger.Break();
        #endif
        throw new ArgumentException($"Custom Control ArrangeContentOverride(): Offset Y {y} cannot be NAN (not a number).");
      }
      //new Rect() will throw an exception if width or height is negative, infinitive or NAN
      #if DEBUG
      if (double.IsNaN(width) || width<0 || double.IsInfinity(width)) {
        System.Diagnostics.Debugger.Break();
      }
      if (double.IsNaN(height) || height<0 || double.IsInfinity(height)) {
        System.Diagnostics.Debugger.Break();
      }
      #endif
      frameworkElement.Arrange(new Rect(arrangeRect.X + x, arrangeRect.Y + y, width, height));
    }
  }
}
