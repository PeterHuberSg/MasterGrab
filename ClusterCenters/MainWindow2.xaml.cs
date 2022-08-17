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
  /// Defines for each x cluster coordinate a combination on TextBlocks and a TextBox
  /// </summary>
  public record Cluster2( 
    int ID,
    int Color,
    TextBlock CanvasClusterTextBlock,
    TextBlock ClusterIdTextBlock,
    TextBox XClusterTextBox,
    TextBox YClusterTextBox,
    TextBlock PercentTextBlock,
    TextBlock DeviationTextBlock) 
  {
    public int XCluster;
    public int YCluster;
    public int XPixel; 
    public int YPixel;
    public int PixelsCount;
  }


  /// <summary>
  /// Interaction logic for MainWindow2.xaml
  /// </summary>
  public partial class MainWindow2: Window {

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


    public MainWindow2() {
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


      ////
      //labelClusterIdTextBlock = new TextBlock { Text="Cluster Id:", FontWeight=FontWeights.Bold };
      //Grid.SetRow(labelClusterIdTextBlock, 0);
      //labelXTextBlock = new TextBlock { Text="X:", FontWeight=FontWeights.Bold };
      //Grid.SetRow(labelXTextBlock, 1);
      //labelYTextBlock = new TextBlock { Text="Y:", FontWeight=FontWeights.Bold };
      //Grid.SetRow(labelYTextBlock, 2);
      //labelPercentTextBlock = new TextBlock { Text="Percent:", FontWeight=FontWeights.Bold };
      //Grid.SetRow(labelPercentTextBlock, 3);
      //labelDeviationTextBlock = new TextBlock { Text="Deviation:", FontWeight=FontWeights.Bold };
      //Grid.SetRow(labelDeviationTextBlock, 4);

      //var gridColumn = 2;
      //for (int clusterIndex = 0; clusterIndex<maxNumberOfClusters; clusterIndex++) {

      //  //TextBlock within Canvas
      //  var canvasClusterTextBlock = new TextBlock { FontSize=22, Background=textBlockBackground, Tag = clusterIndex };
      //  //canvasClusterTextBlock.PreviewMouseDown += CanvasClusterTextBlock_MouseDown;
      //  //canvasClusterTextBlock.MouseMove += CanvasClusterTextBlock_MouseMove;
      //  //canvasClusterTextBlock.MouseUp += CanvasClusterTextBlock_MouseUp;

      //  //ClusterGrid
      //  //row 0: Cluster Id
      //  var clusterIdTextBlock = new TextBlock { Text = clusterIndex.ToString(), FontWeight = FontWeights.Bold };
      //  Grid.SetRow(clusterIdTextBlock, 0);
      //  Grid.SetColumn(clusterIdTextBlock, gridColumn);

      //  //row 1: xCluster coordinate
      //  var xClusterTextBox = new TextBox { Tag = clusterIndex };
      //  xClusterTextBox.GotFocus += clusterTextBox_GotFocus;
      //  xClusterTextBox.PreviewLostKeyboardFocus += xClusterTextBox_PreviewLostKeyboardFocus;
      //  Grid.SetRow(xClusterTextBox, 1);
      //  Grid.SetColumn(xClusterTextBox, gridColumn);

      //  //row 2: yCluster coordinate
      //  var yClusterTextBox = new TextBox { Tag = clusterIndex };
      //  yClusterTextBox.GotFocus += clusterTextBox_GotFocus;
      //  yClusterTextBox.PreviewLostKeyboardFocus += yClusterTextBox_PreviewLostKeyboardFocus;
      //  Grid.SetRow(yClusterTextBox, 2);
      //  Grid.SetColumn(yClusterTextBox, gridColumn);

      //  //row 3: Percentage of window space used by cluster 
      //  var percentTextBlock = new TextBlock { };
      //  Grid.SetRow(percentTextBlock, 3);
      //  Grid.SetColumn(percentTextBlock, gridColumn);

      //  //row 4: Deviation from equally sized cluster space: (actualSpace-idealSpace) * (actualSpace-idealSpace)
      //  var deviationTextBlock = new TextBlock { };
      //  Grid.SetRow(deviationTextBlock, 4);
      //  Grid.SetColumn(deviationTextBlock, gridColumn);

      //  Clusters[clusterIndex] = new Cluster2(
      //    clusterIndex,
      //    ClusterColours[clusterIndex],
      //    canvasClusterTextBlock,
      //    clusterIdTextBlock,
      //    xClusterTextBox,
      //    yClusterTextBox,
      //    percentTextBlock,
      //    deviationTextBlock);
      //  gridColumn += 2;
      //}

      //for (int numberOfClusters = 2; numberOfClusters<16; numberOfClusters++) {
      //  NumberOfCCombobox.Items.Add(new ComboBoxItem() { Content = numberOfClusters.ToString() });
      //}
      //NumberOfCCombobox.SelectionChanged += NumberOfCCombobox_SelectionChanged;
      //ConfigCombobox.SelectionChanged += ConfigCombobox_SelectionChanged;
      //NumberOfCCombobox.SelectedIndex = 6 - 2;

      //SizeChanged += MainWindow2_SizeChanged;
      //MainCanvas.SizeChanged += MainCanvas_SizeChanged;
      //NextButton.Click += NextButton_Click;
      //PreviousButton.Click += PreviousButton_Click;
      //AddButton.Click += AddButton_Click;
      //CopyButton.Click += CopyButton_Click;
      //RemoveButton.Click += RemoveButton_Click;
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


    //int clustersUsedCount;
    //bool isSetupClusters;
    //int averagePercentage;


    //private void setupClusters(int numberOfClusters) {
    //  if (clustersUsedCount==numberOfClusters) return;

    //  if (numberOfClusters<2) throw new ArgumentException("");
    //  if (numberOfClusters>MaxNumberOfClusters) throw new ArgumentException("");

    //  clustersUsedCount = numberOfClusters;

    //  isSetupClusters = true;
    //  ConfigCombobox.Items.Clear();
    //  var configList = LocalClusterConfigurations[clustersUsedCount];
    //  for (int configurationsIndex = 0; configurationsIndex<configList.Count; configurationsIndex++) {
    //    ConfigCombobox.Items.Add(new ComboBoxItem() {Content = configList[configurationsIndex].Description});
    //  }
    //  isSetupClusters = false;
    //  ConfigCombobox.SelectedIndex = 0;

    //  MainCanvas.Children.Clear();
    //  ClusterGrid.Children.Clear();

    //  ClusterGrid.ColumnDefinitions.Clear();
    //  ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
    //  ClusterGrid.Children.Add(labelClusterIdTextBlock);
    //  ClusterGrid.Children.Add(labelXTextBlock);
    //  ClusterGrid.Children.Add(labelYTextBlock);
    //  ClusterGrid.Children.Add(labelPercentTextBlock);
    //  ClusterGrid.Children.Add(labelDeviationTextBlock);

    //  for (int clusterIndex = 0; clusterIndex<clustersUsedCount; clusterIndex++) {
    //    var cluster = clusters[clusterIndex];
    //    MainCanvas.Children.Add(cluster.CanvasClusterTextBlock);

    //    ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
    //    ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

    //    if (clusterIndex%2==1) {
    //      var rectangle = new Rectangle { Fill=Brushes.DarkGray };
    //      Grid.SetColumn(rectangle, 2 + clusterIndex*2);
    //      Grid.SetRowSpan(rectangle, 4);
    //      ClusterGrid.Children.Add(rectangle);
    //    }

    //    ClusterGrid.Children.Add(cluster.ClusterIdTextBlock);
    //    ClusterGrid.Children.Add(cluster.XClusterTextBox);
    //    ClusterGrid.Children.Add(cluster.YClusterTextBox);
    //    ClusterGrid.Children.Add(cluster.PercentTextBlock);
    //    ClusterGrid.Children.Add(cluster.DeviationTextBlock);
    //  }

    //  averagePercentage = (1000 / clustersUsedCount);
    //  AverageTextBlock.Text = averagePercentage.ToString();
    //}


    //private void changeClusterConfiguration(int configurationIndex) {
    //  var clusterConfiguration = LocalClusterConfigurations[clustersUsedCount][configurationIndex];
    //  for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
    //    clusters[clusterIndex].XCluster = clusterConfiguration.Clusters[clusterIndex].X;
    //    clusters[clusterIndex].YCluster = clusterConfiguration.Clusters[clusterIndex].Y;
    //  }
    //  reDraw();
    //}


    //#region Event Handlers
    ////      --------------

    ////drawing area measured in pixels
    //int pixelsWidth;
    //int pixelsWidthHalf;
    //int pixelsHeight;
    //int pixelsHeightHalf;
    //int rawStride;
    //int[] pixels;


    //DpiScale dpiScale;
    //PixelFormat pixelFormat = PixelFormats.Bgr32;


    //private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e) {
    //  if (pixelsWidth==0) {
    //    //very first time this event is raised
    //    var xMargin = ActualWidth - e.NewSize.Width;
    //    var yMargin = ActualHeight - e.NewSize.Height;
    //    var newCanvasSize = Math.Min(e.NewSize.Height, e.NewSize.Width) * 0.9;
    //    Width = xMargin + newCanvasSize;
    //    Height = yMargin + newCanvasSize;
    //  }

    //  //use MainCanvas to detect size change. MainCanvas has the same size like MainImage, which can be empty and then
    //  //has size 0.
    //  dpiScale =  VisualTreeHelper.GetDpi(this);
    //  pixelsWidth = (int)(e.NewSize.Width*dpiScale.DpiScaleX);
    //  pixelsWidthHalf = pixelsWidth / 2;
    //  pixelsHeight = (int)(e.NewSize.Height*dpiScale.DpiScaleX);
    //  pixelsHeightHalf = pixelsHeight / 2;
    //  rawStride = (pixelsWidth * pixelFormat.BitsPerPixel + 7) / 8;
    //  pixels = new int[rawStride/4 * pixelsHeight];
    //  reDraw();
    //}


    //private void NumberOfCCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
    //  setupClusters(NumberOfCCombobox.SelectedIndex + 2);
    //}

    //private void ConfigCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
    //  if (isSetupClusters || isRemove) return;

    //  changeClusterConfiguration(ConfigCombobox.SelectedIndex);
    //}


    //private void NextButton_Click(object sender, RoutedEventArgs e) {
    //  //nothing happens when ConfigCombobox.SelectedIndex++ and SelectedIndex has reached the last available item
    //  if (ConfigCombobox.SelectedIndex==ConfigCombobox.Items.Count-1) {
    //    ConfigCombobox.SelectedIndex = 0;
    //  } else {
    //    ConfigCombobox.SelectedIndex++;
    //  }
    //}


    //private void PreviousButton_Click(object sender, RoutedEventArgs e) {
    //  if (ConfigCombobox.SelectedIndex==0) {
    //    ConfigCombobox.SelectedIndex = ConfigCombobox.Items.Count-1;
    //  } else {
    //    ConfigCombobox.SelectedIndex--;
    //  }
    //}


    //private void AddButton_Click(object sender, RoutedEventArgs e) {
    //  var coordinates = new (int X, int Y)[clustersUsedCount];
    //  var sb = new StringBuilder();
    //  for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
    //    var xCluster = clusters[clusterIndex].XCluster;
    //    coordinates[clusterIndex].X = xCluster;
    //    var yCluster = clusters[clusterIndex].YCluster;
    //    coordinates[clusterIndex].Y = yCluster;
    //    sb.Append($"{xCluster},{yCluster} ");
    //  }
    //  var description = sb.ToString();
    //  LocalClusterConfigurations[clustersUsedCount].Add((description, coordinates));
    //  ConfigCombobox.Items.Add(new ComboBoxItem(){Content = description});
    //  ConfigCombobox.SelectedIndex = ConfigCombobox.Items.Count - 1;
    //  //copyConfigToClipboard();
    //}


    //private void CopyButton_Click(object sender, RoutedEventArgs e) {
    //  copyConfigToClipboard();
    //}


    //bool isRemove;


    //private void RemoveButton_Click(object sender, RoutedEventArgs e) {
    //  var selectedIndex = ConfigCombobox.SelectedIndex;
    //  if (selectedIndex<(int)ClusteringEnum.horizontal) {
    //    MessageBox.Show($"Cannot remove {(ClusteringEnum)selectedIndex+1}.");
    //    return;
    //  }

    //  var configList = LocalClusterConfigurations[clustersUsedCount];
    //  configList.RemoveAt(ConfigCombobox.SelectedIndex);
    //  isRemove = true;
    //  ConfigCombobox.Items.RemoveAt(ConfigCombobox.SelectedIndex);
    //  isRemove = false;
    //  ConfigCombobox.SelectedIndex = selectedIndex - 1;
    //}


    //private void copyConfigToClipboard() {
    //  //config[6].Add(("6,6 8,0 10,6 0,0 2,6 4,0", new (int, int)[] {(6, 6), (8, 0), (10, 6), (0, 0), (2, 6), (4, 0)}));
    //  var descriptionSB = new StringBuilder();
    //  var coordinatesSB = new StringBuilder();
    //  var clusterSB = new StringBuilder();
    //  var configList = LocalClusterConfigurations[clustersUsedCount];
    //  for (int clusterConfigIndex=(int)ClusteringEnum.horizontal; clusterConfigIndex<configList.Count; clusterConfigIndex++) {
    //    var clusterConfig = configList[clusterConfigIndex];
    //    for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
    //      var cluster = clusterConfig.Clusters[clusterIndex];
    //      var xCluster = cluster.X;
    //      var yCluster = cluster.Y;
    //      descriptionSB.Append($"{xCluster},{yCluster} ");
    //      coordinatesSB.Append($"({xCluster}, {yCluster}), ");
    //    }
    //    var description = descriptionSB.ToString()[..^1];
    //    var coordinates = coordinatesSB.ToString()[..^2];
    //    clusterSB.AppendLine($"config[{clustersUsedCount}].Add((\"{description}\", new (int, int)[] {{{coordinates}}}));");
    //    descriptionSB.Clear();
    //    coordinatesSB.Clear();
    //  }
    //  Clipboard.SetText(clusterSB.ToString());
    //}


    //private void clusterTextBox_GotFocus(object sender, RoutedEventArgs e) {
    //  var textBox = (TextBox)sender;
    //  textBox.SelectAll();
    //}


    //private void xClusterTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e) {
    //  var textBox = (TextBox)sender;
    //  var number = getIntNumber(textBox, 2*clustersUsedCount-1);
    //  if (number<0) {
    //    //prevent keyboard focus to move away, the entered number is illegal
    //    e.Handled = true;
    //    return;
    //  }
    //  var cluster = clusters[(int)textBox.Tag];
    //  if (cluster.XCluster==number) return; //nothing has changed

    //  cluster.XCluster = number;
    //  reDraw();
    //}


    //private void yClusterTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e) {
    //  var textBox = (TextBox)sender;
    //  var number = getIntNumber(textBox, 2*clustersUsedCount-1);
    //  if (number<0) {
    //    //prevent keyboard focus to move away, the entered number is illegal
    //    e.Handled = true;
    //    return;
    //  }
    //  var cluster = clusters[(int)textBox.Tag];
    //  if (cluster.YCluster==number) return; //nothing has changed

    //  cluster.YCluster = number;
    //  reDraw();
    //}


    //private int getIntNumber(TextBox textBox, int max) {
    //  try {
    //    var number = int.Parse(textBox.Text);
    //    if (number<0) {
    //      MessageBox.Show($"'{number}' must be >=0.");
    //      return -1;
    //    }
    //    if (number>max) {
    //      MessageBox.Show($"'{number}' must be <={max}.");
    //      return -1;
    //    }
    //    return number;

    //  } catch (Exception) {
    //    MessageBox.Show($"'{textBox.Text}' is not a number.");
    //    return -1;
    //  }
    //}
    //#endregion


    //#region Cluster methods
    ////      ---------------

    //private int calcDistance(int x0, int y0, int x1, int y1) {
    //  var xDif = Math.Abs(x0 - x1);
    //  if (xDif>pixelsWidthHalf) {
    //    xDif = pixelsWidth - xDif;
    //  }
    //  var yDif = Math.Abs(y0 - y1);
    //  if (yDif>pixelsHeightHalf) {
    //    yDif = pixelsHeight - yDif;
    //  }
    //  return xDif*xDif + yDif*yDif;
    //}
    //#endregion


    //#region Drawing
    ////      -------

    //private void reDraw() {
    //  if (pixelsWidth==0) return;//wait with drawing until MainCanvas_SizeChanged has executed and pixelsWidth is defined

    //  for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
    //    var cluster = clusters[clusterIndex];
    //    cluster.XPixel = cluster.XCluster*pixelsWidth / clustersUsedCount / 2;
    //    cluster.YPixel = cluster.YCluster*pixelsHeight / clustersUsedCount / 2;
    //    cluster.PixelsCount = 0;
    //  }

    //  _ = Parallel.For(0, pixelsHeight, (y) => {
    //    var offset = y * pixelsWidth;
    //    var pixelCounts = new int[clustersUsedCount];
    //    for (int x = 0; x<pixelsWidth; x++) {
    //      var distance = int.MaxValue;
    //      var clusterIndexFound = int.MaxValue;
    //      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
    //        var cluster = clusters[clusterIndex];
    //        var newDistance = calcDistance(cluster.XPixel, cluster.YPixel, x, y);
    //        if (distance>newDistance) {
    //          distance = newDistance;
    //          clusterIndexFound = clusterIndex;
    //        }
    //      }
    //      pixels[offset++] = clusters[clusterIndexFound].Color;
    //      pixelCounts[clusterIndexFound]++;
    //    }
    //    lock (clusters) {
    //      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
    //        clusters[clusterIndex].PixelsCount += pixelCounts[clusterIndex];
    //      }
    //    }
    //  });

    //  var total = 0;
    //  for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
    //    var cluster = clusters[clusterIndex];
    //    drawCross(new IntPoint(cluster.XPixel, cluster.YPixel));
    //    total += cluster.PixelsCount;
    //  }
    //  if (total!=pixels.Length) System.Diagnostics.Debugger.Break();

    //  System.Diagnostics.Debug.WriteLine("");
    //  var deviationTotal = 0;
    //  total /= 1000;
    //  for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
    //    var cluster = clusters[clusterIndex];
    //    var textBlock = cluster.CanvasClusterTextBlock;
    //    var percentage = cluster.PixelsCount/total;
    //    textBlock.Text = $"{cluster.ID}: {cluster.XCluster},{cluster.YCluster} {percentage}";
    //    Canvas.SetLeft(textBlock, cluster.XPixel/dpiScale.DpiScaleX);
    //    Canvas.SetTop(textBlock, cluster.YPixel/dpiScale.DpiScaleY);

    //    cluster.XClusterTextBox.Text = cluster.XCluster.ToString();
    //    cluster.YClusterTextBox.Text = cluster.YCluster.ToString();
    //    cluster.PercentTextBlock.Text = percentage.ToString();
    //    var deviation = averagePercentage - percentage;
    //    deviation *= deviation;
    //    cluster.DeviationTextBlock.Text = deviation.ToString();
    //    deviationTotal += deviation;
    //  }
    //  TotalDeviationTextBlock.Text = deviationTotal.ToString();
    //  BitmapSource bitmap = BitmapSource.Create(pixelsWidth, pixelsHeight,
    //      dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchX, pixelFormat, null,
    //      pixels, rawStride);
    //  MainImage.Source = bitmap;
    //}


    //private void testBorderDrawing() {
    //  //test if drawArea() paints precisely into border
    //  drawArea(new IntPoint(0, 0), new IntPoint(100, 100));
    //  drawArea(new IntPoint(pixelsWidth-100, 0), new IntPoint(pixelsWidth, 100));
    //  drawArea(new IntPoint(0, pixelsHeight-100), new IntPoint(100, pixelsHeight));
    //  drawArea(new IntPoint(pixelsWidth-100, pixelsHeight-100), new IntPoint(pixelsWidth, pixelsHeight));
    //}


    //private void testCrossesDrawing() {
    //  int xLeft = 3;
    //  int yTop = 7;
    //  int xMid = pixelsWidth/2;
    //  int yMid = pixelsHeight/2;
    //  int xRight = pixelsWidth - 3;
    //  int yBottom = pixelsHeight - 7;
    //  IntPoint[] points = {
    //    new IntPoint(xLeft, yTop),         //top left corner
    //    new IntPoint(xLeft, yBottom),      //bottom left corner 
    //    new IntPoint(xLeft, yMid),         //middle left border
    //    new IntPoint(xRight, yTop),        //top right corner
    //    new IntPoint(xRight, yBottom),     //bottom right corner
    //    new IntPoint(xRight, yMid),        //middle right border
    //    new IntPoint(xMid, yTop),          //middle top border
    //    new IntPoint(xMid, yBottom),       //middle bottom border
    //    new IntPoint(xMid, yMid)};         //center

    //  foreach (var point in points) {
    //    drawCross(point);
    //  }
    //}


    //private void drawCross(IntPoint point) {
    //  //const int length = 40; //used for testing drawing of crosses
    //  //const int width = 12;
    //  const int length = 20;
    //  const int width = 6;
    //  drawRectangle(new IntPoint(point.X-length, point.Y-width), new IntPoint(point.X+length, point.Y+width));
    //  drawRectangle(new IntPoint(point.X-width, point.Y-length), new IntPoint(point.X+width, point.Y+length));
    //}


    ///// <summary>
    ///// Draws a rectangle which might cross a border. If it does, the original rectangle will be drawn as
    ///// 2 recttangles at the opposite sides of the border (wrap around)'
    ///// </summary>
    //private void drawRectangle(IntPoint minPoint, IntPoint maxPoint) {
    //  if (minPoint.X>maxPoint.X || minPoint.Y>maxPoint.Y) throw new ArgumentException();

    //  if (minPoint.X<0) {
    //    if (minPoint.Y<0) {
    //      //top left corner
    //      draw4Corners(maxPoint.X, maxPoint.Y, minPoint.X + pixelsWidth, minPoint.Y+pixelsHeight);
    //    } else if (maxPoint.Y>pixelsHeight) {
    //      //bottom left corner
    //      draw4Corners(maxPoint.X, maxPoint.Y-pixelsHeight, minPoint.X + pixelsWidth, minPoint.Y);
    //    } else {
    //      //left border
    //      drawArea(new IntPoint(0, minPoint.Y), maxPoint);
    //      drawArea(new IntPoint(pixelsWidth + minPoint.X, minPoint.Y), new IntPoint(pixelsWidth, maxPoint.Y));
    //    }
    //  } else if (maxPoint.X>pixelsWidth) {
    //    if (minPoint.Y<0) {
    //      //top right corner
    //      draw4Corners(maxPoint.X-pixelsWidth, maxPoint.Y, minPoint.X, minPoint.Y+pixelsHeight);
    //    } else if (maxPoint.Y>pixelsHeight) {
    //      //bottom right corner
    //      draw4Corners(maxPoint.X-pixelsWidth, maxPoint.Y-pixelsHeight, minPoint.X, minPoint.Y);
    //    } else {
    //      //right border
    //      drawArea(new IntPoint(0, minPoint.Y), new IntPoint(maxPoint.X-pixelsWidth, maxPoint.Y));
    //      drawArea(new IntPoint(minPoint.X, minPoint.Y), new IntPoint(pixelsWidth, maxPoint.Y));
    //    }
    //  } else if (minPoint.Y<0) {
    //    //top border
    //    drawArea(new IntPoint(minPoint.X, 0), new IntPoint(maxPoint.X, maxPoint.Y));
    //    drawArea(new IntPoint(minPoint.X, minPoint.Y+pixelsHeight), new IntPoint(maxPoint.X, pixelsHeight));
    //  } else if (maxPoint.Y>pixelsHeight) {
    //    //bottom border
    //    drawArea(new IntPoint(minPoint.X, 0), new IntPoint(maxPoint.X, maxPoint.Y-pixelsHeight));
    //    drawArea(new IntPoint(minPoint.X, minPoint.Y), new IntPoint(maxPoint.X, pixelsHeight));
    //  } else {
    //    //inside of map, not crossing any border
    //    drawArea(minPoint, maxPoint);
    //  }
    //}


    //private void draw4Corners(int xLeft, int yTop, int xRight, int yBottom) {
    //  drawArea(new IntPoint(0, 0), new IntPoint(xLeft, yTop));
    //  drawArea(new IntPoint(xRight, 0), new IntPoint(pixelsWidth, yTop));
    //  drawArea(new IntPoint(0, yBottom), new IntPoint(xLeft, pixelsHeight));
    //  drawArea(new IntPoint(xRight, yBottom), new IntPoint(pixelsWidth, pixelsHeight));
    //}


    ///// <summary>
    ///// Draws also a rectangle, one that does not cross any border
    ///// </summary>
    //private void drawArea(IntPoint minPoint, IntPoint maxPoint) {
    //  var lineOffset = minPoint.Y*pixelsWidth + minPoint.X;
    //  for (int y = minPoint.Y; y < maxPoint.Y; y++) {
    //    var pixelOffset = lineOffset;
    //    for (int x = minPoint.X; x < maxPoint.X; x++) {
    //      pixels[pixelOffset++] = 0;
    //    }
    //    lineOffset += pixelsWidth;
    //  }
    //}
    //#endregion
  }
}
