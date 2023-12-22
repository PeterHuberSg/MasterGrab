/********************************************************************************************************

MasterGrab.BL.Country
=====================

Gives access to fixed data of a country, like ID of the country and its neighbour countries, and data of a 
country which changes over time, like owner of the country or army size in the country

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


/*
 Country versus CountryFix
 --------------------------

 Country gives access to country data actually stored in CountryFix which do not change during a game (example: neighbour 
 countries) and data which does change when game moves are made (example: owner of country, which are actually stored 
 directly in Country.

 The GuiPlayer and the Robots get their own copy of each country to keep the information stable while the Robots make 
 their moves and to prevent that a Robot could change the ownership of a country (=cheating). Since CountryFix data does 
 not need to be copied for every player, having CountryFix saves space and time.
 */


namespace MasterGrab {

  #region Enumerations
  //      ------------

  /// <summary>
  /// State of a country during a game. It gets updated each time before the GUI player can make another move
  /// </summary>
  public enum CountryStateEnum {
    /// <summary>
    /// Nothing special about this country.
    /// </summary>
    normal,
    /// <summary>
    /// Armies from same owner were moved to this country.
    /// </summary>
    moved,
    /// <summary>
    /// This country was attacked, but the attack failed.
    /// </summary>
    attacked,
    /// <summary>
    /// Enemy attacked this country and won it
    /// </summary>
    taken,
    /// <summary>
    /// Country has only a small army
    /// </summary>
    cheap }
  #endregion


  /// <summary>
  /// Gives access to fixed data of a country, like ID of the country and its neighbour countries, and data of a country
  /// which changes over time, like owner of the country or army size in the country
  /// </summary>
  public class Country: IComparable<Country> {

    #region Properties
    //      ----------

    #region Fixed Data
    //      ----------

    /// <summary>
    /// Unique number of country
    /// </summary>
    public int Id { get; }


    /// <summary>
    /// X and y pixels where country started to grow from
    /// </summary>
    public Coordinate Coordinate { get; }
    
    /// <summary>
    /// Is country a mountain, i.e. cannot be owned
    /// </summary>
    public bool IsMountain { get; }


    /// <summary>
    /// Number of pixels within the country
    /// </summary>
    public int Size { get; }


    /// <summary>
    /// Biggest army size the country can host
    /// </summary>
    public double Capacity { get; }


    /// <summary>
    /// Ids of all countries touching this country
    /// </summary>
    public IReadOnlyList<int> NeighbourIds => neighbourIds;
    readonly int[] neighbourIds;
    #endregion


    #region Changing data
    //      -------------

    //Player.Id of owner of the country
    public int OwnerId {
      get => ownerId;
      internal set {
        PreviousOwnerId = ownerId;
        //update for previous and new country owner which countries he owns
        if (ownerId!=int.MinValue && Map!=null) {
          var previousOwner = Map.Game.Players[ownerId];
          previousOwner.RemoveCountry(Id);
        }
        ownerId = value;
        if (ownerId!=int.MinValue && Map!=null) {
          var newOwner = Map.Game.Players[ownerId];
          newOwner.AddCountry(Id);
        }
      }
    }
    int ownerId = int.MinValue;


    /// <summary>
    /// Player.Id of owner who owned the country previously
    /// </summary>
    public int PreviousOwnerId { get; internal set; }


    /// <summary>
    /// Size of the army within the country
    /// </summary>
    public double ArmySize {
      get => armySize;
      internal set {
        if (double.IsNaN(value)) {
          System.Diagnostics.Debugger.Break();
        }
        armySize = value;
      }
    }
    double armySize;


    /// <summary>
    /// State of a country during a game. It gets updated each time before the GUI player can make another move
    /// </summary>
    public CountryStateEnum State { get; internal set; }


    /// <summary>
    /// Has this country a neighbouring country with a different owner ?
    /// </summary>
    public bool HasEnemies() {
      foreach (var neighbourId in NeighbourIds) {
        var neighbour = Map[neighbourId];
        if (neighbour.ownerId!=ownerId) return true;
      }
      return false;
    }


