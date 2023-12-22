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

    /// <summary>
    /// x and y coordinates of cluster centers.
    /// </summary>
    public (int x, int y)[]? ClusterCoordinates { get; }//Not readonly because it is used for debugging, changing its values has no influence
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
      var sortedCountryFixArray = countryFixArray.OrderByDescending(c => c.Size).ToArray();
      MaxCountrySize = sortedCountryFixArray[0].Size;
      MinCountrySize = sortedCountryFixArray[sortedCountryFixArray.Length-game.MountainsCount - 1].Size;

      var totalCountrySizeByPlayer = new int[game.Players.Count];
      var ownerIdsByCountry = new int[options.CountriesCount];
      for (var ownerIdsByCountryIndex = 0; ownerIdsByCountryIndex < ownerIdsByCountry.Length; ownerIdsByCountryIndex++) {
        ownerIdsByCountry[ownerIdsByCountryIndex] = int.MinValue;
      }

      if (options.Clustering==ClusteringEnum.random) {
        //one owner's countries are scattered randomly all over the map
        //assign countries starting with the biggest country to each player. The player with the smallest countries size (pixel) gets
        //the next country. This ensures that every player has in the end about the same total countries size.
        for (var sortedCountryIndex = 0; sortedCountryIndex < sortedCountryFixArray.Length-game.MountainsCount; sortedCountryIndex++) {
          var countryFix = sortedCountryFixArray[sortedCountryIndex];
          var smallestPlayerIndex = 0;
          if (countryFix.IsMountain) throw new Exception();

          var smallestSize = int.MaxValue;
          for (var playerIndex = 0; playerIndex < totalCountrySizeByPlayer.Length; playerIndex++) {
            if (smallestSize>totalCountrySizeByPlayer[playerIndex]) {
              smallestSize = totalCountrySizeByPlayer[playerIndex];
              smallestPlayerIndex = playerIndex;
            }
          }
          ownerIdsByCountry[sortedCountryFixArray[sortedCountryIndex].Id] = smallestPlayerIndex;
          totalCountrySizeByPlayer[smallestPlayerIndex] += sortedCountryFixArray[sortedCountryIndex].Size;
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
            if (ownerIdsByCountry[countryFix.Id]==game.GuiPlayerId) {
              if (minGuiSize>countryFix.Size) {
                minGuiSize = countryFix.Size;
                guiCountry = countryFix;
              }
            } else {
              if (maxSize<countryFix.Size) {
                maxSize = countryFix.Size;
                maxCountry = countryFix;
                maxOwnerId = ownerIdsByCountry[countryFix.Id];
              }
            }
          }
          //swap owners
          ownerIdsByCountry[maxCountry!.Id] = game.GuiPlayerId;
          ownerIdsByCountry[guiCountry!.Id] = maxOwnerId;
        }

      } else {
        //one owner's countries are clustered together at start
        var clusterConfiguration = ClusterConfigurations.Get(game.Players.Count, options.Clustering);
        ClusterCoordinates = new (int x, int y)[clusterConfiguration.Length];
        var multiplier = game.Players.Count * 2;
        var maxX = options.XCount-1;
        var maxY = options.YCount-1;
        for (int clusterIndex = 0; clusterIndex<clusterConfiguration.Length; clusterIndex++) {
          var (x, y)= clusterConfiguration[clusterIndex];
          ClusterCoordinates[clusterIndex] = (x*maxX/multiplier, y*maxY/multiplier);
        }
        for (var sortedCountryIndex = 0; sortedCountryIndex < sortedCountryFixArray.Length-game.MountainsCount; sortedCountryIndex++) {
          var playerIndex = getClusterOwner(options, ClusterCoordinates, sortedCountryFixArray[sortedCountryIndex]);
          ownerIdsByCountry[sortedCountryFixArray[sortedCountryIndex].Id] = playerIndex;
          totalCountrySizeByPlayer[playerIndex] += sortedCountryFixArray[sortedCountryIndex].Size;
        }
      }

      //generate countries
      countries = new Country[options.CountriesCount];
      var random = new Random();
      for (var countryIndex = 0; countryIndex < countries.Length; countryIndex++) {
        var countryFix = countryFixArray[countryIndex];
        var ownerId = ownerIdsByCountry[countryIndex];
        var armySize = double.MinValue;
        if (countryFix.IsMountain) {
          if (ownerId!=int.MinValue) {
            throw new Exception();
          }
        } else {
          armySize = countryFix.Capacity * (1 + random.NextDouble()) / 2;
        }
        var country = new Country(this, countryFix, ownerId, armySize);
        countries[countryIndex] = country;
      }
    }

    private static int getClusterOwner(Options options, (int x, int y)[] clusterCoordinates, CountryFix countryFix) {
      var minDistance = int.MaxValue;
      var owner = 0;
      for (int clusterIndex = 0; clusterIndex < clusterCoordinates.Length; clusterIndex++) {
        var (x, y) = clusterCoordinates[clusterIndex];
        var xDistance = Math.Abs(x - countryFix.Center.X);
        if (xDistance>options.XCount/2) {
          xDistance = options.XCount - xDistance;
        }
        var yDistance = Math.Abs(y - countryFix.Center.Y);
        if (yDistance>options.YCount/2) {
          yDistance = options.YCount - yDistance;
        }
        var distance = xDistance*xDistance + yDistance*yDistance;
        if (minDistance>distance) {
          minDistance = distance;
          owner = clusterIndex;
        }
      }
      return owner;
    }


    //private int getClusterOwner(Options options, int playersCount, CountryFix countryFix) {
    //  if (countryFix.IsMountain) throw new Exception();
    //  switch (playersCount) {
    //  case 4:
    //    if (countryFix.Center.X<options.XCount/2) {
    //      if (countryFix.Center.Y<options.YCount/2) {
    //        return 0;
    //      } else {
    //        return 1;
    //      }
    //    } else {
    //      if (countryFix.Center.Y<options.YCount/2) {
    //        return 2;
    //      } else {
    //        return 3;
    //      }
    //    }

    //  default:
    //    throw new NotSupportedException("options.IsClusteredOwnership does not support {playersCount} players.");
    //  }
    //}


    /// <summary>
    /// Clone constructor, used to give each Player its own copy of the Map.
    /// </summary>
    public Map(Game game, Map map) {
      Game = game;

      MinCountrySize = map.MinCountrySize;
      MaxCountrySize = map.MaxCountrySize;
      countries = new Country[map.countries.Length];
      for (var countryIndex = 0; countryIndex < countries.Length; countryIndex++) {
        var countryOriginal = map[countryIndex];
        var countryCloned = new Country(this, countryOriginal);
        //if (!countryOriginal.IsMountain) {
        //  //OwnerId gets set twice
        //  countryCloned.OwnerId =  countryOriginal.OwnerId;//this adds also the country to Owner.Countries
        //  countryCloned.PreviousOwnerId = countryOriginal.PreviousOwnerId==int.MinValue ? int.MinValue : countryOriginal.PreviousOwnerId;
        //}
        countries[countryIndex] = countryCloned;
      }
      //Todo: make ClusterCoordinates private once they are no longer needed for debugging in 
      //ClusterCoordinates = map.ClusterCoordinates;//there is not need to give a copy, changing its values has no influence
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
    /// It's not possible to remove all countries once the Map is constructed
    /// </summary>
    public void Clear() {
        throw new NotSupportedException();
    }


    /// <summary>
    /// checks if country item is in countries. it uses object comparison to determine equality.
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
    /// Finds index of country within countries. </para>
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
    /// Returns an enumerator through all countries of the Map.
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator() {
      return countries.GetEnumerator();
    }
    #endregion
  }
}
