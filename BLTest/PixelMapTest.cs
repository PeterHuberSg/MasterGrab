/********************************************************************************************************

MasterGrab.BLTest.PixelMapTest
==============================

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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace MasterGrab {


  [TestClass]
  public class PixelMapTest {

    #region String based PixelMap Test
    //      --------------------------
    [TestMethod]
    public void TestPixelMapString() {
      string mapString;
      PixelMap map;
      //CountryFixArray
      //Game game;
      string plainString;
      string expectedString;

      //#:0  $:1  %:2  &:3
      mapString =
        @"####$$$$!"+
        @"# ##$$$$!"+
        @"####$ $$!"+
        @"####$$$$!"+
        @"%%%%&&&&!"+
        @"%%%%&& &!"+
        @"%% %&&&&!"+
        @"%%%%&&&&!";
      map = new PixelMap(mapString, 0, 100, out var cFA/*CountryFixArray*/, out var armiesPerSize);
      verifyCountry(map, cFA, 0, 1, 1, 16, 100, new int[] { 1, 2 }, "2,0!3,0!3,1!3,2!3,3!2,3!1,3!0,3!0,2!0,1!0,0!1,0");
      verifyCountry(map, cFA, 1, 5, 2, 16, 100, new int[] { 0, 3 }, "6,0!7,0!7,1!7,2!7,3!6,3!5,3!4,3!4,2!4,1!4,0!5,0");
      verifyCountry(map, cFA, 2, 2, 6, 16, 100, new int[] { 0, 3 }, "3,4!3,5!3,6!3,7!2,7!1,7!0,7!0,6!0,5!0,4!1,4!2,4");
      verifyCountry(map, cFA, 3, 6, 5, 16, 100, new int[] { 1, 2 }, "7,4!7,5!7,6!7,7!6,7!5,7!4,7!4,6!4,5!4,4!5,4!6,4");
      Assert.AreEqual(100.0/16, armiesPerSize);

      mapString =
        @"####$$$$!"+
        @"# ##$$$$!"+
        @"####$ $$!"+
        @"####$$$$!";
      map = new PixelMap(mapString, 0, 100, out cFA, out armiesPerSize);
      verifyCountry(map, cFA, 0, 1, 1, 16, 100, new int[] { 1 }, "3,2!3,3!3,0!3,1");
      verifyCountry(map, cFA, 1, 5, 2, 16, 100, new int[] { 0 }, "7,3!7,0!7,1!7,2");
      Assert.AreEqual(100.0/16, armiesPerSize);

      mapString =
        @"####!"+
        @"# ##!"+
        @"####!"+
        @"####!"+
        @"$$$$!"+
        @"$$$$!"+
        @"$$ $!"+
        @"$$$$!";
      map = new PixelMap(mapString, 0, 100, out cFA, out armiesPerSize);
      verifyCountry(map, cFA, 0, 1, 1, 16, 100, new int[] { 1 }, "2,0!3,0!0,0!1,0");
      verifyCountry(map, cFA, 1, 2, 6, 16, 100, new int[] { 0 }, "3,4!0,4!1,4!2,4");
      Assert.AreEqual(100.0/16, armiesPerSize);

      //test removal of single pixels '#' and '$'
      mapString =
        @"####$$$$!"+
        @"# ##$$$#!"+
        @"#####$ $!"+
        @"####$$$$!"+
        @"%%#%&&&&!"+
        @"%%%%&& &!"+
        @"%% %&&&&!"+
        @"%%#%&&&&!";
      map = new PixelMap(mapString, 0, 100, out cFA, out armiesPerSize);
      verifyCountry(map, cFA, 0, 1, 1, 18, 100, new int[] { 1, 2, 3 }, "2,0!3,0!3,1!3,2!3,3!2,3!1,3!0,3!0,2!7,1!7,0!0,0!1,0");
      verifyCountry(map, cFA, 1, 6, 2, 14,  77, new int[]  { 0, 3 }, "6,1!7,2!7,3!6,3!5,3!4,3!4,2!4,1!4,0!5,0!6,0");
      verifyCountry(map, cFA, 2, 2, 6, 16,  88, new int[]  { 0, 3 }, "3,4!3,5!3,6!3,7!2,7!1,7!0,7!0,6!0,5!0,4!1,4!2,4");
      verifyCountry(map, cFA, 3, 6, 5, 16,  88, new int[]  { 0, 1, 2 }, "7,4!7,5!7,6!7,7!6,7!5,7!4,7!4,6!4,5!4,4!5,4!6,4");
      plainString = map.ToPlainString();
      expectedString =
        @"@""####$$$#!""+" + Environment.NewLine +
        @"@""# ##$$$#!""+" + Environment.NewLine +
        @"@""####$$ $!""+" + Environment.NewLine +
        @"@""####$$$$!""+" + Environment.NewLine +
        @"@""%%%%&&&&!""+" + Environment.NewLine +
        @"@""%%%%&& &!""+" + Environment.NewLine +
        @"@""%% %&&&&!""+" + Environment.NewLine +
        @"@""%%%%&&&&!""+" + Environment.NewLine;
      Assert.AreEqual(expectedString, plainString);
      Assert.AreEqual(100.0/18, armiesPerSize);

      //test country borders not on map border
      mapString =
        @"$$$###$$!"+
        @"$ $# #$$!"+
        @"$$$###%%!"+
        @"%&&&&% %!"+
        @"%& &&%%%!"+
        @"%&&&##%%!"+
        @"$$####$$!"+
        @"$$####$$!";
      map = new PixelMap(mapString, 0, 100, out cFA, out armiesPerSize);
      verifyCountry(map, cFA, 0, 4, 1, 19,  90, new int[]  { 1, 2, 3 }, "5,5!5,6!5,7!5,0!5,1!5,2!4,2!3,2!3,1!3,0!2,7!2,6!3,6!4,5");
      verifyCountry(map, cFA, 1, 1, 1, 21, 100, new int[]  { 0, 2, 3 }, "1,7!2,0!2,1!2,2!1,2!0,2!7,1!6,1!6,0!6,7!6,6!7,6!0,6!1,6");
      verifyCountry(map, cFA, 2, 6, 3, 13,  61, new int[]  { 0, 1, 3 }, "7,2!0,3!0,4!0,5!7,5!6,5!5,4!5,3!6,2");
      verifyCountry(map, cFA, 3, 2, 4, 11,  52, new int[]  { 0, 1, 2 }, "3,3!4,3!4,4!3,5!2,5!1,5!1,4!1,3!2,3");
      Assert.AreEqual(100.0/21, armiesPerSize);

      mapString =
      @"###$$$$$$$!"+
      @"###$$$$ $$!"+
      @"####$$$$$$!"+
      @"####$$$$$$!"+
      @"####$$$$$$!"+
      @"####%%$$$$!"+
      @"####%%$$$$!"+
      @"#####%%$$$!"+
      @"# ###% %%%!"+
      @"#####%%%%%!";
      map = new PixelMap(mapString, 0, 100, out _, out armiesPerSize);
      plainString = map.ToPlainString();
      expectedString =
      @"@""###$$$$$$$!""+" + Environment.NewLine +
      @"@""###$$$$ $$!""+" + Environment.NewLine +
      @"@""####$$$$$$!""+" + Environment.NewLine +
      @"@""####$$$$$$!""+" + Environment.NewLine +
      @"@""####$$$$$$!""+" + Environment.NewLine +
      @"@""####$$$$$$!""+" + Environment.NewLine +
      @"@""#####$$$$$!""+" + Environment.NewLine +
      @"@""#####%%$$$!""+" + Environment.NewLine +
      @"@""# ###% %%%!""+" + Environment.NewLine +
      @"@""#####%%%%%!""+" + Environment.NewLine;
      Assert.AreEqual(expectedString, plainString);
      Assert.AreEqual(100.0/46, armiesPerSize);

      mapString =
      @"###$$$$$##!"+
      @"# #$$ $$##!"+
      @"###$$$$$##!"+
      @"&&&%%%%&&&!"+
      @"& &%% %&&&!"+
      @"&&&%%%%&&&!"+
      @"&&&%%%%&&&!"+
      @"###$$$$$##!"+
      @"###$$$$$##!"+
      @"###$$$$$##!";
      _ = new PixelMap(mapString, 0, 100, out _, out armiesPerSize);
      Assert.AreEqual(100.0/30, armiesPerSize);

      //test if first pixel is at different border than last pixel
      mapString =
        @"####$$$$!"+
        @"# ##$$$$!"+
        @"####$ $$!"+
        @"####$$$$!"+
        @"%%%%&&&&!"+
        @"%%%%&& &!"+
        @"%% %&&&&!"+
        @"%%%%&&&&!";
      map = new PixelMap(mapString, 0, 100, out cFA, out armiesPerSize);
      verifyCountry(map, cFA, 0, 1, 1, 16, 100, new int[] { 1, 2 }, "2,0!3,0!3,1!3,2!3,3!2,3!1,3!0,3!0,2!0,1!0,0!1,0");
      verifyCountry(map, cFA, 1, 5, 2, 16, 100, new int[] { 0, 3 }, "6,0!7,0!7,1!7,2!7,3!6,3!5,3!4,3!4,2!4,1!4,0!5,0");
      verifyCountry(map, cFA, 2, 2, 6, 16, 100, new int[] { 0, 3 }, "3,4!3,5!3,6!3,7!2,7!1,7!0,7!0,6!0,5!0,4!1,4!2,4");
      verifyCountry(map, cFA, 3, 6, 5, 16, 100, new int[] { 1, 2 }, "7,4!7,5!7,6!7,7!6,7!5,7!4,7!4,6!4,5!4,4!5,4!6,4");
      Assert.AreEqual(100.0/16, armiesPerSize);

    }


    private static void verifyCountry(PixelMap map, CountryFix[] countryFixArray, int countryId, int x, int y, int size, 
      int capacity, int[] neighbours, string borderCordinatesString) 
    {
      var country = countryFixArray[countryId];
      Assert.AreEqual(countryId, country.Id);
      Assert.AreEqual(x, country.Coordinate.X);
      Assert.AreEqual(y, country.Coordinate.Y);
      Assert.AreEqual(size, country.Size);
      Assert.AreEqual(capacity, country.Capacity);
      Assert.AreEqual(neighbours.Length, country.NeighbourIds.Count);
      for (var neighbourIndex = 0; neighbourIndex < neighbours.Length; neighbourIndex++) {
        Assert.AreEqual(neighbours[neighbourIndex], country.NeighbourIds[neighbourIndex]);
      }
      Assert.AreEqual(countryId, map[country.Coordinate]);
      foreach (var coordinate in country.BorderCoordinates) {
        Assert.AreEqual(countryId, map[coordinate]);
      }
      var borderCordinatesStrings = borderCordinatesString.Split('!');
      Assert.AreEqual(borderCordinatesStrings.Length, country.BorderCoordinates.Count);
      for (var bcIndex = 0; bcIndex < borderCordinatesStrings.Length; bcIndex++) {
        var coordinateStrings = borderCordinatesStrings[bcIndex].Split(',');
        Assert.AreEqual(2, coordinateStrings.Length);

        var expectedCoordinate = new Coordinate(map, int.Parse(coordinateStrings[0]), int.Parse(coordinateStrings[1]));
        Assert.AreEqual(expectedCoordinate, country.BorderCoordinates[bcIndex]);
      }
    }
    #endregion


    #region Option based PixelMap test
    //      --------------------------

    [TestMethod]
    public void TestPixelMapOption() {
      Options options;
      var robotInfos = Array.Empty<RobotInfo>();

      for (var countryCountIndex = 5; countryCountIndex<=50; countryCountIndex += 5) {
        options = new Options(
        countriesCount: countryCountIndex,
        mountainsPercentage: 0,
        xCount: 200,
        yCount: 200,
        armiesInBiggestCountry: 100.0,
        armyGrowthFactor: double.MinValue,
        protectionFactor: double.MinValue,
        attackFactor: double.MinValue,
        attackBenefitFactor: double.MinValue,
        isRandomOptions: false,
        isHumanPlaying: true,
        robots: robotInfos);
        _ = new PixelMap(options, 0, out var countryFixArray, out _);

        var totalSize = 0;
        foreach (var countryFix in countryFixArray) {
          totalSize += countryFix.Size;

          foreach (var neighbourId in countryFix.NeighbourIds) {
            var neighbour = countryFixArray[neighbourId];
            Assert.IsTrue(neighbour.NeighbourIds.Contains(countryFix.Id));
          }
        }
        Assert.AreEqual(options.XCount * options.YCount, totalSize);
      }
    }
    #endregion
  }
}
