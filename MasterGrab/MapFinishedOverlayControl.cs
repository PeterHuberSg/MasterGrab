/********************************************************************************************************

MasterGrab.MasterGrab.MapFinishedOverlayControl
===============================================

Displayed once game jas finished

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace MasterGrab {

  /// <summary>
  /// Displayed once game jas finished
  /// </summary>
  public class MapFinishedOverlayControl: Canvas {

    #region Constructor
    //      -----------

    readonly Ellipse circle;
    readonly ScaleTransform circleScaleTransform;
    readonly StackPanel mainStackPanel;
    readonly RotateTransform mainStackPanelRotateTransform;
    readonly ScaleTransform mainStackPanelScaleTransform;
    readonly Storyboard storyboard;
    readonly Brush animatedFontBrush;
    readonly Brush darkBackgroundBrush;
    readonly Brush grayBackgroundBrush;
    readonly TextBlock youTextBlock;
    readonly TextBlock resultTextBlock;


    public MapFinishedOverlayControl() {
      NameScope.SetNameScope(this, new NameScope());

      //Circle
      //------
      circle = new Ellipse { VerticalAlignment=VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};
      Children.Add(circle);
      var radialGradientBrush = new RadialGradientBrush {
        GradientOrigin = new Point(0.5, 0.5),
        Center = new Point(0.5, 0.5),
        RadiusX = 0.5,
        RadiusY = 0.5
      };
      radialGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xF8, 0xF0, 0xF0, 0xF0), 0.0));
      radialGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xF8, 0xF0, 0xF0, 0xF0), 0.25));
      radialGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xE0, 0xF0, 0xF0, 0xF0), 0.75));
      radialGradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0xD0, 0xD0, 0xD0), 1.0));

      // Freeze the brush (make it unmodifiable) for performance benefits.
      radialGradientBrush.Freeze();
      circle.Fill = radialGradientBrush;

      var circleTransformGroup = new TransformGroup();
      circle.RenderTransform = circleTransformGroup;
      circleScaleTransform = new ScaleTransform();
      RegisterName("circleScaleTransform", circleScaleTransform);
      circleTransformGroup.Children.Add(circleScaleTransform);

      //mainStackPanel
      //--------------
      mainStackPanel = new StackPanel { Orientation=Orientation.Horizontal, Name = "mainStackPanel" };
      mainStackPanel.SizeChanged += MainStackPanel_SizeChanged;
      RegisterName(mainStackPanel.Name, mainStackPanel);
      Children.Add(mainStackPanel);

      var mainStackPanelTransformGroup = new TransformGroup();
      mainStackPanel.RenderTransform = mainStackPanelTransformGroup;

      mainStackPanelRotateTransform = new RotateTransform { CenterX=25, CenterY=50, Angle=0 };
      RegisterName("mainStackPanelRotateTransform", mainStackPanelRotateTransform);
      mainStackPanelTransformGroup.Children.Add(mainStackPanelRotateTransform);

      mainStackPanelScaleTransform = new ScaleTransform();
      RegisterName("mainStackPanelScaleTransform", mainStackPanelScaleTransform);
      mainStackPanelTransformGroup.Children.Add(mainStackPanelScaleTransform);

      //TextBlocks
      //----------
      animatedFontBrush = new SolidColorBrush(Colors.White);
      RegisterName("animatedFontBrush", animatedFontBrush);
      darkBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xE0, 0x60, 0x60, 0x60));
      grayBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xE0, 0xA0, 0xA0, 0xA0));

      youTextBlock = new TextBlock {FontWeight = FontWeights.Bold, Foreground = animatedFontBrush };
      youTextBlock.Text = " You ";
      mainStackPanel.Children.Add(youTextBlock);

      resultTextBlock = new TextBlock {FontWeight = FontWeights.Bold, Foreground = animatedFontBrush };
      mainStackPanel.Children.Add(resultTextBlock);
      Visibility = Visibility.Hidden;

      //Storyboard
      //----------
      storyboard = new Storyboard {
        RepeatBehavior = RepeatBehavior.Forever,
        AutoReverse = false,
        Duration =  longAnimationDuration
      };

      var circlePanelScaleXDoubleAnimation = new DoubleAnimation {
        From = 0.5,
        To = 1,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      storyboard.Children.Add(circlePanelScaleXDoubleAnimation);
      Storyboard.SetTargetName(circlePanelScaleXDoubleAnimation, "circleScaleTransform");
      Storyboard.SetTargetProperty(circlePanelScaleXDoubleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

      var circleScaleYDoubleAnimation = new DoubleAnimation {
        From = 0.5,
        To = 1,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      storyboard.Children.Add(circleScaleYDoubleAnimation);
      Storyboard.SetTargetName(circleScaleYDoubleAnimation, "circleScaleTransform");
      Storyboard.SetTargetProperty(circleScaleYDoubleAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

      var mainStackPanelRotateDoubleAnimation = new DoubleAnimation {
        From = 0,
        To = 360,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      storyboard.Children.Add(mainStackPanelRotateDoubleAnimation);
      Storyboard.SetTargetName(mainStackPanelRotateDoubleAnimation, "mainStackPanelRotateTransform");
      Storyboard.SetTargetProperty(mainStackPanelRotateDoubleAnimation, new PropertyPath(RotateTransform.AngleProperty));

      var mainStackPanelScaleXDoubleAnimation = new DoubleAnimation {
        From = 0,
        To = 1,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      storyboard.Children.Add(mainStackPanelScaleXDoubleAnimation);
      Storyboard.SetTargetName(mainStackPanelScaleXDoubleAnimation, "mainStackPanelScaleTransform");
      Storyboard.SetTargetProperty(mainStackPanelScaleXDoubleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

      var mainStackPanelScaleYDoubleAnimation = new DoubleAnimation {
        From = 0,
        To = 1,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      storyboard.Children.Add(mainStackPanelScaleYDoubleAnimation);
      Storyboard.SetTargetName(mainStackPanelScaleYDoubleAnimation, "mainStackPanelScaleTransform");
      Storyboard.SetTargetProperty(mainStackPanelScaleYDoubleAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

      var fontColorAnimation = new ColorAnimationUsingKeyFrames {
        FillBehavior=FillBehavior.HoldEnd,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      fontColorAnimation.KeyFrames.Add(new LinearColorKeyFrame { Value=Colors.Blue, KeyTime = KeyTime.Uniform });
      //fontColorAnimation.KeyFrames.Add(new LinearColorKeyFrame { Value=Colors.LightGreen, KeyTime = KeyTime.Uniform });
      fontColorAnimation.KeyFrames.Add(new LinearColorKeyFrame { Value=Colors.DarkBlue, KeyTime = KeyTime.Uniform });
      storyboard.Children.Add(fontColorAnimation);
      Storyboard.SetTargetName(fontColorAnimation, "animatedFontBrush");
      Storyboard.SetTargetProperty(fontColorAnimation, new PropertyPath(SolidColorBrush.ColorProperty));

      SizeChanged +=MapFinishedOverlayControl_SizeChanged;
    }
    #endregion


    #region Eventhandlers
    //      -------------

    double fontSize;
    static readonly Duration shortAnimationDuration = new(TimeSpan.FromSeconds(3));
    static readonly Duration longAnimationDuration = new(TimeSpan.FromSeconds(6));


    private void MapFinishedOverlayControl_SizeChanged(object sender, SizeChangedEventArgs e) {
      var smaller = Math.Min(e.NewSize.Width, e.NewSize.Height);
      fontSize = smaller / 9;
      if (youTextBlock!=null) {
        if (youTextBlock.FontSize==fontSize) {
          adjustMainStackPanelPosition(mainStackPanel.ActualHeight, mainStackPanel.ActualWidth);
        }
        youTextBlock.FontSize = fontSize;
        resultTextBlock.FontSize = fontSize;
      }

      circle.Width = smaller;
      circle.Height = smaller;
      SetTop(circle, (e.NewSize.Height-smaller) / 2);
      SetLeft(circle, (e.NewSize.Width-smaller) / 2);
      circleScaleTransform.CenterX = smaller/2;
      circleScaleTransform.CenterY = smaller/2;
    }


    private void MainStackPanel_SizeChanged(object sender, SizeChangedEventArgs e) {
      adjustMainStackPanelPosition(e.NewSize.Height, e.NewSize.Width);
    }


    private void adjustMainStackPanelPosition(double height, double width) {
      SetTop(mainStackPanel, (ActualHeight - height) / 2);
      SetLeft(mainStackPanel, (ActualWidth - width) / 2);
      mainStackPanelScaleTransform.CenterX = 0.8*width;
      mainStackPanelScaleTransform.CenterY = 0.8*height;
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Unhides Canvas displaying if human player has won or lost
    /// </summary>
    public void ShowResult(bool isHumanPlaying, int winnerId) {
      if (isHumanPlaying) {
        if (winnerId==0) {
          resultTextBlock.Text = "WON";
          storyboard.Begin(this, isControllable: true);
          circle.Visibility = Visibility.Visible;
          Background = Brushes.Transparent;
          youTextBlock.Visibility = Visibility.Visible;
          youTextBlock.Foreground = animatedFontBrush;
          resultTextBlock.Foreground = animatedFontBrush;
        } else {
          storyboard.Stop(this);
          resultTextBlock.Text = "LOST";
          circle.Visibility = Visibility.Collapsed;
          Background = darkBackgroundBrush;
          youTextBlock.Visibility = Visibility.Visible;
          youTextBlock.Foreground = Brushes.Black;
          resultTextBlock.Foreground = Brushes.Black;
        }

      } else {
        //only robots playing
        storyboard.Stop(this);
        resultTextBlock.Text = $"Robot{winnerId+1} won";
        circle.Visibility = Visibility.Collapsed;
        Background = Brushes.Transparent;
        youTextBlock.Visibility = Visibility.Collapsed;
        resultTextBlock.Foreground = Brushes.Black;
      }
      Visibility = Visibility.Visible;
    }


    /// <summary>
    /// Hides Canvas displaying if human player has won or lost
    /// </summary>
    public void Hide() {
      Visibility = Visibility.Collapsed;
      storyboard.Stop(this);
    }
  }
  #endregion
}
