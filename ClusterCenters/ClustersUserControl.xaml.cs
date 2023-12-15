using MasterGrab;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace ClusterCenters {


  /// <summary>
  /// Interaction logic for ClustersUserControl.xaml
  /// </summary>
  public partial class ClustersUserControl: UserControl {


    public bool IsSquare {
      get { return isSquare; }
      set { 
        isSquare = value;
        if (isSquare) {
          var newSize = Math.Min(MainCanvas.ActualWidth, MainCanvas.ActualHeight);
          MainCanvas.Width = newSize;
          MainCanvas.Height = newSize;
        } else {
          MainCanvas.Width = double.NaN;
          MainCanvas.Height = double.NaN;
        }
      }
    }
    private bool isSquare;


    readonly MainWindow mainWindow;
    readonly TextBlock labelClusterIdTextBlock;
    readonly TextBlock labelXTextBlock;
    readonly TextBlock labelYTextBlock;
    readonly TextBlock labelPercentTextBlock;
    readonly TextBlock labelDeviationTextBlock;
    readonly TextBlock labelIncrementTextBlock;
    readonly TextBox incrementXTextBox;
    readonly TextBox incrementYTextBox;
    readonly Button incrementApplyButton;
    readonly Button incrementStartButton;
    readonly Button incrementNextButton;
    readonly Button incrementPreviousButton;


    readonly Cluster[] clusters = new Cluster[MainWindow.MaxNumberOfClusters];


    public ClustersUserControl(MainWindow mainWindow) {
      InitializeComponent();
      this.mainWindow = mainWindow;
      labelClusterIdTextBlock = new TextBlock { Text="Cluster Id:", FontWeight=FontWeights.Bold };
      Grid.SetRow(labelClusterIdTextBlock, 0);
      labelXTextBlock = new TextBlock { Text="X:", FontWeight=FontWeights.Bold };
      Grid.SetRow(labelXTextBlock, 1);
      labelYTextBlock = new TextBlock { Text="Y:", FontWeight=FontWeights.Bold };
      Grid.SetRow(labelYTextBlock, 2);
      labelPercentTextBlock = new TextBlock { Text="Percent:", FontWeight=FontWeights.Bold };
      Grid.SetRow(labelPercentTextBlock, 3);
      labelDeviationTextBlock = new TextBlock { Text="Deviation:", FontWeight=FontWeights.Bold };
      Grid.SetRow(labelDeviationTextBlock, 4);
      labelIncrementTextBlock = new TextBlock { Text="Incr.", FontWeight=FontWeights.Bold };
      Grid.SetRow(labelIncrementTextBlock, 0);
      incrementXTextBox = new TextBox ();
      incrementXTextBox.GotFocus += clusterTextBox_GotFocus;
      Grid.SetRow(incrementXTextBox, 1);
      incrementYTextBox = new TextBox();
      incrementYTextBox.GotFocus += clusterTextBox_GotFocus;
      Grid.SetRow(incrementYTextBox, 2);

      incrementApplyButton = new Button {
        Content = "_Apply",
        VerticalAlignment = VerticalAlignment.Center,
        ToolTip = "Calculates new X and Y values using increments."
      };
      Grid.SetRow(incrementApplyButton, 3);
      incrementApplyButton.Click += incrementApplyButton_Click;

      incrementStartButton = new Button {
        Content = "_Start",
        VerticalAlignment = VerticalAlignment.Center,
        ToolTip = "Set increments X and Y to start values."
      };
      Grid.SetRow(incrementStartButton, 3);
      incrementStartButton.Click += incrementStartButton_Click;

      incrementNextButton = new Button {
        Content = "_Next",
        VerticalAlignment = VerticalAlignment.Center,
        ToolTip = "Increase increments by 1"
      };
      Grid.SetRow(incrementNextButton, 1);
      incrementNextButton.Click += incrementNextButton_Click;

      incrementPreviousButton = new Button {
        Content = "_Prev",
        VerticalAlignment = VerticalAlignment.Center,
        ToolTip = "Decrease increments by 1"
      };
      Grid.SetRow(incrementPreviousButton, 2);
      incrementPreviousButton.Click += incrementPreviousButton_Click;
      KeyUp += ClustersUserControl_KeyUp;

      var gridColumn = 2;
      for (int clusterIndex = 0; clusterIndex<MainWindow.MaxNumberOfClusters; clusterIndex++) {

        //TextBlock within Canvas
        var canvasClusterTextBlock = new TextBlock { FontSize=22, Background=mainWindow.TextBlockBackground, Tag = clusterIndex };
        //canvasClusterTextBlock.PreviewMouseDown += CanvasClusterTextBlock_MouseDown;
        //canvasClusterTextBlock.MouseMove += CanvasClusterTextBlock_MouseMove;
        //canvasClusterTextBlock.MouseUp += CanvasClusterTextBlock_MouseUp;

        //ClusterGrid
        //row 0: Cluster Id
        var clusterIdTextBlock = new TextBlock { Text = clusterIndex.ToString(), FontWeight = FontWeights.Bold };
        Grid.SetRow(clusterIdTextBlock, 0);
        Grid.SetColumn(clusterIdTextBlock, gridColumn);

        //row 1: xCluster coordinate
        var xClusterTextBox = new TextBox { Tag = clusterIndex };
        xClusterTextBox.GotFocus += clusterTextBox_GotFocus;
        xClusterTextBox.PreviewLostKeyboardFocus += xClusterTextBox_PreviewLostKeyboardFocus;
        Grid.SetRow(xClusterTextBox, 1);
        Grid.SetColumn(xClusterTextBox, gridColumn);

        //row 2: yCluster coordinate
        var yClusterTextBox = new TextBox { Tag = clusterIndex };
        yClusterTextBox.GotFocus += clusterTextBox_GotFocus;
        yClusterTextBox.PreviewLostKeyboardFocus += yClusterTextBox_PreviewLostKeyboardFocus;
        Grid.SetRow(yClusterTextBox, 2);
        Grid.SetColumn(yClusterTextBox, gridColumn);

        //row 3: Percentage of window space used by cluster 
        var percentTextBlock = new TextBlock { };
        Grid.SetRow(percentTextBlock, 3);
        Grid.SetColumn(percentTextBlock, gridColumn);

        //row 4: Deviation from equally sized cluster space: (actualSpace-idealSpace) * (actualSpace-idealSpace)
        var deviationTextBlock = new TextBlock { };
        Grid.SetRow(deviationTextBlock, 4);
        Grid.SetColumn(deviationTextBlock, gridColumn);

        clusters[clusterIndex] = new Cluster(
          clusterIndex,
          mainWindow.ClusterColours[clusterIndex],
          canvasClusterTextBlock,
          clusterIdTextBlock,
          xClusterTextBox,
          yClusterTextBox,
          percentTextBlock,
          deviationTextBlock);
        gridColumn += 2;//leave empty gap between 2 columns
      }

      for (int numberOfClusters = 2; numberOfClusters<17; numberOfClusters++) {
        NumberOfComboBox.Items.Add(new ComboBoxItem() { Content = numberOfClusters.ToString() });
      }
      NumberOfComboBox.SelectionChanged += NumberOfComboBox_SelectionChanged;
      ConfigCombobox.SelectionChanged += ConfigComboBox_SelectionChanged;
      NumberOfComboBox.SelectedIndex = 6 - 2;

      MainCanvas.SizeChanged += MainCanvas_SizeChanged;
      NextButton.Click += NextButton_Click;
      PreviousButton.Click += PreviousButton_Click;
      AddButton.Click += AddButton_Click;
      CopyButton.Click += CopyButton_Click;
      RemoveButton.Click += RemoveButton_Click;
    }


    int clustersUsedCount;
    bool isSetupClusters;
    int averagePercentage;


    private void setupClusters(int numberOfClusters) {
      if (clustersUsedCount==numberOfClusters) return;

      if (numberOfClusters<2) throw new ArgumentException("");
      if (numberOfClusters>MainWindow.MaxNumberOfClusters) throw new ArgumentException("");

      clustersUsedCount = numberOfClusters;

      isSetupClusters = true;
      ConfigCombobox.Items.Clear();
      var configList = mainWindow.ExtendedClusterConfigurations[clustersUsedCount];
      for (int configurationsIndex = 0; configurationsIndex<configList.Count; configurationsIndex++) {
        ConfigCombobox.Items.Add(new ComboBoxItem() { Content = configList[configurationsIndex].Description });
      }
      isSetupClusters = false;
      ConfigCombobox.SelectedIndex = 0;

      MainCanvas.Children.Clear();
      ClusterGrid.Children.Clear();

      ClusterGrid.ColumnDefinitions.Clear();
      ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
      ClusterGrid.Children.Add(labelClusterIdTextBlock);
      ClusterGrid.Children.Add(labelXTextBlock);
      ClusterGrid.Children.Add(labelYTextBlock);
      ClusterGrid.Children.Add(labelPercentTextBlock);
      ClusterGrid.Children.Add(labelDeviationTextBlock);

      var clusterIndex = 0;
      for (; clusterIndex<clustersUsedCount; clusterIndex++) {
        var cluster = clusters[clusterIndex];
        MainCanvas.Children.Add(cluster.CanvasClusterTextBlock);

        ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
        ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        if (clusterIndex%2==1) {
          var rectangle = new System.Windows.Shapes.Rectangle { Fill=Brushes.DarkGray };
          Grid.SetColumn(rectangle, 2 + clusterIndex*2);
          Grid.SetRowSpan(rectangle, 4);
          ClusterGrid.Children.Add(rectangle);
        }

        ClusterGrid.Children.Add(cluster.ClusterIdTextBlock);
        ClusterGrid.Children.Add(cluster.XClusterTextBox);
        ClusterGrid.Children.Add(cluster.YClusterTextBox);
        ClusterGrid.Children.Add(cluster.PercentTextBlock);
        ClusterGrid.Children.Add(cluster.DeviationTextBlock);
      }

      var columnIndex = 2 + clusterIndex*2;
      ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
      ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
      Grid.SetColumn(labelIncrementTextBlock, columnIndex);
      ClusterGrid.Children.Add(labelIncrementTextBlock);
      Grid.SetColumn(incrementXTextBox, columnIndex);
      ClusterGrid.Children.Add(incrementXTextBox);
      Grid.SetColumn(incrementYTextBox, columnIndex);
      ClusterGrid.Children.Add(incrementYTextBox);
      Grid.SetColumn(incrementApplyButton, columnIndex);
      ClusterGrid.Children.Add(incrementApplyButton);

      columnIndex +=2;
      ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
      ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
      Grid.SetColumn(incrementNextButton, columnIndex);
      ClusterGrid.Children.Add(incrementNextButton);
      Grid.SetColumn(incrementPreviousButton, columnIndex);
      ClusterGrid.Children.Add(incrementPreviousButton);
      Grid.SetColumn(incrementStartButton, columnIndex);
      ClusterGrid.Children.Add(incrementStartButton);

      averagePercentage = (1000 / clustersUsedCount);
      AverageTextBlock.Text = averagePercentage.ToString();
    }


    private void changeClusterConfiguration(int configurationIndex) {
      var (_, Clusters)= mainWindow.ExtendedClusterConfigurations[clustersUsedCount][configurationIndex];
      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
        clusters[clusterIndex].XCluster = Clusters[clusterIndex].X;
        clusters[clusterIndex].YCluster = Clusters[clusterIndex].Y;
      }
      incrementXTextBox.Text = null;
      incrementYTextBox.Text = null;
      reDraw();
    }


    #region Event Handlers
    //      --------------

    //drawing area measured in pixels
    int pixelsWidth;
    int pixelsWidthHalf;
    int pixelsHeight;
    int pixelsHeightHalf;
    int rawStride;
    int[] pixels;


    DpiScale dpiScale;
    PixelFormat pixelFormat = PixelFormats.Bgr32;


    private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e) {
      //if (pixelsWidth==0) {
      //  //very first time this event is raised
      //  var xMargin = ActualWidth - e.NewSize.Width;
      //  var yMargin = ActualHeight - e.NewSize.Height;
      //  var newCanvasSize = Math.Min(e.NewSize.Height, e.NewSize.Width) * 0.9;
      //  //Width = xMargin + newCanvasSize;
      //  //Height = yMargin + newCanvasSize;
      //}

      //use MainCanvas to detect size change. MainCanvas has the same size like MainImage, which can be empty and then
      //has size 0.
      dpiScale =  VisualTreeHelper.GetDpi(this);
      pixelsWidth = (int)(e.NewSize.Width*dpiScale.DpiScaleX);
      pixelsWidthHalf = pixelsWidth / 2;
      pixelsHeight = (int)(e.NewSize.Height*dpiScale.DpiScaleX);
      pixelsHeightHalf = pixelsHeight / 2;
      rawStride = (pixelsWidth * pixelFormat.BitsPerPixel + 7) / 8;
      pixels = new int[rawStride/4 * pixelsHeight];
      reDraw();
    }


    private void NumberOfComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      setupClusters(NumberOfComboBox.SelectedIndex + 2);
    }

    private void ConfigComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (isSetupClusters || isRemove) return;

      changeClusterConfiguration(ConfigCombobox.SelectedIndex);
    }


    private void NextButton_Click(object sender, RoutedEventArgs e) {
      //nothing happens when ConfigComboBox.SelectedIndex++ and SelectedIndex has reached the last available item
      if (ConfigCombobox.SelectedIndex==ConfigCombobox.Items.Count-1) {
        ConfigCombobox.SelectedIndex = 0;
      } else {
        ConfigCombobox.SelectedIndex++;
      }
    }


    private void PreviousButton_Click(object sender, RoutedEventArgs e) {
      if (ConfigCombobox.SelectedIndex==0) {
        ConfigCombobox.SelectedIndex = ConfigCombobox.Items.Count-1;
      } else {
        ConfigCombobox.SelectedIndex--;
      }
    }


    private void AddButton_Click(object sender, RoutedEventArgs e) {
      var coordinates = new (int X, int Y)[clustersUsedCount];
      var sb = new StringBuilder();
      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
        var xCluster = clusters[clusterIndex].XCluster;
        coordinates[clusterIndex].X = xCluster;
        var yCluster = clusters[clusterIndex].YCluster;
        coordinates[clusterIndex].Y = yCluster;
        sb.Append($"{xCluster},{yCluster} ");
      }
      var description = sb.ToString();
      mainWindow.ExtendedClusterConfigurations[clustersUsedCount].Add((description, coordinates));
      ConfigCombobox.Items.Add(new ComboBoxItem() { Content = description });
      ConfigCombobox.SelectedIndex = ConfigCombobox.Items.Count - 1;
      //copyConfigToClipboard();
    }


    private void CopyButton_Click(object sender, RoutedEventArgs e) {
      copyConfigToClipboard();
    }


    bool isRemove;


    private void RemoveButton_Click(object sender, RoutedEventArgs e) {
      var selectedIndex = ConfigCombobox.SelectedIndex;
      if (selectedIndex<(int)ClusteringEnum.horizontal) {
        MessageBox.Show($"Cannot remove {(ClusteringEnum)selectedIndex+1}.");
        return;
      }

      var configList = mainWindow.ExtendedClusterConfigurations[clustersUsedCount];
      configList.RemoveAt(ConfigCombobox.SelectedIndex);
      isRemove = true;
      ConfigCombobox.Items.RemoveAt(ConfigCombobox.SelectedIndex);
      isRemove = false;
      ConfigCombobox.SelectedIndex = selectedIndex - 1;
    }


    private void copyConfigToClipboard() {
      //config[6].Add(("6,6 8,0 10,6 0,0 2,6 4,0", new (int, int)[] {(6, 6), (8, 0), (10, 6), (0, 0), (2, 6), (4, 0)}));
      var descriptionSB = new StringBuilder();
      var coordinatesSB = new StringBuilder();
      var clusterSB = new StringBuilder();
      var configList = mainWindow.ExtendedClusterConfigurations[clustersUsedCount];
      for (int clusterConfigIndex = (int)ClusteringEnum.horizontal; clusterConfigIndex<configList.Count; clusterConfigIndex++) {
        var (_, Clusters)= configList[clusterConfigIndex];
        for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
          var (xCluster, yCluster) = Clusters[clusterIndex];
          descriptionSB.Append($"{xCluster},{yCluster} ");
          coordinatesSB.Append($"({xCluster}, {yCluster}), ");
        }
        var description = descriptionSB.ToString()[..^1];
        var coordinates = coordinatesSB.ToString()[..^2];
        clusterSB.AppendLine($"config[{clustersUsedCount}].Add((\"{description}\", new (int, int)[] {{{coordinates}}}));");
        descriptionSB.Clear();
        coordinatesSB.Clear();
      }
      Clipboard.SetText(clusterSB.ToString());
    }


    private void clusterTextBox_GotFocus(object sender, RoutedEventArgs e) {
      var textBox = (TextBox)sender;
      textBox.SelectAll();
    }


    private void xClusterTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e) {
      if (isApplyIncrements) return;

      var textBox = (TextBox)sender;
      var number = getIntNumber(textBox, 2*clustersUsedCount-1);
      if (number<0) {
        //prevent keyboard focus to move away, the entered number is illegal
        e.Handled = true;
        return;
      }
      var cluster = clusters[(int)textBox.Tag];
      if (cluster.XCluster==number) return; //nothing has changed

      cluster.XCluster = number;
      reDraw();
    }


    private void yClusterTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e) {
      if (isApplyIncrements) return;

      var textBox = (TextBox)sender;
      var number = getIntNumber(textBox, 2*clustersUsedCount-1);
      if (number<0) {
        //prevent keyboard focus to move away, the entered number is illegal
        e.Handled = true;
        return;
      }
      var cluster = clusters[(int)textBox.Tag];
      if (cluster.YCluster==number) return; //nothing has changed

      cluster.YCluster = number;
      reDraw();
    }


    private static int getIntNumber(TextBox textBox, int max) {
      try {
        var number = int.Parse(textBox.Text);
        if (number<0) {
          MessageBox.Show($"'{number}' must be >=0.");
          return -1;
        }
        if (number>max) {
          MessageBox.Show($"'{number}' must be <={max}.");
          return -1;
        }
        return number;

      } catch (Exception) {
        MessageBox.Show($"'{textBox.Text}' is not a number.");
        return -1;
      }
    }


    private void incrementApplyButton_Click(object sender, RoutedEventArgs e) {
      applyIncrements(int.Parse(incrementXTextBox.Text), int.Parse(incrementYTextBox.Text));
    }


    private void incrementStartButton_Click(object sender, RoutedEventArgs e) {
      applyIncrements(2, 2);
    }


    private void incrementNextButton_Click(object sender, RoutedEventArgs e) {
      incrementNext();
    }


    private void incrementNext() {
      var incrementX = int.Parse(incrementXTextBox.Text);
      var incrementY = int.Parse(incrementYTextBox.Text) + 1;
      if (incrementY>clustersUsedCount) {
        incrementX++;
        if (incrementX>clustersUsedCount) {
          incrementX = 2;
        }
        incrementY = incrementX;
      }
      applyIncrements(incrementX, incrementY);
    }


    private void incrementPreviousButton_Click(object sender, RoutedEventArgs e) {
      incrementPrevious();
    }


    private void incrementPrevious() {
      var incrementX = int.Parse(incrementXTextBox.Text);
      var incrementY = int.Parse(incrementYTextBox.Text) - 1;
      if (incrementY<incrementX) {
        incrementX--;
        if (incrementX<2) {
          incrementX = clustersUsedCount;
        }
        incrementY = clustersUsedCount;
      }
      applyIncrements(incrementX, incrementY);
    }


    private void ClustersUserControl_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key == Key.PageUp) {
        incrementNext();
      } else if (e.Key == Key.PageDown) {
        incrementPrevious();
      }
    }


    bool isApplyIncrements;


    private void applyIncrements(int incrementX, int incrementY) {
      isApplyIncrements = true;
      var cluster = clusters[0];
      var xCluster = cluster.XCluster = clustersUsedCount;
      cluster.XClusterTextBox.Text = xCluster.ToString();
      var yCluster = cluster.YCluster = clustersUsedCount;
      cluster.YClusterTextBox.Text = yCluster.ToString();
      var maxXY = clustersUsedCount * 2;
      var xIncrement = (incrementX + maxXY) % maxXY;
      incrementXTextBox.Text = xIncrement.ToString();
      var yIncrement = (incrementY + maxXY) % maxXY;
      incrementYTextBox.Text = yIncrement.ToString();
      for (int clusterIndex = 1; clusterIndex < clustersUsedCount; clusterIndex++) {
        cluster = clusters[clusterIndex];
        xCluster = (xCluster + xIncrement) % maxXY;
        cluster.XCluster = xCluster;
        cluster.XClusterTextBox.Text = xCluster.ToString();
        yCluster = (yCluster + yIncrement) % maxXY;
        cluster.YCluster = yCluster;
        cluster.YClusterTextBox.Text = yCluster.ToString();
      }
      isApplyIncrements = false;

      reDraw();
    }
    #endregion


    #region Cluster methods
    //      ---------------

    private int calcDistance(int x0, int y0, int x1, int y1) {
      var xDif = Math.Abs(x0 - x1);
      if (xDif>pixelsWidthHalf) {
        xDif = pixelsWidth - xDif;
      }
      var yDif = Math.Abs(y0 - y1);
      if (yDif>pixelsHeightHalf) {
        yDif = pixelsHeight - yDif;
      }
      return xDif*xDif + yDif*yDif;
    }
    #endregion


    #region Drawing
    //      -------

    private void reDraw() {
      if (pixelsWidth==0) return;//wait with drawing until MainCanvas_SizeChanged has executed and pixelsWidth is defined

      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
        var cluster = clusters[clusterIndex];
        cluster.XPixel = cluster.XCluster*pixelsWidth / clustersUsedCount / 2;
        cluster.YPixel = cluster.YCluster*pixelsHeight / clustersUsedCount / 2;
        cluster.PixelsCount = 0;
      }

      _ = Parallel.For(0, pixelsHeight, (y) => {
        var offset = y * pixelsWidth;
        var pixelCounts = new int[clustersUsedCount];
        for (int x = 0; x<pixelsWidth; x++) {
          var distance = int.MaxValue;
          var clusterIndexFound = int.MaxValue;
          for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
            var cluster = clusters[clusterIndex];
            var newDistance = calcDistance(cluster.XPixel, cluster.YPixel, x, y);
            if (distance>newDistance) {
              distance = newDistance;
              clusterIndexFound = clusterIndex;
            }
          }
          pixels[offset++] = clusters[clusterIndexFound].Color;
          pixelCounts[clusterIndexFound]++;
        }
        lock (clusters) {
          for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
            clusters[clusterIndex].PixelsCount += pixelCounts[clusterIndex];
          }
        }
      });

      var total = 0;
      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
        var cluster = clusters[clusterIndex];
        drawCross(new IntPoint(cluster.XPixel, cluster.YPixel));
        total += cluster.PixelsCount;
      }
      if (total!=pixels.Length) System.Diagnostics.Debugger.Break();

      var deviationTotal = 0;
      total /= 1000;
      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
        var cluster = clusters[clusterIndex];
        var textBlock = cluster.CanvasClusterTextBlock;
        var percentage = cluster.PixelsCount/total;
        textBlock.Text = $"{cluster.ID}: {cluster.XCluster},{cluster.YCluster} {percentage}";
        Canvas.SetLeft(textBlock, cluster.XPixel/dpiScale.DpiScaleX);
        Canvas.SetTop(textBlock, cluster.YPixel/dpiScale.DpiScaleY);

        cluster.XClusterTextBox.Text = cluster.XCluster.ToString();
        cluster.YClusterTextBox.Text = cluster.YCluster.ToString();
        cluster.PercentTextBlock.Text = percentage.ToString();
        var deviation = averagePercentage - percentage;
        deviation *= deviation;
        cluster.DeviationTextBlock.Text = deviation.ToString();
        deviationTotal += deviation;
      }
      TotalDeviationTextBlock.Text = deviationTotal.ToString();
      BitmapSource bitmap = BitmapSource.Create(pixelsWidth, pixelsHeight,
          dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchX, pixelFormat, null,
          pixels, rawStride);
      MainImage.Source = bitmap;
    }


    private void testBorderDrawing() {
      //test if drawArea() paints precisely into border
      drawArea(new IntPoint(0, 0), new IntPoint(100, 100));
      drawArea(new IntPoint(pixelsWidth-100, 0), new IntPoint(pixelsWidth, 100));
      drawArea(new IntPoint(0, pixelsHeight-100), new IntPoint(100, pixelsHeight));
      drawArea(new IntPoint(pixelsWidth-100, pixelsHeight-100), new IntPoint(pixelsWidth, pixelsHeight));
    }


    private void testCrossesDrawing() {
      int xLeft = 3;
      int yTop = 7;
      int xMid = pixelsWidth/2;
      int yMid = pixelsHeight/2;
      int xRight = pixelsWidth - 3;
      int yBottom = pixelsHeight - 7;
      IntPoint[] points = {
        new(xLeft, yTop),         //top left corner
        new (xLeft, yBottom),      //bottom left corner 
        new (xLeft, yMid),         //middle left border
        new (xRight, yTop),        //top right corner
        new (xRight, yBottom),     //bottom right corner
        new (xRight, yMid),        //middle right border
        new (xMid, yTop),          //middle top border
        new (xMid, yBottom),       //middle bottom border
        new (xMid, yMid)};         //center

      foreach (var point in points) {
        drawCross(point);
      }
    }


    private void drawCross(IntPoint point) {
      //const int length = 40; //used for testing drawing of crosses
      //const int width = 12;
      const int length = 20;
      const int width = 6;
      drawRectangle(new IntPoint(point.X-length, point.Y-width), new IntPoint(point.X+length, point.Y+width));
      drawRectangle(new IntPoint(point.X-width, point.Y-length), new IntPoint(point.X+width, point.Y+length));
    }


    /// <summary>
    /// Draws a rectangle which might cross a border. If it does, the original rectangle will be drawn as
    /// 2 rectangles at the opposite sides of the border (wrap around)'
    /// </summary>
    private void drawRectangle(IntPoint minPoint, IntPoint maxPoint) {
      if (minPoint.X>maxPoint.X) throw new ArgumentException($"minPoint.X {minPoint.X} is greater than maxPoint.X {maxPoint.X}.");
      if (minPoint.Y>maxPoint.Y) throw new ArgumentException($"minPoint.Y {minPoint.Y} is greater than maxPoint.Y {maxPoint.Y}.");

      if (minPoint.X<0) {
        if (minPoint.Y<0) {
          //top left corner
          draw4Corners(maxPoint.X, maxPoint.Y, minPoint.X + pixelsWidth, minPoint.Y+pixelsHeight);
        } else if (maxPoint.Y>pixelsHeight) {
          //bottom left corner
          draw4Corners(maxPoint.X, maxPoint.Y-pixelsHeight, minPoint.X + pixelsWidth, minPoint.Y);
        } else {
          //left border
          drawArea(new IntPoint(0, minPoint.Y), maxPoint);
          drawArea(new IntPoint(pixelsWidth + minPoint.X, minPoint.Y), new IntPoint(pixelsWidth, maxPoint.Y));
        }
      } else if (maxPoint.X>pixelsWidth) {
        if (minPoint.Y<0) {
          //top right corner
          draw4Corners(maxPoint.X-pixelsWidth, maxPoint.Y, minPoint.X, minPoint.Y+pixelsHeight);
        } else if (maxPoint.Y>pixelsHeight) {
          //bottom right corner
          draw4Corners(maxPoint.X-pixelsWidth, maxPoint.Y-pixelsHeight, minPoint.X, minPoint.Y);
        } else {
          //right border
          drawArea(new IntPoint(0, minPoint.Y), new IntPoint(maxPoint.X-pixelsWidth, maxPoint.Y));
          drawArea(new IntPoint(minPoint.X, minPoint.Y), new IntPoint(pixelsWidth, maxPoint.Y));
        }
      } else if (minPoint.Y<0) {
        //top border
        drawArea(new IntPoint(minPoint.X, 0), new IntPoint(maxPoint.X, maxPoint.Y));
        drawArea(new IntPoint(minPoint.X, minPoint.Y+pixelsHeight), new IntPoint(maxPoint.X, pixelsHeight));
      } else if (maxPoint.Y>pixelsHeight) {
        //bottom border
        drawArea(new IntPoint(minPoint.X, 0), new IntPoint(maxPoint.X, maxPoint.Y-pixelsHeight));
        drawArea(new IntPoint(minPoint.X, minPoint.Y), new IntPoint(maxPoint.X, pixelsHeight));
      } else {
        //inside of map, not crossing any border
        drawArea(minPoint, maxPoint);
      }
    }


    private void draw4Corners(int xLeft, int yTop, int xRight, int yBottom) {
      drawArea(new IntPoint(0, 0), new IntPoint(xLeft, yTop));
      drawArea(new IntPoint(xRight, 0), new IntPoint(pixelsWidth, yTop));
      drawArea(new IntPoint(0, yBottom), new IntPoint(xLeft, pixelsHeight));
      drawArea(new IntPoint(xRight, yBottom), new IntPoint(pixelsWidth, pixelsHeight));
    }


    /// <summary>
    /// Draws also a rectangle, one that does not cross any border
    /// </summary>
    private void drawArea(IntPoint minPoint, IntPoint maxPoint) {
      var lineOffset = minPoint.Y*pixelsWidth + minPoint.X;
      for (int y = minPoint.Y; y < maxPoint.Y; y++) {
        var pixelOffset = lineOffset;
        for (int x = minPoint.X; x < maxPoint.X; x++) {
          pixels[pixelOffset++] = 0;
        }
        lineOffset += pixelsWidth;
      }
    }
    #endregion

  }
}
