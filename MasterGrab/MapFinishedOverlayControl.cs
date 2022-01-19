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


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Many fields can only get filled in MapFinishedOverlayControl_Loaded()
    public MapFinishedOverlayControl() {
    #pragma warning restore CS8618
      circle = new Ellipse();
      Children.Add(circle);

      SizeChanged +=MapFinishedOverlayControl_SizeChanged;
      Loaded += MapFinishedOverlayControl_Loaded;
    }
    #endregion


    #region Eventhandlers
    //      -------------

    double fontSize;
    static readonly Duration shortAnimationDuration = new(TimeSpan.FromSeconds(3));
    static readonly Duration longAnimationDuration = new(TimeSpan.FromSeconds(6));


    private void MapFinishedOverlayControl_SizeChanged(object sender, SizeChangedEventArgs e) {
      double smaller = Math.Min(e.NewSize.Width, e.NewSize.Height);
      fontSize = smaller / 9;
      if (youTextBlock!=null) {
        youTextBlock.FontSize = fontSize;
        resultTextBlock.FontSize = fontSize;
      }
      circle.Width = smaller;
      circle.Height = smaller;
      Canvas.SetTop(circle, (e.NewSize.Height-smaller) / 2);
      Canvas.SetLeft(circle, (e.NewSize.Width-smaller) / 2);
    }


    StackPanel mainStackPanel;
    Storyboard storyboard;
    Brush animatedFontBrush;
    Brush darkBackgroundBrush;
    TextBlock youTextBlock;
    TextBlock resultTextBlock;


    private void MapFinishedOverlayControl_Loaded(object sender, RoutedEventArgs e) {
      //mainStackPanel
      //--------------
      mainStackPanel = new StackPanel { Orientation=Orientation.Horizontal, Name = "mainStackPanel" };
      mainStackPanel.SizeChanged += MainStackPanel_SizeChanged;
      RegisterName(mainStackPanel.Name, mainStackPanel);
      Children.Add(mainStackPanel);

      var mainStackPanelTransformGroup = new TransformGroup();
      mainStackPanel.RenderTransform = mainStackPanelTransformGroup;

      var mainStackPanelPRotateTransform = new RotateTransform{CenterX=25, CenterY=50, Angle=0 };
      RegisterName("mainStackPanelPRotateTransform", mainStackPanelPRotateTransform);
      mainStackPanelTransformGroup.Children.Add(mainStackPanelPRotateTransform);

      var mainStackPanelScaleTransform = new ScaleTransform { CenterX=300, CenterY=50, ScaleX=1, ScaleY=1 };
      RegisterName("mainStackPanelScaleTransform", mainStackPanelScaleTransform);
      mainStackPanelTransformGroup.Children.Add(mainStackPanelScaleTransform);

      //Circle
      //------
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
      var circleScaleTransform = new ScaleTransform { CenterX=ActualWidth/2 - circle.ActualWidth/2, CenterY=ActualHeight/2, ScaleX=1, ScaleY=1 };
      RegisterName("circleScaleTransform", circleScaleTransform);
      circleTransformGroup.Children.Add(circleScaleTransform);

      //TextBlocks
      //----------
      animatedFontBrush = new SolidColorBrush(Colors.White);
      RegisterName("animatedFontBrush", animatedFontBrush);
      darkBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xE0, 0x60, 0x60, 0x60));

      youTextBlock = new TextBlock { FontSize = fontSize, FontWeight = FontWeights.Bold, Foreground = animatedFontBrush };
      youTextBlock.Text = " You ";
      mainStackPanel.Children.Add(youTextBlock);

      resultTextBlock = new TextBlock { FontSize = fontSize, FontWeight = FontWeights.Bold, Foreground = animatedFontBrush };
      mainStackPanel.Children.Add(resultTextBlock);
      Visibility = Visibility.Hidden;

      //Storyboard
      //----------
      storyboard = new Storyboard {
        RepeatBehavior = RepeatBehavior.Forever,
        AutoReverse = false,
        Duration =  longAnimationDuration
      };

      var circlePanelScaleXDoubleAnimation = new DoubleAnimation{
        From = 0.5,
        To = 1,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      storyboard.Children.Add(circlePanelScaleXDoubleAnimation);
      Storyboard.SetTargetName(circlePanelScaleXDoubleAnimation, "circleScaleTransform");
      Storyboard.SetTargetProperty(circlePanelScaleXDoubleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

      var circleScaleYDoubleAnimation = new DoubleAnimation{
        From = 0.5,
        To = 1,
        Duration = shortAnimationDuration,
        AutoReverse = false 
      };
      storyboard.Children.Add(circleScaleYDoubleAnimation);
      Storyboard.SetTargetName(circleScaleYDoubleAnimation, "circleScaleTransform");
      Storyboard.SetTargetProperty(circleScaleYDoubleAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

      var mainStackPanelRotateDoubleAnimation = new DoubleAnimation{
        From = 0,
        To = 360,
        Duration = shortAnimationDuration,
        AutoReverse = false 
      };
      storyboard.Children.Add(mainStackPanelRotateDoubleAnimation);
      Storyboard.SetTargetName(mainStackPanelRotateDoubleAnimation, "mainStackPanelPRotateTransform");
      Storyboard.SetTargetProperty(mainStackPanelRotateDoubleAnimation, new PropertyPath(RotateTransform.AngleProperty));

      var mainStackPanelScaleXDoubleAnimation = new DoubleAnimation{
        From = 0,
        To = 1,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      storyboard.Children.Add(mainStackPanelScaleXDoubleAnimation);
      Storyboard.SetTargetName(mainStackPanelScaleXDoubleAnimation, "mainStackPanelScaleTransform");
      Storyboard.SetTargetProperty(mainStackPanelScaleXDoubleAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

      var mainStackPanelScaleYDoubleAnimation = new DoubleAnimation{
        From = 0,
        To = 1,
        Duration = shortAnimationDuration,
        AutoReverse = false
      };
      storyboard.Children.Add(mainStackPanelScaleYDoubleAnimation);
      Storyboard.SetTargetName(mainStackPanelScaleYDoubleAnimation, "mainStackPanelScaleTransform");
      Storyboard.SetTargetProperty(mainStackPanelScaleYDoubleAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

      var fontColorAnimation = new ColorAnimationUsingKeyFrames{
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
    }


    private void MainStackPanel_SizeChanged(object sender, SizeChangedEventArgs e) {
      Canvas.SetTop(mainStackPanel, (ActualHeight - e.NewSize.Height) / 2);
      Canvas.SetLeft(mainStackPanel, (ActualWidth - e.NewSize.Width) / 2);
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Unhides Canvas displaying if human player has won or lost
    /// </summary>
    public void ShowResult(bool hasGuiPlayerWon) {
      if (hasGuiPlayerWon) {
        resultTextBlock.Text = "WON";
        storyboard.Begin(this, isControllable: true);
        circle.Visibility = Visibility.Visible;
        Background = Brushes.Transparent;
        youTextBlock.Foreground = animatedFontBrush;
        resultTextBlock.Foreground = animatedFontBrush;
      } else {
        storyboard.Stop(this);
        resultTextBlock.Text = "LOST";
        circle.Visibility = Visibility.Collapsed;
        Background = darkBackgroundBrush;
        youTextBlock.Foreground = Brushes.Black;
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
