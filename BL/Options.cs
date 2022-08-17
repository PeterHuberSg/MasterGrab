/********************************************************************************************************

MasterGrab.BL.Options
=====================

Options holds the data used to generate a game (=option) and various constraints. 

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MasterGrab {


  /// <summary>
  /// Options holds the data used to generate a game (=option) and various constraints. An Option which the user can change has
  /// also an OptionDef, defining name of the option, value range, etc.</para>
  /// A single option cannot be changed, since all option values are readonly. A complete new set needs to be created.</para>
  /// The static Options.Default provides a recommended set of options, which gets automatically applied. A class using this 
  /// library can use static InitialiseDefault() to change the default.
  /// </summary>
  public class Options {

    #region Properties
    //      ----------

    /// <summary>
    /// Number of countries generated
    /// </summary>
    public readonly int CountriesCount;
    public static readonly OptionDef<int> CountriesCountDef = new("Countries Count", "Number of countries generated.",
      defaultValue: 140, minValue: 10, maxValue: 777, randomOffset: 20, randomRange: 300, nullValue: int.MinValue);


    /// <summary>
    /// Percentage of mountains generated of total countries
    /// </summary>
    public readonly int MountainsPercentage;
    public static readonly OptionDef<int> MountainsPercentageDef = new("Mountains Percentage", "Percentage of mountains in the map.",
      defaultValue: 5, minValue: 0, maxValue: 50, randomOffset: 0, randomRange: 30, nullValue: int.MinValue);


    /// <summary>
    /// Number of map-pixels in x direction. This is usually the available screen width. Note that the biggest x coordinate is 
    /// XCount-1. 
    /// </summary>
    public int XCount; //gets set in MapControl.OnRenderContent


    /// <summary>
    /// Number of map-pixels in y direction. This is usually the available screen height. Note that the biggest y coordinate is 
    /// YCount-1
    /// </summary>
    public int YCount; //gets set in MapControl.OnRenderContent


    /// <summary>
    /// Maximal number of armies the biggest country can contain. Smaller countries can contain only fewer armies according to 
    /// their size (number of pixels).
    /// </summary>
    public readonly double ArmiesInBiggestCountry;
    public static readonly OptionDef<double> ArmiesInBiggestCountryDef = new("Armies In Biggest Country", 
      "Maximal number of armies the biggest country can contain.",
      defaultValue: 20.0, minValue: 10.0, maxValue: 200.0, randomOffset: 10.0, randomRange: 100.0, nullValue: double.MinValue);


    /// <summary>
    /// During a cycle (i.e. player and all robots have made one move), each country grows its armies based on its size and 
    /// the ArmyGrowthFactor. Factor 1 means that after a cycle each country reaches his maximum capcity.
    /// </summary>
    public readonly double ArmyGrowthFactor;
    public static readonly OptionDef<double> ArmyGrowthFactorDef = new("Army Growth Factor", 
      "1: Armies reach full capacity after every move. 0: armies don't grow at all.",
      defaultValue:0.1, minValue: 0.01, maxValue: 1.0, randomOffset: 0.03, randomRange: 0.3, nullValue: double.MinValue);


    /// <summary>
    /// When moving armies and there is an enemy neighbour, (neighbour.ArmySize * ProtectionFactor) armies will remain. With 
    /// several enemies nearby, the protection armies needed against the biggest enemy get retained.
    /// </summary>
    public readonly double ProtectionFactor;
    public static readonly OptionDef<double> ProtectionFactorDef = new("Protection Factor", 
      "When moving armies and there is an enemy neighbour, (neighbour.ArmySize * ProtectionFactor) armies will remain. "+ 
      "With several enemies nearby, the protection armies needed against the biggest enemy get retained.",  
      defaultValue:2.0/3.0, minValue: 0.0, maxValue: 10.0, randomOffset: 0.2, randomRange: 0.6, nullValue: double.MinValue);


    /// <summary>
    /// The attacker's armies get multiplied by the AttackFactor. If the result is bigger than the defending army, he wins.
    /// </summary>
    public readonly double AttackFactor;
    public static readonly OptionDef<double> AttackFactorDef = new("Attack Factor", 
      "The attacker's armies get multiplied by the AttackFactor. If the result is bigger than the defending army, he wins.", 
      defaultValue: 0.5, minValue: 0.0, maxValue: 1.0, randomOffset: 0.2, randomRange: 0.6, nullValue: double.MinValue);


    /// <summary>
    /// When attack wins, the attacker gains AttackBenefitFactor * Country.Capacity armies 
    /// </summary>
    public readonly double AttackBenefitFactor;
    public static readonly OptionDef<double> AttackBenefitFactorDef = new("AttackBenefit Factor", 
      "When attack wins, the attacker gains AttackBenefitFactor * Country.Capacity armies.", 
      defaultValue: 1.0, minValue: 0.1,maxValue: 10.0, randomOffset: 0.5, randomRange: 1.0, nullValue: double.MinValue);


    /// <summary>
    /// All options get set by random generator 
    /// </summary>
    public readonly bool IsRandomOptions;
    public static readonly OptionDef<bool> IsRandomOptionsDef = new("Random Options", 
      "All options get set by random generator.", 
      defaultValue: false, minValue: false, maxValue: true, randomOffset: false, randomRange: true, 
      nullValue: /*not used*/ false);


    /// <summary>
    /// Is human playing too or only robots ? 
    /// </summary>
    public readonly bool IsHumanPlaying;
    public static readonly OptionDef<bool> IsHumanPlayingDef = new("Is Human Playing",
      "Human is playing too, not just robots.",
      defaultValue: true, minValue: false, maxValue: true, randomOffset: true, randomRange: true,
      nullValue: /*not used*/ false);


    /// <summary>
    /// How are the contries of one player distributed, i.e. randomly over the map or clustered together ? 
    /// </summary>
    public readonly ClusteringEnum Clustering;
    public static readonly OptionDef<int> ClusteringDef = new("Countries distribution",
      "At start, all countries of one owner are randomly distributed over the map.",
      defaultValue: 0, minValue: 0, maxValue: (int)ClusteringEnum.horizontal, randomOffset: 0, randomRange: (int)ClusteringEnum.horizontal,
      nullValue: /*not used*/ -1);


    /// <summary>
    /// Defines which robots will be in the game
    /// </summary>
    public IReadOnlyList<RobotInfo> Robots => robots;

    readonly RobotInfo[] robots;


    /// <summary>
    /// Maximum number of colours which can be defined. They are used in the GUI to mark the countries and their owners.
    /// </summary>
    public const int ColorsCount = 16;


    /// <summary>
    /// A color is stored as 4 bytes A, R G, B
    /// </summary>
    public const int BytesPerColor = 4;


    /// <summary>
    /// Defines all colors which can maximal be used in the GUI to mark the countries and their owners.
    /// </summary>
    public byte[,] Colors = new byte[ColorsCount, BytesPerColor];


    #region Static Default Options and static constructor
    //      ---------------------------------------------

    //The user can change some options. If he wants to undo every change, he can apply the Default options. Options 
    //defines a set of proposed default options. They can be changed with InitialiseDefault() by another class using
    //this library, without changing the code here.

    /// <summary>
    /// Contains all the classes in the Robots directory which inherit from Robot
    /// </summary>
    public static IReadOnlyList<RobotInfo> RobotInfos => robotInfos;
    static readonly RobotInfo[] robotInfos;


    /// <summary>
    /// Contains all robots which can be used by the default options.
    /// </summary>
    public static IReadOnlyList<RobotInfo> DefaultRobots => defaultRobots;
    static readonly RobotInfo[] defaultRobots;


    /// <summary>
    /// Provides a set of recommended default option values. This can be used when starting the game or when the user
    /// has changed some values and wants to reset the options.
    /// </summary>
    public static Options Default { get; private set; }


    static Options() {
      var robotInfos = new List<RobotInfo>();
      var id = 0;
      var robotsAssembly = Assembly.LoadFrom(new FileInfo("Robots.dll").FullName);
      foreach (var robotType in robotsAssembly.ExportedTypes) {
        if (robotType.BaseType?.Name=="Robot") {
          var robotAttribute = robotType.GetCustomAttribute(typeof(RobotAttribute)) as RobotAttribute;
          robotInfos.Add(new RobotInfo(
            id++,
            robotType,
            robotAttribute?.Name??robotType.Name,
            robotAttribute?.Description,
            robotAttribute?.IsUsedForDefault??true));
        }
      }

      if (robotInfos.Count==0) {
        throw new Exception($"The must be at least 1 class inheriting from Robot in the Robots directory " +
          $"{new DirectoryInfo("Robots").FullName}.");
      }

      Options.robotInfos = robotInfos.ToArray();

      defaultRobots = RobotInfos.Where(ri => ri.IsUsedForDefault).ToArray();
      if (defaultRobots.Length==0) {
        //if no robot is marked to be included in the default options, then include all of them
        defaultRobots = RobotInfos.ToArray();
      }

      var robots = new RobotInfo[3];
      var defaultRobotsIndex = 0;
      for (var robotsIndex = 0; robotsIndex < robots.Length; robotsIndex++) {
        robots[robotsIndex] = DefaultRobots[defaultRobotsIndex++];
        defaultRobotsIndex %= DefaultRobots.Count;
      }

      Default = new Options(
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
        clustering: ClusteringEnum.random,
        robots: robots);
      Default.SetColor(0, 0xFF, 0xFF, 0x60, 0x60);
      Default.SetColor(1, 0xFF, 0xFF, 0xFF, 0x30);
      Default.SetColor(2, 0xFF, 0x80, 0xEF, 0x80);
      Default.SetColor(3, 0xFF, 0x80, 0xEF, 0xEF);
      Default.SetColor(4, 0xFF, 0x80, 0x80, 0xEF);
      Default.SetColor(5, 0xFF, 0xEF, 0x80, 0xEF);
      Default.SetColor(6, 0xFF, 0xE0, 0xE0, 0x70);
      Default.SetColor(7, 0xFF, 0xE0, 0xE0, 0xE0);
      Default.SetColor(8, 0xFF, 0xCF, 0x80, 0x80);
      Default.SetColor(9, 0xFF, 0xCF, 0xFF, 0x30);
      Default.SetColor(10, 0xFF, 0x80, 0xBF, 0x80);
      Default.SetColor(11, 0xFF, 0x80, 0xBF, 0xEF);
      Default.SetColor(12, 0xFF, 0x80, 0x80, 0xBF);
      Default.SetColor(13, 0xFF, 0xEF, 0x80, 0xBF);
      Default.SetColor(14, 0xFF, 0xA0, 0xE0, 0x70);
      Default.SetColor(15, 0xFF, 0xA0, 0xA0, 0xE0);
    }

    /// <summary>
    /// A class using this library can change Options.Default without changing the code in Options.cs
    /// </summary>
    public static void InitialiseDefault(Options options) {
      Default = options;
    }
    #endregion
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor
    /// </summary>
    public Options(
      int countriesCount,
      int mountainsPercentage,
      int xCount,
      int yCount,
      double armiesInBiggestCountry,
      double armyGrowthFactor,
      double protectionFactor,
      double attackFactor,
      double attackBenefitFactor,
      bool isRandomOptions,
      bool isHumanPlaying,
      ClusteringEnum clustering,
      RobotInfo[] robots) 
    {
      CountriesCount = countriesCount;
      MountainsPercentage = mountainsPercentage;
      XCount = xCount;
      YCount = yCount;
      ArmiesInBiggestCountry = armiesInBiggestCountry;
      ArmyGrowthFactor = armyGrowthFactor;
      ProtectionFactor = protectionFactor;
      AttackFactor = attackFactor;
      AttackBenefitFactor = attackBenefitFactor;
      IsRandomOptions = isRandomOptions;
      IsHumanPlaying = isHumanPlaying;
      Clustering = clustering;
      this.robots = robots;
    }


    /// <summary>
    /// returns random options, keeps screen size, IsHumanPlaying and Colors from options
    /// </summary>
    public Options(Options options) {

      CountriesCount = randomize(CountriesCountDef);
      MountainsPercentage = randomize(MountainsPercentageDef);
      XCount = options.XCount;
      YCount = options.YCount;
      ArmiesInBiggestCountry = randomize(ArmiesInBiggestCountryDef, 0);
      ArmyGrowthFactor = randomize(ArmyGrowthFactorDef, 2);
      ProtectionFactor = randomize(ProtectionFactorDef, 2);
      AttackFactor = randomize(AttackFactorDef, 2);
      AttackBenefitFactor = randomize(AttackBenefitFactorDef, 2);
      IsRandomOptions = true;
      IsHumanPlaying = options.IsHumanPlaying;
      Clustering = randomizeClustering();
      var robotsCount = random.Next(5)+1;
      if (!IsHumanPlaying) {
        robotsCount++;
      }
      var robots = new RobotInfo[robotsCount];
      for (var robotsIndex = 0; robotsIndex < robotsCount; robotsIndex++) {
        robots[robotsIndex] = DefaultRobots[random.Next(DefaultRobots.Count)];
      }
      this.robots = robots;
      for (var colorsIndex = 0; colorsIndex<ColorsCount; colorsIndex++) {
        Colors[colorsIndex, 0] = options.Colors[colorsIndex, 0];
        Colors[colorsIndex, 1] = options.Colors[colorsIndex, 1];
        Colors[colorsIndex, 2] = options.Colors[colorsIndex, 2];
        Colors[colorsIndex, 3] = options.Colors[colorsIndex, 3];
      }
    }


    static readonly Random random = new();


    private static int randomize(OptionDef<int> optionDef) {
      return random.Next(optionDef.RandomOffset, optionDef.RandomRange);
    }


    private static double randomize(OptionDef<double> optionDef, int digits) {
        return Math.Round(optionDef.RandomOffset + (optionDef.RandomRange-optionDef.RandomOffset)*random.NextDouble(), digits);
    }


    private ClusteringEnum randomizeClustering() {
      const int chanceForDiagHorVertDistribution = 3;
      const int chanceCompactDistribution = 2 * chanceForDiagHorVertDistribution;
      const int chanceRandomeDistribution = chanceCompactDistribution;
      const int allChancesCount = chanceForDiagHorVertDistribution + chanceCompactDistribution + chanceRandomeDistribution;
      var randomResult = random.Next(0, allChancesCount);
      if (randomResult<chanceRandomeDistribution) return ClusteringEnum.random;
      if (randomResult<chanceRandomeDistribution + chanceCompactDistribution) return ClusteringEnum.compact;
      var clustering = ClusteringEnum.diagonal + randomResult - chanceRandomeDistribution - chanceCompactDistribution;
      if (clustering<ClusteringEnum.diagonal || clustering>ClusteringEnum.horizontal)
        throw new ArgumentOutOfRangeException(((int)clustering).ToString());

      return clustering;
    }
      #endregion


      #region Methods
      //       -------

      /// <summary>
      /// Sets one of the predefined colors.
      /// </summary>
      public void SetColor(int ColorIndex, int A, int R, int G, int B) {
      Colors[ColorIndex, 0] = (byte)A;
      Colors[ColorIndex, 1] = (byte)R;
      Colors[ColorIndex, 2] = (byte)G;
      Colors[ColorIndex, 3] = (byte)B;
    }


    /// <summary>
    /// Copies one color from Options.Default[colorIndex] to Options.Colors[colorIndex]
    /// </summary>
    /// <param name="colorIndex"></param>
    public void UseDefaultColor(int colorIndex) {
      Colors[colorIndex, 0] = Default.Colors[colorIndex, 0];
      Colors[colorIndex, 1] = Default.Colors[colorIndex, 1];
      Colors[colorIndex, 2] = Default.Colors[colorIndex, 2];
      Colors[colorIndex, 3] = Default.Colors[colorIndex, 3];
    }


    /// <summary>
    /// Writes the Options values into the stringBuilder
    /// </summary>
    /// <param name="stringBuilder"></param>
    public void AppendTo(StringBuilder stringBuilder) {
      stringBuilder.Append("CountriesCount: ");
      stringBuilder.AppendLine(CountriesCount.ToString());

      stringBuilder.Append("MountainsPercentage: ");
      stringBuilder.AppendLine(MountainsPercentage.ToString());

      stringBuilder.Append("ArmiesInBiggestCountry: ");
      stringBuilder.AppendLine(ArmiesInBiggestCountry.ToString());

      stringBuilder.Append("ArmyGrowthFactor: ");
      stringBuilder.AppendLine(ArmyGrowthFactor.ToString());

      stringBuilder.Append("ProtectionFactor: ");
      stringBuilder.AppendLine(ProtectionFactor.ToString());

      stringBuilder.Append("AttackFactor: ");
      stringBuilder.AppendLine(AttackFactor.ToString());

      stringBuilder.Append("AttackBenefitFactor: ");
      stringBuilder.AppendLine(AttackBenefitFactor.ToString());

      stringBuilder.Append("IsRandomOptions: ");
      stringBuilder.AppendLine(IsRandomOptions.ToString());

      stringBuilder.Append("XCount: ");
      stringBuilder.AppendLine(XCount.ToString());

      stringBuilder.Append("YCount: ");
      stringBuilder.AppendLine(YCount.ToString());

      stringBuilder.Append("IsHumanPlaying: ");
      stringBuilder.AppendLine(IsHumanPlaying.ToString());

      for (var robotIndex = 0; robotIndex < robots.Length; robotIndex++) {
        stringBuilder.Append($"Robot{robotIndex}: ");
        stringBuilder.AppendLine(robots[robotIndex].ToString());
      }
      stringBuilder.AppendLine();
    }


    /// <summary>
    /// returns all option values in one string, each option on its own line.
    /// </summary>
    /// <returns></returns>
    public string ToOptionString() {
      var stringBuilder = new StringBuilder();
      AppendTo(stringBuilder);
      return stringBuilder.ToString();
    }
    #endregion
  }
}
