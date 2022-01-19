/********************************************************************************************************

MasterGrab.BL.Game
==================

Game holds all data about a game:</para>
+ Map with the countries</para>
+ Players</para>
+ Results from the last move of every player (statistics)</para>

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

using System.Collections.Generic;
using System.Text;


namespace MasterGrab {


  /// <summary>
  /// A Game holds all data about a game:</para>
  /// + Map with the countries</para>
  /// + Players</para>
  /// + Results from the last move of every player (statistics)</para>
  /// 
  /// After every move, some Game data might change. Each player, when it's his turn to make a move, gets his own Game
  /// copy. This prevents one player making a change giving another player a problem and prevents cheating.</para>
  /// 
  /// There is also a GameFix class, which is used to construct a new game and holds all the data which does not change
  /// when players make moves. Game has no access to GameFix. It accesses country data through the Map.</para>
  /// 
  /// The only class which can change the Game data (i.e. who holds which country, what is the army size in a country, etc.) is
  /// the GameController.</para>
  /// 
  /// GameContoller -> GameFix -> Game -> Map -> Country -> CountryFix
  /// </summary>
  public class Game {

    #region Properties
    //      ----------

    /// <summary>
    /// During a cycle (i.e. player and all robots have made one move), each country grows its armies based on its size and 
    /// the ArmyGrowthFactor. Factor 1 means that after a cycle each country reaches his maximum capcity.
    /// </summary>
    public double ArmyGrowthFactor { get; private set; }


    /// <summary>
    /// When moving armies and there is an enemy neighbour, (neighbour.ArmySize * ProtectionFactor) armies will remain. With 
    /// several enemies nearby, the protection armies needed against the biggest enemy get retained.
    /// </summary>
    public double ProtectionFactor { get; private set; }


    /// <summary>
    /// The attacker's armies get multiplied by the AttackFactor. If the result is bigger than the defending army, he wins.
    /// </summary>
    public double AttackFactor { get; private set; }


    /// <summary>
    /// When attack wins, the attacker gains AttackBenefitFactor * Country.Capacity armies 
    /// </summary>
    public double AttackBenefitFactor { get; private set; }


    /// <summary>
    /// Number of mountains in the Map
    /// </summary>
    public int MountainsCount;


    /// <summary>
    /// Id of GUI Player, i.e. the person who plays the game on the PC. By starting the GUI he starts the creation of 
    /// the Game. He can change some Options and restart a new Game. While a Robot gets called when he can make a move,
    /// the GuiPlayer gets informed once the Robots have made their moves and GameController then waits for the GuiPlayer
    /// to make his next Move.
    /// </summary>
    public readonly int GuiPlayerId;


    /// <summary>
    /// Collection of all Players
    /// </summary>
    public IReadOnlyList<Player> Players => players;
    readonly Player[] players;


    /// <summary>
    /// Collection of all countries
    /// </summary>
    public readonly Map Map;


    /// <summary>
    /// Collection of all results. It holds for each player the result of his last move
    /// </summary>
    public IReadOnlyList<Result> Results => ResultsInternal;
    internal Result[] ResultsInternal { get; private set; }


    /// <summary>
    /// Indicates if user owns all countries or has lost all his countries
    /// </summary>
    public bool HasGameFinished { get; internal set; }


    /// <summary>
    /// Indicates if user won, meaning he owns all countries once game has finished
    /// </summary>
    public bool HasUserWon { get; internal set; }
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor for randomly created countries. Normally GameFix calls this constructor. 
    /// </summary>
    public Game(Options options, IReadOnlyList<CountryFix> countryFixArray, int mountainsCount, int guiPlayerId){
      ArmyGrowthFactor = options.ArmyGrowthFactor;
      ProtectionFactor  = options.ProtectionFactor;
      AttackFactor  = options.AttackFactor;
      AttackBenefitFactor = options.AttackBenefitFactor;
      MountainsCount = mountainsCount;
      if(options.IsHumanPlaying) {
        GuiPlayerId = guiPlayerId;
        players = new Player[options.RobotTypes.Count + 1];
      } else {
        GuiPlayerId = int.MinValue;
        players = new Player[options.RobotTypes.Count];
      }
      for (var playerIndex = 0; playerIndex < players.Length; playerIndex++) {
        players[playerIndex] = new Player(playerIndex, this);
      }

      ResultsInternal = new Result[players.Length];
      for (var resultsIndex = 0; resultsIndex < ResultsInternal.Length; resultsIndex++) {
        ResultsInternal[resultsIndex] = new Result(MoveTypeEnum.none, isError: false, playerId: resultsIndex, 
          defenderId: int.MinValue, countryId: int.MinValue, selectedCountryIds: null, isSuccess: true,
          beforeArmies: null, afterArmies: null, probability: double.MinValue);
      }

      HasGameFinished = false;
      HasUserWon = false;
      Map = new Map(options, this, countryFixArray);
    }


    /// <summary>
    /// Cloning constructor, creates deep copy of game, i.e. the clone and the original do not share any objects with data which 
    /// can change aftera Move.
    /// </summary>
    public Game(Game game) {
      ArmyGrowthFactor = game.ArmyGrowthFactor;
      ProtectionFactor  = game.ProtectionFactor;
      AttackFactor  = game.AttackFactor;
      AttackBenefitFactor = game.AttackBenefitFactor;
      MountainsCount = game.MountainsCount;
      GuiPlayerId = game.GuiPlayerId;
      players = new Player[game.Players.Count];
      for (var playerIndex = 0; playerIndex < game.Players.Count; playerIndex++) {
        players[playerIndex] = new Player(this, game.Players[playerIndex]);
      }

      Map = new Map(this, game.Map);

      ResultsInternal = new Result[Players.Count];
      for (var resultIndex = 0; resultIndex < ResultsInternal.Length; resultIndex++) {
        ResultsInternal[resultIndex] = game.Results[resultIndex].Clone();
      }
      HasGameFinished = game.HasGameFinished;
      HasUserWon = game.HasUserWon;
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Returns Statistics (rank, number of countries and armies, ...) about of the players. The sttistics are sorted by Player.Id.
    /// </summary>
    public Statistic[] GetPlayerStatistics() {
      //initialise empty statistics
      var statistics = new Statistic[Players.Count];
      for (var statisticsIndex = 0; statisticsIndex < statistics.Length; statisticsIndex++) {
        statistics[statisticsIndex] = new Statistic();
      }

      //loop through all countries to add up countries and armies per Player
      var pointsPerCountry = 1.0 / (Map.Count - Map.Game.MountainsCount);
      foreach (var country in Map) {
        if (country.OwnerId!=int.MinValue) {
          statistics[country.OwnerId].Countries++;
          statistics[country.OwnerId].CountriesPercent += pointsPerCountry;
          statistics[country.OwnerId].Armies += (int)country.ArmySize;
          statistics[country.OwnerId].Size += country.Size;
        }
      }

      //calculate ranking based on number of countries owned by Player. If 2 Players have the same number of countries, they
      //have also the same rank
      var searchBelow = int.MaxValue;
      var equalRank = 0;
      var ranksCount = 0;
      for (var rankIndex = 0; rankIndex < statistics.Length; rankIndex++) {
        var maxCountries = int.MinValue;
        var rankStatisticsIndex = int.MinValue;
        for (var statisticsIndex = 0; statisticsIndex < statistics.Length; statisticsIndex++) {
          var countriesCount = statistics[statisticsIndex].Countries;
          if (countriesCount==searchBelow && statistics[statisticsIndex].Rank==0) {
            rankStatisticsIndex = statisticsIndex;
            break;
          }
          if (countriesCount<searchBelow && maxCountries<countriesCount) {
            maxCountries = countriesCount;
            rankStatisticsIndex = statisticsIndex;
          }
        }
        if (statistics[rankStatisticsIndex].Countries==searchBelow) {
          statistics[rankStatisticsIndex].Rank = equalRank;
        } else {
          statistics[rankStatisticsIndex].Rank = rankIndex + 1;
          equalRank = rankIndex + 1; ;
          searchBelow = maxCountries;
        }
        ranksCount++;
        if (ranksCount>=statistics.Length) break;
      }


      return statistics;
    }


    /// <summary>
    /// Adds the Game data (Options, Players, Results and each Country) to stringBuilder
    /// </summary>
    public void AppendTo(StringBuilder stringBuilder) {
      stringBuilder.AppendLine("Options");
      stringBuilder.AppendLine("-------");
      stringBuilder.AppendLine($"ArmyGrowthFactor: {ArmyGrowthFactor}");
      stringBuilder.AppendLine($"ProtectionFactor: {ProtectionFactor}");
      stringBuilder.AppendLine($"AttackFactor: {AttackFactor}");
      stringBuilder.AppendLine($"AttackBenefitFactor: {AttackBenefitFactor}");
      stringBuilder.AppendLine();

      stringBuilder.AppendLine("Players");
      stringBuilder.AppendLine("-------");
      foreach (var player in Players) {
        stringBuilder.AppendLine(player.ToString());
      }
      stringBuilder.AppendLine();

      stringBuilder.AppendLine("Results");
      stringBuilder.AppendLine("-------");
      foreach (var result in Results) {
        stringBuilder.AppendLine(result.ToString());
      }
      stringBuilder.AppendLine();

      
      stringBuilder.AppendLine("Map");
      Map.AppendTo(stringBuilder);
      stringBuilder.AppendLine();
    }


    /// <summary>
    /// returns the Game data in a single string
    /// </summary>
    public string ToGameString() {
      var stringBuilder = new StringBuilder();
      AppendTo(stringBuilder);
      return stringBuilder.ToString();
    }
    #endregion
  }
}
