﻿/********************************************************************************************************

MasterGrab.MasterGrab.OptionsWindow
===================================

Setting of advanced game options, which will change how the game behaves.

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


namespace MasterGrab {


  /// <summary>
  /// Interaction logic for OptionsWindow.xaml
  /// </summary>
  public partial class OptionsWindow: Window {

    #region Properties
    //      ----------

    /// <summary>
    /// Options as set by user when he open OptionsWindow
    /// </summary>
    public Options OriginalOptions { get; private set; }


    /// <summary>
    /// Options as selected by user when he clicks Apply button
    /// </summary>
    public Options NewOptions { get; private set; }
    #endregion


    #region Constructor
    //      -----------

    readonly int robotsCount;
    readonly TextBlock[,] textblocks;
    readonly ColorBox[] colorBoxes;
    readonly ComboBox[] comboBoxes;
    readonly ComboBoxItem[] robotsComboboxItems;

    int mountainsPercentage;
    double armiesInBiggestCountry;
    double armyGrowthFactor;
    double protectionFactor;
    double attackFactor;
    double attackBenefitFactor;
    bool isRandomOptions;


    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. NewOptions gets set when user clicks apply button
    public OptionsWindow(Options options) {
    #pragma warning restore CS8618
      InitializeComponent();

      OriginalOptions = options;
      NumberOfCountriesNumberTextBox.Set(Options.CountriesCountDef, options.CountriesCount);
      ClusteredCheckBox.IsChecked = options.IsClusteredOwnership;
      ClusteredCheckBox.ToolTip = Options.IsClusteredOwnershipDef.ToolTip;

      AdvancedOptionsButton.Click += AdvancedOptionsButton_Click;
      DefaultButton.Click += DefaultButton_Click;
      ApplyButton.Click += ApplyButton_Click;

      //store values for AdvancedOptionsWindow
      mountainsPercentage = options.MountainsPercentage;
      armiesInBiggestCountry = options.ArmiesInBiggestCountry;
      armyGrowthFactor = options.ArmyGrowthFactor;
      protectionFactor = options.ProtectionFactor;
      attackFactor = options.AttackFactor;
      attackBenefitFactor = options.AttackBenefitFactor;
      isRandomOptions = options.IsRandomOptions;

      //RobotsGrid
      robotsCount = Options.ColorsCount - 1;
      textblocks = new TextBlock[robotsCount, 4];
      colorBoxes = new ColorBox[robotsCount];
      comboBoxes = new ComboBox[robotsCount];
      RobotsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
      RobotsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
      RobotsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
      RobotsGrid.ColumnDefinitions.Add(new ColumnDefinition());

      //ComboBoxItem values for robots
      robotsComboboxItems = new ComboBoxItem[Options.RobotInfos.Count];
      for (int robotInfosIndex = 0; robotInfosIndex < Options.RobotInfos.Count; robotInfosIndex++) {
        var robotInfo = Options.RobotInfos[robotInfosIndex];
        robotsComboboxItems[robotInfosIndex] = new ComboBoxItem { Content = robotInfo.Name, ToolTip = robotInfo.Description };
      }

      copyOptionsToScreen(options);

      updateButtonState();
      PlayerEnabledCheckBox.Checked += PlayerEnabledCheckBox_Checked;
      PlayerEnabledCheckBox.Unchecked += PlayerEnabledCheckBox_Checked;
      AddButton.Click += AddButton_Click;
      RemoveButton.Click += RemoveButton_Click;
    }


    private void copyOptionsToScreen(Options options) {
      PlayerEnabledCheckBox.IsChecked = options.IsHumanPlaying;
      PlayerColorBox.Visibility = PlayerEnabledCheckBox.IsChecked.Value ? Visibility.Visible : Visibility.Hidden;
      PlayerColorBox.Color = Color.FromArgb(options.Colors[0, 0], options.Colors[0, 1], options.Colors[0, 2], options.Colors[0, 3]);
      for (var robotIndex = 0; robotIndex < options.Robots.Count; robotIndex++) {
        addRobotRow(options, robotIndex);
      }
    }


    readonly int lineWidth = 1;


    private void addRobotRow(Options options, int robotIndex) {
      RobotsGrid.RowDefinitions.Add(new RowDefinition());
      TextBlock textblock;
      if (textblocks[robotIndex, 0]==null) {
        textblock = new TextBlock {Padding=new Thickness(2) };
        textblocks[robotIndex, 0] = textblock;
        textblock.Text = "Robot" + (robotIndex + 1);
      } else {
        textblock = textblocks[robotIndex, 0];
      }
      textblock.Background = Brushes.White;
      textblock.Margin = robotIndex==0 ? new Thickness(lineWidth) : new Thickness(lineWidth, 0, lineWidth, lineWidth);
      RobotsGrid.Children.Add(textblock);
      Grid.SetRow(textblock, robotIndex);
      Grid.SetColumn(textblock, 0);

      ColorBox colorBox;
      if (colorBoxes[robotIndex]==null) {
        colorBox = new ColorBox();
        colorBoxes[robotIndex] = colorBox;
        int robotColorIndex = robotIndex + 1;
        colorBox.Color = Color.FromArgb(options.Colors[robotColorIndex, 0], options.Colors[robotColorIndex, 1], options.Colors[robotColorIndex, 2], options.Colors[robotColorIndex, 3]);
      } else {
        colorBox = colorBoxes[robotIndex];
      }
      colorBox.Margin = robotIndex==0 ? new Thickness(0, lineWidth, lineWidth, lineWidth) : new Thickness(0, 0, lineWidth, lineWidth);
      RobotsGrid.Children.Add(colorBox);
      Grid.SetRow(colorBox, robotIndex);
      Grid.SetColumn(colorBox, 1);

      ComboBox comboBox;
      if (comboBoxes[robotIndex]==null) {
        comboBox = new ComboBox {ItemsSource = robotsComboboxItems};
        comboBoxes[robotIndex] = comboBox;
      } else {
        comboBox = comboBoxes[robotIndex];
      }
      if (robotIndex<options.Robots.Count) {
        var robotInfo = options.Robots[robotIndex];
        for (int robotInfoIndex = 0; robotInfoIndex < Options.RobotInfos.Count; robotInfoIndex++) {
          if (Options.RobotInfos[robotInfoIndex]==robotInfo) {
            comboBox.SelectedIndex = robotInfoIndex;
            break;
          }
        }
      } else {
        comboBox.SelectedIndex = robotIndex==0 ? 0 : comboBoxes[robotIndex-1].SelectedIndex;
      }
      comboBox.Margin = robotIndex==0 ? new Thickness(0, lineWidth, lineWidth, lineWidth) : new Thickness(0, 0, lineWidth, lineWidth);
      RobotsGrid.Children.Add(comboBox);
      Grid.SetRow(comboBox, robotIndex);
      Grid.SetColumn(comboBox, 2);
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void PlayerEnabledCheckBox_Checked(object sender, RoutedEventArgs e) {
      PlayerColorBox.Visibility = PlayerEnabledCheckBox.IsChecked==true ? Visibility.Visible : Visibility.Hidden;
      updateButtonState();
    }


    private void AddButton_Click(object sender, RoutedEventArgs e) {
      if (RobotsGrid.RowDefinitions.Count<robotsCount) {
        addRobotRow(OriginalOptions, RobotsGrid.RowDefinitions.Count);
      }
      updateButtonState();
    }


    private void updateButtonState() {
      AddButton.IsEnabled = RobotsGrid.RowDefinitions.Count<robotsCount;
      RemoveButton.IsEnabled = PlayerEnabledCheckBox.IsChecked!.Value ? 
        RobotsGrid.RowDefinitions.Count>0 : RobotsGrid.RowDefinitions.Count>1;
      ApplyButton.IsEnabled = RemoveButton.IsEnabled;
    }


    private void RemoveButton_Click(object sender, RoutedEventArgs e) {
      if (RobotsGrid.RowDefinitions.Count>0) {
        removeRobot();
      }
      updateButtonState();
    }


    private void removeRobot() {
      int robotId = RobotsGrid.RowDefinitions.Count-1;
      RobotsGrid.RowDefinitions.RemoveAt(robotId);
      RobotsGrid.Children.Remove(textblocks[robotId, 0]);
      RobotsGrid.Children.Remove(colorBoxes[robotId]);
      RobotsGrid.Children.Remove(comboBoxes[robotId]);
    }


    private void AdvancedOptionsButton_Click(object sender, RoutedEventArgs e) {
      AdvancedOptionsWindow advancedOptionsWindow = new AdvancedOptionsWindow(mountainsPercentage, armiesInBiggestCountry, 
        armyGrowthFactor, protectionFactor, attackFactor, attackBenefitFactor, isRandomOptions);
      bool? dialogResult = advancedOptionsWindow.ShowDialog(); 

      if (dialogResult.HasValue && dialogResult.Value) {
        mountainsPercentage = advancedOptionsWindow.MountainsPercentage;
        armiesInBiggestCountry = advancedOptionsWindow.ArmiesInBiggestCountry;
        armyGrowthFactor = advancedOptionsWindow.ArmyGrowthFactor;
        protectionFactor = advancedOptionsWindow.ProtectionFactor;
        attackFactor = advancedOptionsWindow.AttackFactor;
        attackBenefitFactor = advancedOptionsWindow.AttackBenefitFactor;
        isRandomOptions = advancedOptionsWindow.IsRandomOptions;
      }
    }


    private void DefaultButton_Click(object sender, RoutedEventArgs e) {
      while (RobotsGrid.RowDefinitions.Count>0) {
        removeRobot();
      }
      copyOptionsToScreen(Options.Default);
      updateButtonState();
    }


    private void ApplyButton_Click(object sender, RoutedEventArgs e) {
      DialogResult = true;

      RobotInfo[] robots = new RobotInfo[RobotsGrid.RowDefinitions.Count];
      for (int robotsIndex = 0; robotsIndex < robots.Length; robotsIndex++) {
        robots[robotsIndex] = Options.RobotInfos[comboBoxes[robotsIndex].SelectedIndex];
      }

      //these options get overwritten when isRandomOptions is true
      NewOptions = new Options(
        countriesCount: (int)NumberOfCountriesNumberTextBox.Value,
        xCount: OriginalOptions.XCount,
        yCount: OriginalOptions.YCount,
        mountainsPercentage: mountainsPercentage,
        armiesInBiggestCountry: armiesInBiggestCountry,
        armyGrowthFactor: armyGrowthFactor,
        protectionFactor: protectionFactor,
        attackFactor: attackFactor,
        attackBenefitFactor: attackBenefitFactor,
        isRandomOptions: isRandomOptions,
        isHumanPlaying: PlayerEnabledCheckBox.IsChecked??false,
        isClusteredOwnership: ClusteredCheckBox.IsChecked!.Value,
        robots: robots);

      var playerColor = PlayerColorBox.Color;
      NewOptions.SetColor(0, playerColor.A, playerColor.R, playerColor.G, playerColor.B);
      for (int robotsIndex = 0; robotsIndex < robots.Length; robotsIndex++) {
        var robotColor = colorBoxes[robotsIndex].Color;
        NewOptions.SetColor(robotsIndex+1, robotColor.A, robotColor.R, robotColor.G, robotColor.B);
      }
      for (int colorIndex = robots.Length + 1; colorIndex < NewOptions.Colors.Length/4; colorIndex++) {
        NewOptions.UseDefaultColor(colorIndex);
      }
    }
    #endregion
  }
}
