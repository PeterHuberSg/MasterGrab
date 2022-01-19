/********************************************************************************************************

MasterGrab.BLTest.TestGame
==========================

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
  /// Creates a game for testing, i.e. without GameController.
  /// </summary>
  public static class TestGame {

    static readonly char[] blank = { ' ' };
    static readonly char[] lineChar = { '|' };


    public static GameFix? Create(Player testPlayer, Player opponent,
      out Action<double, CountryStateEnum, int>[]? updates, string mapString) 
    {
      return processString(expectedCountries: null, testPlayer, opponent, out updates, true, mapString);
    }


    public static void Verify(Country[] expectedCountries, Player testPlayer, Player opponent,
      out Action<double, CountryStateEnum, int>[]? updates, string mapString) 
    {
      processString(expectedCountries, testPlayer, opponent, out updates, false, mapString);
    }


    private static GameFix? processString(
      Country[]? expectedCountries, 
      Player testPlayer, 
      Player opponent, 
      out Action<double, 
      CountryStateEnum, 
      int>[]? updates,
      bool isCreate, 
      string mapString) 
    {
      var lines = mapString.Split(lineChar, StringSplitOptions.RemoveEmptyEntries);
      var cells = new string[lines.Length][];
      for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++) {
        var line = lines[lineIndex];
        cells[lineIndex] = line.Split(blank, StringSplitOptions.RemoveEmptyEntries);
        if (cells[lineIndex].Length!=cells[0].Length)
          throw new Exception();

      }
      var cellsPerLine = cells[0].Length;
      var countriesCount = lines.Length * cellsPerLine;
      Country[]? newCountries = null;
      CountryFix[]? newCountrFixArray = null;
      updates = null;
      //Action<Country[]>[] setNeighbours = null;
      if (isCreate) {
        newCountries = new Country[countriesCount];
        newCountrFixArray = new CountryFix[countriesCount];
        updates = new Action<double, CountryStateEnum, int>[countriesCount];
        //setNeighbours = new Action<Country[]>[countriesCount];
      }

      //  0  1  2  3
      //  4  5  6  7
      //  8  9 10 11
      // 12 13 14 15
      for (var countryIndex = 0; countryIndex < countriesCount; countryIndex++) {
        var left = countryIndex%cellsPerLine == 0 ? countryIndex + cellsPerLine - 1 : countryIndex - 1;
        int right;
        right = countryIndex + 1;
        if (right%cellsPerLine == 0) {
          right -= cellsPerLine;
        }

        var top = countryIndex - cellsPerLine;
        if (top<0) {
          top += countriesCount;
        }

        var bottom = countryIndex + cellsPerLine;
        if (bottom>=countriesCount) {
          bottom -= countriesCount;
        }

        var lineIndex = countryIndex / cellsPerLine;
        var cellindex = countryIndex % cellsPerLine;
        var cellString = cells[lineIndex][cellindex];
        var owner = cellString[0] switch {
          't' => testPlayer,
          'o' => opponent,
          _ => throw new NotSupportedException(),
        };
        var armySize = double.Parse(cellString[1..]);

        if (isCreate) {
          const int capacity = 100;
          const int size = 1;
          var countryFix = new CountryFix(countryIndex, noMountain, size, capacity, new List<int> { left, right, top, bottom });
          newCountrFixArray![countryIndex] = countryFix;
          newCountries![countryIndex] = 
            new Country(countryFix, armySize, CountryStateEnum.normal, owner.Id, out updates![countryIndex]);
        
        } else {
          //isVerify
          var expectedCountry = expectedCountries![countryIndex];
          if (expectedCountry.OwnerId!=owner.Id)
            throw new Exception();

          if ((int)expectedCountry.ArmySize!=armySize)
            throw new Exception();
        }
      }

      if (!isCreate) return null; //successfully verified

      return new GameFix(null, newCountrFixArray!, newCountries!, out var execReplay, out var execMove);
    }


    const bool noMountain = false;
  }
}
