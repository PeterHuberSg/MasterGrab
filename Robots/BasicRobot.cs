/********************************************************************************************************

MasterGrab.Robots.BasicRobot
============================

Robot player for MasterGrab

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


namespace MasterGrab {

  /// <summary>
  /// Robot used in first version of game.It firstchecks which countires he is strong enough to attack and attacks then
  /// the biggest country with the smallest loss. Country size is more important than army loss. </para>
  /// 
  /// If he is too weak to attack another player, he tries to find an own Country,
  /// to which he can move as many armies as possible. In the next moves, he will move these armies to the nearest enemy.
  /// </summary>
  public class BasicRobot: Robot {

    #region Constructor
    //      -----------

    readonly List<Country> selectedCountries; //Countries cannot be used for more than 1 Move and selectedCountries could
    //be defined in DoPlanMove(). To prevent the repeatedly creation of the collection, it is created here and just cleared
    //when used.

    /// <summary>
    /// Constructor
    /// </summary>
    public BasicRobot(): base() {
      selectedCountries = new List<Country>();
    }
    #endregion


    #region Methods
    //      -------

    int maxMoverCountryId = int.MinValue; //CountryId where previously armies have been moved to and from where they should
                                          //be moved to the nearest enemy.

    bool[]? isEnemy;


    /// <summary>
    /// Called by GameController to calculate the next Move of this Robot.
    /// </summary>
    protected override Move DoPlanMove() {
      if (RobotCountryIds.Count==0) return Move.NoMove; //Robot has lost an no countries left, just return

      if (isEnemy==null) {
        isEnemy = new bool[Map.Count]; //create array only once and reuse. It could also be used to detect which
                                       //countries have changed ownership.
      } else {
        Array.Clear(isEnemy, 0, isEnemy.Length);
      }
      double ratio = 0;
      double movingArmies = 0; //armies could be moved to a single country
      Country movingCountry;   //country where the biggest armies can be moved to

      //find all neighbours. Also check how many armies could be moved to a single country
      foreach (int countryId in RobotCountryIds) {
        Country country = Map[countryId];
        double newMovingArmies = country.ArmySize;
        foreach (int neighbourId in country.NeighbourIds) {
          Country neighbour = Map[neighbourId];
          if (neighbour.OwnerId!=Player.Id) {
            //find all enemies
            isEnemy[neighbour.Id] = true;
          } else {
            if (neighbour.IsInland()) {
              newMovingArmies += neighbour.ArmySize;
            } else {
              newMovingArmies += neighbour.ArmySize / 3;
            }
          }
        }
        if (movingArmies < newMovingArmies) {
          movingArmies = newMovingArmies;
          movingCountry = country;
        }
      }

      //find best target to attack
      Country? target = null;
      for (int countryId = 0; countryId < isEnemy.Length; countryId++) {
        if (isEnemy[countryId]) {
          Country newTarget = Map[countryId];
          double attackingArmies = 0;
          foreach (int attackerId in newTarget.NeighbourIds) {
            Country attacker = Map[attackerId];
            if (attacker.OwnerId==Player.Id) {
              if (attacker.IsInland(newTarget)) {
                attackingArmies += attacker.ArmySize;
              } else {
                attackingArmies += attacker.ArmySize / 2; //assume that half the army needs to be kept for defence
              }
            }
          }
          double newRatio = attackingArmies / newTarget.ArmySize;
          if (newRatio*Game.AttackFactor>1.1) {
            //we have more than double the army than the defender. Attack is possible
            newRatio *= newTarget.Size; //give size more weight than army size
            if (ratio<newRatio) {
              ratio = newRatio;
              target = newTarget;
            }
          }
        }
      }

      Move move = Move.NoMove;
      bool isContinuePlanning = false;
      if (target==null) {
        //nothing to attack. Can armies be moved ?
        if (maxMoverCountryId>=0) {
          //some armies have been moved to one country. Move them now closer to the enemy
          Country movefromCountry = Map[maxMoverCountryId];
          if (movefromCountry.OwnerId!=Player.Id) {
            //owner has changed since the last move
            maxMoverCountryId = int.MinValue;
            isContinuePlanning = true;
          } else {
            Country moveToCountry = movefromCountry.GetNeigbourToNearestEnemy();
            if (moveToCountry==null) {
              // maxMoverCountryId is no longer inland, maybe it got just recently an enemy
              maxMoverCountryId = int.MinValue;
              isContinuePlanning = true;
            } else {
              selectedCountries.Clear();
              foreach (int neighbourId in moveToCountry.NeighbourIds) {
                Country neighbour = Map[neighbourId];
                if (neighbour.IsInland()) {
                  selectedCountries.Add(neighbour);
                }
              }
              move = new Move(MoveTypeEnum.move, Player, moveToCountry, selectedCountries);
              if (moveToCountry.IsInland()) {
                if (moveToCountry.IsMountain)
                  System.Diagnostics.Debugger.Break();
                maxMoverCountryId = moveToCountry.Id;
              } else {
                maxMoverCountryId = int.MinValue;
              }
            }
          }
        } else {
          isContinuePlanning = true;
        }
        if (isContinuePlanning) {
          //Can some armies be moved ?
          //try first to find a country with no enemies
          double maxMovableArmies = 0;
          foreach (int countryId in RobotCountryIds) {
            Country country = Map[countryId];
            if (country.IsInland()) {
              double movableArmies = 0;
              foreach (int neighbourId in country.NeighbourIds) {
                Country neighbour = Map[neighbourId];
                if (neighbour.IsInland()) {
                  if (neighbour.OwnerId!=Player.Id)
                    throw new Exception("Inland countries should border only to countries from the same player.");

                  movableArmies += neighbour.ArmySize;
                }
              }
              if (maxMovableArmies<movableArmies) {
                maxMovableArmies = movableArmies;
                maxMoverCountryId = country.Id;
              }
            }
          }
          if (maxMovableArmies>0) {
            Country moveToCountry = Map[maxMoverCountryId];
            selectedCountries.Clear();
            foreach (int neighbourId in moveToCountry.NeighbourIds) {
              Country neighbour = Map[neighbourId];
              if (neighbour.IsInland()) {
                selectedCountries.Add(neighbour);
              }
            }
            move = new Move(MoveTypeEnum.move, Player, moveToCountry, selectedCountries);

          } else {
            //concentrate armies even when enemy is neighbour
            foreach (int countryId in RobotCountryIds) {
              Country country = Map[countryId];
              double movableArmies = 0;
              foreach (int neighbourId in country.NeighbourIds) {
                Country neighbour = Map[neighbourId];
                if (neighbour.OwnerId==Player.Id) {
                  movableArmies += neighbour.SpendableArmies();
                }
              }
              if (maxMovableArmies<movableArmies) {
                maxMovableArmies = movableArmies;
                if (country.IsMountain)
                  System.Diagnostics.Debugger.Break();
                maxMoverCountryId = country.Id;
              }
            }
            if (maxMovableArmies>0) {
              Country moveToCountry = Map[maxMoverCountryId];
              selectedCountries.Clear();
              foreach (int neighbourId in moveToCountry.NeighbourIds) {
                Country neighbour = Map[neighbourId];
                if (neighbour.OwnerId==Player.Id) {
                  selectedCountries.Add(neighbour);
                }
              }
              move = new Move(MoveTypeEnum.move, Player, moveToCountry, selectedCountries);

            } else {
              //concentrate armies even when enemy is neighbour


              //nothing to do
              move = Move.NoMove;
            }
          }
        }

      } else {
        //execute attack
        selectedCountries.Clear();
        foreach (int attackerId in target.NeighbourIds) {
          Country attacker = Map[attackerId];
          if (attacker.OwnerId==Player.Id) {
            selectedCountries.Add(attacker);
          }
        }
        move = new Move(MoveTypeEnum.attack, Player, target, selectedCountries);
      }
      return move;
    }
    #endregion
  }
}
