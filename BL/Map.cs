/********************************************************************************************************

MasterGrab.BL.Map
=================

The Map holds all countries. 

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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;


namespace MasterGrab {


  /// <summary>
  /// The Map holds all countries. Each player receives for each move a new map. This prevents one player making a change 
  /// giving another player a problem and prevents cheating. If a player changes the map it received, it doesn't change
  /// the map the GameController uses.
  /// </summary>
  public class Map: IList<Country> {


    #region Properties
    //      ----------

    /// <summary>
    /// A Map is hold by a Game, which gives access to Options, Players, etc. 
    /// </summary>
    public readonly Game Game;


    /// <summary>
    /// A list of all countries
    /// </summary>
    public IReadOnlyList<Country> Countries => countries;
    readonly Country[] countries;


    /// <summary>
    /// Number of pixels in smallest country, without mountains
    /// </summary>
    public int MinCountrySize { get; }


    /// <summary>
    /// Number of pixels in biggest country
    /// </summary>
    public int MaxCountrySize { get; }
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor for randomly created countries. This constructor gets called only once and assigns initial owners to
    /// each country. The Maps passed to the players are created with the Clone constructor public Map(Game game, Map map).
    /// </summary>
    public Map(Options options, Game game, IReadOnlyList<CountryFix> countryFixArray) {
      Game = game;

      if (options.CountriesCount!=countryFixArray.Count) throw new Exception();

      //sort countries by size
      var sortedcountryFixArray = countryFixArray.OrderByDescending(c => c.Size).ToArray();
      MaxCountrySize = sortedcountryFixArray[0].Size;
      MinCountrySize = sortedcountryFixArray[sortedcountryFixArray.Length-game.MountainsCount - 1].Size;

      var totalCountrySizeByPlayer = new int[game.Players.Count];
      var ownerIdsbyCountry = new int[options.CountriesCount];
      for (var ownerIdsbyCountryIndex = 0; ownerIdsbyCountryIndex < ownerIdsbyCountry.Length; ownerIdsbyCountryIndex++) {
        ownerIdsbyCountry[ownerIdsbyCountryIndex] = int.MinValue;
      }

      if (options.IsClusteredOwnership) {
        //one owner's countries are clustered together at start
        for (var sortedCountryIndex = 0; sortedCountryIndex < sortedcountryFixArray.Length-game.MountainsCount; sortedCountryIndex++) {
          var playerIndex = getClusterOwner(options, game.Players.Count, sortedcountryFixArray[sortedCountryIndex]);
          ownerIdsbyCountry[sortedcountryFixArray[sortedCountryIndex].Id] = playerIndex;
          totalCountrySizeByPlayer[playerIndex] += sortedcountryFixArray[sortedCountryIndex].Size;
        }


      } else {
        //one owner's countries are distributed all over the map
        //assign countries starting with the biggest country to each player. The player with the smallest countries size (pixel) gets
        //the next country. This ensures that every player has in the end about the same total countries size.
        for (var sortedCountryIndex = 0; sortedCountryIndex < sortedcountryFixArray.Length-game.MountainsCount; sortedCountryIndex++) {
          var countryFix = sortedcountryFixArray[sortedCountryIndex];
          var smallestPlayerIndex = 0;
          if (countryFix.IsMountain) throw new Exception();

          var smallestsize = int.MaxValue;
          for (var playerIndex = 0; playerIndex < totalCountrySizeByPlayer.Length; playerIndex++) {
            if (smallestsize>totalCountrySizeByPlayer[playerIndex]) {
              smallestsize = totalCountrySizeByPlayer[playerIndex];
              smallestPlayerIndex = playerIndex;
            }
          }
          ownerIdsbyCountry[sortedcountryFixArray[sortedCountryIndex].Id] = smallestPlayerIndex;
          totalCountrySizeByPlayer[smallestPlayerIndex] += sortedcountryFixArray[sortedCountryIndex].Size;
        }

        if (options.IsHumanPlaying) {
          //give GuiPlayer a small advantage, because he has to start and needs to compete against many computer players. Without a
          //small advantage, it is rather difficult for the human player to start.
          var maxSize = 0.0;
          CountryFix? maxCountry = null;
          var maxOwnerId = int.MinValue;
          double minGuiSize = int.MaxValue;
          CountryFix? guiCountry = null;
          //find smallest gui country and biggest other country
          foreach (var countryFix in countryFixArray) {
            if (ownerIdsbyCountry[countryFix.Id]==game.GuiPlayerId) {
              if (minGuiSize>countryFix.Size) {
                minGuiSize = countryFix.Size;
                guiCountry = countryFix;
              }
            } else {
              if (maxSize<countryFix.Size) {
                maxSize = countryFix.Size;
                maxCountry = countryFix;
                maxOwnerId = ownerIdsbyCountry[countryFix.Id];
              }
            }
          }
          //swap owners
          ownerIdsbyCountry[maxCountry!.Id] = game.GuiPlayerId;
          ownerIdsbyCountry[guiCountry!.Id] = maxOwnerId;
        }

      }

      //generate countries
      countries = new Country[options.CountriesCount];
      var random = new Random();
      for (var countryIndex = 0; countryIndex < countries.Length; countryIndex++) {
        var coutryFix = countryFixArray[countryIndex];
        var ownerId = ownerIdsbyCountry[countryIndex];
        var armySize = double.MinValue;
        if (coutryFix.IsMountain) {
          if (ownerId!=int.MinValue) {
            throw new Exception();
          }
        } else {
          armySize = coutryFix.Capacity * (1 + random.NextDouble()) / 2;
        }
        var country = new Country(this, coutryFix, ownerId, armySize);
        countries[countryIndex] = country;
      }
    }

    private int getClusterOwner(Options options, int playersCount, CountryFix countryFix) {
      if (countryFix.IsMountain) throw new Exception();
      switch (playersCount) {
      case 4:
        if (countryFix.Center.X<options.XCount/2) {
          if (countryFix.Center.Y<options.YCount/2) {
            return 0;
          } else {
            return 1;
          }
        } else {
          if (countryFix.Center.Y<options.YCount/2) {
            return 2;
          } else {
            return 3;
          }
        }

      default:
        throw new NotSupportedException("options.IsClusteredOwnership does not support {playersCount} players.");
      }

    }


    /// <summary>
    /// Clone constructor, used to give each Player its own copy of the Map.
    /// </summary>
    public Map(Game game, Map map) {
      Game = game;

      MinCountrySize = map.MinCountrySize;
      MaxCountrySize = map.MaxCountrySize;
      countries = new Country[map.countries.Length];
      for (var countryindex = 0; countryindex < countries.Length; countryindex++) {
        var countryOriginal = map[countryindex];
        var countryCloned = new Country(this, countryOriginal);
        //if (!countryOriginal.IsMountain) {
        //  //OwnerId wird zweimal gesetzt
        //  countryCloned.OwnerId =  countryOriginal.OwnerId;//this adds also the country to Owner.Countries
        //  countryCloned.PreviousOwnerId = countryOriginal.PreviousOwnerId==int.MinValue ? int.MinValue : countryOriginal.PreviousOwnerId;
        //}
        countries[countryindex] = countryCloned;
      }
    }


    /// <summary>
    /// Adds each country on its own line to stringBuilder
    /// </summary>
    public void AppendTo(StringBuilder stringBuilder) {
      Country.AppendHeaderTo(stringBuilder);
      foreach (var country in countries) {
        country.AppendTo(stringBuilder);
      }
    }


    /// <summary>
    /// Returns each country on its own line in a string
    /// </summary>
    public string ToMapString() {
      var stringBuilder = new StringBuilder();
      AppendTo(stringBuilder);
      return stringBuilder.ToString();
    }
    #endregion


    #region IList Interface
    //      ---------------

    /// <summary>
    /// Indexer to countries by country.Id
    /// </summary>
    public Country this[int index] {
      get => countries[index];

      //countries should only be read from once the map is constructed
      set => throw new NotSupportedException();
    }


    /// <summary>
    /// Number of countries
    /// </summary>
    public int Count => countries.Length;


    /// <summary>
    /// countries should only be read from once the map is constructed
    /// </summary>
    public bool IsReadOnly => true;


    /// <summary>
    /// It's not possible to add a country once the Map is constructed
    /// </summary>
    public void Add(Country item) {
      throw new NotSupportedException();
    }


    /// <summary>
    /// It's not possible to remove all countrie once the Map is constructed
    /// </summary>
    public void Clear() {
        throw new NotSupportedException();
    }


    /// <summary>
    /// checks if country item is in countries. it uses object comparision to determine equality.
    /// </summary>
    /// <param name="item"></param>
    public bool Contains(Country item) {
      for (var countryIndex = 0; countryIndex < countries.Length; countryIndex++) {
        if (countries[countryIndex]==item) return true;
      }
      return false;
    }


    /// <summary>
    /// It's not possible to copy countries
    /// </summary>
    public void CopyTo(Country[] array, int arrayIndex) {
      throw new NotSupportedException();
    }


    /// <summary>
    /// Enumerator through all countries of the Map
    /// </summary>
    public IEnumerator<Country> GetEnumerator() {
      return ((IEnumerable<Country>)countries).GetEnumerator();
    }


    /// <summary>
    /// Finds index ofcountry within countries. </para>
    /// Remark: the index should be equal to country.Id
    /// </summary>
    public int IndexOf(Country item) {
      for (var countryIndex = 0; countryIndex < countries.Length; countryIndex++) {
        if (countries[countryIndex]==item)
          return countryIndex;
      }
      return -1;
    }


    /// <summary>
    /// It's not possible to insert a country once the Map is constructed
    /// </summary>
    public void Insert(int index, Country item) {
      throw new NotSupportedException();
    }


    /// <summary>
    /// It's not possible to remove a country once the Map is constructed
    /// </summary>
    public bool Remove(Country item) {
      throw new NotSupportedException();
    }


    /// <summary>
    /// It's not possible to remove a country once the Map is constructed
    /// </summary>
    public void RemoveAt(int index) {
      throw new NotSupportedException();
    }


    /// <summary>
    /// Returns an enumerator thourgh all countries of the Map.
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator() {
      return countries.GetEnumerator();
    }
    #endregion
  }
}
