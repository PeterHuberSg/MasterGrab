/********************************************************************************************************

MasterGrab.MasterGrab.ErrorWindow
=================================

Displays error messages, which the user can copy, including stack trace.

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
using System.Windows.Documents;
using System.Windows.Media;


namespace MasterGrab {
  /// <summary>
  /// Interaction logic for ErrorWindow.xaml
  /// </summary>
  public partial class ErrorWindow: Window {

    #region Constructor
    //      -----------

    internal static ErrorWindow Show(Window window, Exception exception, MapControl mapControl) {
      var errorWindow = new ErrorWindow(window, exception, mapControl);
      errorWindow.Show();

      return errorWindow;
    }


    public ErrorWindow(Window owner, Exception exception, MapControl mapControl) : this() {
      Owner = owner;
      MaxWidth = Owner.ActualWidth/2;//this() executed InitializeComponent() already

      var gameException = exception as GameException;
      var run = gameException is null
          ? new Run("Game made an error, an exception occurred")
          : new Run("Player made an error");
      run.FontSize = FontSize * 1.5;
      run.FontWeight = FontWeights.Bold;
      run.Foreground = Brushes.DarkRed;
      ErrorTextBlock.Inlines.Add(run);
      ErrorTextBlock.Inlines.Add(new LineBreak());

      //It is not possible to move armies from Country (0, Owner: 2, Armies: 20) to Country (1, Owner: 1, Armies: 11), ...
      var messageSpan = new Span();
      //Country (0, Owner: 2, Armies: 20)
      var messageParts = exception.Message.Split(new string[] { "Country (", ")" }, StringSplitOptions.RemoveEmptyEntries);
      var isFoundCountry = false;
      var isWrongFormat = false;
      foreach (var messagePart in messageParts) {
        if (isFoundCountry) {
          isFoundCountry = false;
          var integerPos = messagePart.IndexOf(Country.DebugStringOwnerLabel) + Country.DebugStringOwnerLength;
          if (integerPos<0) {
            isWrongFormat = true;
            break;
          }

          var commaPos = messagePart.IndexOf(',', integerPos);
          if (commaPos<0) {
            isWrongFormat = true;
            break;
          }
          var playerIdString = messagePart[integerPos..commaPos];
          if (!int.TryParse(playerIdString, out var playerId)) {
            isWrongFormat = true;
            break;
          }
          run = new Run(messagePart) {Background = mapControl.PlayerBrushes[playerId]};
          messageSpan.Inlines.Add(run);
          messageSpan.Inlines.Add(new LineBreak());

        } else {
          isFoundCountry = true;
          run = new Run(messagePart);
          messageSpan.Inlines.Add(run);
          messageSpan.Inlines.Add(new LineBreak());
        }
      }
      if (isWrongFormat) {
        ErrorTextBlock.Inlines.Add(new Run(exception.Message));
      } else {
        ErrorTextBlock.Inlines.Add(messageSpan);
      }

      DetailsTextBox.Text = exception.ToDetailString();
      if (gameException!=null && gameException.Game!=null) {
        DetailsTextBox.Text += Environment.NewLine + gameException.Game.ToGameString();
      }
      DetailsDockPanel.Visibility = Visibility.Collapsed;

      //DetailButton.Click += DetailButton_Click;
      DetailCheckbox.Checked += DetailCheckbox_Checked;
      DetailCheckbox.Unchecked += DetailCheckbox_Unchecked;
      CopyButton.Click += CopyButton_Click;
    }


    public ErrorWindow() {
      InitializeComponent();
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void DetailCheckbox_Checked(object sender, RoutedEventArgs e) {
      DetailsDockPanel.Visibility = Visibility.Visible;
    }


    private void DetailCheckbox_Unchecked(object sender, RoutedEventArgs e) {
      DetailsDockPanel.Visibility = Visibility.Collapsed;
    }


    private void CopyButton_Click(object sender, RoutedEventArgs e) {
      Clipboard.SetText(DetailsTextBox.Text);
    }
    #endregion
  }
}
