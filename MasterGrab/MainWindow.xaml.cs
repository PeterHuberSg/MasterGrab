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
      HelpButton.Click += HelpButton_Click;

      Options defaultOptions = new Options(
      countriesCount: 140,
      mountainsPercentage: 5,
      xCount: int.MinValue,
      yCount: int.MinValue,
      armiesInBiggestCountry: 20.0,
      armyGrowthFactor: 0.1,
      protectionFactor: 2.0 / 3.0,
      attackFactor: 0.5,
      attackBenefitFactor: 1.0,
      isRandomOptions: false,
      isHumanPlaying: true,
      robotTypes: new Type[] { typeof(BasicRobot), typeof(BasicRobot), typeof(BasicRobot) });
      defaultOptions.SetColor( 0, 0xFF, 0xFF, 0x60, 0x60);
      defaultOptions.SetColor( 1, 0xFF, 0xFF, 0xFF, 0x30);
      defaultOptions.SetColor( 2, 0xFF, 0x80, 0xEF, 0x80);
      defaultOptions.SetColor( 3, 0xFF, 0x80, 0xEF, 0xEF);
      defaultOptions.SetColor( 4, 0xFF, 0x80, 0x80, 0xEF);
      defaultOptions.SetColor( 5, 0xFF, 0xEF, 0x80, 0xEF);
      defaultOptions.SetColor( 6, 0xFF, 0xE0, 0xE0, 0x70);
      defaultOptions.SetColor( 7, 0xFF, 0xE0, 0xE0, 0xE0);
      defaultOptions.SetColor( 8, 0xFF, 0xCF, 0x80, 0x80);
      defaultOptions.SetColor( 9, 0xFF, 0xCF, 0xFF, 0x30);
      defaultOptions.SetColor(10, 0xFF, 0x80, 0xBF, 0x80);
      defaultOptions.SetColor(11, 0xFF, 0x80, 0xBF, 0xEF);
      defaultOptions.SetColor(12, 0xFF, 0x80, 0x80, 0xBF);
      defaultOptions.SetColor(13, 0xFF, 0xEF, 0x80, 0xBF);
      defaultOptions.SetColor(14, 0xFF, 0xA0, 0xE0, 0x70);
      defaultOptions.SetColor(15, 0xFF, 0xA0, 0xA0, 0xE0);
      Options.InitialiseDefault(defaultOptions);

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
        AutoPlayStackPanel.Visibility = options.IsHumanPlaying ? Visibility.Collapsed : Visibility.Visible;
        mapControl.StartNewGame(options);
      }
    }


    private void ReplayButton_Click(object sender, RoutedEventArgs e) {
      if (MessageBox.Show("Play the same game again ? ", "Replay ?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)==MessageBoxResult.Yes) {
        mapControl.Replay();
      }
    }


    private void InfoWindowComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      mapControl.InfoWindow = InfoWindowComboBox.SelectedIndex switch {
        0 => InfoWindowEnum.none,
        1 => InfoWindowEnum.ranking,
        2 => InfoWindowEnum.trace,
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


    private void HelpButton_Click(object sender, RoutedEventArgs e) {
      new HelpWindow().ShowDialog();
    }
    #endregion
  }
}
