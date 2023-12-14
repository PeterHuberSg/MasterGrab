// See https://aka.ms/new-console-template for more information
using ClusterCenters;
using System.Text;
#pragma warning disable CS0162 // Unreachable code detected


Console.WriteLine("Calculate optimal points distribution");
Console.WriteLine("*************************************");
Console.WriteLine();
const int pointsCount = 8;
const bool isShowPixelMapInConsole = false;
const bool isShowAllResults = false;//if true, the results for each combination in the first loop gets displayed. If false,
                                    //if false, only the results from the second loops are displayed, which are
                                    //sorted by deviation.
                                    //The second loop is limited to at most 100 lines.
const int showMaxDeviation = 100;//The second loop shows only results with deviation smaller showMaxDeviation

//const int pointsWidth = pointsCount;
const int pointsWidth = 2;

const int pixelsWidth = pointsCount * pointsWidth;
const int pixelsWidthHalf = pixelsWidth / 2;
const int totalPixelCount = pixelsWidth * pixelsWidth;
const int expectedSizePerPoint = totalPixelCount / pointsCount;
Console.WriteLine($"PointsCount: {pointsCount}");
Console.WriteLine($"Ideal average size: {expectedSizePerPoint}");

//try to calculate optimal solution
//x coordinate increase constantly
//y coordinates with all possibilities
var points = new IntPoint[pointsCount];
var resultPoints = new List<(double Deviation, IntPoint[] Points)>();

//This algorithm places x points in a x*x pixel square, then decides for each pixel which point is the nearest. That point 
//becomes the owner of that pixel. Next the sum of all pixels owned by a points is calculated. The goal is that each 
//sum has the same value.
//
//The pixel coordinates of each point gets calculated as follows:
//1) point 0 has always the pixel coordinates [0,0]
//2) the x-coordinate by each following point gets incremented, i.e point1 is at [1,?]
//3) yAvailables contains the y-coordinates available. If point1 gets set at [1,1], value 1 gets removed from yAvailables
//4) the remaining points also take any y-coordinate they like, as long yAvailables still contains it.
//5) Once the last point is created, yAvailable will be 0

//keep first point fixed in top left corner
var x = 0;
points[0] = new IntPoint(0, 0);
var yAvailables = new bool[pointsCount];
yAvailables[0] = false;
for (int yAvailablesIndex = 1; yAvailablesIndex < pointsCount; yAvailablesIndex++) {
  yAvailables[yAvailablesIndex] = true;
}
StringBuilder sb = new();
Console.Write("Configurations calculated: ");
(int cursorResultLeft, int cursorResultTop) = Console.GetCursorPosition();
calcOptimum(x);
Console.SetCursorPosition(cursorResultLeft, cursorResultTop);
Console.Write(resultPoints.Count);
Console.WriteLine();

sb.Clear();
Console.WriteLine("======================================");
var pointIndex = 0;
foreach (var (Deviation, Points) in resultPoints.OrderBy(rp => rp.Deviation).Take(100)) {
  /*
      ("5 clusters C", new []{
      new PercentPoint(5/10.0, 5/10.0),
      new PercentPoint(1/10.0, 1/10.0),
      new PercentPoint(3/10.0, 3/10.0),
      new PercentPoint(7/10.0, 7/10.0),
      new PercentPoint(9/10.0, 9/10.0)}),
  */
  if (Deviation>showMaxDeviation) continue;

  sb.AppendLine($"      (\"{pointsCount} clusters {pointIndex++} Deviation: {Deviation}\", new []{{");
  var divider = $"{pointsCount}.0";
  foreach (var point in Points) {
    Console.Write($"{point.X,3},{point.Y,3}|");
    sb.AppendLine($"      new PercentPoint({point.X/pointsWidth}/{divider}, {point.Y/pointsWidth}/{divider}),");
  }
  Console.WriteLine($": {Deviation}");
  sb.Length -= (',' + Environment.NewLine).Length;
  sb.AppendLine("}),");
  sb.AppendLine();
}
var codeLines = sb.ToString();
Console.WriteLine(codeLines);
//System.Diagnostics.Debugger.Break();


