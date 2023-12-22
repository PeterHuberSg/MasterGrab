/********************************************************************************************************

MasterGrab.BL.CountryFix
========================

CountryFix holds all information of one country which do not change during a game, like the CountryId, its 
border, neighbours, etc.

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
using System.Text;


namespace MasterGrab {


  /// <summary>
  /// CountryFix holds all information which do not change during a game, like the CountryId, its border, 
  /// neighbours, etc.
  /// </summary>
  public class CountryFix: IComparable<CountryFix> {

    
    #region Properties
    //      ----------
    
    /// <summary>
    /// Unique number of country
    /// </summary>
    public readonly int Id;


    /// <summary>
    /// Coordinate where the country was started as single pixel and then grown until touching the neighbours
    /// </summary>
    public readonly Coordinate Coordinate;


    /// <summary>
    /// Gravity center of country, i.e. middle of the country
    /// </summary>
    public readonly Coordinate Center;


    /// <summary>
    /// Is country a mountain, i.e. cannot be owned
    /// </summary>
    public readonly bool IsMountain;


    /// <summary>
    /// Number of pixels within the country
    /// </summary>
    public readonly int Size;


    /// <summary>
    /// Biggest army size the country can host
    /// </summary>
    public readonly int Capacity;


    /// <summary>
    /// Ids of all countries touching this country
    /// </summary>
    public IReadOnlyList<int> NeighbourIds => neighbours;
    readonly List<int> neighbours;


    /// <summary>
    /// Every pixel on the border of the country. This is used to draw the country as vector graph.
    /// </summary>
    public IReadOnlyList<Coordinate> BorderCoordinates => borderCoordinates;
    readonly List<Coordinate> borderCoordinates;
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor
    /// </summary>
    public CountryFix(int id, Coordinate coordinate, Coordinate center, bool isMountain, int size, int capacity, 
      List<Coordinate> borderCoordinates) 
    {
      Id = id;
      Coordinate = coordinate;
      Center = center;
      IsMountain = isMountain;
      Size = size;
      Capacity = capacity;
      neighbours = new List<int>();
      this.borderCoordinates = borderCoordinates;
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Used by PixelMap to add neighbours, since it is not possible to add the neighbours in the constructor, because
    /// the neighbour might not be constructed yet.
    /// </summary>
    internal void AddNeighbour(CountryFix neighbour) {
      neighbours.Add(neighbour.Id);
    }


    /// <summary>
    /// Compares the countries based on their Ids
    /// </summary>
    public int CompareTo(CountryFix? other) {
      return other is null ? -1 : Id.CompareTo(other.Id);
    }


    /// <summary>
    /// Returns the most important information about the country as string.
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
      return returnString;
    }
    #endregion


    #region Testing
    //      =======

    /// <summary>
    /// Test constructor, where the neighbours can be assigned during construction
    /// </summary>
    public CountryFix(int id, bool isMountain, int size, int capacity, List<int> neighbours) : 
      this(id, Coordinate.Null, Coordinate.Null, isMountain, size, capacity, createEmptyList()) {
      this.neighbours = neighbours;
    }


    private static List<Coordinate> createEmptyList() {
      var coordinateList = new List<Coordinate>();
      for (var listIndex = 0; listIndex < 30; listIndex++) {
        coordinateList.Add(Coordinate.Null);
      }
      return coordinateList;
    }


    /// <summary>
    /// Returns the border pixel coordinates as string, which is useful for debugging display problems.
    /// </summary>
    public string GetCondensedBorder() {
      var stringBuilder = new StringBuilder();
      var map = BorderCoordinates[0].PixelMap!;
      foreach (var coordinate in BorderCoordinates) {
        if (coordinate.X==0 || coordinate.X==map.XMax ||coordinate.Y==0 || coordinate.Y==map.YMax) {
          stringBuilder.AppendLine(coordinate.ToString());
        }
      }
      stringBuilder.AppendLine();
      foreach (var coordinate in BorderCoordinates) {
        stringBuilder.AppendLine(coordinate.ToString());
      }
      return stringBuilder.ToString();
    }
    #endregion
    }
}
