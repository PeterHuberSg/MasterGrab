//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Controls;
//using System.Windows.Media;

//namespace ClusterCenters {


//  /// <summary>
//  /// Point using integer coordinates
//  /// </summary>
//  public record struct IntPoint(int X, int Y) {
//    static public PercentPoint Undef = new(double.MinValue, double.MinValue);


//    public override string ToString() {
//      return $"X: {X}, Y: {Y}";
//    }
//  }


//  /// <summary>
//  /// Point using percentage of available pixels as coordinates
//  /// </summary>
//  public record struct PercentPoint(double X, double Y) {
//    static public PercentPoint Undef = new(double.MinValue, double.MinValue);


//    public override string ToString() {
//      return $"X: {X:N3}, Y: {Y:N3}";
//    }
//  }


//  public class Cluster {

//    public readonly int Id;
//    public IntPoint IntCenter;
//    public PercentPoint PercentCenter;
//    /// <summary>
//    /// Number of pixels belonging to cluster
//    /// </summary>
//    public int PixelsCount;
//    /// <summary>
//    /// Percentage of drawing space belonging to cluster
//    /// </summary>
//    public double PixelPercent;
//    public readonly ClusterTextBlock ClusterTextBlock;
//    public readonly int Colour;

//    public TextBox IntCenterXTextBox;
//    public TextBox IntCenterYTextBox;
//    public TextBox PercentCenterXTextBox;
//    public TextBox PercentCenterYTextBox;
//    public TextBox PercentTextBox;

//    readonly int[] clusterColours = { 
//      0x40ffff, 0xff40ff, 0xffff40, 0x4040ff, 0x40ff40, 0xff4040, 
//      0xff8000, 0x8000ff, 0xff80, 0x80ff00, 0xff0080, 0x80ff,
//      0x808080, 0xa0a0a0, 0xc0c0c0, 0xe0e0e0};
//    readonly SolidColorBrush textBlockBackground = new(Color.FromArgb(0xa0, 0xff, 0xff, 0xff));


//    public Cluster(int id, MainWindow mainWindow, Canvas canvas) {
//      Id = id;
//      ClusterTextBlock = new ClusterTextBlock(this, mainWindow) { FontSize=22, Background=textBlockBackground };
//      canvas.Children.Add(ClusterTextBlock);

//      Colour = clusterColours[id];
//    }


//    override public string ToString() {
//      return $"Id: {Id}, IntCenter: {IntCenter}, PercentCenter: {PercentCenter}, PixelsCount: {PixelsCount}, PixelPercent: {PixelPercent:1}";
//    }
//  }
//}
