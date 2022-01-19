/********************************************************************************************************

MasterGrab.BL.PixelMap
======================

The PixelMap stores for every pixel in the display area to which country it belongs. 

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
  /// The PixelMap stores for every pixel in the display area to which country it belongs. The Owner defines the color of
  /// a pixel, although for displaying the country a vetor graphic is used. The PixelMap is mainly used for the creation of
  /// the countries and to find out over which country the mouse is.<para />
  /// The PixelMap is bent around, the pixels on the left border sit next to the pixels of the right border and the pixels
  /// of the top border sit next to the pixelson the bottom border.
  /// </summary>
  public class PixelMap {

    #region Properties
    //      ----------

    /// <summary>
    /// Number of map-pixels in x direction. Note that the biggest x coordinate is XCount-1
    /// </summary>
    public readonly int XCount;


    /// <summary>
    /// Greatest possible x coordinate
    /// </summary>
    public readonly int XMax;


    /// <summary>
    /// Number of map-pixels in y direction. Note that the biggest y coordinate is YCount-1
    /// </summary>
    public readonly int YCount;


    /// <summary>
    /// Greatest possible y coordinate
    /// </summary>
    public readonly int YMax;


    /// <summary>
    /// Shows the CountryId for each pixel
    /// </summary>
    readonly ushort[,] countryIdsPerPixel;//there are hundred thousands of pixels. It's better to store the countryId as ushort


    /// <summary>
    /// Indexer, returns the countryId of the country owning that pixel
    /// </summary>
    public int this[Coordinate coordinate] => countryIdsPerPixel[coordinate.X, coordinate.Y];


    /// <summary>
    /// Number of pixel the biggest country occupies
    /// </summary>
    public double BiggestCountrySize { get; private set; }
    #endregion


    #region Constructor
    //      -----------

    readonly Random random; //random generator used during creation of PixelMap
    const int padding = 15; //minimal distance from the border of a country's coordinate
    readonly Coordinate[] coordinatesbyCountry; //coordinate of every country, which is the starting point 
                                       //for the algorithm assigning pixels to a country


    /// <summary>
    /// Constructor using options to randomly create the PixelMap
    /// </summary>
    /// <param name="options">parameters used to define how many countrues, etc. should be generated</param>
    /// <param name="mountainCount">how many mountains should be generated</param>
    /// <param name="countryFixArray">Array holding for every country a CountryFix. The CountryFix stores data which cannot be changed
    /// during the game.</param>
    /// <param name="armiesPerSize">armies in biggest country / biggest country size</param>
    public PixelMap(Options options, int mountainCount, out CountryFix[] countryFixArray, out double armiesPerSize) {
      XCount = options.XCount;
      XMax = options.XCount-1;
      YCount = options.YCount;
      YMax = options.YCount-1;
      random = new Random();

      //place for each country a random starting point on the map
      coordinatesbyCountry = new Coordinate[options.CountriesCount];
      for (var countryIndex = 0; countryIndex<options.CountriesCount; countryIndex++) {
        var isCountryTooClose = false;
        var loopCounter = 0;
        Coordinate coordinate1;
        do {
          coordinate1 = new Coordinate(this, random.Next(padding, XMax-padding), random.Next(padding, YMax-padding));
          isCountryTooClose = false;
          for (var countryIndex2 = 0; countryIndex2<countryIndex; countryIndex2++) {
            var coordinate2 = coordinatesbyCountry[countryIndex2];
            if (coordinate1.GetSquareDistance(coordinate2)<200) {
              //closer than 20 pixels
              isCountryTooClose = true;
              break;
            }
          }
          loopCounter++;
          if (loopCounter>100) {
            throw new Exception("100 failed tries to place countries.");
          }
        } while (isCountryTooClose);
        coordinatesbyCountry[countryIndex] = coordinate1;
      }

      //use the country coordinates as start pixel for map filling
      var fillCoordinatesByCountry = new List<Coordinate>[options.CountriesCount];
      var borderPixelsByCountry = new HashSet<Coordinate>[options.CountriesCount];
      //createa list of fillCoordinates for every country, with the country's coordinate as start for every country
      for (var countryIndex = 0; countryIndex<options.CountriesCount; countryIndex++) {
        #pragma warning disable IDE0028 // Simplify collection initialization
        var countryCoordinates = new List<Coordinate>();
        countryCoordinates.Add(coordinatesbyCountry[countryIndex]);
        #pragma warning restore IDE0028
        fillCoordinatesByCountry[countryIndex] = countryCoordinates;
        borderPixelsByCountry[countryIndex] = new HashSet<Coordinate>();
      }

      //fill map
      countryIdsPerPixel = getEmptyMap();
      bool isIncomplete;
      var workCoordinates = new List<Coordinate>();
      do {
        isIncomplete = false;
        for (var countryIndex = 0; countryIndex<options.CountriesCount; countryIndex++) {
          var countryCoordinates = fillCoordinatesByCountry[countryIndex];
          foreach (var coordinate in countryCoordinates) {
            occupyIfAvailable(coordinate, c => c.Left(), countryIndex, workCoordinates, borderPixelsByCountry, random);
            occupyIfAvailable(coordinate, c => c.Up(), countryIndex, workCoordinates, borderPixelsByCountry, random);
            occupyIfAvailable(coordinate, c => c.Right(), countryIndex, workCoordinates, borderPixelsByCountry, random);
            occupyIfAvailable(coordinate, c => c.Down(), countryIndex, workCoordinates, borderPixelsByCountry, random);
          }
          if (workCoordinates.Count>0) {
            isIncomplete = true;
            var tmpCoordinates = countryCoordinates;
            fillCoordinatesByCountry[countryIndex] = workCoordinates;
            workCoordinates = tmpCoordinates;
            workCoordinates.Clear(); //reuse this list for next country instead creating each time a new list
          }
        }
      } while (isIncomplete);

      processMap(options.CountriesCount, mountainCount, options.ArmiesInBiggestCountry, coordinatesbyCountry, out countryFixArray, out armiesPerSize);
    }


    const ushort noCountry = ushort.MaxValue;


    /// <summary>
    /// Create map from string. Each pixel on the map is a character between '#' (=0) and '~' (=91). The end of a line is a '!'. 
    /// This format can be generated with Map.ToPlainString(), which returns C# code to create the map string.
    /// </summary>
    public PixelMap(
      string mapString, 
      int mountainCount, 
      double armiesInBiggestCountry, 
      out CountryFix[] countryFixArray, 
      out double armiesPerSize) 
    {
      // string mapString =
      // @"####$$$$!"+
      // @"# ##$$$$!"+
      // @"####$ $$!"+
      // @"####$$$$!"+
      // @"%%%%&&&&!"+
      // @"%%%%&& &!"+
      // @"%% %&&&&!"+
      // @"%%%%&&&&!";
      random = new Random();
      XCount = 0;
      //find end of first row
      while (mapString[XCount]!='!') {
        XCount++;
      }
      XMax = XCount - 1;
      YCount = mapString.Length / (XCount + 1);
      YMax = YCount - 1;
      if ((XCount + 1)*YCount!=mapString.Length)
        throw new Exception("mapString should be (XCount + 1)*YCount: " + XCount + " + 1 x " + YCount + " = " + (XCount + 1)*YCount + ", but was " + mapString.Length + ".");

      countryIdsPerPixel = getEmptyMap();

      var countryCodes = new HashSet<int>();
      var coordinatesByCountryDictionary = new Dictionary<int, Coordinate>();
      var x = 0;
      var y = 0;
      var countryCode = 0;
      foreach (var character in mapString) {
        if (character=='!') {
          //new line
          x = 0;
          y++;
        } else {
          if (character==' ') {
            var countryCoordinate = new Coordinate(this, x, y);
            if (x==0) {
              throw new Exception("Country coordinates should not be on upmost line of PixelMap.");
            }
            if (this[countryCoordinate.Up()]!=countryCode) {
              //the pixels top and left from countryCoordinate belong to different countries
              throw new Exception("Country coordinates must be surrounded by pixels with the same countrycode.");
            }
            coordinatesByCountryDictionary.Add(countryCode, countryCoordinate);
          } else {
            countryCode = character - '#';
            if (!countryCodes.Contains(countryCode)) {
              //new country found
              countryCodes.Add(countryCode);
            }
          }
          countryIdsPerPixel[x, y] = (ushort)countryCode;
          x++;
        }
      }
      var countryCodesCount = countryCodes.Count;
      if (countryCodesCount==0)
        throw new Exception("No country found. Each country needs a blank ' ' chracter marking the start position.");
      if (countryCodesCount<1)
        throw new Exception("Only 1 country is not enough.");
      if (countryCodesCount!=coordinatesByCountryDictionary.Count)
        throw new Exception("There should be the same number of country coordinates " + coordinatesByCountryDictionary.Count + " as countries " + countryCodesCount + ".");

      coordinatesbyCountry = new Coordinate[countryCodesCount];
      foreach (var countryCoordinate in coordinatesByCountryDictionary) {
        coordinatesbyCountry[countryCoordinate.Key] = countryCoordinate.Value;
      }

      processMap(countryCodesCount, mountainCount, armiesInBiggestCountry, coordinatesbyCountry, out countryFixArray, out armiesPerSize);
    }


    #region Constructor Methods
    //      -------------------

    private ushort[,] getEmptyMap() {
      var map = new ushort[XCount, YCount];
      for (var y = 0; y < YCount; y++) {
        for (var x = 0; x < XCount; x++) {
          map[x, y] = noCountry;
        }
      }
      return map;
    }


    private void processMap(
      int countriesCount, 
      int mountainCount, 
      double armiesInBiggestCountry, 
      Coordinate[] coordinatesbyCountry,
      out CountryFix[] countryFixArray, 
      out double armiesPerSize) 
    {
      int[] sizeByCountry;
      Coordinate[] centerByCountry;
      bool[,] neighboursByCountry;
      List<Coordinate>[] borderCoordinatesByCountry;
      var loopCounter = 0;
      do {
        loopCounter++;
        if (loopCounter>3)
          throw new Exception("Failed too many times to process this PixelMap.");

        remove1PixelAreas();

        //find border pixels, size and center
        sizeByCountry = new int[countriesCount];

        var isLeftBorderCountry = new bool[countriesCount];
        for (var x = 0; x < XCount; x++) {
          isLeftBorderCountry[countryIdsPerPixel[x, 0]] = true;
        }
        var isTopBorderCountry = new bool[countriesCount];
        for (var y = 0; y < YCount; y++) {
          isTopBorderCountry[countryIdsPerPixel[0, y]] = true;
        }

        var sumCoordinatesByCountry = new long[countriesCount, 2];
        neighboursByCountry = new bool[countriesCount, countriesCount];
        var yHalf = YCount / 2;
        var xHalf = XCount / 2;
        for (var y = 0; y < YCount; y++) {
          for (var x = 0; x < XCount; x++) {
            int countryId = countryIdsPerPixel[x, y];
            sizeByCountry[countryId]++;
            var coordinate = new Coordinate(this, x, y);
            checkNeighbour(countryId, coordinate.Right(), neighboursByCountry);
            checkNeighbour(countryId, coordinate.Down(), neighboursByCountry);

            if (isTopBorderCountry[countryId] && x<xHalf) {
              sumCoordinatesByCountry[countryId, 0] += x + XCount;
            } else {
              sumCoordinatesByCountry[countryId, 0] += x;
            }

            if (isLeftBorderCountry[countryId] && y<yHalf) {
              sumCoordinatesByCountry[countryId, 1] += y + YCount;
            } else {
              sumCoordinatesByCountry[countryId, 1] += y;
            }
          }
        }

        var xPadding = 0;
        if (XCount>100)
          xPadding = 15;
        var yPadding = 0;
        if (YCount>100)
          yPadding = 15;
        centerByCountry = new Coordinate[countriesCount];
        for (var countryIndex = 0; countryIndex < countriesCount; countryIndex++) {

          var x = (int)(sumCoordinatesByCountry[countryIndex, 0] / sizeByCountry[countryIndex]);
          if (x>XMax)
            x -= XCount;
          x = Math.Max(x, xPadding);
          x = Math.Min(x, XMax-xPadding);

          var y = (int)(sumCoordinatesByCountry[countryIndex, 1] / sizeByCountry[countryIndex]);
          if (y>YMax)
            y -= YCount;
          y = Math.Max(y, yPadding);
          y = Math.Min(y, YMax-yPadding);
          centerByCountry[countryIndex] = new Coordinate(this, x, y);
        }
        borderCoordinatesByCountry = new List<Coordinate>[countriesCount];
      } while (!findBorderLines(coordinatesbyCountry, borderCoordinatesByCountry));

      //sort countries by size
      var countrySizeByIds = new List<Tuple<int, int>>(countriesCount);
      for (var countryIndex = 0; countryIndex < countriesCount; countryIndex++) {
        countrySizeByIds.Add(new Tuple<int, int>(countryIndex, sizeByCountry[countryIndex]));
      }
      var sortedcountrySizeByIds = countrySizeByIds.OrderByDescending(c => c.Item2).ToArray();
      BiggestCountrySize = sortedcountrySizeByIds[0].Item2;
      armiesPerSize = armiesInBiggestCountry / BiggestCountrySize;

      //mark the smallest countries as mountains
      var isMountainByCountry = new bool[countriesCount];
      for (var sortedCountryIndex = sortedcountrySizeByIds.Length-mountainCount; sortedCountryIndex < sortedcountrySizeByIds.Length; sortedCountryIndex++) {
        var countryIndex = sortedcountrySizeByIds[sortedCountryIndex].Item1;
        isMountainByCountry[countryIndex] = true;
      }

      //mark a country as mountain if it is surrounded by mountains only
      for (var sortedCountryIndex = 0; sortedCountryIndex < sortedcountrySizeByIds.Length-mountainCount; sortedCountryIndex++) {
        var countryIndex = sortedcountrySizeByIds[sortedCountryIndex].Item1;
        var allNeighboursAreMountains = true;
        for (var neighbourIndex = 0; neighbourIndex < countriesCount; neighbourIndex++) {
          if (neighbourIndex!=countryIndex && neighboursByCountry[countryIndex, neighbourIndex]) {
            if (!isMountainByCountry[neighbourIndex]) {
              allNeighboursAreMountains = false;
              break;
            }
          }
        }
        if (allNeighboursAreMountains) {
          isMountainByCountry[countryIndex] = true;
        }
      }

      //create CountryFix
      countryFixArray = new CountryFix[countriesCount];
      for (var countryIndex = 0; countryIndex < countriesCount; countryIndex++) {
        var countryFix = new CountryFix(countryIndex, coordinatesbyCountry[countryIndex], centerByCountry[countryIndex],
          isMountainByCountry[countryIndex], sizeByCountry[countryIndex], (int)(sizeByCountry[countryIndex] * armiesPerSize),
          borderCoordinatesByCountry[countryIndex]);
        countryFixArray[countryIndex] = countryFix;
      }

      //find neighbours
      for (var countryIndex = 0; countryIndex < countriesCount; countryIndex++) {
        var countryFix = countryFixArray[countryIndex];
        if (countryFix.IsMountain)
          continue;

        for (var neighbourIndex = 0; neighbourIndex < countryIndex; neighbourIndex++) {
          if (neighboursByCountry[countryIndex, neighbourIndex]) {
            var neighbour = countryFixArray[neighbourIndex];
            if (neighbour.IsMountain)
              continue;
            countryFix.AddNeighbour(neighbour);
            neighbour.AddNeighbour(countryFix);
          }
        }
      }
    }

    private void checkNeighbour(int countrId, Coordinate neighbourCoordinate, bool[,] neighboursByCountry) {
      var neighbourId = this[neighbourCoordinate];
      if (countrId!=neighbourId) {
        //border found
        neighboursByCountry[neighbourId, countrId] = true;
        neighboursByCountry[countrId, neighbourId] = true;
      }
    }



    private void remove1PixelAreas() {
      //remove 1 pixel wide areas. They cause broblems when tracing borders and look ugly
      for (var y = 0; y < YCount; y++) {
        for (var x = 0; x < XCount; x++) {
          int countryId = countryIdsPerPixel[x, y];
          var pixel = new Coordinate(this, x, y);
          if (
            (this[pixel.Up()]!=countryId && this[pixel.Down()]!=countryId) ||
            (this[pixel.Left()]!=countryId && this[pixel.Right()]!=countryId)) {
            remove(pixel);

            Coordinate prev1Pixel;
            Coordinate prev2Pixel;
            if (pixel.X>0) {
              //check pixel left. There is no need to check pixel 0, because pixel XMax will be tested later
              prev1Pixel = pixel.Left();
              prev2Pixel = prev1Pixel.Left();
              if (this[prev1Pixel]==countryId && this[prev2Pixel]!=countryId) {
                //there were 2 'single' pixels in a row with the same countryId. Since the second got removed, the first is now single and 
                //needs to be removed too:
                //....
                //.21.
                //.*..
                //****
                //after 1 gets removed, 2 becomes a single pixel area and must be removed too. Note: '1', '2' and '*' have the same countryId, 
                // but different from '.'
                remove(prev1Pixel);
              }
            }

            if (pixel.Y>0) {
              //check pixel above
              prev1Pixel = pixel.Up();
              prev2Pixel = prev1Pixel.Up();
              if (this[prev1Pixel]==countryId && this[prev2Pixel]!=countryId) {
                remove(prev1Pixel);
              }
            }
          }

          //If connection between 2 country regions is only 1 pixel, add another pixel. Otherwise border tracing gets stuck in
          //an endless loop
          //***..       ***..
          //**1..       **1**
          //..***   or  ...**
          //..***       ...**

        }
      }
    }


    #pragma warning disable IDE0051 // Remove unused private members: verifyNo1PixelAreas() might be used again 
    private void verifyNo1PixelAreas() {
    #pragma warning restore IDE0051
      //check if there are still "single" pixels, i.e. without neighbours with the same countryId
      for (var y = 0; y < YCount; y++) {
        for (var x = 0; x < XCount; x++) {
          int countryId = countryIdsPerPixel[x, y];
          var pixel = new Coordinate(this, x, y);
          if (
            (this[pixel.Up()]!=countryId && this[pixel.Down()]!=countryId) ||
            (this[pixel.Left()]!=countryId && this[pixel.Right()]!=countryId)) {
            ;
          }
        }
      }
    }


    private void remove(Coordinate pixel) {
      var countryId = this[pixel];
      var countryIdCounts = new Dictionary<int, int>(8);
      var startPixel = pixel;
      pixel = pixel.Up();
      count(pixel, countryIdCounts);
      pixel = pixel.Right();
      count(pixel, countryIdCounts);
      pixel = pixel.Down();
      count(pixel, countryIdCounts);
      pixel = pixel.Down();
      count(pixel, countryIdCounts);
      pixel = pixel.Left();
      count(pixel, countryIdCounts);
      pixel = pixel.Left();
      count(pixel, countryIdCounts);
      pixel = pixel.Up();
      count(pixel, countryIdCounts);
      pixel = pixel.Up();
      count(pixel, countryIdCounts);

      var biggestCount = 0;
      var majorityCountryId = 0;
      foreach (var keyValuePair in countryIdCounts) {
        if (countryId!=keyValuePair.Key && biggestCount<keyValuePair.Value) {
          biggestCount = keyValuePair.Value;
          majorityCountryId = keyValuePair.Key;
        }
      }
      countryIdsPerPixel[startPixel.X, startPixel.Y] = (ushort)majorityCountryId;
    }


    private void count(Coordinate pixel, Dictionary<int, int> countryIdCounts) {
      var countryId = this[pixel];
      countryIdCounts[countryId] =countryIdCounts.TryGetValue(countryId, out var count) ? count + 1 : 1;
    }


    #pragma warning disable IDE0044 // Add readonly modifier. isTestBorderProblem can be manually set during debugging
    bool isTestBorderProblem;
    #pragma warning restore IDE0044


    private bool findBorderLines(Coordinate[] coordinatesbyCountry, List<Coordinate>[] borderCoordinatesByCountry) {
      var hasFound = true;
      //find border line 
      for (var countryIndex = 0; countryIndex<coordinatesbyCountry.Length; countryIndex++) {
        var startPixel = coordinatesbyCountry[countryIndex];
        Coordinate lastStartPixel;
        var isSearchUp = true;
        var stepCount = 0;

        //search up or right, until a different country is found
        do {
          lastStartPixel = startPixel;
          startPixel = isSearchUp ? startPixel.Up() : startPixel.Right();
          stepCount++;
          if (stepCount==YCount) {
            //no border found searching vertically. search horizontally
            isSearchUp = false;
          }
          if (stepCount>YCount + XCount) {
            throw new Exception("No border found for country ID: " + countryIndex);
          }
        } while (this[startPixel]==countryIndex);
        //first border pixel found
        startPixel = lastStartPixel;

        var pixel = startPixel;
        var borderCoordinates = new List<Coordinate>();
        var lastCoordinates = new Coordinate[20];
        int lastCoordinatesIndex;
        for (lastCoordinatesIndex = 0; lastCoordinatesIndex < lastCoordinates.Length; lastCoordinatesIndex++) {
          lastCoordinates[lastCoordinatesIndex] = Coordinate.Null;
        }
        lastCoordinatesIndex = 0;

        do {
          //move to pixel above current pixel
          pixel = pixel.Up();
          if (countryIdsPerPixel[pixel.X, pixel.Y]==countryIndex) {
            //top pixel has  countryId, search counter clock wise for different countryId
            searchNextPixelInside(ref pixel, countryIndex);
          } else {
            //top pixel has different countryId, search clock wise for countryId
            searchNextPixelOutside(ref pixel, countryIndex);
          }
          if (arePixelRepeating(pixel, lastCoordinates, ref lastCoordinatesIndex)) {
            //tracing the border line is stuck in a loop. The problematic pixels were removed from this country
            //and the processing of the map has to be repeated, since the map has changed
            pixel = startPixel;
            hasFound = false;
          }
          borderCoordinates.Add(pixel);

          if (borderCoordinates.Count>10000) {
            #if debugBorderCoordinates
            string s;
            if (coordinatesbyCountry.Length<92) {
              s = ToPlainString(countryIdsPerPixel, pixel, 5);
            }
            #endif
            throw new Exception("too many border points. Press \"New Game\" to create a new game.");
          }
        } while (!pixel.Equals(startPixel));
        if (isTestBorderProblem) {
          int firstPixelIndex;
          var isLastPixelAtBottomBorder = false;
          for (firstPixelIndex = 0; firstPixelIndex < borderCoordinates.Count; firstPixelIndex++) {
            var y = borderCoordinates[firstPixelIndex].Y;
            if (isLastPixelAtBottomBorder) {
              if (y==0) {
                //border of country as switched from bottom border of window to top border of window
                //for testing for border problem, shift border pixels so that it starts with y=0
                var borderCoordinatesCopy = new List<Coordinate>(borderCoordinates.Count);
                for (var copyPixelIndex = 0; copyPixelIndex < borderCoordinates.Count; copyPixelIndex++) {
                  borderCoordinatesCopy.Add(borderCoordinates[(firstPixelIndex + copyPixelIndex) % borderCoordinates.Count]);
                }
                borderCoordinates = borderCoordinatesCopy;
                break;
              }
            }
            isLastPixelAtBottomBorder = y==YMax;
          }
        }
        borderCoordinatesByCountry[countryIndex] = borderCoordinates;
      }
      return hasFound;
    }


    private bool arePixelRepeating(Coordinate pixel, Coordinate[] lastCoordinates, ref int lastCoordinatesIndex) {
      //check if Pixel is in lastCoordinates
      int sameIndex;
      for (sameIndex = 0; sameIndex < lastCoordinates.Length; sameIndex++) {
        if (lastCoordinates[sameIndex].Equals(pixel)) {
          break;
        }
      }
      if (sameIndex>=lastCoordinates.Length) {
        lastCoordinates[lastCoordinatesIndex] = pixel;
        lastCoordinatesIndex = (lastCoordinatesIndex + 1) % lastCoordinates.Length;
        return false;
      }

      //same pixel was processed before. Remove the remaining pixels from the map
      var removeIndex = (sameIndex + 1) % lastCoordinates.Length; //leave the first repeated pixel
      if (removeIndex==lastCoordinatesIndex)
        throw new Exception("The loop consists of only 1 pixel. This can not happen, but checking anyway.");

      do {
        remove(lastCoordinates[removeIndex]);
        lastCoordinates[removeIndex] = Coordinate.Null;
        removeIndex = (removeIndex + 1) % lastCoordinates.Length;
      } while (removeIndex!=lastCoordinatesIndex);

      return true;
    }


    enum positionEnum {
      top,
      topright,
      right,
      bottomright,
      bottom,
      bottomleft,
      left,
      topleft
    }


    //Description how to track a border line in a bitmap:
    //http://www.vbforums.com/showthread.php?235662-trace-the-outline-of-a-shape-on-a-bitmap-**RESOLVED**


    private void searchNextPixelOutside(ref Coordinate pixel, int countryId) {
      //search clock wise from Top for countryId
      //use last pixel found, it has already the countryId

      var position = positionEnum.top;
      do {
        //turn clockwise
        switch (position) {
        case positionEnum.top:
          position = positionEnum.topright;
          pixel = pixel.Right();
          break;
        case positionEnum.topright:
          position = positionEnum.right;
          pixel = pixel.Down();
          break;
        case positionEnum.right:
          position = positionEnum.bottomright;
          pixel = pixel.Down();
          break;
        case positionEnum.bottomright:
          position = positionEnum.bottom;
          pixel = pixel.Left();
          break;
        case positionEnum.bottom:
          position = positionEnum.bottomleft;
          pixel = pixel.Left();
          break;
        case positionEnum.bottomleft:
          position = positionEnum.left;
          pixel = pixel.Up();
          break;
        case positionEnum.left:
          position = positionEnum.topleft;
          pixel = pixel.Up();
          break;
        case positionEnum.topleft:
          throw new Exception("all neighbouring pixels have the country countryId: " + countryId + ".");
        default:
          throw new NotSupportedException();
        }
      } while (countryIdsPerPixel[pixel.X, pixel.Y]!=countryId);
    }


    private void searchNextPixelInside(ref Coordinate pixel, int countryId) {
      //search counter clock wise from Top for pixel different from countryId
      //use second last pixel, which still has the countryId
      var position = positionEnum.top;
      Coordinate lastpixel;
      do {
        //turn counter clockwise
        lastpixel = pixel;
        switch (position) {
        case positionEnum.top:
          position = positionEnum.topleft;
          pixel = pixel.Left();
          break;
        case positionEnum.topright:
          throw new Exception("none of the neighbouring pixels have the country Id: " + countryId + ".");
        case positionEnum.right:
          position = positionEnum.topright;
          pixel = pixel.Up();
          break;
        case positionEnum.bottomright:
          position = positionEnum.right;
          pixel = pixel.Up();
          break;
        case positionEnum.bottom:
          position = positionEnum.bottomright;
          pixel = pixel.Right();
          break;
        case positionEnum.bottomleft:
          position = positionEnum.bottom;
          pixel = pixel.Right();
          break;
        case positionEnum.left:
          position = positionEnum.bottomleft;
          pixel = pixel.Down();
          break;
        case positionEnum.topleft:
          position = positionEnum.left;
          pixel = pixel.Down();
          break;
        default:
          throw new NotSupportedException();
        }
      } while (countryIdsPerPixel[pixel.X, pixel.Y]==countryId);
      pixel = lastpixel;//lastpixel has still countryId
    }


    private void occupyIfAvailable(
      Coordinate originCoordinate,
      Func<Coordinate, Coordinate> move1Pixel,
      int countryIndex,
      List<Coordinate> nextCoordinates,
      HashSet<Coordinate>[] borderPixelsByCountry,
      Random random) 
    {
      var pixelIndexMax = random.Next(1, 4);
      for (var pixelIndex = 0; pixelIndex <pixelIndexMax; pixelIndex++) {
        var nextCoordinate = move1Pixel(originCoordinate);
        var mapCountryId = this[nextCoordinate];
        if (mapCountryId==noCountry) {
          //empty pixel found
          countryIdsPerPixel[nextCoordinate.X, nextCoordinate.Y] = (ushort)countryIndex;
          nextCoordinates.Add(nextCoordinate);
        } else {
          if (mapCountryId!=countryIndex + 1) {
            //neighbour found
            borderPixelsByCountry[countryIndex].Add(originCoordinate);
          }
          break;
        }
        originCoordinate = nextCoordinate;
      }
    }
#endregion
#endregion


#region Methods
    //      -------

    /// <summary>
    /// Returns true if x and y are within the map
    /// </summary>
    public bool IsWithin(int x, int y) {
      return x>=0 && x<XCount &&y>=0 && y<YCount;
    }


    const int maxCountryId = '~' - '#' + 1;


    /// <summary>
    /// Writes map content to a string using exactly 1 character for each Pixel and endOfLine to separate lines. To get the 
    /// integer value of the pixel, subtract '#' from the pixel character. 
    /// This format can be easily pasted into C# code to specify a map. One line looks like this:
    /// @"$&&&&&&&&&&&&&%%%%%%%%%%%%$$$$!"+
    /// </summary>
    public string ToPlainString() {
      return ToPlainString(countryIdsPerPixel, new Coordinate(this, 0, 0), 0);
    }


    public string ToPlainString(ushort[,] map, Coordinate pixel, int offset) {
      var startX = pixel.X;
      var startY = pixel.Y;
      var endX = XCount;
      var endY = YCount;
      if (offset>0) {
        endX = Math.Min(XCount, startX+offset);
        endY = Math.Min(YCount, startY+offset);
        startX = Math.Max(0, startX-offset);
        startY = Math.Max(0, startY-offset);
      }
      var stringBuilder = new StringBuilder();
      for (var y = startY; y < endY; y++) {
        stringBuilder.Append("@\"");
        for (var x = startX; x < endX; x++) {
          int countryId = map[x, y];
          var coordinate = coordinatesbyCountry[countryId];
          if (coordinate.X==x && coordinate.Y==y) {
            stringBuilder.Append(' ');
          } else {
            if (countryId>maxCountryId) {
              throw new Exception("countryId " + countryId + " must be smaller than " + maxCountryId + ".");
            }
            stringBuilder.Append((char)('#' + countryId));
          }
        }
        stringBuilder.Append("!\"+" + Environment.NewLine);
      }
      return stringBuilder.ToString();
    }


    /// <summary>
    /// Writes map content to a string using tab character to seperate pixels and endofLine to separate lines. This
    /// can be easily pasted into Excel to visualise the map. The borderCoordinates will be displayed as '*'.
    /// </summary>
    public string ToTabbedString(List<Coordinate>? borderCoordinates) {
      var stringBuilder = new StringBuilder();
      for (var y = 0; y < YCount; y++) {
        for (var x = 0; x < XCount; x++) {
          if (borderCoordinates!=null && borderCoordinates.Exists(c => c.X==x && c.Y==y)) {
            stringBuilder.Append(countryIdsPerPixel[x, y] + "*\t");
          } else {
            stringBuilder.Append(countryIdsPerPixel[x, y] + "\t");
          }
        }
        stringBuilder.Append(Environment.NewLine);
      }
      return stringBuilder.ToString();
    }
#endregion
  }
}
