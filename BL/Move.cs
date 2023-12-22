/********************************************************************************************************

MasterGrab.BL.Move
==================

Each Player can do one Move taking turns. A move can be moving armies between countries of the same 
owner, using several countries of one owner to attack a country of a different owner or no activity at 
all.

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

  #region Enumerations
  //      ------------

  /// <summary>
  /// Is the Move a move, an attack or no activity at all ?
  /// </summary>
  public enum MoveTypeEnum {
    /// <summary>
    /// No activity
    /// </summary>
    none,
    /// <summary>
    /// Move armies between countries of the same owner
    /// </summary>
    move,
    /// <summary>
    /// Use several countries of one owner to attack an enemy country
    /// </summary>
    attack
  }
  #endregion


  /// <summary>
  /// Each Player can do one Move taking turns. A move can be moving armies between countries of the same owner, using
  /// several countries of one owner to attack a country of a different owner or no activity at all.
  /// </summary>
  public class Move {

    #region Properties
    //      ----------

    /// <summary>
    /// Type of Move: move, attack or nothing
    /// </summary>
    public readonly MoveTypeEnum MoveType;

    /// <summary>
    /// Who makes the move
    /// </summary>
    public readonly int PlayerId;

    /// <summary>
    /// target country
    /// </summary>
    public readonly int CountryId;

    /// <summary>
    /// Source countries
    /// </summary>
    public readonly int[] CountryIds;


    /// <summary>
    /// Singleton which can be used to indicate that no activity should be executed
    /// </summary>
    public static readonly Move NoMove = new(MoveTypeEnum.none);
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// private constructor for NoMove
    /// </summary>
    private Move(MoveTypeEnum actionType) {
      MoveType = actionType;
      PlayerId = int.MinValue;
      CountryId = int.MinValue;
      CountryIds = null!;
    }


    /// <summary>
    /// Defines a move or attack executed by one Player. country is where the armies go to, countries provides the armies.
    /// </summary>
    public Move(MoveTypeEnum actionType, Player player, Country country, IReadOnlyList<Country> countries) {
      switch (actionType) {
      case MoveTypeEnum.none:
        break;
      case MoveTypeEnum.move:
        if (country.OwnerId!=player.Id) {
          throw new GameException(null, country.Id, Country.ToCountryIdsArray(countries), "It is not possible to move armies to country Id: {0} by player {0}.",
            country.ToDebugString(), player.Id);
        }
        foreach (var selectedCountry in countries) {
          if (selectedCountry.OwnerId!=player.Id) {
            throw new GameException(null, country.Id, Country.ToCountryIdsArray(countries), "It is not possible to move armies from country Id: {0}, to country Id: {1}. They have different owners.",
                country.ToDebugString(), selectedCountry.ToDebugString());
          }
          if (!country.IsNeighbour(selectedCountry)) {
            throw new GameException(null, country.Id, Country.ToCountryIdsArray(countries), "Move from country {0} to country {1} is not possible, they are not neighbours.",
                country.ToDebugString(), selectedCountry.ToDebugString());
          }
        }
        break;

      case MoveTypeEnum.attack:
        if (country.OwnerId==player.Id) {
          throw new GameException(null, country.Id, Country.ToCountryIdsArray(countries), "Player {0} cannot attack its own country {1}.",
            player.Id, country.ToDebugString());
        }
        foreach (var selectedCountry in countries) {
          if (selectedCountry.OwnerId!=player.Id) {
            throw new GameException(null, country.Id, Country.ToCountryIdsArray(countries), "It is not possible to use country {0} to attack. It does not belong to player {1} but {2}.",
                selectedCountry.ToDebugString(), player, selectedCountry.OwnerId);
          }
          if (!country.IsNeighbour(selectedCountry)) {
            throw new GameException(null, country.Id, Country.ToCountryIdsArray(countries), "Country {0} cannot attack country {1}, it is not a neighbour.",
              selectedCountry.ToDebugString(), country.ToDebugString());
          }
        }
        break;

      default:
        throw new NotSupportedException();
      }
      MoveType = actionType;
      PlayerId = player.Id;
      CountryId = country.Id;
      CountryIds = new int[countries.Count];
      for (var countryIndex = 0; countryIndex < countries.Count; countryIndex++) {
        CountryIds[countryIndex] = countries[countryIndex].Id;
      }
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Returns string with all the Move data
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
      if (MoveType==MoveTypeEnum.none) {
        return "No move";
      }

      var countryIdsString = "";
      var isFirst = true;
      foreach (var countryId in CountryIds) {
        if (isFirst) {
          isFirst = false;
        } else {
          countryIdsString += ", ";
        }
        countryIdsString += countryId.ToString();
      }
      return
        "MoveType: " + MoveType +
        " PlayerId: " + PlayerId +
        " CountryId: " + CountryId +
        " CountryIds: " + countryIdsString;
    }
    #endregion

  }
}