    /// <summary>
    /// Borders neighbour to this country ?
    /// </summary>
    public bool IsNeighbour(Country neighbour) {
      return NeighbourIds.Contains(neighbour.Id);
    }
    #endregion
    #endregion


    #region Constructor
    //      -----------

    public readonly Map Map;


    public Country(Map map, CountryFix countryFix, int ownerId, double armySize) {
      Id = countryFix.Id;
      Coordinate = countryFix.Coordinate;
      IsMountain = countryFix.IsMountain;
      Size = countryFix.Size;
      Capacity = countryFix.Capacity;
      neighbourIds = new int[countryFix.NeighbourIds.Count];
      for (var neighbourIdsIndex = 0; neighbourIdsIndex < neighbourIds.Length; neighbourIdsIndex++) {
        neighbourIds[neighbourIdsIndex] = countryFix.NeighbourIds[neighbourIdsIndex];
      }

      Map = map;
      OwnerId = ownerId;
      ArmySize = armySize;
      State = CountryStateEnum.normal;
    }


    /// <summary>
    /// Cloning Constructor
    /// </summary>
    public Country(Map map, Country country) {
      Id = country.Id;
      Coordinate = country.Coordinate;
      IsMountain = country.IsMountain;
      Size = country.Size;
      Capacity = country.Capacity;
      neighbourIds = (int[])country.neighbourIds.Clone();
      Map = map;
      ownerId = country.ownerId;
      PreviousOwnerId = country.PreviousOwnerId;
      ArmySize = country.ArmySize;
      State = country.State;
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Copies a list of countries to an array of countries
    /// </summary>
    public static int[] ToCountryIdsArray(IReadOnlyList<Country> countries) {
      var countryIdsArray = new int[countries.Count];
      var countryIndex = 0;
      foreach (var country in countries) {
        countryIdsArray[countryIndex++] = country.Id;
      }
      return countryIdsArray;
    }


    /// <summary>
    /// Orders countries according their Country.Id
    /// </summary>
    public int CompareTo(Country? other) {
      return Map!=other!.Map ? throw new ArgumentException() : Id.CompareTo(other.Id);
    }


    /// <summary>
    /// Have all neighbours the same owner ?
    /// </summary>
    public bool IsInland() {
      foreach (var neighbourId in NeighbourIds) {
        var neighbour = Map[neighbourId];
        if (neighbour.OwnerId!=OwnerId) {
          return false;
        }
      }
      return true;
    }


    /// <summary>
    /// Have all neighbours the same owner except the attacked country.This is used to decide how many armies should be
    /// moved to the attacked country.
    /// </summary>
    public bool IsInland(Country attacked) {
      foreach (var neighbourId in NeighbourIds) {
        var neighbour = Map[neighbourId];
        if (neighbour==attacked)
          continue;

        if (neighbour.OwnerId!=OwnerId) {
          return false;
        }
      }
      return true;
    }


    /// <summary>
    /// Returns how many armies are needed for defence. For each enemy, the sum of all neighbour armies gets
    /// calculated. The biggest sum * ProtectionFactor has to remain. 
    /// </summary>
    public double DefenceArmiesNeeded() {
      Dictionary<int, double>? neededArmiesByEnemy = null;
      foreach (var neighbourId in NeighbourIds) {
        var neighbour = Map[neighbourId];
        if (neighbour.ownerId!=OwnerId) {
          neededArmiesByEnemy ??= new Dictionary<int, double>();
          if (neededArmiesByEnemy.ContainsKey(neighbour.ownerId)) {
            neededArmiesByEnemy[neighbour.ownerId] += neighbour.ArmySize;
          } else {
            neededArmiesByEnemy[neighbour.ownerId] = neighbour.ArmySize;
          }
        }
      }
      if (neededArmiesByEnemy==null) {
        return 0;
      }
      double maxArmiesNeeded = 0;
      foreach (var armiesNeeded in neededArmiesByEnemy.Values) {
        if (maxArmiesNeeded<armiesNeeded) {
          maxArmiesNeeded = armiesNeeded;
        }
      }
      return maxArmiesNeeded * Map.Game.ProtectionFactor;
    }


    /// <summary>
    /// Returns how many armies are not needed for defence, but can be moved away. For each enemy, the sum of all neighbour 
    /// armies gets calculated. The biggest sum * ProtectionFactor has to remain. The rest can be moved.
    /// </summary>
    public double SpendableArmies() {
      return Math.Max(0, ArmySize - DefenceArmiesNeeded());
    }


    /// <summary>
    /// returns true if all selectedCountries are neighbours of this country and have a different owner
    /// </summary>
    public bool CanBeAttacked(IReadOnlyList<Country> selectedCountries) {
      if (selectedCountries.Count==0)
        return false;

      foreach (var selectedCountry in selectedCountries) {
        if (selectedCountry.OwnerId==OwnerId)
          return false;

        if (!selectedCountry.NeighbourIds.Contains(Id))
          return false;
      }
      return true;
    }

    #region Find path to nearest enemy
    //      --------------------------

    // A robot might collect the highest armyCount possible in one country, then move them to the nearest enemy, which can
    // be done with GetNeighbourToNearestEnemy().


    private class searchCountryInfo {
      public readonly Country Country;
      public readonly int Iteration;
      public readonly int TotalArmies;
      public readonly searchCountryInfo? Parent;


      public searchCountryInfo(Country Country, int Iteration, int TotalArmies, searchCountryInfo? Parent) {
        this.Country = Country;
        this.Iteration = Iteration;
        this.TotalArmies = TotalArmies;
        this.Parent = Parent;
      }
    }


    /// <summary>
    /// GetNeighbourToNearestEnemy returns the neighbour which needs to cross the fewest number of countries to reach 
    /// an enemy. All neighbours of this country must have the same owner. 
    /// </summary>
    public Country GetNeighbourToNearestEnemy() {
      //process every country only once
      var searchedCountryIds = new HashSet<int>();
      //next country to be searched. The search goes in circles. First are the neighbours of the starting country checked
      //if they have a different owner, then the neighbours of the neighbours, etc.
      var searchCountries = new Queue<searchCountryInfo>();
      searchedCountryIds.Add(Id);
      var startCountry = new searchCountryInfo(this, 0, 0, null);
      foreach (var neighbourId in NeighbourIds) {
        var neighbour = Map[neighbourId];
        if (neighbour.ownerId!=ownerId) {
          //a neighbour has a different owner. This is actually an error, but it's better not to throw an exception
          //otherwise testing gets difficult. It's easier if the caller just assumes that the return value is never
          //null and an exception gets thrown if it is not so.
          return null!;
        }
        //mark the neighbour as searched and add it to searchCountries
        neighbour.addToSearch(searchCountries, searchedCountryIds, startCountry);
      }

      //search repeatedly until enemy is found
      searchCountryInfo? finalDestinationCountryInfo = null;
      var maxTotalArmies = int.MinValue; //max armySize of nearest enemies. Attack the enemy with the greatest armySize
      var maxIteration = int.MaxValue; //max iterations needed to find nearest enemy
      while (searchCountries.Count>0) {
        var searchCountry = searchCountries.Dequeue();
        if (maxIteration<searchCountry.Iteration) {
          //this country is farer away than the enemy country already found
          break;
        }
        foreach (var neighbourId in searchCountry.Country.NeighbourIds) {
          var neighbour = Map[neighbourId];
          if (neighbour.ownerId!=ownerId) {
            //enemy found
            if (maxTotalArmies<searchCountry.TotalArmies) {
              maxTotalArmies = searchCountry.TotalArmies;
              maxIteration = searchCountry.Iteration;
              finalDestinationCountryInfo = searchCountry;
            }
          } else {
            neighbour.addToSearch(searchCountries, searchedCountryIds, searchCountry);
          }
        }
      }

      //an enemy country should be found here
      if (finalDestinationCountryInfo==null || finalDestinationCountryInfo.Parent==null) {
        //could throw ane exception here, but better just return null and let the caller throw an exception if
        //null is a problem;
        return null!;
      }
      //trace back to immediate neighbour of the country where the search started
      while (finalDestinationCountryInfo.Parent!.Parent!=null) {
        finalDestinationCountryInfo = finalDestinationCountryInfo.Parent;
      }

      return finalDestinationCountryInfo.Country;
    }


    private void addToSearch(
      Queue<searchCountryInfo> toSearchCountries, 
      HashSet<int> searchedCountryIds, 
      searchCountryInfo parent) 
    {
      if (searchedCountryIds.Contains(Id))
        return;

      searchedCountryIds.Add(Id);
      toSearchCountries.Enqueue(new searchCountryInfo(this, parent.Iteration + 1, parent.TotalArmies + (int)ArmySize, parent));
    }
    #endregion


    /// <summary>
    /// Adds header row for a list of countries with tab separated columns to stringBuilder
    /// </summary>
    public static void AppendHeaderTo(StringBuilder stringBuilder) {
      stringBuilder.Append("Id");
      stringBuilder.Append(" \t");
      stringBuilder.Append("Owner");
      stringBuilder.Append(" \t");
      stringBuilder.Append("Armies");
      stringBuilder.Append(" \t");
      stringBuilder.Append("State");
      stringBuilder.AppendLine();
    }


    /// <summary>
    /// Adds country row for a list of countries with tab separated columns to stringBuilder
    /// </summary>
    public void AppendTo(StringBuilder stringBuilder) {
      stringBuilder.Append(Id);
      stringBuilder.Append(" \t");
      stringBuilder.Append(OwnerId);
      stringBuilder.Append(" \t");
      stringBuilder.Append(ArmySize.ToString("0.0"));
      stringBuilder.Append(" \t");
      stringBuilder.Append(State);
      stringBuilder.AppendLine();
    }


    /// <summary>
    /// Returns the countries properties in a ';' separated string
    /// </summary>
    public override string ToString() {
      var returnString =
        "Id: " + Id + "; " +
        "Size: " + Size + "; " +
        "Coordinate: " + Coordinate  + "; ";
      if (NeighbourIds.Count>0) {
        returnString += "Neighbours: ";
        foreach (var neighbourId in NeighbourIds) {
          returnString += neighbourId + ", ";
        }
        returnString = returnString[0..^2];
      }
      if (IsMountain) {
        returnString += "; IsMountain";
      }
      if (OwnerId!=int.MinValue) {
        returnString += 
          "; Owner: " + OwnerId +
          "; Armies: " + ArmySize.ToString("0.0") +
          "; State: " + State;
      }
      return returnString;
    }


    public const string DebugStringOwnerLabel = ", Owner: ";
    public static readonly int DebugStringOwnerLength = DebugStringOwnerLabel.Length;


    /// <summary>
    /// Condensed country info for error reporting
    /// </summary>
    public string ToDebugString() {
      return
        "Country (" + Id +
        DebugStringOwnerLabel + OwnerId +
        ", Armies: " + ArmySize.ToString("0.0") + ")";
    }
    #endregion


    #region Testing
    //      =======

    public Country(CountryFix countryFix, 
      double armySize, 
      CountryStateEnum state, 
      int ownerId,
      out Action<double, CountryStateEnum, int> update) 
    {
      Id = countryFix.Id;
      Coordinate = countryFix.Coordinate;
      IsMountain = countryFix.IsMountain;
      Size = countryFix.Size;
      Capacity = countryFix.Capacity;
      neighbourIds = new int[countryFix.NeighbourIds.Count];
      for (var neighbourIdsIndex = 0; neighbourIdsIndex < neighbourIds.Length; neighbourIdsIndex++) {
        neighbourIds[neighbourIdsIndex] = countryFix.NeighbourIds[neighbourIdsIndex];
      }
      Map = null!;
      ArmySize = armySize;
      State = state;
      OwnerId = ownerId;
      update = testUpdate;
    }


    private void testUpdate(double armySize, CountryStateEnum state, int ownerId) {
      ArmySize = armySize;
      State = state;
      OwnerId = ownerId;
    }
    #endregion
  }
}
