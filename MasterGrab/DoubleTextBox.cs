/********************************************************************************************************

MasterGrab.MasterGrab.DoubleTextBox
===================================

TextBox accepting only double as input.

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


namespace MasterGrab {


  /// <summary>
  /// TextBox accepting only double as input from user. If the user enters an invalid number, he cannot
  /// move the focus away from the DoubleTextBox.
  /// </summary>
  public class DoubleTextBox: TextBox {

    #region Properties
    //      ----------

    public double Value {
      get => value;
      set {
        this.value = value;
        Text = value.ToString();
      }
    }
    double value;
    #endregion


    #region Constructor
    //      -----------

    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. 
    OptionDef<double> optionDef;
    string toolTipString;
    #pragma warning restore CS8618 


    /// <summary>
    /// to set options from code behind
    /// </summary>
    public void Set(OptionDef<double> optionDef, double value) {
      this.optionDef = optionDef;
      this.value = value;
      Text = value.ToString();
      toolTipString = optionDef.ToolTip + " Acceptable value range Min: " + optionDef.MinValue + "; Max: " + optionDef.MaxValue + ";";
      ToolTip = toolTipString;
      PreviewLostKeyboardFocus += NumberTextBox_PreviewLostKeyboardFocus;
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void NumberTextBox_PreviewLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) {
      if (double.TryParse(Text, out var number)) {
        value = number;
      } else {
        if (Window.GetWindow(this).IsVisible) { //if the window is not visible, it is closing. In that case, don't show the error message.
          MessageBox.Show("Wrong value " + Text + "." + Environment.NewLine + toolTipString, optionDef.Name + " Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }
    #endregion
  }
}
