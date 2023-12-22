/********************************************************************************************************

MasterGrab.BL.Coordinate
========================

Stores the X and Y address of a Pixel within the area available for display (PixelMap), which defines the
biggest valid values for X and Y. The map bends around at the borders, i.e. the left most pixels are the 
neighbours of the right most pixels and the top most pixels are the neighbours of the bottom most pixels.

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


namespace MasterGrab {

  /// <summary>
  /// Stores the X and Y address of a Pixel within the area available for display (PixelMap), which defines the
  /// biggest valid values for X and Y. The map bends around at the borders, i.e. the left most pixels are the neighbours 
  /// of the right most pixels and the top most pixels are the neighbours of the bottom most pixels.
  /// </summary>
  public readonly struct Coordinate: IEquatable<Coordinate> {

    #region Properties
    //      ----------

    /// <summary>
    /// Offset from left border of PixelMap
    /// </summary>
    public readonly int X;


    /// <summary>
    /// Offset from top border of PixelMap
    /// </summary>
    public readonly int Y;


    /// <summary>
    /// A coordinate belongs to a PixelMap (display area), which defines the biggest valid values for X and Y.
    /// </summary>
    public readonly PixelMap? PixelMap;

    
    /// <summary>
    /// Coordinate.Null can be used in the same way as Null is used for objects
    /// </summary>
    public static readonly Coordinate Null = new(null);
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor
    /// </summary>
    public Coordinate(PixelMap pixelMap, int x, int y) {
      PixelMap = pixelMap;
      if (x<0 || x>=PixelMap.XCount) {
        throw new ArgumentException("Coordinate X " + x + " should be between 0 and " + (PixelMap.XCount-1) + ".");
      }

      if (y<0 || y>=PixelMap.YCount) {
        throw new ArgumentException("Coordinate Y " + y + " should be between 0 and " + (PixelMap.YCount-1) + ".");
      }

      X = x;
      Y = y;
    }


    /// <summary>
    /// Constructor used to create illegal coordinates
    /// </summary>
    private Coordinate(PixelMap? pixelMap) {
      PixelMap = pixelMap;
      X = int.MinValue;
      Y = int.MinValue;
    }

    
    /// <summary>
    /// Returns a coordinate for x and y, if they are within pixelMap, otherwise Coordinate.Null and false.
    /// </summary>
    public static bool TryConvert(PixelMap pixelMap, int x, int y, out Coordinate coordinate) {
      if (pixelMap.IsWithin(x, y)) {
        coordinate = new Coordinate(pixelMap, x, y);
        return true;
      }
      coordinate = Null;
      return false;
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Returns square distance between 2 coordinates. It is faster to calculate the square distance than the distance, 
    /// bit often the square is enough.
    /// </summary>
    public int GetSquareDistance(Coordinate coordinate2) {
      if (PixelMap!=coordinate2.PixelMap) throw new ArgumentException("Both coordinates must be from the same map.");

      var xDistance = (PixelMap!.XCount + X - coordinate2.X) % PixelMap.XCount;
      if (xDistance>PixelMap.XCount/2) {
        //in a wrap around map, the x-distance between 2 points cannot be more than half the map height
        xDistance = PixelMap.XCount - xDistance;
      }
      var yDistance = (PixelMap.YCount + Y - coordinate2.Y) % PixelMap.YCount;
      if (yDistance>PixelMap.YCount/2) {
        //in a wrap around map, the y-distance between 2 points cannot be more than half the map width
        yDistance = PixelMap.YCount - yDistance;
      }
      return xDistance*xDistance + yDistance*yDistance;
    }


    /// <summary>
    /// Returns true if the 2 coordinates sit exactly next to each other, but both at another map border.
    /// </summary>
    public bool IsCrossingBorder(Coordinate coordinate2) {
      if (PixelMap!=coordinate2.PixelMap)
        throw new ArgumentException("Both coordinates must be from the same map.");

      if (X==0) {
        if (coordinate2.X==PixelMap!.XMax) {
          return true;
        }
      } else if (X==PixelMap!.XMax) {
        if (coordinate2.X==0) {
          return true;
        }
      }

      if (Y==0) {
        if (coordinate2.Y==PixelMap.YMax) {
          return true;
        }
      } else if (Y==PixelMap.YMax) {
        if (coordinate2.Y==0) {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Returns the coordinate of the pixel left of this coordinate pixel. If the coordinate is at the left border, the right most
    /// pixel gets returned.
    /// </summary>
    public Coordinate Left() {
      var newX = X<1 ? PixelMap!.XMax : X - 1;
      return new Coordinate(PixelMap!, newX, Y);
    }


    /// <summary>
    /// Returns the coordinate of the pixel right of this coordinate pixel. If the coordinate is at the right border, the left most
    /// pixel gets returned.
    /// </summary>
    public Coordinate Right() {
      var newX = X + 1;
      if (newX>=PixelMap!.XCount)
        newX = 0;

      return new Coordinate(PixelMap, newX, Y);
    }


    /// <summary>
    /// Returns the coordinate of the pixel above of this coordinate pixel. If the coordinate is at the top border, the bottom most
    /// pixel gets returned.
    /// </summary>
    public Coordinate Up() {
      var newY = Y<1 ? PixelMap!.YCount-1 : Y - 1;
      return new Coordinate(PixelMap!, X, newY);
    }


    /// <summary>
    /// Returns the coordinate of the pixel below of this coordinate pixel. If the coordinate is at the bottom border, the top most
    /// pixel gets returned.
    /// </summary>
    public Coordinate Down() {
      var newY = Y + 1;
      if (newY>=PixelMap!.YCount)
        newY = 0;

      return new Coordinate(PixelMap, X, newY);
    }


    /// <summary>
    /// Returns true if both coordinates have the same values
    /// </summary>
    public bool Equals(Coordinate other) {
      return PixelMap==other.PixelMap && X==other.X && Y==other.Y;
    }


    /// <summary>
    /// Returns true if both are coordinates and have the same values
    /// </summary>
    public override bool Equals(object? obj) {
      return obj is Coordinate coordinate && Equals(coordinate);
    }


    public static bool operator ==(Coordinate left, Coordinate right) {
      return left.Equals(right);
    }


    public static bool operator !=(Coordinate left, Coordinate right) {
      return !left.Equals(right);
    }


    /// <summary>
    /// Returns the hash code for this coordinate.
    /// </summary>
    public override int GetHashCode() {
      //Map is not included, too complicated. Usually, coordinates of 2 different maps don't get mixed anyway
      return X*PixelMap!.YCount + Y;
    }


    /// <summary>
    /// Returns the X and Y value of the coordinate as string
    /// </summary>
    public override string ToString() {
      return "X: " + (X==int.MinValue ? "undef" : X.ToString()) + ", Y: " + (Y==int.MinValue ? "undef" : Y.ToString());
    }
    #endregion
  }
}
