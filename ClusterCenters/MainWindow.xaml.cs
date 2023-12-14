using MasterGrab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace ClusterCenters {


  /// <summary>
  /// Point using integer coordinates
  /// </summary>
  public record struct IntPoint(int X, int Y) {
    static public IntPoint Undef = new(int.MinValue, int.MinValue);


    public override string ToString() {
      return $"X: {X}, Y: {Y}";
    }
  }


  ///// <summary>
  ///// Point using percentage of available pixels as coordinates
  ///// </summary>
  //public record struct PercentPoint(double X, double Y) {
  //  static public PercentPoint Undef = new(double.MinValue, double.MinValue);


  //  public override string ToString() {
  //    return $"X: {X:N3}, Y: {Y:N3}";
  //  }
  //}


  /// <summary>
  /// Defines for each x cluster coordinate a combination on TextBlocks and a TextBox
  /// </summary>
  public record Cluster(
    int ID,
    int Color,
    TextBlock CanvasClusterTextBlock,
    TextBlock ClusterIdTextBlock,
    TextBox XClusterTextBox,
    TextBox YClusterTextBox,
    TextBlock PercentTextBlock,
    TextBlock DeviationTextBlock) {
    public int XCluster;
    public int YCluster;
    public int XPixel;
    public int YPixel;
    public int PixelsCount;
  }


  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window {

    public const int MaxNumberOfClusters = 15;


    /// <summary>
    /// Contains cluster configurations from:
    /// 1) ClusterConfigurations.cs
    /// 2) createLocalClusterConfigurations()
    /// 3) added by user while ClusterCenter is running
    /// </summary>
    public readonly List<(string Description, (int X, int Y)[] Clusters)>[] LocalClusterConfigurations = 
      createLocalClusterConfigurations();


    private static List<(string, (int, int)[])>[] createLocalClusterConfigurations() {
      var config = new List<(string, (int, int)[])>[MaxNumberOfClusters+1];
      for (int configIndex=2; configIndex<config.Length; configIndex++) {
        var configList = new List<(string, (int, int)[])>();
        config[configIndex] = configList;

        for (ClusteringEnum clusteringIndex = ClusteringEnum.compact; clusteringIndex<=ClusteringEnum.horizontal; clusteringIndex++) {
          var clusterConfiguration = ClusterConfigurations.Get(configIndex, clusteringIndex);
          configList.Add((clusteringIndex.ToString(), clusterConfiguration));
        }
      }

      config[6].Add(("rectangles", new (int, int)[] { (6, 6), (10, 6), (2, 6), (6, 0), (10, 0), (2, 0) }));
      config[6].Add(("diagonal 2", new (int, int)[] { (6, 6), (8, 2), (10, 10), (0, 6), (2, 2), (4, 10) }));

      config[10].Add(("10,10 14,10 18,10 2,0 6,0 10,0 14,0 18,0 2,10 6,10", new (int, int)[] { (10, 10), (14, 10), (18, 10), (2, 0), (6, 0), (10, 0), (14, 0), (18, 0), (2, 10), (6, 10) }));
      config[10].Add(("10,10 14,12 18,14 2,16 6,18 10,0 14,2 18,4 2,6 6,8", new (int, int)[] { (10, 10), (14, 12), (18, 14), (2, 16), (6, 18), (10, 0), (14, 2), (18, 4), (2, 6), (6, 8) }));
      config[10].Add(("10,10 18,12 6,14 14,16 2,18 10,0 18,2 6,4 14,6 2,8", new (int, int)[] { (10, 10), (18, 12), (6, 14), (14, 16), (2, 18), (10, 0), (18, 2), (6, 4), (14, 6), (2, 8) }));
      config[10].Add(("10,10 18,10 6,15 14,15 2,0 10,0 18,0 6,5 14,5 2,10", new (int, int)[] { (10, 10), (18, 10), (6, 15), (14, 15), (2, 0), (10, 0), (18, 0), (6, 5), (14, 5), (2, 10) }));
      config[10].Add(("10,10 12,14 14,18 16,2  18,6  0,10 2,14 4,18 6,2  8,6", new (int, int)[] { (10, 10), (12, 14), (14, 18), (16, 2), (18, 6), (0, 10), (2, 14), (4, 18), (6, 2), (8, 6) }));
      config[10].Add(("10,10 12,16 14,2  16,8  18,14 0,0  2,6  4,12 6,18 8,4", new (int, int)[] { (10, 10), (12, 16), (14, 2), (16, 8), (18, 14), (0, 0), (2, 6), (4, 12), (6, 18), (8, 4) }));
      config[10].Add(("10,10 12,18 14,6  16,14 18,2  0,10 2,18 4,6  6,14 8,2", new (int, int)[] { (10, 10), (12, 18), (14, 6), (16, 14), (18, 2), (0, 10), (2, 18), (4, 6), (6, 14), (8, 2) }));
      config[10].Add(("10,10 12,0  14,10 16,0  18,10 0,0  2,10 4,0  6,10 8,0", new (int, int)[] { (10, 10), (12, 0), (14, 10), (16, 0), (18, 10), (0, 0), (2, 10), (4, 0), (6, 10), (8, 0) }));

      return config;
    }


    public readonly int[] ClusterColours = {
      0x40ffff, 0xff40ff, 0xffff40, 0x4040ff, 0x40ff40, 0xff4040,
      0xff8000, 0x8000ff, 0xff80, 0x80ff00, 0xff0080, 0x80ff,
      0x808080, 0xa0a0a0, 0xc0c0c0, 0xe0e0e0};
    public readonly SolidColorBrush TextBlockBackground = new(Color.FromArgb(0xa0, 0xff, 0xff, 0xff));


    //readonly DispatcherTimer timer;
    //readonly TextBlock labelClusterIdTextBlock;
    //readonly TextBlock labelXTextBlock;
    //readonly TextBlock labelYTextBlock;
    //readonly TextBlock labelPercentTextBlock;
    //readonly TextBlock labelDeviationTextBlock;

    readonly StackPanel topStackPanel;
    readonly CheckBox splitCheckBox;
    readonly CheckBox squareCheckBox;
    readonly List<ClustersUserControl> clustersUserControls = new();


    public MainWindow() {
      InitializeComponent();

      MainGrid.Background = Brushes.Black;
      topStackPanel = new StackPanel { Orientation=Orientation.Horizontal, Background=Brushes.LightGray };
      Grid.SetColumnSpan(topStackPanel, 3);
      var label = new Label { Content = "Split Window:" };
      topStackPanel.Children.Add(label);
      splitCheckBox = new CheckBox { VerticalAlignment=VerticalAlignment.Center };
      splitCheckBox.Click += splitCheckBox_Click;
      topStackPanel.Children.Add(splitCheckBox);
      
      label = new Label { Content = "Square:" };
      topStackPanel.Children.Add(label);
      squareCheckBox = new CheckBox { VerticalAlignment=VerticalAlignment.Center };
      squareCheckBox.Click += SquareCheckBox_Click;
      topStackPanel.Children.Add(squareCheckBox);

      var clustersUserControl = new ClustersUserControl(this);
      Grid.SetRow(clustersUserControl, 1);
      Grid.SetColumn(clustersUserControl, 0);
      clustersUserControls.Add(clustersUserControl);

      clustersUserControl = new ClustersUserControl(this);
      Grid.SetRow(clustersUserControl, 1);
      Grid.SetColumn(clustersUserControl, 2);
      clustersUserControls.Add(clustersUserControl);

      clustersUserControl = new ClustersUserControl(this);
      Grid.SetRow(clustersUserControl, 3);
      Grid.SetColumn(clustersUserControl, 0);
      clustersUserControls.Add(clustersUserControl);

      clustersUserControl = new ClustersUserControl(this);
      Grid.SetRow(clustersUserControl, 3);
      Grid.SetColumn(clustersUserControl, 2);
      clustersUserControls.Add(clustersUserControl);

      splitCheckBox.IsChecked = true;
      setupSplitScreen();
    }


    private void splitCheckBox_Click(object sender, RoutedEventArgs e) {
      setupSplitScreen();
    }


    private void SquareCheckBox_Click(object sender, RoutedEventArgs e) {
      foreach (var clustersUserControl in clustersUserControls) {
        clustersUserControl.IsSquare = squareCheckBox.IsChecked!.Value;
      }
    }


    private void setupSplitScreen() {
      MainGrid.Children.Clear();
      MainGrid.RowDefinitions.Clear();
      MainGrid.ColumnDefinitions.Clear();

      if (splitCheckBox.IsChecked!.Value) {
        //show 4 ClustersUserControls
        MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
        MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        MainGrid.Children.Add(topStackPanel);
        MainGrid.Children.Add(clustersUserControls[0]);
        MainGrid.Children.Add(clustersUserControls[1]);
        MainGrid.Children.Add(clustersUserControls[2]);
        MainGrid.Children.Add(clustersUserControls[3]);

      } else {
        //show 1 ClustersUserControl
        MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        MainGrid.Children.Add(topStackPanel);
        MainGrid.Children.Add(clustersUserControls[0]);
      }
    }
  }
}