void calcOptimum(int x) {
  if (++x>=pointsCount) {
    //all points are created
    if (isShowAllResults) {
      sb.Clear();
    }
    //counts how many pixels belongs to one point * pointPixelCountsWeight
    var pointPixelCounts = new int[pointsCount];
    //If 2 points have the same distance to once pixel, each should count half. If 3 points have the same distance
    //to once pixel, each should count 1/3. To avoid double calculation, the easiest way is to give every pixel
    //a weight of 6. It's extremely seldom that 4 or more points have exactly the same distance to a pixel. To keep
    //integer calculation, those pixels get not counted in pointPixelCounts.
    const int pixelWeight = 6;
    var pointIsClosest = new bool[pointsCount];
    var closestPointsCount = 0;
    for (int rowIndex = 0; rowIndex < pixelsWidth; rowIndex++) {
      for (int colIndex = 0; colIndex < pixelsWidth; colIndex++) {
        //if (rowIndex==7 && colIndex==5) {

        //}
        var point0 = new IntPoint(colIndex, rowIndex);
        var distance = int.MaxValue;
        var point1IndexFound = int.MaxValue;
        var point2IndexFound = int.MaxValue;
        for (int point1Index = 0; point1Index < pointsCount; point1Index++) {
          var point1 = points[point1Index];
          var xDif = Math.Abs(point0.X - point1.X);
          if (xDif>pixelsWidthHalf) {
            xDif = pixelsWidth- xDif;
          }
          var yDif = Math.Abs(point0.Y - point1.Y);
          if (yDif>pixelsWidthHalf) {
            yDif = pixelsWidth - yDif;
          }
          var newDistance = xDif*xDif + yDif*yDif;
          if (distance>newDistance) {
            distance = newDistance;
            if (closestPointsCount>2) {
              Array.Clear(pointIsClosest);
            }
            closestPointsCount = 1;
            point1IndexFound = point1Index;

          } else if (distance==newDistance) {
            closestPointsCount++;
            if (closestPointsCount==2) {
              point2IndexFound = point1Index;
            } else {
              if (closestPointsCount==3) {
                Array.Clear(pointIsClosest);
                pointIsClosest[point1IndexFound] = true;
                pointIsClosest[point2IndexFound] = true;
                pointIsClosest[point1Index] = true;
              }
            }
          }
        }

        if (closestPointsCount==1) {
          //only 1 point found
          pointPixelCounts[point1IndexFound] += 6;
          if (isShowPixelMapInConsole) {
            sb.Append(point1IndexFound);
          }
        } else if (closestPointsCount==2) {
          //2 points have the same distance
          pointPixelCounts[point1IndexFound] += 3;
          pointPixelCounts[point2IndexFound] += 3;
          if (isShowPixelMapInConsole) {
            sb.Append('.');
          }
        } else if (closestPointsCount==3) {
          for (int pointIsClosestIndex = 0; pointIsClosestIndex < pointIsClosest.Length; pointIsClosestIndex++) {
            if (pointIsClosest[pointIsClosestIndex]) {
              pointPixelCounts[pointIsClosestIndex] += 2;
            }
          }
          if (isShowPixelMapInConsole) {
            sb.Append(',');
          }
        } else {
          //many points hav exactly the same distance. This happens very seldom. The pointPixelCounts increment becomes
          //too big if many points can have the same distance.
          if (isShowPixelMapInConsole) {
            sb.Append('?');
          }
        }
      }
      if (isShowPixelMapInConsole) {
        sb.AppendLine();
      }
    }















    var deviationTotal = 0;
    foreach (var pointPixelCount in pointPixelCounts) {
      var deviation = expectedSizePerPoint - pointPixelCount/pixelWeight;
      deviationTotal += deviation * deviation;
    }

    var pointsCoordinates = new IntPoint[pointsCount];
    for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++) {
      var point = points[pointIndex];
      pointsCoordinates[pointIndex] = point;
      if (isShowAllResults) {
        sb.Append($"{point.X,3}, {point.Y,3}, {pointPixelCounts[pointIndex]/6}| ");
        if (isShowPixelMapInConsole) {
          //mark pixel where point is located with a '+'
          var pointOffset = point.X + (pixelsWidth+Environment.NewLine.Length) * point.Y;
          sb[pointOffset] = '+';
        }
      }
    }
    resultPoints.Add((deviationTotal, pointsCoordinates));
    if (isShowAllResults) {
      sb.Append(deviationTotal);
      Console.WriteLine(sb.ToString());
    }

    if (resultPoints.Count % 1000 == 0) {
      Console.SetCursorPosition(cursorResultLeft, cursorResultTop);
      Console.Write(resultPoints.Count);
    }
    return;
  }

  for (int y = 0; y<pointsCount; y++) {
    if (yAvailables[y]) {
      yAvailables[y] = false;
      points[x] = new IntPoint(x*pointsWidth, y*pointsWidth);
      //try one more call
      calcOptimum(x);
      yAvailables[y] = true;
    }
  }
}


