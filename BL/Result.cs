/********************************************************************************************************

MasterGrab.BL.Result
====================

Describes the outcome of a move

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
  /// Describes the outcome of a move
  /// </summary>
  public class Result {

    #region Properties
    //      ----------
    
    /// <summary>
    /// Move, attack or nothing ?
    /// </summary>
    public readonly MoveTypeEnum MoveType;
    
    /// <summary>
    /// Was the move illegal ?
    /// </summary>
    public readonly bool IsError;

    /// <summary>
    /// Who made the move ?
    /// </summary>
    public readonly int PlayerId;

    /// <summary>
    /// Who was attacked ? DefenderId is int.MinValue if there was no defender involved
    /// </summary>
    public readonly int DefenderId;

    /// <summary>
    /// Destination Country
    /// </summary>
    public readonly int CountryId;

    /// <summary>
    /// Countries where the armies are coming from
    /// </summary>
    public IReadOnlyList<int>? CountryIds => countryIds;
    readonly int[]? countryIds;

    /// <summary>
    /// True is an attacks succeeds and always true for a mover, if there was no error. Always false if there was an error.
    /// </summary>
    public readonly bool IsSuccess;

    /// <summary>
    /// Armies in the countries before the move was executed. The first country is the destination country.
    /// </summary>
    public IReadOnlyList<int>? BeforeArmies => beforeArmies;
    readonly int[]? beforeArmies;

    /// <summary>
    /// Armies in the countries after the move was executed. The first country is the destination country.
    /// </summary>
    public IReadOnlyList<int>? AfterArmies => afterArmies;
    readonly int[]? afterArmies;

    /// <summary>
    /// probability used to determine winner in an attack
    /// </summary>
    public readonly double Probability;
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// constructor
    /// </summary>
    public Result(
      MoveTypeEnum moveType,
      bool isError,
      int playerId,
      int defenderId,
      int countryId,
      int[]? selectedCountryIds,
      bool isSuccess,
      int[]? beforeArmies,
      int[]? afterArmies,
      double probability) 
    {
      MoveType = moveType;
      IsError = isError;
      PlayerId = playerId;
      if (moveType==MoveTypeEnum.none) return;

      if (selectedCountryIds!.Length + 1 != beforeArmies!.Length) throw new Exception($"beforeArmies should have {selectedCountryIds.Length + 1} entries, but had {beforeArmies.Length}.");

      if (selectedCountryIds.Length + 1 != afterArmies!.Length) throw new Exception($"afterArmies should have {selectedCountryIds.Length + 1} entries, but had {afterArmies.Length}.");

      DefenderId = defenderId;
      CountryId = countryId;
      countryIds = new int[selectedCountryIds.Length];
      for (var countryIndex = 0; countryIndex < selectedCountryIds.Length; countryIndex++) {
        countryIds[countryIndex] = selectedCountryIds[countryIndex];
      }
      IsSuccess = isSuccess;

      this.beforeArmies = new int[beforeArmies.Length];
      for (var beforeArmiesIndex = 0; beforeArmiesIndex < beforeArmies.Length; beforeArmiesIndex++) {
        this.beforeArmies[beforeArmiesIndex] = beforeArmies[beforeArmiesIndex];
      }

      this.afterArmies = new int[afterArmies.Length];
      for (var afterArmiesIndex = 0; afterArmiesIndex < afterArmies.Length; afterArmiesIndex++) {
        this.afterArmies[afterArmiesIndex] = afterArmies[afterArmiesIndex];
      }

      Probability = probability;
    }


    /// <summary>
    /// Creates deep copy of result
    /// </summary>
    public Result Clone() {
      #pragma warning disable IDE0046 // Convert to conditional expression
      if (MoveType==MoveTypeEnum.none) {
        return new Result(MoveType, IsError, PlayerId, DefenderId, CountryId, null, IsSuccess,
          null, null, Probability);
      } else {
        return new Result(MoveType, IsError, PlayerId, DefenderId, CountryId, CountryIds!.ToArray(), IsSuccess,
          BeforeArmies!.ToArray(), AfterArmies!.ToArray(), Probability);
      }
      #pragma warning restore IDE0046
    }
    #endregion


    #region Methods
    //      -------

    static readonly StringBuilder stringBuilder = new();


    /// <summary>
    /// Returns the Result data in a string.
    /// </summary>
    public override string ToString() {
      lock (stringBuilder) {//it's not expected that 2 threads call ToString() simultaneously
        stringBuilder.Clear();
        stringBuilder.Append("Player" + PlayerId + ": ");
        if (IsError) {
          stringBuilder.Append("Error ");
        }

        string typeString;
        switch (MoveType) {
        case MoveTypeEnum.none:
          stringBuilder.Append("no activity.");
          return stringBuilder.ToString();
        case MoveTypeEnum.move:
          typeString = "move";
          break;
        case MoveTypeEnum.attack:
          typeString =IsSuccess ? "wins attack against player" + DefenderId : "loses attack against player" + DefenderId;
          break;
        default:
          throw new NotSupportedException();
        }
        stringBuilder.Append(typeString + " from ");
        for (var CountryIdsIndex = 0; CountryIdsIndex < CountryIds!.Count; CountryIdsIndex++) {
          stringBuilder.Append(CountryIds[CountryIdsIndex] + ", " + beforeArmies![CountryIdsIndex + 1] + " -> " + afterArmies![CountryIdsIndex + 1] + "; ");
        }
        stringBuilder.Append("to " + CountryId + ", " + beforeArmies![0] + " -> " + afterArmies![0] + "; ");
        if (MoveType==MoveTypeEnum.attack) {
          stringBuilder.Append("Probability: " + (int)(100*Probability) + "%; ");
        }
        return stringBuilder.ToString();
      }
    }
    #endregion
  }
}
