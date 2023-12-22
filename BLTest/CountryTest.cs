/********************************************************************************************************

MasterGrab.BLTest.CountryTest
=============================

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

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace MasterGrab {


  [TestClass]
  public class CountryTest {
    [TestMethod]
    public void TestCountryGetNeighbourToNearestEnemy() {
      var testPlayer = new Player(0, "testPlayer", null!);
      var opponent = new Player(1, "opponent",null!);
      GameFix gameFix;

      gameFix = TestGame.Create(testPlayer, opponent, out var _,
        //0  1  2  3  4  5  6
        "o1 o1 o1 o1 o1 o1 o1|" + //0
        "o1 t1 t1 t1 t1 t1 o1|" + //1
        "o1 t1 t1 t1 t1 t1 o1|" + //2
        "o1 t1 t1 t1 t1 t1 o1|" + //3
        "o1 t1 t1 t1 t1 t1 o1|" + //4
        "o1 t1 t1 t1 t1 t1 o1|" + //5
        "o1 o1 o1 o1 o1 o1 o1|")!; //6
      //              X1 Y1 X2 Y2 countries per row
      //assert(gameFix, 3, 3, 4, 3, 7); this test returns 4 different, valid results, since all 4 borders have the same distance. This is too difficult to test.
      assert(gameFix, 2, 3, 1, 3, 7);
      assert(gameFix, 1, 3, -1, -1, 7);

      assert(gameFix, 3, 2, 3, 1, 7);
      assert(gameFix, 3, 1, -1, -1, 7);

      assert(gameFix, 3, 4, 3, 5, 7);
      assert(gameFix, 3, 5, -1, -1, 7);

      assert(gameFix, 4, 3, 5, 3, 7);
      assert(gameFix, 5, 3, -1, -1, 7);

    }


    private static void assert(GameFix gameFix, int x1, int y1, int x2, int y2, int countriesPerRow) {
      var country = gameFix.GetClonedGame().Map[x1 + countriesPerRow*y1];
      var neighbour = country.GetNeighbourToNearestEnemy();
      if (x2>=0 && y2>=0) {
        var neighbourId = x2 + countriesPerRow*y2;
        Assert.AreEqual(neighbourId, neighbour.Id);
      } else {
        Assert.IsNull(neighbour);
      }
    }
  }
}
