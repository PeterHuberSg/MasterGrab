/********************************************************************************************************

MasterGrab.MasterGrab.MainWindow
================================

Startup window of MasterGrab, showing map and letting the user play the game.

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
using System.Windows.Threading;

namespace MasterGrab {


  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window {

    #region Constructor
    //      -----------

    Options options;
    readonly MapControl mapControl;


    public MainWindow() {
      InitializeComponent();

      StartButton.Click += StartButton_Click;
      OptionsButton.Click += OptionsButton_Click;
      ReplayButton.Click += ReplayButton_Click;
      InfoWindowComboBox.SelectionChanged += InfoWindowComboBox_SelectionChanged;
      ShowComboBox.SelectionChanged += ShowComboBox_SelectionChanged;
      NextStepButton.Click += NextStepButton_Click;
      AutoPlayComboBox.SelectionChanged += AutoPlayComboBox_SelectionChanged;
      HelpButton.Click += HelpButton_Click;

      options = Options.Default;
      mapControl = new MapControl(options, 1, isShowArmySizeChanged);
      MainDockPanel.Children.Add(mapControl);
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void StartButton_Click(object sender, RoutedEventArgs e) {
      if (MessageBox.Show("Start a new game ? ", "New game ?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)==MessageBoxResult.Yes) {
        mapControl.StartNewGame(options);
      }
    }


    private void OptionsButton_Click(object sender, RoutedEventArgs e) {
      var optionsWindow = new OptionsWindow(options) {
        Owner = this
      };
      bool? isOk = optionsWindow.ShowDialog();
      if (isOk.HasValue && isOk.Value) {
        options = optionsWindow.NewOptions;
        if (options.IsHumanPlaying) {
          NextStepButton.IsEnabled = true;
          AutoPlayStackPanel.Visibility = Visibility.Collapsed;
        } else {
          NextStepButton.IsEnabled = AutoPlayComboBox.SelectedIndex==0;
          AutoPlayStackPanel.Visibility = Visibility.Visible;
        }
        mapControl.StartNewGame(options);
      }
    }


    private void ReplayButton_Click(object sender, RoutedEventArgs e) {
      if (MessageBox.Show("Play the same game again ? ", "Replay ?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)==MessageBoxResult.Yes) {
        mapControl.Replay();
      }
    }


    private void InfoWindowComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      mapControl.InfoWindowMode = InfoWindowComboBox.SelectedIndex switch {
        0 => InfoWindowModeEnum.none,
        1 => InfoWindowModeEnum.ranking,
        2 => InfoWindowModeEnum.trace,
        _ => throw new NotSupportedException(),
      };
    }


    private void ShowComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      mapControl.IsShowArmySize = ShowComboBox.SelectedIndex switch {
        0 => true,
        1 => false,
        _ => throw new NotSupportedException(),
      };
    }


    private void isShowArmySizeChanged(bool isShowArmySize) {
      ShowComboBox.SelectedIndex =isShowArmySize ? 0 : 1;
    }


    private void NextStepButton_Click(object sender, RoutedEventArgs e) {
      mapControl.ControllerMove(Move.NoMove);
    }


    private void AutoPlayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      switch (AutoPlayComboBox.SelectedIndex) {
      case 0: NextStepButton.IsEnabled = true; mapControl.SetTimer(null); break;
      case 1: NextStepButton.IsEnabled = false; mapControl.SetTimer(TimeSpan.FromMilliseconds(50)); break;
      case 2: NextStepButton.IsEnabled = false; mapControl.SetTimer(TimeSpan.FromMilliseconds(200)); break;
      case 3: NextStepButton.IsEnabled = false; mapControl.SetTimer(TimeSpan.FromMilliseconds(500)); break;
      case 4: NextStepButton.IsEnabled = false; mapControl.SetTimer(TimeSpan.FromSeconds(1)); break;
      case 5: NextStepButton.IsEnabled = false; mapControl.SetTimer(TimeSpan.FromSeconds(2)); break;
      default: throw Tracer.Exception();
      }
    }


    private void HelpButton_Click(object sender, RoutedEventArgs e) {
      new HelpWindow().ShowDialog();
    }
    #endregion
  }
}
