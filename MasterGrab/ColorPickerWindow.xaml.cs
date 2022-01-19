/********************************************************************************************************

MasterGrab.MasterGrab.ColorPickerWindow
=======================================

Control displaying some colors, letting the user define any color.

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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MasterGrab {


  /// <summary>
  /// Control displaying some colors, letting the user define any color
  /// </summary>
  public partial class ColorPickerWindow: Window {

    #region Properties
    //      ----------

    public Color Color { get; private set; }
    #endregion


    #region Constructor
    //      -----------

    public static Color ShowDialog(Window owner, Color color) {
      var window = new ColorPickerWindow(color) { Owner = owner };
      window.ShowDialog();
      return window.isOk ? window.color : color;
    }


    const int colorsCount = 13;
    const int variantCountHalf = 2;
    const int variantCount = 2*variantCountHalf + 1;

    Color color;


    public ColorPickerWindow(Color? color = null) {
      this.color = color??Colors.Gray;
      InitializeComponent();
      ColorRectangle.Fill = new SolidColorBrush(this.color);
      RTextBox.Text = this.color.R.ToString();
      GTextBox.Text = this.color.G.ToString();
      BTextBox.Text = this.color.B.ToString();

      RTextBox.TextChanged += RTextBox_TextChanged;
      GTextBox.TextChanged += GTextBox_TextChanged;
      BTextBox.TextChanged += BTextBox_TextChanged;
      OkButton.Click += OkButton_Click;

      for (var colorIndex = 0; colorIndex < colorsCount; colorIndex++) {
        ColorGrid.ColumnDefinitions.Add(new ColumnDefinition());
      }
      for (var variantIndex = 0; variantIndex < variantCount; variantIndex++) {
        ColorGrid.RowDefinitions.Add(new RowDefinition());
      }
      for (var ColorIndex = 0; ColorIndex < colorsCount; ColorIndex++) {
        Color mainColor;
        switch (ColorIndex) {
        case  0: mainColor = Color.FromRgb(0xff, 0x00, 0x00); break;
        case  1: mainColor = Color.FromRgb(0xff, 0x80, 0x00); break;

        case  2: mainColor = Color.FromRgb(0xff, 0xff, 0x00); break;
        case  3: mainColor = Color.FromRgb(0x80, 0xff, 0x00); break;

        case  4: mainColor = Color.FromRgb(0x00, 0xff, 0x00); break;
        case  5: mainColor = Color.FromRgb(0x00, 0xff, 0x80); break;

        case  6: mainColor = Color.FromRgb(0x00, 0xff, 0xff); break;
        case  7: mainColor = Color.FromRgb(0x00, 0x80, 0xff); break;

        case  8: mainColor = Color.FromRgb(0x00, 0x00, 0xff); break;
        case  9: mainColor = Color.FromRgb(0x80, 0x00, 0xff); break;

        case 10: mainColor = Color.FromRgb(0xff, 0x00, 0xff); break;
        case 11: mainColor = Color.FromRgb(0xff, 0x00, 0x80); break;

        case 12: mainColor = Color.FromRgb(0x80, 0x80, 0x80); break;
        default:
          throw new NotSupportedException(); ;
        }

        for (var variantIndex = 0; variantIndex < variantCount; variantIndex++) {
          var rectColor = mainColor;
          if (variantIndex<variantCountHalf) {
            var multiply = variantIndex==0 ? 1 : 2;
            var div = variantIndex==0 ? 3 : 4;
            rectColor = Color.FromRgb(
              (byte)(0xFF - (0xFF-rectColor.R) * multiply / div),
              (byte)(0xFF - (0xFF-rectColor.G) * multiply / div),
              (byte)(0xFF - (0xFF-rectColor.B) * multiply / div));
          } else if (variantIndex>variantCountHalf) {
            var multiply = variantIndex==variantCount-variantCountHalf ? 3 : 1;
            var div      = variantIndex==variantCount-variantCountHalf ? 4 : 2;
            rectColor = Color.FromRgb(
              (byte)(rectColor.R * multiply / div),
              (byte)(rectColor.G * multiply / div),
              (byte)(rectColor.B * multiply / div));
          } else {
            //just use rectColor as it is
          }
          var rectangle = new Rectangle { 
            Fill=new SolidColorBrush(rectColor), 
            Stroke=Brushes.Black };
          Grid.SetColumn(rectangle, ColorIndex);
          Grid.SetRow(rectangle, variantIndex);
          rectangle.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
          ColorGrid.Children.Add(rectangle);
        }
      }
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void RTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (byte.TryParse(RTextBox.Text, out var number)) {
        setColor(number, color.G, color.B);
      }
    }


    private void GTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (byte.TryParse(GTextBox.Text, out var number)) {
        setColor(color.R, number, color.B);
      }
    }


    private void BTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (byte.TryParse(BTextBox.Text, out var number)) {
        setColor(color.R, color.G, number);
      }
    }


    private void setColor(byte r, byte g, byte b) {
      color = Color.FromRgb(r, g, b);
      ColorRectangle.Fill = new SolidColorBrush(color);
    }


    private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      var rectangle = (Rectangle)sender;
      ColorRectangle.Fill = rectangle.Fill;
      color = ((SolidColorBrush)rectangle.Fill).Color;
      RTextBox.Text = color.R.ToString();
      GTextBox.Text = color.G.ToString();
      BTextBox.Text = color.B.ToString();
    }


    bool isOk;


    private void OkButton_Click(object sender, RoutedEventArgs e) {
      isOk = true;
      Close();
    }
    #endregion
  }
}
