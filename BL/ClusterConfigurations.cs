using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterGrab {

  /*
  ClusterConfigurations specifies where exctly on the screen clusters have to be placed for 2..15 players.

  Cluster: all the countries of a player are neighbours to each other, as opposed to randomly scattered over the map

  If a game has 2 players, there are 2 clusters, with 3 players 3 clusters, etc.

  The centers of the clusters decide the shape of the clusters:

  Compact:
  The easiest cluster shape for a player to defend is a circle, but circles would leave holes. The next best solution 
  would be hexagon, which cover all pixels of a screen, but if there are only 4 players, the resulting cluster shape
  looks more like a rectangel.
  +------+--------+
  | 0,0   | 2,0   |
  |       |       |
  +-------+-------+
  | 0,2   | 2.2   |
  |       |       |
  +-------+-------+

  Diagonal:
  +---------------+
  |0,0            |
  |    1,1        |
  |        2,2    |
  |            3,3|
  +---------------+

  Horizontal:
  +---------------+
  |    1,0        |
  +---------------+
  |    1,1        |
  +---------------+
  |    1,2        |
  +---------------+
  |    1,3        |
  +---------------+

  Vertical:
  +---+---+---+---+
  |   |   |   |   |
  |0,1|1,1|2,1|3,1|
  |   |   |   |   |
  |   |   |   |   |
  +---+---+---+---+

  The calculation of the compact cluster distribution is difficult when there are many clusters, because each cluster
  should have the same size. The console program CalcOptimalPointsConsole enumerates through all possible cluster center 
  positions and returns the best of them. The WPF program ClusterCenters shows the various cluster shapes.

  Luckily, a cluster center cannot be located on most pixels because of:
  1) the human player cluster should be in the middle of the screen. For a screen with the dimension 1,1, that center
     has to be at 0.5, 0.5
  2) The centers have to be placed in a way that for a very wide, a squarish or a very long screen still every cluster 
     has the equal size. Obviously, for vertical, horizontal and diagonal distributions this means the coordinates 
     have to be multiples of 1/x, where x is the number of clusters. The same is true for compact distribution.

  To cluster of the first player (normally human) should be in the center of the screen, i.e. at 0.5. In order to allow 
  for integer calculation and to avoid rounding problems, the height and the width of the screen is taken as 2 * number 
  of clusters. It has to be 2* so that for odd number of clusters the first one still can be in the middle of the 
  screen:

  Example: 3 clusters get distributed at "pixels" 1, 3, 6
  +-----------+
  | 1         |
  |     2     |
  |         3 |
  0-1-2-3-4-5-6

  For easy reading of the coordinates, also '2 * number  of clusters' is taken for even number of clusters.

  */

  public enum ClusteringEnum {
    random,
    compact,
    diagonal,
    vertical,
    horizontal,
  }


  public static class ClusterConfigurations {

    /// <summary>
    /// Returns an array of x,y coordinates for the number of clusters and the cluster distributon provided. The array 
    /// contains the same bumber of x,y tuples as the number of clusters. The coordinates are specified for a 1 x 1 scren.
    /// To get the actual x and y values for a Width x Height window, the calculation is:
    /// pixel x = x / (2 *  number of clusters) * Width
    /// pixel y = y / (2 *  number of clusters) * Height
    /// </summary>
    public static (int x, int y)[] Get(int numberOfClusters, ClusteringEnum clustering) {
      return clusterConfigurations[numberOfClusters-2, (int)clustering-1];
    }


    static readonly (int, int)[,][] clusterConfigurations =
      {
        {//2 clusters
          //compact, same as vertical
          new (int, int)[]{(2, 2), (0, 2)},
          //diagonal
          new (int, int)[]{(2, 2), (0, 0)},
          //vertical
          new (int, int)[]{(2, 2), (0, 2)},
          //horizontal
          new (int, int)[]{(2, 2), (2, 0)},
        },

        {//3 clusters
          //compact, result looks like vertical
          new (int, int)[]{(3, 3), (5, 3), (1, 3)},
          //diagonal
          new (int, int)[]{(3, 3), (1, 1), (5, 5)},
          //vertical
          new (int, int)[]{(3, 3), (5, 3), (1, 3)},
          //horizontal
          new (int, int)[]{(3, 3), (3, 5), (3, 1)},
        },

        {//4 clusters
          //compact
          new (int, int)[] {(4, 4), (0, 6), (4, 0), (0, 2)},
          //new (int, int)[]{(4, 4), (6, 2), (0, 6), (2, 0)},
          //new (int, int)[]{(4, 4), (0, 4), (4, 0), (0, 0)},
          //diagonal
          new (int, int)[]{(4, 4), (6, 6), (0, 0), (2, 2)},
          //vertical
          new (int, int)[]{(4, 4), (6, 4), (0, 4), (2, 4)},
          //horizontal
          new (int, int)[]{(4, 4), (4, 6), (4, 0), (4, 2)},
        },

        {//5 clusters
          //compact
          new (int, int)[]{(5, 5), (7, 9), (9, 3), (1, 7), (3, 1)},
          //diagonal
          new (int, int)[]{(5, 5), (7, 7), (9, 9), (1, 1), (3, 3)},
          //vertical
          new (int, int)[]{(5, 5), (7, 5), (9, 5), (1, 5), (3, 5)},
          //horizontal
          new (int, int)[]{(5, 5), (5, 7), (5, 9), (5, 1), (5, 3)},
        },

        {//6 clusters
          //compact
          new (int, int)[] {(6, 6), (8, 0), (10, 6), (0, 0), (2, 6), (4, 0)},
          //diagonal
          new (int, int)[]{(6, 6), (8, 8), (10, 10), (0, 0), (2, 2), (4, 4)},
          //vertical
          new (int, int)[]{(6, 6), (8, 6), (10, 6), (0, 6), (2, 6), (4, 6)},
          //horizontal
          new (int, int)[]{(6, 6), (6, 8), (6, 10), (6, 0), (6, 2), (6, 4)},
        },

        {//7 clusters
          //compact
          new (int, int)[]{(7, 7), (9, 13), (11, 5), (13, 11), (1, 3), (3, 9), (5, 1)},
          //diagonal
          new (int, int)[]{(7, 7), (9, 9), (11, 11), (13, 13), (1, 1), (3, 3), (5, 5)},
          //vertical
          new (int, int)[]{(7, 7), (9, 7), (11, 7), (13, 7), (1, 7), (3, 7), (5, 7)},
          //horizontal
          new (int, int)[]{(7, 7), (7, 9), (7, 11), (7, 13), (7, 1), (7, 3), (7, 5)},
        },

        {//8 clusters
          //compact
          new (int, int)[]{(8, 8), (10, 14), (12, 4), (14, 10), (0, 0), (2, 6), (4, 12), (6, 2)},
          //diagonal
          new (int, int)[]{(8, 8), (10, 10), (12, 12), (14, 14), (0, 0), (2, 2), (4, 4), (6, 6)},
          //vertical
          new (int, int)[]{(8, 8), (10, 8), (12, 8), (14, 8), (0, 8), (2, 8), (4, 8), (6, 8)},
          //horizontal
          new (int, int)[]{(8, 8), (8, 10), (8, 12), (8, 14), (8, 0), (8, 2), (8, 4), (8, 6)},
        },

        {//9 clusters
          //compact
          //config[9].Add(("9,9 9,15 15,5 15,11 15,17 3,7 3,13 3,1 9,3", new (int, int)[] {(9, 9), (9, 15), (15, 5), (15, 11), (15, 17), (3, 7), (3, 13), (3, 1), (9, 3)}));
          //new (int, int)[]{(9, 9), (11, 17), (13, 7), (15, 15), (17, 5), (1, 13), (3, 3), (5, 11), (7, 1)},
          new (int, int)[] {(9, 9), (9, 15), (15, 5), (15, 11), (15, 17), (3, 7), (3, 13), (3, 1), (9, 3)},
          //diagonal (!)
          new (int, int)[]{(9, 9), (11, 13), (13, 17), (15, 3), (17, 7), (1, 11), (3, 15), (5, 1), (7, 5)},
          //vertical
          new (int, int)[]{(9, 9), (11, 9), (13, 9), (15, 9), (17, 9), (1, 9), (3, 9), (5, 9), (7, 9)},
          //horizontal
          new (int, int)[]{(9, 9), (9, 11), (9, 13), (9, 15), (9, 17), (9, 1), (9, 3), (9, 5), (9, 7)},
        },

        {//10 clusters
          //("10 clusters 1 Deviation: 0", new []{
          //new PercentPoint(0/10.0, 0/10.0),
          //new PercentPoint(1/10.0, 1/10.0),
          //new PercentPoint(2/10.0, 6/10.0),
          //new PercentPoint(3/10.0, 7/10.0),
          //new PercentPoint(4/10.0, 2/10.0),
          //new PercentPoint(5/10.0, 3/10.0),
          //new PercentPoint(6/10.0, 8/10.0),
          //new PercentPoint(7/10.0, 9/10.0),
          //new PercentPoint(8/10.0, 4/10.0),
          //new PercentPoint(9/10.0, 5/10.0)}),
          //compact
          new (int, int)[]{(10, 10), (12, 12), (14, 2), (16, 4), (18, 14), (0, 16), (2, 6), (4, 8), (6, 18), (8, 0)},
          //diagonal (!)
          new (int, int)[]{(10, 10), (12, 12), (14, 14), (16, 16), (18, 18), (0, 0), (2, 2), (4, 4), (6, 6), (8, 8)},
          //vertical
          new (int, int)[]{(10, 10), (12, 10), (14, 10), (16, 10), (18, 10), (0, 10), (2, 10), (4, 10), (6, 10), (9, 10)},
          //horizontal
          new (int, int)[]{(10, 10), (10, 12), (10, 14), (10, 16), (10, 18), (10, 0), (10, 2), (10, 4), (10, 6), (10, 8)},
        },

        {//11 clusters
          //compact
          new (int, int)[]{(11, 11), (13, 13), (15, 15), (17, 17), (19, 19), (21, 21), (1, 1), (3, 3), (5, 5), (7, 7), (9, 9)},
          //diagonal (!)
          new (int, int)[]{(11, 11), (13, 13), (15, 15), (17, 17), (19, 19), (21, 21), (1, 1), (3, 3), (5, 5), (7, 7), (9, 9)},
          //vertical
          new (int, int)[]{(11, 11), (13, 11), (15, 11), (17, 11), (19, 11), (21, 11), (1, 11), (3, 11), (5, 11), (7, 11), (9, 11)},
          //horizontal
          new (int, int)[]{(11, 11), (11, 13), (11, 15), (11, 17), (11, 19), (11, 21), (11, 1), (11, 3), (11, 5), (11, 7), (11, 9)},
        },

        {//12 clusters
          //compact
          new (int, int)[]{(12, 12), (14, 14), (16, 16), (18, 18), (20, 20), (22, 22), (0, 0), (2, 2), (4, 4), (6, 6), (8, 8), (10, 10)},
          //diagonal (!)
          new (int, int)[]{(12, 12), (14, 14), (16, 16), (18, 18), (20, 20), (22, 22), (0, 0), (2, 2), (4, 4), (6, 6), (8, 8), (10, 10)},
          //vertical
          new (int, int)[]{(12, 12), (14, 12), (16, 12), (18, 12), (20, 12), (22, 12), (0, 12), (2, 12), (4, 12), (6, 12), (8, 12), (10, 12)},
          //horizontal
          new (int, int)[]{(12, 12), (12, 14), (12, 16), (12, 18), (12, 20), (12, 22), (12, 0), (12, 2), (12, 4), (12, 6), (12, 8), (12, 10)},
        },

        {//13 clusters
          //compact
          new (int, int)[]{(13, 13), (15, 15), (17, 17), (19, 19), (21, 21), (23, 23), (25, 25), (1, 1), (3, 3), (5, 5), (7, 7), (9, 9), (11, 11)},
          //diagonal (!)
          new (int, int)[]{(13, 13), (15, 15), (17, 17), (19, 19), (21, 21), (23, 23), (25, 25), (1, 1), (3, 3), (5, 5), (7, 7), (9, 9), (11, 11)},
          //vertical
          new (int, int)[]{(13, 13), (15, 13), (17, 13), (19, 13), (21, 13), (23, 13), (25, 13), (1, 13), (3, 13), (5, 13), (7, 13), (9, 13), (11, 13)},
          //horizontal
          new (int, int)[]{(13, 13), (13, 15), (13, 17), (13, 19), (13, 21), (13, 23), (13, 25), (13, 1), (13, 3), (13, 5), (13, 7), (13, 9), (13, 11)},
        },

        {//14 clusters
          //compact
          new (int, int)[]{(14, 14), (16, 16), (18, 18), (20, 20), (22, 22), (24, 24), (26, 26), (0, 0), (2, 2), (4, 4), (6, 6), (8, 8), (10, 10), (12, 12)},
          //diagonal (!)
          new (int, int)[]{(14, 14), (16, 16), (18, 18), (20, 20), (22, 22), (24, 24), (26, 26), (0, 0), (2, 2), (4, 4), (6, 6), (8, 8), (10, 10), (12, 12)},
          //vertical
          new (int, int)[]{(14, 14), (16, 14), (18, 14), (20, 14), (22, 14), (24, 14), (26, 14), (0, 14), (2, 14), (4, 14), (6, 14), (8, 14), (10, 14), (12, 14)},
          //horizontal
          new (int, int)[]{(14, 14), (14, 16), (14, 18), (14, 20), (14, 22), (14, 24), (14, 26), (14, 0), (14, 2), (14, 4), (14, 6), (14, 8), (14, 10), (14, 12)},
        },

        {//15 clusters
          //compact
          new (int, int)[]{(15, 15), (17, 17), (19, 19), (21, 21), (23, 23), (25, 25), (27, 27), (29, 29), (1, 1), (3, 3), (5, 5), (7, 7), (9, 9), (11, 11), (13, 13)},
          //diagonal (!)
          new (int, int)[]{(15, 15), (17, 17), (19, 19), (21, 21), (23, 23), (25, 25), (27, 27), (29, 29), (1, 1), (3, 3), (5, 5), (7, 7), (9, 9), (11, 11), (13, 13)},
          //vertical
          new (int, int)[]{(15, 15), (17, 15), (19, 15), (21, 15), (23, 15), (25, 15), (27, 15), (29, 15), (1, 15), (3, 15), (5, 15), (7, 15), (9, 15), (11, 15), (13, 15)},
          //horizontal
          new (int, int)[]{(15, 15), (15, 17), (15, 19), (15, 21), (15, 23), (15, 25), (15, 27), (15, 29), (15, 1), (15, 3), (15, 5), (15, 7), (15, 9), (15, 11), (15, 13)},
        },
      };
  }
}
