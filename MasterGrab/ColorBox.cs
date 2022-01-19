/********************************************************************************************************

MasterGrab.MasterGrab.ColorBox
==============================

Rectangular control displaying a color which the user can change.

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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MasterGrab {

  /// <summary>
  /// Displays a colored rectangle with a border. When the user clicks on the ColorBox, he can change the Color with a ColorCanvas.
  /// </summary>
  public class ColorBox: Grid {

    #region Properties
    //      ----------

    public Color Color {
      get => color;
      set {
        if (color!=value) {
          color = value;
          colorRectangle.Fill = new SolidColorBrush(color);
        }
      }
    }
    Color color;
    readonly Rectangle colorRectangle;
    #endregion


    #region Constructor
    //      -----------

    public ColorBox() {
      Focusable = true;
      Background = Brushes.Black;
      colorRectangle = new Rectangle {
        Margin = new Thickness(1),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch
      };
      Children.Add(colorRectangle);
      colorRectangle.MouseLeftButtonDown += ColorRectangle_MouseLeftButtonDown;
      colorRectangle.MouseLeftButtonUp += ColorRectangle_MouseLeftButtonUp;
      colorRectangle.MouseEnter += ColorRectangle_MouseEnter;
      Color = Colors.White;
    }
    #endregion


    #region Eventhandlers
    //      -------------

    bool isMouseDownGotFocus;


    private void ColorRectangle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
      isMouseDownGotFocus = false;
    }


    private void ColorRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      isMouseDownGotFocus = Focus();
    }


    private void ColorRectangle_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      if (isMouseDownGotFocus) {
        isMouseDownGotFocus = false;
        Color = ColorPickerWindow.ShowDialog(Window.GetWindow(this), color);
      }
    }


    private void ColorOkButton_Click(object sender, RoutedEventArgs e) {
      var window = Window.GetWindow((Button)sender);
      window.DialogResult = true;
      window.Close();
    }
    #endregion
  }
}
