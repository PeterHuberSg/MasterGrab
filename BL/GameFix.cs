/********************************************************************************************************

MasterGrab.BL.GameFix
=====================

Stores all the information, which don't change during a game, like the options.

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
using System.Linq;
using System.Text;


namespace MasterGrab {


  /// <summary>
  /// Stores all the information, which don't change during a game, like the options.
  /// It contains the masterGame, which stores the actual state of the Game. Each user and robots get their own Game copy (clone), 
  /// which has 2 advantages:
  /// 1) The game of a user or robot doesn't change when others make their move. When it's time for them to plan their
  ///    next move, they get a new game copy.
  /// 2) If a programmer of a Robot tries to cheat by changing values in a game, like who is the owner, it doesn't matter, 
  ///    because he can change these values only in his copy.
  /// </summary>
  public class GameFix {

    #region Properties
    //      ----------

    /// <summary>
    /// Options used to create and run the game 
    /// </summary>
    public readonly Options Options;


    /// <summary>
    /// Number of countries in the Map
    /// </summary>
    public readonly int MountainsCount;


    /// <summary>
    /// Armies in biggest Country / Pixels in biggestCountry
    /// </summary>
    public readonly double ArmiesPerSize;


    /// <summary>
    /// Pixel representation of Game Map. This is mainly used for drawing the COuntry borders and deciding over which Country
    /// the mouse hovers.
    /// </summary>
    public readonly PixelMap PixelMap;


    /// <summary>
    /// Collection of all country-data which doesn't change during a game
    /// </summary>
    public IReadOnlyList<CountryFix> CountryFixArray => countryFixArray;
    readonly CountryFix[] countryFixArray;


    /// <summary>
    /// Id of GUI Player, i.e. the person who plays the game on the PC. By starting the GUI he starts the creation of 
    /// the Game. He can change some Options and restart a new Game. While a Robot gets called when he can make a move,
    /// the GuiPlayer gets informed once the Robots have made their moves and GameController then waits for the GuiPlayer
    /// to make his next Move.
    /// </summary>
    public const int GuiPlayerId = 0;


    Game masterGame; //The Game used to track the who owns what and to decide who is the winner. No public access is 
    //given to protect the MasterGame from unauthorised changes. A cloned copy is given to the players to plan their next move.

    readonly Game startGame; //Deep copy of masterGame as it was when GameFix was constructed
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor for randomly generated map. Usually, the GameController constructs the Game once the Gui has intialised it or
    /// when the user changes an option. executeMove() is used by the GameController to tell the Game the next Move of a Player.
    /// executeMove() is passed back as a constructor out parameter to hide it from other classes, especially the Robots.
    /// </summary>
    public GameFix(Options options, out Action replay, out Action<int/*playerId*/, Move> executeMove) {
      Options = options;
      replay = this.replay;
      executeMove = this.executeMove;

      // generate random PixelMap and CountryFix. Their content will not change throughout the game, like pixel 
      // representation and topology (neighbours)
      MountainsCount = options.MountainsPercentage * options.CountriesCount / 100;
      PixelMap = new PixelMap(options, MountainsCount, out countryFixArray, out var armiesPerSize);
      ArmiesPerSize = armiesPerSize;

      //generate Game, Map and Countries. Their content will change after every move
      masterGame = new Game(Options, CountryFixArray, MountainsCount, GuiPlayerId);
      startGame = new Game(masterGame);
    }


    /// <summary>
    /// Constructor using predefined countries. This is useful for testing
    /// </summary>
    #pragma warning disable CS8618 // Non-nullable fields PixelMap and startGame must contain a non-null value when exiting constructor.
    public GameFix(
      Options? options, 
      CountryFix[] countryFixArray, 
      Country[] countries, 
      out Action replay, 
      out Action<int/*playerId*/, 
        Move> executeMove) 
    {
    #pragma warning restore CS8618 
      this.countryFixArray = countryFixArray;
      if (options is null) {
        Options = getOptions(countries);//deduct Options based on predefined countries
      } else {
        Options = options;
      }
      replay = this.replay;
      executeMove = this.executeMove;

      //don't generate PixelMap, it's not needed for testing

      //proccess the country data which are not changing during a game
      MountainsCount = 0;
      foreach (var countryFix in countryFixArray) {
        if (countryFix.IsMountain) {
          MountainsCount++;
        }
      }
      var countryFixIndex = 0;
      CountryFix firstNotMountainCountryFix;
      do {
        firstNotMountainCountryFix = countryFixArray[countryFixIndex++];
      } while (firstNotMountainCountryFix.IsMountain);
      ArmiesPerSize = firstNotMountainCountryFix.Capacity / firstNotMountainCountryFix.Size;

      //generate Game and Map
      masterGame = new Game(Options, CountryFixArray, MountainsCount, GuiPlayerId);

      //correct owner of masterGame.Map.Countries
      foreach (var country in masterGame.Map.Countries) {
        country.OwnerId = countries[country.Id].OwnerId;
      }
    }


    /// <summary>
    /// Calculates options based on existing countries
    /// </summary>
    private static Options getOptions(Country[] countries) {
      var countriesCount = countries.Length;
      var mountainsCount = 0;
      var armiesInBiggestCountry = double.MinValue;
      var maxPlayerid = 0;
      foreach (var country in countries) {
        if (country.IsMountain) {
          mountainsCount++;
        } else {
          if (armiesInBiggestCountry<country.ArmySize) {
            armiesInBiggestCountry = country.ArmySize;
          }
          if (maxPlayerid<country.OwnerId) {
            maxPlayerid = country.OwnerId;
          }
        }
      }
      var mountainsPercentage = mountainsCount * 100 / countriesCount;
      var armyGrowthFactor = 1.0;
      var protectionFactor = 1.0;
      var attackFactor = 0.5;
      var attackBenefitFactor = 1.0;
      var robotTypes = new Type[maxPlayerid];//there is one robot less than players, since player 0 is the guiPlayer
      for (var robotsIndex = 0; robotsIndex < robotTypes.Length; robotsIndex++) {
        robotTypes[robotsIndex] = typeof(Robot);
      }
      var xCount = 100;
      var yCount = 100;
      return new Options(countriesCount, mountainsPercentage, xCount, yCount, armiesInBiggestCountry, armyGrowthFactor, 
        protectionFactor, attackFactor, attackBenefitFactor, isRandomOptions: false, isHumanPlaying: true, robotTypes);
    }


    /// <summary>
    /// Returns a deep copy of the MasterGame, which includes also clones of Players, Countries, etc.
    /// </summary>
    public Game GetClonedGame() {
      return new Game(masterGame);
    }
    #endregion


    #region Methods
    //      -------

    //restarts the game, so the player can play exactly the same game again
    private void replay() {
      masterGame = new Game(startGame);
    }


    /// <summary>
    /// Executes nothing, a move or an attack. It is private, because only the class who constructed the GameFix is
    /// allowed to order a move. Otherwise players could cheat and order moves out of their turn.
    /// </summary>
    private void executeMove(int playerId, Move move) {
      if (playerId==GuiPlayerId) {
        //reset the country state, which is only used by the GUI and shows which countries have changed since the last gui move.
        foreach (var country in masterGame.Map) {
          country.State = CountryStateEnum.normal;
        }
      }

      var player = masterGame.Players[playerId];
      //The Robots sets move.PlayerId. To prevent the Robot from pretending being somebody else, GameController passes playerId.
      if (move.MoveType!=MoveTypeEnum.none && player.Id!= move.PlayerId) {
        throw new ArgumentException($"This move should come from player {player.Id} and not player {move.PlayerId}.");
      }

      //execute move
      switch (move.MoveType) {
      case MoveTypeEnum.none:
        report(new Result(MoveTypeEnum.none, false, player.Id, int.MinValue, int.MinValue, null, true, null, null, double.MinValue));
        break;
      case MoveTypeEnum.move:
        //move armies between the countries of the same owner
        this.move(player, move.CountryIds, move.CountryId);
        break;
      case MoveTypeEnum.attack:
        //Use the countries of one owner to attack one country of an opponent
        attack(player, move.CountryIds, move.CountryId);
        break;
      default:
        throw new NotImplementedException();
      }

      //after every move of any Player, the armies in every country grow a little
      growArmysizes();

      if (playerId==Options.RobotTypes.Count) { //note that playerId = robotId + 1
        //before the GUI makes its move, mark the countries with only few armies 
        markCheapCountries();
        decideIfGameIsFinished();
      }
    }


    private void report(Result result) {
      masterGame.ResultsInternal[result.PlayerId] = result;
      Tracer.Trace(result.ToString());
    }


    const bool isError = true;
    const bool isOk = false;
    const bool isSucces = true;
    const bool isFailure = false;


    /// <summary>
    /// Move armies from selected countries to destination country. All selected countries and the destination country must be owned by player. When
    /// a selected country has an enemy as neighbour, (neighbour.ArmySize * ProtectionFactor) remain in the selected country. If it has several enemies,
    /// the protection armies needed against the biggest enemy get retained.
    /// </summary>
    private void move(Player player, int[] selectedCountryIds, int destinationCountryId) {
      var beforeArmies = new int[selectedCountryIds.Length + 1];
      var afterArmies = new int[selectedCountryIds.Length + 1];

      //verify that destination country is owned by Player
      var destinationCountry = masterGame.Map[destinationCountryId];
      if (destinationCountry.OwnerId!=player.Id) {
        report(new Result(MoveTypeEnum.move, isError, player.Id, player.Id, destinationCountryId, selectedCountryIds, isFailure, beforeArmies, beforeArmies, 1));
        throw new GameException(masterGame, destinationCountryId, selectedCountryIds, "It is not possible to move armies to country Id: {0} by player {0}.",
            destinationCountry.ToDebugString(), player.Id);
      }

      //remember how many armies where in the countries before the move and ensure the move is legal
      beforeArmies[0] = (int)destinationCountry.ArmySize;
      for (var selectedCountryIdsindex = 0; selectedCountryIdsindex < selectedCountryIds.Length; selectedCountryIdsindex++) {
        var selectedCountryId = selectedCountryIds[selectedCountryIdsindex];
        var selectedCountry = masterGame.Map[selectedCountryId];
        beforeArmies[selectedCountryIdsindex + 1] = (int)selectedCountry.ArmySize;

        //verify that the source countries are owned by Player
        if (selectedCountry.OwnerId!=player.Id) {
          report(new Result(MoveTypeEnum.move, isError, player.Id, player.Id, destinationCountryId, selectedCountryIds, isFailure, beforeArmies, beforeArmies, 1));
          throw new GameException(masterGame, destinationCountryId, selectedCountryIds, "It is not possible to move armies from country Id: {0}, to country Id: {1}. They have different owners.",
              destinationCountry.ToDebugString(), selectedCountry.ToDebugString());
        }
        //verify that source countries are neighbours of the destination country
        if (!destinationCountry.NeighbourIds.Contains(selectedCountry.Id)) {
          report(new Result(MoveTypeEnum.move, isError, player.Id, player.Id, destinationCountryId, selectedCountryIds, isFailure, beforeArmies, beforeArmies, 1));
          throw new GameException(masterGame, destinationCountryId, selectedCountryIds, "Move from country {0} to country {1} is not possible, they are not neighbours.",
              destinationCountry.ToDebugString(), selectedCountry.ToDebugString());
        }
      }

      //the move is legal, execute it
      for (var selectedCountryIdsIndex = 0; selectedCountryIdsIndex < selectedCountryIds.Length; selectedCountryIdsIndex++) {
        var selectedCountryId = selectedCountryIds[selectedCountryIdsIndex];
        var selectedCountry = masterGame.Map[selectedCountryId];
        var spendableArmies = selectedCountry.SpendableArmies();//leave enough armies if some neighbours are owned by the enemy
        destinationCountry.ArmySize += spendableArmies;
        selectedCountry.ArmySize -= spendableArmies;
        afterArmies[selectedCountryIdsIndex + 1] = (int)selectedCountry.ArmySize;
      }
      afterArmies[0] = (int)destinationCountry.ArmySize;
      destinationCountry.State = CountryStateEnum.moved;
      report(new Result(MoveTypeEnum.move, isOk, player.Id, player.Id, destinationCountryId, selectedCountryIds, isSucces, beforeArmies, afterArmies, 1));
    }


    /// <summary>
    /// Use selected countries to attack the destination country. All selected countries must be owned by player and the 
    /// destination country by an enemy. When a selected country has an enemy as neighbour, (neighbour.ArmySize * ProtectionFactor) 
    /// remain in the selected country. If it has several enemies, the protection armies needed against the biggest enemy 
    /// get retained.</para>
    /// The attacker succeeds if (total of attacking armies * AttackFactor) > size of defending army
    /// </summary>
    private void attack(Player player, int[] selectedCountryIds, int attackedCountryId) {
      var beforeArmies = new int[selectedCountryIds.Length + 1];
      var afterArmies = new int[selectedCountryIds.Length + 1];

      //verify that owner of attacked country is different from Player
      var attackedCountry = masterGame.Map[attackedCountryId];
      var defenderId = attackedCountry.OwnerId;
      beforeArmies[0] = (int)attackedCountry.ArmySize;
      if (defenderId==player.Id) {
        report(new Result(MoveTypeEnum.attack, isError, player.Id, defenderId, attackedCountry.Id, selectedCountryIds,
          isFailure, beforeArmies, beforeArmies, 1));
        throw new GameException(masterGame, attackedCountryId, selectedCountryIds, "Player {0} cannot attack its own country {1} from country {2}.",
          player.Id, attackedCountry.ToDebugString(), attackedCountry.ToDebugString());
      }

      //verify that all selected countries belong to Player and are next to attacked country
      for (var selectedCountriesIndex = 0; selectedCountriesIndex < selectedCountryIds.Length; selectedCountriesIndex++) {
        var selectedCountry = masterGame.Map[selectedCountryIds[selectedCountriesIndex]];
        beforeArmies[selectedCountriesIndex + 1] = (int)selectedCountry.ArmySize;
        if (selectedCountry.OwnerId!=player.Id) {
          report(new Result(MoveTypeEnum.attack, isError, player.Id, defenderId, attackedCountry.Id, selectedCountryIds,
            isFailure, beforeArmies, beforeArmies, 1));
          throw new GameException(masterGame, attackedCountryId, selectedCountryIds, "It is not possible to use country {0} to attack. It does not belong to player {1} but {2}.",
              selectedCountry.ToDebugString(), player, selectedCountry.OwnerId);
        }

        if (!attackedCountry.NeighbourIds.Contains(selectedCountry.Id)) {
          report(new Result(MoveTypeEnum.attack, isError, player.Id, defenderId, attackedCountry.Id, selectedCountryIds,
            isFailure, beforeArmies, beforeArmies, 1));
          throw new GameException(masterGame, attackedCountryId, selectedCountryIds, "Country {0} cannot attack country {1}, it is not a neighbour.",
            selectedCountry.ToDebugString(), attackedCountry.ToDebugString());
        }
      }

      //calculate outcome of attack
      var hasWon = false;
      for (var selectedCountryIdsIndex = 0; selectedCountryIdsIndex < selectedCountryIds.Length; selectedCountryIdsIndex++) {
        var selectedCountryId = selectedCountryIds[selectedCountryIdsIndex];
        var selectedCountry = masterGame.Map[selectedCountryId];
        if (selectedCountry.ArmySize<1.0)
          //armySize must be bigger than 1 to contribute to attack
          continue;

                              //only armySize bigger than 1 can be used for attack
        var attackingArmies = (selectedCountry.ArmySize - 1) * Options.AttackFactor;
        if (attackedCountry.ArmySize>attackingArmies) {
          //attack not strong enough
          attackedCountry.ArmySize -= attackingArmies;
          attackedCountry.State = CountryStateEnum.attacked;
          selectedCountry.ArmySize = 1;

        } else {
          //attack wins
          hasWon = true;
          attackingArmies -= attackedCountry.ArmySize;
          //attacking country has an armySize of at least 1 plus any armies not used for attack
          selectedCountry.ArmySize = 1 + attackingArmies/Options.AttackFactor;
          //change ownership
          attackedCountry.OwnerId = player.Id;
          attackedCountry.State = CountryStateEnum.taken;

          //armies the attacker has won 
          var availableArmies = attackedCountry.Capacity * Options.AttackBenefitFactor;

          //calculate how many countries are needed for defence
          var neededArmies = attackedCountry.DefenceArmiesNeeded();
          foreach (var attackCountryId in selectedCountryIds) {
            var attackCountry = masterGame.Map[attackCountryId];
            neededArmies += attackCountry.DefenceArmiesNeeded();
            availableArmies += attackCountry.ArmySize;
          }

          //distribute remaining attacking countries and the won armies in selected countries and attacked country
          double distributionFactor;
          if (neededArmies==0) {
            //no enemy left as neighbour. Just distrubte armies evenly under attackers
            availableArmies /= selectedCountryIds.Length;
            attackedCountry.ArmySize = 0;
            foreach (var attackCountryId in selectedCountryIds) {
              var attackCountry = masterGame.Map[attackCountryId];
              attackCountry.ArmySize = availableArmies;
            }

          } else {
            if (availableArmies>neededArmies) {
              //more armies are available than needed for defence
              if (attackedCountry.HasEnemies()) {
                //attacked country has enemies => put all armies not needed for defence into attacked country
                distributionFactor = 1;
                attackedCountry.ArmySize = availableArmies - neededArmies + attackedCountry.DefenceArmiesNeeded();
              } else {
                //attacked country has no enemies => distribute the available armies in the attacking countries with enemy neighbours
                distributionFactor = availableArmies / neededArmies;
                attackedCountry.ArmySize = 0;
              }
            } else {
              //not enough armies available for defence.
              distributionFactor = availableArmies/neededArmies;
              attackedCountry.ArmySize = attackedCountry.DefenceArmiesNeeded() * distributionFactor;
            }

            //distribute surplus armies in attacking countries
            foreach (var attackCountryId in selectedCountryIds) {
              var attackCountry = masterGame.Map[attackCountryId];
              attackCountry.ArmySize = attackCountry.DefenceArmiesNeeded() * distributionFactor;
            }
          }
          break;
        }
      }

      afterArmies[0] = (int)attackedCountry.ArmySize;
      for (var selectedCountriesIndex = 0; selectedCountriesIndex < selectedCountryIds.Length; selectedCountriesIndex++) {
        var selectedCountry = masterGame.Map[selectedCountryIds[selectedCountriesIndex]];
        afterArmies[selectedCountriesIndex + 1] = (int)selectedCountry.ArmySize;
      }
      report(new Result(MoveTypeEnum.attack, isOk, player.Id, defenderId, attackedCountry.Id, selectedCountryIds,
        hasWon, beforeArmies, afterArmies, Options.AttackFactor));
    }


    /// <summary>
    /// grows the armySize of every country a bit
    /// </summary>
    private void growArmysizes() {
      var factor = ArmiesPerSize * Options.ArmyGrowthFactor
        * 2 //random produces a number between 0 and 1, in average .5 
        / masterGame.Players.Count; //to be fair to the player and the robots, countries need to grow after each move, not just 
                                    //once per cycle.
      foreach (var country in masterGame.Map) {
        if (!country.IsMountain && country.ArmySize<country.Capacity) {
          country.ArmySize += country.Size * factor * 0.5;
          if (country.ArmySize>country.Capacity) {
            country.ArmySize = country.Capacity;
          }
        }
      }
    }


    /// <summary>
    /// Updates the Country.State for the GUI, marking the 6% of the cheapest countries
    /// </summary>
    void markCheapCountries() {
      var cheapCountriesCount = masterGame.Map.Count / 15;
      var countryesbyArmySizeQuery =
        from Country country in masterGame.Map
          //        where !country.IsMountain && country.State==CountryStateEnum.normal && !country.IsInland()
        where !country.IsMountain && !country.IsInland()
        orderby country.ArmySize
        select country;
      var countryCount = 0;
      foreach (var country in countryesbyArmySizeQuery) {
        if (country.State==CountryStateEnum.normal) {
          country.State = CountryStateEnum.cheap;
        }
        countryCount++;
        if (countryCount>=cheapCountriesCount) {
          break;
        }
      }
    }


    void decideIfGameIsFinished() {
      var hasGuiPlayerCountries = false;
      var haveRobotsCountries = false;
      foreach (var country in masterGame.Map) {
        if (country.IsMountain) continue;

        if (country.OwnerId==GuiPlayerId) {
          hasGuiPlayerCountries = true;
          if (haveRobotsCountries) {
            //Gui player and robots own countries. Game not finished yet
            return;
          }
        } else {
          haveRobotsCountries = true;
          if (hasGuiPlayerCountries) {
            //Gui player and robots own countries. Game not finished yet
            return;
          }
        }
      }

      masterGame.HasGameFinished = true;
      masterGame.HasUserWon = hasGuiPlayerCountries;
    }


    /// <summary>
    /// Adds the masterGame data to stringBuilder
    /// </summary>
    public void AppendTo(StringBuilder stringBuilder) {
      stringBuilder.AppendLine("Game");
      stringBuilder.AppendLine("====");
      masterGame.AppendTo(stringBuilder);
    }


    /// <summary>
    /// Returns the masterGame data to stringBuilder
    /// </summary>
    public string ToGameFixString() {
      var stringBuilder = new StringBuilder();
      AppendTo(stringBuilder);
      return stringBuilder.ToString();
    }
    #endregion

  }
}
