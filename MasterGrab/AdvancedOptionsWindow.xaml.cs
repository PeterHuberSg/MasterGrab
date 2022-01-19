/********************************************************************************************************

MasterGrab.MasterGrab.AdvancedOptionsWindow
===========================================

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

using System.Windows;
using System.Windows.Controls;


namespace MasterGrab {


  /// <summary>
  /// Interaction logic for AdvancedOptionsWindow.xaml
  /// </summary>
  public partial class AdvancedOptionsWindow: Window {

    #region Properties
    //      ----------

    public int MountainsPercentage { get; private set; }


    public double ArmiesInBiggestCountry { get; private set; }


    public double ArmyGrowthFactor { get; private set; }


    public double ProtectionFactor { get; private set; }


    public double AttackFactor { get; private set; }


    public double AttackBenefitFactor { get; private set; }


    public bool IsRandomOptions { get; private set; }


    public readonly bool IsReadOnly;
    #endregion


    #region Constructor
    //      -----------

    public AdvancedOptionsWindow(
      int mountainsPercentage, 
      double armiesInBiggestCountry, 
      double armyGrowthFactor, 
      double protectionFactor, 
      double attackFactor, 
      double attackBenefitFactor,
      bool isRandomOptions,
      bool isReadOnly = false) 
    {
      InitializeComponent();

      MountainsPercentageNumberTextBox.Set(Options.MountainsPercentageDef, mountainsPercentage);
      addDefault(MountainsPercentageLabel, Options.MountainsPercentageDef);
      ArmiesInBiggestCountryNumberTextBox.Set(Options.ArmiesInBiggestCountryDef, armiesInBiggestCountry);
      addDefault(ArmiesInBiggestCountryLabel, Options.ArmiesInBiggestCountryDef);
      ArmyGrowthFactorNumberTextBox.Set(Options.ArmyGrowthFactorDef, armyGrowthFactor);
      addDefault(ArmyGrowthFactorLabel, Options.ArmyGrowthFactorDef);
      ProtectionFactorNumberTextBox.Set(Options.ProtectionFactorDef, protectionFactor);
      addDefault(ProtectionFactorLabel, Options.ProtectionFactorDef);
      AttackFactorNumberTextBox.Set(Options.AttackFactorDef, attackFactor);
      addDefault(AttackFactorLabel, Options.AttackFactorDef);
      AttackBenefitFactorNumberTextBox.Set(Options.AttackBenefitFactorDef, attackBenefitFactor);
      addDefault(AttackBenefitFactorLabel, Options.AttackBenefitFactorDef);
      RandomOptionsChechbox.IsChecked = isRandomOptions;
      RandomOptionsChechbox.ToolTip = Options.IsRandomOptionsDef.ToolTip;
      IsReadOnly = isReadOnly;

      if (isReadOnly) {
        MountainsPercentageNumberTextBox.IsEnabled = false;
        ArmiesInBiggestCountryNumberTextBox.IsEnabled = false;
        ArmyGrowthFactorNumberTextBox.IsEnabled = false;
        ProtectionFactorNumberTextBox.IsEnabled = false;
        AttackFactorNumberTextBox.IsEnabled = false;
        AttackBenefitFactorNumberTextBox.IsEnabled = false;
        RandomOptionsChechbox.IsEnabled = false;
        RandomOptionsChechbox.IsEnabled = false;
        DefaultButton.IsEnabled = false;
      } else {
        DefaultButton.Click += DefaultButton_Click;
      }
      OkButton.Click += OkButton_Click;
    }


    private static void addDefault(Label label, OptionDef optionDef) {
      label.Content = optionDef.Name + " (" + optionDef.DefaultValueString + ")";
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void DefaultButton_Click(object sender, RoutedEventArgs e) {
      var defaultOptions = Options.Default;
      MountainsPercentageNumberTextBox.Value = defaultOptions.MountainsPercentage;
      ArmiesInBiggestCountryNumberTextBox.Value = defaultOptions.ArmiesInBiggestCountry;
      ArmyGrowthFactorNumberTextBox.Value = defaultOptions.ArmyGrowthFactor;
      ProtectionFactorNumberTextBox.Value = defaultOptions.ProtectionFactor;
      AttackFactorNumberTextBox.Value = defaultOptions.AttackFactor;
      AttackBenefitFactorNumberTextBox.Value = defaultOptions.AttackBenefitFactor;
      RandomOptionsChechbox.IsChecked = defaultOptions.IsRandomOptions;
    }



    private void OkButton_Click(object sender, RoutedEventArgs e) {
      if (!IsReadOnly) {
        MountainsPercentage = (int)MountainsPercentageNumberTextBox.Value;
        ArmiesInBiggestCountry = (double)ArmiesInBiggestCountryNumberTextBox.Value;
        ArmyGrowthFactor = (double)ArmyGrowthFactorNumberTextBox.Value;
        ProtectionFactor = (double)ProtectionFactorNumberTextBox.Value;
        AttackFactor = (double)AttackFactorNumberTextBox.Value;
        AttackBenefitFactor= (double)AttackBenefitFactorNumberTextBox.Value;
        IsRandomOptions = RandomOptionsChechbox.IsChecked!.Value;
        DialogResult = true;
      }

      Close();
    }
    #endregion
  }
}
