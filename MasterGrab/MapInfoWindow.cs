/********************************************************************************************************

MasterGrab.MasterGrab.MapInfoWindow
===================================

Can be opened as small "window" over the game map displaying some addition information

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MasterGrab {


  /// <summary>
  /// MapInfoWindow: a "Window" consisting of a border with content. It gets displayed in the opposite corner of the mouse position
  /// </summary>
  class MapInfoWindow: CustomControlBase {

    #region Properties
    //      ----------

    /// <summary>
    /// Does the info window display ranking, tracing or no information ?
    /// </summary>
    public InfoWindowModeEnum InfoWindowMode {
      get => infoWindowMode;
      set {
        infoWindowMode = value;
        switchInfoWindow();
      }
    }
    InfoWindowModeEnum infoWindowMode = InfoWindowModeEnum.none;
    #endregion


    #region Constructor
    //      -----------

    readonly MapControl mapControl;


    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Many fields get only filled when MapChanged() gets called
    public MapInfoWindow(MapControl mapControl) {
    #pragma warning restore CS8618
      this.mapControl = mapControl;
    }
    #endregion


    #region Methods
    //      -------

    readonly Brush gridLineBrush = new SolidColorBrush(Color.FromRgb(0xE8, 0xE8, 0xFF));
    readonly Brush gridColumnBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x00, 0x00, 0x00));

    bool isFirstTime = true;
    Border border;
    Grid rankingGrid;
    TextBlock[,] rankingTextBlocks;
    Grid traceGrid;
    TextBlock[,] traceTextBlocks;

    const int rankingPlayersIndex = 0;
    const int rankingRankIndex = rankingPlayersIndex + 1;
    const int rankingSizeIndex = rankingRankIndex  + 1;
    const int rankingSizePercentIndex = rankingSizeIndex  + 1;
    const int rankingCCountIndex = rankingSizePercentIndex + 1; //CountriesCount
    const int rankingCPercentIndex = rankingCCountIndex + 1; //CountriesPercent
    const int rankingArmiesIndex = rankingCPercentIndex + 1;
    const int rankingColumnCount = rankingArmiesIndex + 1;

    const int tracePlayerIndex = 0;
    const int traceDefenderIndex = tracePlayerIndex + 1;
    const int traceActionIndex = traceDefenderIndex + 1;
    const int traceToIndex = traceActionIndex + 1;
    const int traceFromIndex = traceToIndex + 1;
    const int traceColumnCount = traceFromIndex + 1;


    internal void MapChanged() {
      if (isFirstTime) {
        isFirstTime = false;
        MouseMove += MapInfoWindow_MouseMove;
      }

      //Info window
      if (border==null) {
        border = new Border();
        AddChild(border);
        border.Background = Brushes.White;
        border.BorderBrush = Brushes.Gray;
        border.BorderThickness = new Thickness(5);
        border.CornerRadius = new CornerRadius(10);
        //border.Padding = new Thickness(3);
        border.HorizontalAlignment = HorizontalAlignment.Left;
        border.VerticalAlignment = VerticalAlignment.Top;
        border.Visibility = Visibility.Collapsed;
      }

      //ranking
      //just overwrite rankingGrid, because number of players might have changed on new map 
      rankingGrid = new Grid();
      for (var rowIndex = 0; rowIndex < mapControl.Game.Players.Count+1; rowIndex++) {
        var rowDefinition = new RowDefinition {Height = GridLength.Auto};
        rankingGrid.RowDefinitions.Add(rowDefinition);
        if (rowIndex % 2 == 1) {
          var rectangle = new Rectangle();
          rankingGrid.Children.Add(rectangle);
          Grid.SetRow(rectangle, rowIndex);
          Grid.SetColumn(rectangle, 1);
          Grid.SetColumnSpan(rectangle, rankingColumnCount-1);
          rectangle.Fill = gridLineBrush;
        }
      }
      for (var columnIndex = 0; columnIndex < rankingColumnCount; columnIndex++) {
        var columnDefinition = new ColumnDefinition {Width = GridLength.Auto};
        rankingGrid.ColumnDefinitions.Add(columnDefinition);
        if (columnIndex is rankingRankIndex or rankingCCountIndex or rankingCPercentIndex) {
          var rectangle = new Rectangle();
          rankingGrid.Children.Add(rectangle);
          Grid.SetRow(rectangle, 0);
          Grid.SetColumn(rectangle, columnIndex);
          Grid.SetRowSpan(rectangle, mapControl.Game.Players.Count + 1);
          rectangle.Fill = gridColumnBrush;
        }
      }
      rankingTextBlocks = new TextBlock[mapControl.Game.Players.Count+1, rankingGrid.ColumnDefinitions.Count];
      for (var rowIndex = 0; rowIndex < mapControl.Game.Players.Count+1; rowIndex++) {
        for (var columnIndex = 0; columnIndex < rankingGrid.ColumnDefinitions.Count; columnIndex++) {
          var textBlock = new TextBlock {Margin = new Thickness(2, 1, 2, 1)};
          if (columnIndex == 0 || rowIndex == 0) {
            textBlock.FontWeight = FontWeights.Bold;
          }
          if (rowIndex>0 && columnIndex>0) {
            textBlock.TextAlignment = TextAlignment.Right;
          }
          rankingGrid.Children.Add(textBlock);
          Grid.SetRow(textBlock, rowIndex);
          Grid.SetColumn(textBlock, columnIndex);
          rankingTextBlocks[rowIndex, columnIndex] = textBlock;
          //textBlock.Text = rowIndex + ", " + columnIndex;
        }
      }
      rankingTextBlocks[0, rankingPlayersIndex].Text = "Rank";
      rankingTextBlocks[0, rankingSizeIndex].Text = "Size";
      Grid.SetColumnSpan(rankingTextBlocks[0, rankingSizeIndex], 2);
      rankingTextBlocks[0, rankingCCountIndex].Text = "Countries";
      Grid.SetColumnSpan(rankingTextBlocks[0, rankingCCountIndex], 2);
      rankingTextBlocks[0, rankingCPercentIndex].Text = "";
      rankingTextBlocks[0, rankingArmiesIndex].Text = "Armies";

      //trace
      traceGrid = new Grid();
      for (var rowIndex = 0; rowIndex < mapControl.Game.Players.Count+1; rowIndex++) {
        var rowDefinition = new RowDefinition {Height = GridLength.Auto};
        traceGrid.RowDefinitions.Add(rowDefinition);
        if (rowIndex % 2 == 1) {
          var rectangle = new Rectangle();
          traceGrid.Children.Add(rectangle);
          Grid.SetRow(rectangle, rowIndex);
          Grid.SetColumn(rectangle, 2);
          Grid.SetColumnSpan(rectangle, traceColumnCount-2);
          rectangle.Fill = gridLineBrush;
        }
      }
      for (var columnIndex = 0; columnIndex < traceColumnCount; columnIndex++) {
        var columnDefinition = new ColumnDefinition {Width = GridLength.Auto};
        traceGrid.ColumnDefinitions.Add(columnDefinition);
        if (columnIndex>1 && columnIndex % 2 == 1) {
          var rectangle = new Rectangle();
          traceGrid.Children.Add(rectangle);
          Grid.SetRow(rectangle, 0);
          Grid.SetColumn(rectangle, columnIndex);
          Grid.SetRowSpan(rectangle, mapControl.Game.Players.Count + 1);
          rectangle.Fill = gridColumnBrush;
        }
      }
      traceTextBlocks = new TextBlock[mapControl.Game.Players.Count+1, traceGrid.ColumnDefinitions.Count];
      for (var rowIndex = 0; rowIndex < mapControl.Game.Players.Count+1; rowIndex++) {
        for (var columnIndex = 0; columnIndex < traceGrid.ColumnDefinitions.Count; columnIndex++) {
          var textBlock = new TextBlock {Margin = new Thickness(2, 1, 2, 1)};
          if (columnIndex == 0 || rowIndex == 0) {
            textBlock.FontWeight = FontWeights.Bold;
          }
          traceGrid.Children.Add(textBlock);
          Grid.SetRow(textBlock, rowIndex);
          Grid.SetColumn(textBlock, columnIndex);
          traceTextBlocks[rowIndex, columnIndex] = textBlock;
          textBlock.Text = rowIndex + ", " + columnIndex;
        }
      }
      traceTextBlocks[0, tracePlayerIndex].Text = "Player";
      traceTextBlocks[0, traceDefenderIndex].Text = "Defender";
      traceTextBlocks[0, traceActionIndex].Text = "Action";
      traceTextBlocks[0, traceToIndex].Text = "To";
      traceTextBlocks[0, traceFromIndex].Text = "From";

      switchInfoWindow();

      InvalidateMeasure();
      InvalidateVisual();
    }


    internal void GameChanged() {
      updateInfoWindow();

      InvalidateMeasure();
      InvalidateVisual();
    }
    #endregion


    #region Eventhandlers
    //      -------------

    bool rankingPositionLeft = true;
    bool rankingPositionTop = true;


    private void MapInfoWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
      var mousePoint = e.GetPosition(this);
      if (!mapControl.PixelMap.IsWithin((int)mousePoint.X, (int)mousePoint.Y)) {
        return;
      }

      //place ranking away from cursor
      //var isMoveRanking = false;
      //if (rankingPositionLeft) {
      //  if (mousePoint.X<mapControl.PixelMap.XMax/3) {
      //    isMoveRanking = true;
      //    rankingPositionLeft = false;
      //  }
      //} else {
      //  if (mousePoint.X>mapControl.PixelMap.XMax*2/3) {
      //    isMoveRanking = true;
      //    rankingPositionLeft = true;
      //  }
      //}
      //if (rankingPositionTop) {
      //  if (mousePoint.Y<mapControl.PixelMap.YMax/3) {
      //    isMoveRanking = true;
      //    rankingPositionTop = false;
      //  }
      //} else {
      //  if (mousePoint.Y>mapControl.PixelMap.YMax*2/3) {
      //    isMoveRanking = true;
      //    rankingPositionTop = true;
      //  }
      //}
      //if (isMoveRanking) {
      //  InvalidateArrange();
      //}


      bool shouldPositionLeftChange;
      if (rankingPositionLeft) {
        shouldPositionLeftChange = mousePoint.X<mapControl.PixelMap.XMax/3;
      } else {
        shouldPositionLeftChange = mousePoint.X>mapControl.PixelMap.XMax*2/3;
      }
      bool shouldPositionTopChange;
      if (rankingPositionTop) {
        shouldPositionTopChange = mousePoint.Y<mapControl.PixelMap.YMax*3/7;
      } else {
        shouldPositionTopChange = mousePoint.Y>mapControl.PixelMap.YMax*4/7;
      }

      if (shouldPositionLeftChange && shouldPositionTopChange) {
        rankingPositionLeft = !rankingPositionLeft;
        rankingPositionTop = !rankingPositionTop;
        InvalidateArrange();
      }
    }


    private void switchInfoWindow() {
      switch (infoWindowMode) {
      case InfoWindowModeEnum.none:
        border.Visibility = Visibility.Collapsed;
        return;
      case InfoWindowModeEnum.ranking:
        border.Child = rankingGrid;
        break;
      case InfoWindowModeEnum.trace:
        border.Child = traceGrid;
        break;
      default:
        throw new NotImplementedException();
      }
      border.Visibility = Visibility.Visible;
      border.InvalidateMeasure();
      updateInfoWindow();
    }


    private void updateInfoWindow() {
      if (mapControl.Game==null)
        return;

      switch (infoWindowMode) {
      case InfoWindowModeEnum.none:
        break;
      case InfoWindowModeEnum.ranking:
        updateRanking();
        break;
      case InfoWindowModeEnum.trace:
        updateTrace();
        break;
      default:
        throw new NotImplementedException();
      }
    }


    private void updateRanking() {
      var playerStatistics = mapControl.Game.GetPlayerStatistics();
      var rowIndex = 1;
      do {
        var searchRank = rowIndex;
        for (var playerIdIndex = 0; playerIdIndex < playerStatistics.Length; playerIdIndex++) {
          if (playerStatistics[playerIdIndex].Rank==searchRank) {
            var nameTextBlock = rankingTextBlocks[rowIndex, rankingPlayersIndex];
            nameTextBlock.Text = mapControl.Game.Players[playerIdIndex].Name;
            nameTextBlock.Background = mapControl.PlayerBrushes[playerIdIndex];
            rankingTextBlocks[rowIndex, rankingRankIndex].Text = playerStatistics[playerIdIndex].Rank.ToString();
            rankingTextBlocks[rowIndex, rankingSizeIndex].Text = playerStatistics[playerIdIndex].Size.ToString();
            rankingTextBlocks[rowIndex, rankingSizePercentIndex].Text = playerStatistics[playerIdIndex].SizePercent.ToString("#0.0%");
            rankingTextBlocks[rowIndex, rankingCCountIndex].Text = playerStatistics[playerIdIndex].Countries.ToString();
            rankingTextBlocks[rowIndex, rankingCPercentIndex].Text = playerStatistics[playerIdIndex].CountriesPercent.ToString("#0.0%");
            rankingTextBlocks[rowIndex, rankingArmiesIndex].Text = playerStatistics[playerIdIndex].Armies.ToString();
            rowIndex++;
          }
        }
      } while (rowIndex < playerStatistics.Length+1);
    }


    private void updateTrace() {
      var results = mapControl.Game.Results;
      for (var resultsIndex = 0; resultsIndex < results.Count; resultsIndex++) {
        var result = results[resultsIndex];
        var rowIndex = resultsIndex + 1;

        var attackerTextBlock = traceTextBlocks[rowIndex, tracePlayerIndex];
        attackerTextBlock.Text = mapControl.Game.Players[result.PlayerId].Name;
        attackerTextBlock.Background = mapControl.PlayerBrushes[result.PlayerId];

        var defenderTextBlock = traceTextBlocks[rowIndex, traceDefenderIndex];
        if (result.MoveType==MoveTypeEnum.attack) {
          defenderTextBlock.Text = mapControl.Game.Players[result.DefenderId].Name;
          defenderTextBlock.Background = mapControl.PlayerBrushes[result.DefenderId];
        } else {
          defenderTextBlock.Text = "";
          defenderTextBlock.Background = null;
        }

        traceTextBlocks[rowIndex, traceActionIndex].Text = result.MoveType switch {
          MoveTypeEnum.none => "none",
          MoveTypeEnum.move => "moves",
          MoveTypeEnum.attack => result.IsSuccess ? "wins" : "loses",
          _ => throw new NotSupportedException(),
        };
        if (result.IsError) {
          traceTextBlocks[rowIndex, traceActionIndex].Text += " error";
        }

        if (result.MoveType==MoveTypeEnum.none) {
          traceTextBlocks[rowIndex, traceToIndex].Text = "";
          traceTextBlocks[rowIndex, traceFromIndex].Text = "";
        } else {
          traceTextBlocks[rowIndex, traceToIndex].Text = result.BeforeArmies![0] + "->" + result.AfterArmies![0];

          var attackString = "";
          for (var countryIndex = 1; countryIndex < result.BeforeArmies.Count; countryIndex++) {
            attackString += result.BeforeArmies[countryIndex] + "->" + result.AfterArmies[countryIndex] + ", ";
          }
          traceTextBlocks[rowIndex, traceFromIndex].Text = attackString;
        }
      }
      traceGrid.InvalidateMeasure();
    }
    #endregion


    #region Graphic Overrides
    //      -----------------

    protected override Size MeasureContentOverride(Size constraint) {
      if (border!=null) {
        border.Measure(new Size(constraint.Width, constraint.Height));
      }
      return constraint;
    }


    protected override Size ArrangeContentOverride(Rect arrangeRect) {
      if (border!=null) {
        double left;
        #pragma warning disable IDE0045 // Convert to conditional expression
        if (rankingPositionLeft) {
          left = mapControl.PixelMap.XMax / 10;
        } else {
          left = mapControl.PixelMap.XMax - border.DesiredSize.Width - mapControl.PixelMap.XMax / 10;
        }
        double top;
        if (rankingPositionTop) {
          top = mapControl.PixelMap.YMax / 10;
        } else {
          top = mapControl.PixelMap.YMax - border.DesiredSize.Height - mapControl.PixelMap.YMax / 10;
        }
        #pragma warning restore IDE0045
        border.ArrangeBorderPadding(arrangeRect, left, top, border.DesiredSize.Width, border.DesiredSize.Height);
      }
      return arrangeRect.Size;
    }
    #endregion
  }
}
