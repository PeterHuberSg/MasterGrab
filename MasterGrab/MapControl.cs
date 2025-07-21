/********************************************************************************************************

MasterGrab.MasterGrab.MapControl
================================

MapControl displays the game map with countries.

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
using System.Windows.Threading;


namespace MasterGrab {


  /// <summary>
  /// Info Window can be hidden or show ranking of players or the last move of each player
  /// </summary>
  public enum InfoWindowModeEnum {
    none,
    ranking,
    trace
  }


  /// <summary>
  /// Indicates what the map should show
  /// </summary>
  public enum ShowOptionEnum {
    /// <summary>
    /// Paint countries in the colours of their owners and show how many armies are in it
    /// </summary>
    Armies,
    /// <summary>
    /// Paint countries in the colours of their owners and show the countries' ID
    /// </summary>
    CountryId,
    /// <summary>
    /// Paint countries in a gray shade according to their size
    /// </summary>
    CountrySize,
    /// <summary>
    /// Display countries owned by robots which has the fewest neighbours
    /// </summary>
    FewestNeighbours,
    /// <summary>
    /// max value of ShowOptionEnum
    /// </summary>
    max
  }


  /// <summary>
  /// MapControl displays a map with countries.
  /// </summary>
  public class MapControl: CustomControlBase {

    #region Properties
    //      ----------

    public ShowOptionEnum ShowOption {
      get => showOption;
      set {
        if (showOption!=value) {
          showOption = value;
          isShowArmySizeChanged?.Invoke(value);
          InvalidateVisual();
        }
      }
    }
    ShowOptionEnum showOption = ShowOptionEnum.Armies;
    readonly Action<ShowOptionEnum> isShowArmySizeChanged;


    /// <summary>
    /// Should the ClusterCenter be shown for debugging reasons ?
    /// </summary>
    public const bool IsShowClusterCenter = true;

    public InfoWindowModeEnum InfoWindowMode {
      get => mapInfoWindow.InfoWindowMode;
      set => mapInfoWindow.InfoWindowMode = value;
    }
    #endregion


    #region Constructor
    //      -----------

    Options options;
    readonly int scale;
    readonly MapOverlayControl mapOverlayControl;
    readonly MapErrorOverlayControl mapErrorOverlayControl;
    readonly MapFinishedOverlayControl mapFinishedOverlayControl;
    bool isHighlightCountries;
    readonly MapInfoWindow mapInfoWindow;
    readonly DispatcherTimer autoPlayTimer;


    /// <summary>
    /// constructor
    /// </summary>
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor: many fields get only a value when OnRenderContent is executed for the first time
    public MapControl(Options options, int scale, Action<ShowOptionEnum> showOptionChanged) {
    #pragma warning restore CS8618
      this.options = options;
      initialisePlayerBrushes(options.Robots.Count + 1);
      this.scale = scale;
      this.isShowArmySizeChanged = showOptionChanged;
      autoPlayTimer = new(DispatcherPriority.Input);
      autoPlayTimer.Tick += AutoPlayTimer_Tick;

      //change some CustomControlBase properties
      Background = Brushes.Gray;
      BorderBrush = Brushes.DarkGoldenrod;

      mapOverlayControl = new MapOverlayControl(this);
      AddChild(mapOverlayControl);
      mapErrorOverlayControl = new MapErrorOverlayControl(this);
      mapInfoWindow = new MapInfoWindow(this);
      AddChild(mapInfoWindow);
      mapFinishedOverlayControl = new MapFinishedOverlayControl();
      AddChild(mapFinishedOverlayControl);
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Let's the user play the last game again, with the same country map
    /// </summary>
    internal void Replay() {
      if (isAutoPlaySelected && !options.IsHumanPlaying) {
        autoPlayTimer.Start();
      }
      isGuiMoveAwaited = false;
      mapFinishedOverlayControl.Hide();
      controllerReplay();
    }


    /// <summary>
    /// Lets the user play a new game with new country map
    /// </summary>
    internal void StartNewGame(Options newOptions){
      autoPlayTimer.Stop();
      isGuiMoveAwaited = false;
      Visibility = Visibility.Hidden;
      mapFinishedOverlayControl.Hide();
      newOptions.XCount = (int)RenderSize.Width + 1; //it seems that with width 1, actually 2 pixels are involved, pixel 0 and 1
      newOptions.YCount = (int)RenderSize.Height + 1;
      options = controllerStartNewGame(newOptions);
      var playersCount = options.IsHumanPlaying ? options.Robots.Count + 1 : options.Robots.Count;
      initialisePlayerBrushes(playersCount);
    }


    Action<Move> controllerMove;
    
    
    /// <summary>
    /// Executes the user's next move
    /// </summary>
    internal void ControllerMove(Move move) {
      if (advancedOptionsWindow!=null) {
        advancedOptionsWindow.Close(); //it seems closing an already closed window doesn't give a problem
        advancedOptionsWindow = null;
      }
      if (!isGuiMoveAwaited) {
        return;
      }
      isGuiMoveAwaited = false;
      controllerMove(move);
    }


    /// <summary>
    /// Displays just one country, which is useful while debugging display problems
    /// </summary>
    public void DisplayOneCountry(int countryId) {
      GeometryByCountry = new Geometry[Game.Map.Count];
      var geometry = new PathGeometry();
      addCountryPath(geometry, countryFixArray[countryId]);
      GeometryByCountry[countryId] = geometry;
      InvalidateVisual();
    }


    /// <summary>
    /// Switches back to displaying all countries, which might be needed after DisplayOneCountry()
    /// </summary>
    public void DisplayAllCountries() {
      GeometryByCountry = new Geometry[Game.Map.Count];
      foreach (var countryFix in countryFixArray) {
        var geometry = new PathGeometry();
        addCountryPath(geometry, countryFix);
        GeometryByCountry[countryFix.Id] = geometry;
      }
    }


    /// <summary>
    /// When human player is not enabled, SetTimer() starts autoplay every timespan. If timespan is null, autoplay gets
    /// stopped.
    /// </summary>
    /// <param name="timerState"></param>
    public void SetTimer(TimeSpan? timeSpan) {
      isAutoPlaySelected = timeSpan is not null;
      if (timeSpan is null || options.IsHumanPlaying || Game.HasGameFinished) {
        autoPlayTimer.Stop();
        autoPlayTimerIntervalMilliSec = int.MinValue;
      } else {
        autoPlayTimer.Start();
        autoPlayTimer.Interval = timeSpan.Value;
        autoPlayTimerIntervalMilliSec = (int)timeSpan.Value.TotalMilliseconds;
      }
      isTimerWaiting = false;
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void ParentWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
      autoPlayTimer.Stop();
      isGuiMoveAwaited = false;
      controllerStop();
    }


    private void AutoPlayTimer_Tick(object? sender, EventArgs e) {
      var now = DateTime.Now;
      var nowMilliseconds = (int)now.TimeOfDay.TotalMilliseconds;
      var timerInterval = autoPlayTimerIntervalMilliSec - nowMilliseconds%autoPlayTimerIntervalMilliSec + 5;//5: sometimes the tick comes few millisecs early
      autoPlayTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
      if (isGuiMoveAwaited) {
        ControllerMove(Move.NoMove);
      } else {
        isTimerWaiting = true;
      }
    }
    #endregion


    #region Painting
    //      --------

    const double percent10 = 0.1;
    const double percent80 = 0.8;


    protected override Size MeasureContentOverride(Size constraint) {
      mapOverlayControl.Measure(new Size(constraint.Width, constraint.Height));
      if (isHighlightCountries) {
        mapErrorOverlayControl.Measure(new Size(constraint.Width, constraint.Height));
      }
      mapInfoWindow.Measure(new Size(constraint.Width, constraint.Height));
      mapFinishedOverlayControl.Measure(new Size(constraint.Width*percent80, constraint.Height*percent80));
      return constraint;
    }


    protected override Size ArrangeContentOverride(Rect arrangeRect) {
      mapOverlayControl.ArrangeBorderPadding(arrangeRect, 0, 0, arrangeRect.Width, arrangeRect.Height);
      if (isHighlightCountries) {
        mapErrorOverlayControl.ArrangeBorderPadding(arrangeRect, 0, 0, arrangeRect.Width, arrangeRect.Height);
      }
      mapInfoWindow.ArrangeBorderPadding(arrangeRect, 0, 0, arrangeRect.Width, arrangeRect.Height);
      mapFinishedOverlayControl.ArrangeBorderPadding(arrangeRect, arrangeRect.Width*percent10, arrangeRect.Height*percent10, 
        arrangeRect.Width*percent80, arrangeRect.Height*percent80);
      return arrangeRect.Size;
    }


    GameController? controller;

    Action controllerReplay;
    Func<Options, Options> controllerStartNewGame;
    Action controllerStop;

    internal Player? GuiPlayer { get; private set; }
    internal PixelMap PixelMap { get; private set; }
    internal Game Game { get; private set; }
    IReadOnlyList<CountryFix> countryFixArray;

    readonly Brush cheapBrush = new SolidColorBrush(Color.FromArgb(0xA0, 0xFF, 0xFF, 0xFF));


    public Pen BorderPen { get; private set; } = new(new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60)), 1);
    public Pen BorderPen2 { get; private set; } = new(Brushes.Cyan, 1);

    readonly Pen movePen = new(Brushes.Gray, 2);
    readonly Pen attackSuccessPen = new(new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0)), 2);
    readonly Pen attackFailPen = new(new SolidColorBrush(Color.FromArgb(0x80, 0xff, 0xff, 0xff)), 2);
    readonly Pen clusterCenterPen = new(new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0)), 8);
    internal Geometry[] GeometryByCountry { get; private set; }
    internal GlyphDrawer GlyphDrawerNormal { get; private set; }
    internal GlyphDrawer GlyphDrawerBold { get; private set; }
    internal GlyphDrawer GlyphDrawerItalic { get; private set; }


    bool[]? countriesMinNeighboursIds;


    protected override void OnRenderContent(DrawingContext drawingContext, Size renderContentSize) {
      if (controller==null) {
        //initialise map once window size is known
        options.XCount = (int)renderContentSize.Width + 1; //it seems that with width 1, actually 2 pixels are involved, pixel 0 and 1
        options.YCount = (int)renderContentSize.Height + 1;
        controller = new GameController(options,
          mapChanged, gameChanged, exceptionRaised,
          out controllerMove, out controllerReplay, out controllerStartNewGame, out controllerStop);
        //a Control has no closing event, use parent window instead
        var parentWindow = Window.GetWindow(this);
        parentWindow.Closing += ParentWindow_Closing;
      }

      if (PixelMap!=null) {
        if (showOption==ShowOptionEnum.FewestNeighbours && (countriesMinNeighboursIds is null || countriesMinNeighboursIds.Length!=Game.Map.Count)) {
          countriesMinNeighboursIds = new bool[Game.Map.Count];
        }
        double maxArmies = 0;
        foreach (var country in Game.Map) {
          maxArmies = Math.Max(maxArmies, (int)country.ArmySize);
        }
        var armyDelta = (maxArmies + 1) / shadeCount;

        if (showOption==ShowOptionEnum.FewestNeighbours) {
          //collect the robot owned countries with the fewest neighbours
          var minNeighbourCount = int.MaxValue;
          foreach (var country in Game.Map) {
            if (country.IsMountain || country.OwnerId==GuiPlayer!.Id) continue;

            if (minNeighbourCount>country.NeighbourIds.Count) {
              Array.Clear(countriesMinNeighboursIds!);
              minNeighbourCount = country.NeighbourIds.Count;
            }
            if (minNeighbourCount==country.NeighbourIds.Count) {
              countriesMinNeighboursIds![country.Id] = true;
            }
          }
        }

        //other thread has created a map
        //draw country colours and borders
        foreach (var country in Game.Map) {
          if (showOption is ShowOptionEnum.Armies or ShowOptionEnum.CountryId or ShowOptionEnum.FewestNeighbours) {
            if (country.IsMountain) {
              drawingContext.DrawGeometry(BorderPen.Brush, BorderPen, GeometryByCountry[country.Id]);
            } else {
              Brush countryBrush;
              if (showOption==ShowOptionEnum.FewestNeighbours && countriesMinNeighboursIds![country.Id]) {
                countryBrush = Brushes.White;
              } else { 
                var shade = Math.Max(shadeCount - 1 - (int)(country.ArmySize / armyDelta), 0);
                countryBrush = PlayerBrushes2[country.OwnerId, shade];
              }
              drawingContext.DrawGeometry(countryBrush, BorderPen, GeometryByCountry[country.Id]);
            }

          } else {
            var maxMinusMin = Game.Map.MaxCountrySize - Game.Map.MinCountrySize;

            if (country.IsMountain) {
              drawingContext.DrawGeometry(Brushes.DarkBlue, BorderPen, GeometryByCountry[country.Id]);
            } else {
              //shade = 0xff - 0xa0 * (max - size) / (max - min)
              byte shade = (byte)(0xff - 0xb0 * (Game.Map.MaxCountrySize - country.Size) / maxMinusMin);
              var countryBrush = new SolidColorBrush(Color.FromRgb(shade, shade, shade));
              drawingContext.DrawGeometry(countryBrush, BorderPen2, GeometryByCountry[country.Id]);
            }
          }
        }

        if (IsShowClusterCenter && options.Clustering>0 && Game.Map.ClusterCoordinates is not null) {
          //draw cluster centers
          foreach (var (x, y) in Game.Map.ClusterCoordinates) {
            drawingContext.DrawLine(clusterCenterPen, new Point(x-24, y), new Point(x+24, y));
            drawingContext.DrawLine(clusterCenterPen, new Point(x, y-24), new Point(x, y+24));
          }
        }

        //draw arrows
        if (showOption is ShowOptionEnum.Armies or ShowOptionEnum.CountryId or ShowOptionEnum.FewestNeighbours) {
          foreach (var result in Game.Results) {
            if (result.CountryIds!=null) {
              var toCountry = Game.Map[result.CountryId];
              var toCoordinate = countryFixArray[result.CountryId].Center;
              var brush = PlayerBrushes[result.PlayerId];
              var pen =
                result.MoveType==MoveTypeEnum.attack ? (result.IsSuccess ? attackSuccessPen : attackFailPen) : movePen;
              foreach (var fromCountryId in result.CountryIds) {
                var fromCoordinate = countryFixArray[fromCountryId].Center;
                draw1or2Arrows(drawingContext, fromCoordinate, toCoordinate, brush, pen);
              }
            }
          }
        }

        //draw country states and country infos
        foreach (var country in Game.Map) {
          var countryFix = countryFixArray[country.Id];
          if (country.IsMountain)
            continue;

          switch (showOption) {
          case ShowOptionEnum.Armies:
          case ShowOptionEnum.FewestNeighbours:
            switch (country.State) {
            case CountryStateEnum.normal:
              break;
            case CountryStateEnum.moved:
              drawingContext.DrawEllipse(PlayerBrushes[country.OwnerId], BorderPen, new Point(countryFix.Center.X*scale, countryFix.Center.Y*scale), 10*scale, 10*scale);
              break;
            case CountryStateEnum.attacked:
              drawingContext.DrawEllipse(PlayerBrushes[country.OwnerId], BorderPen, new Point(countryFix.Center.X*scale, countryFix.Center.Y*scale), 15*scale, 15*scale);
              break;
            case CountryStateEnum.taken:
              drawingContext.DrawEllipse(PlayerBrushes[country.PreviousOwnerId], BorderPen, new Point(countryFix.Center.X*scale, countryFix.Center.Y*scale), 20*scale, 20*scale);
              break;
            case CountryStateEnum.cheap:
              drawingContext.DrawEllipse(cheapBrush, BorderPen, new Point(countryFix.Center.X*scale, countryFix.Center.Y*scale), 13*scale, 13*scale);
              break;
            default:
              throw new NotSupportedException();
            }

            var circleString = ((int)country.ArmySize).ToString();
            if (country.ArmySize>0.9*country.Capacity) {
              GlyphDrawerBold.WriteCentered(drawingContext, new Point(countryFix.Center.X*scale, countryFix.Center.Y*scale), circleString, 12, Brushes.Black);
            } else {
              GlyphDrawerNormal.WriteCentered(drawingContext, new Point(countryFix.Center.X*scale, countryFix.Center.Y*scale), circleString, 12, Brushes.Black);
            }
            break;

          case ShowOptionEnum.CountryId:
            GlyphDrawerItalic.WriteCentered(drawingContext, new Point(countryFix.Center.X*scale, countryFix.Center.Y*scale), country.Id.ToString(), 12, Brushes.Black);
            break;

          case ShowOptionEnum.CountrySize:
            GlyphDrawerItalic.WriteCentered(drawingContext, new Point(countryFix.Center.X*scale, countryFix.Center.Y*scale), country.Size.ToString(), 12, Brushes.Black);
            break;
          default:
            break;
          }
          #if ShowSomeDebuggingInfoInWindow
          drawingContext.DrawText(
            //new FormattedText(country.ToString().Replace("; ", Environment.NewLine),
            //new FormattedText(country.Id.ToString() + Environment.NewLine + neighboursString,
            //new FormattedText(country.Id + ", " + ((int)country.ArmySize),
            new FormattedText(((int)country.ArmySize).ToString(),
              Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
              new Typeface("Verdana"), 12, Brushes.Black),
            new Point(country.Center.X*scale - 10, country.Center.Y*scale - 10));

          drawingContext.DrawText(
            //            new FormattedText(country.ToString().Replace("; ", Environment.NewLine),
            //new FormattedText(country.Id.ToString() + Environment.NewLine + neighboursString,
            new FormattedText(country.Id.ToString(),
              Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight,
              new Typeface("Verdana"), 12, Brushes.Black),
            new Point(country.Coordinate.X*scale, country.Coordinate.Y*scale));
          #endif
        }
      }
    }


    int errorWindowsCount;
    ShowOptionEnum oldShowOption;


    private void exceptionRaised(Exception exception) {
      if (!Dispatcher.CheckAccess()) {
        // We're not in the UI thread, so we need to call BeginInvoke
        Dispatcher.BeginInvoke(new Action<Exception>(exceptionRaised), exception);
        return;
      }

      if (errorWindowsCount<=0) {
        oldShowOption = ShowOption;
        ShowOption = ShowOptionEnum.CountryId;
        isHighlightCountries = true;
        AddChild(mapErrorOverlayControl);
      }
      errorWindowsCount++;
      var errorWindow = ErrorWindow.Show(Window.GetWindow(this), exception, this);
      errorWindow.Closed += ErrorWindow_Closed;

      if (exception is GameException gameException) {
        var messageParts = exception.Message.Split(new string[] { "Country (", ")" }, StringSplitOptions.RemoveEmptyEntries);
        var isFoundCountry = false;
        foreach (var messagePart in messageParts) {
          if (isFoundCountry) {
            isFoundCountry = false;
            var commaPos = messagePart.IndexOf(',');
            if (commaPos<0) {
              break;
            }
            var countryIdString = messagePart[..commaPos];
            if (!int.TryParse(countryIdString, out var countryId)) {
              break;
            }
            //country ID found
            mapErrorOverlayControl.AddCountry(countryId);


          } else {
            isFoundCountry = true;
          }
        }
      }
    }


    private void ErrorWindow_Closed(object? sender, EventArgs e) {
      errorWindowsCount--;
      if (errorWindowsCount<=0) {
        ShowOption = oldShowOption;
        isHighlightCountries = false;
        RemoveChild(mapErrorOverlayControl);
        mapErrorOverlayControl.RemoveAllCountries();
      }
    }


    AdvancedOptionsWindow? advancedOptionsWindow; //gets displayed for new random game
    bool isAutoPlaySelected; //did user select auto play. Even if this is true, autoplay will not run when user plays too.
    bool isGuiMoveAwaited; //Is GameController waiting for the GUI player to make a move ?
    bool isTimerWaiting; //did autoplay timer tick happen when controller was still busy ?
    int autoPlayTimerIntervalMilliSec;


    private void mapChanged(PixelMap map, IReadOnlyList<CountryFix> countryFixArray, Game game, Player? guiPlayer) {
      if (!Dispatcher.CheckAccess()) {
        // We're not in the UI thread, so we need to call BeginInvoke
        Dispatcher.BeginInvoke(new Action<PixelMap, IReadOnlyList<CountryFix>, Game, Player>(mapChanged), 
          map, countryFixArray, game, guiPlayer);
        return;
      }

      // on the WPF UI thread
      Visibility = Visibility.Visible;
      PixelMap = map;
      this.countryFixArray = countryFixArray;
      Game = game;
      GuiPlayer = guiPlayer;
      GeometryByCountry = new Geometry[game.Map.Count];
      var pixelsPerDip = (float)VisualTreeHelper.GetDpi(this).PixelsPerDip;
      GlyphDrawerNormal = new GlyphDrawer(FontFamily, FontStyle, FontWeight, FontStretch, pixelsPerDip);
      GlyphDrawerBold = new GlyphDrawer(FontFamily, FontStyle, FontWeights.Bold, FontStretch, pixelsPerDip);
      GlyphDrawerItalic = new GlyphDrawer(FontFamily, FontStyles.Italic, FontWeight, FontStretch, pixelsPerDip);

      foreach (var countryFix in countryFixArray) {
        var geometry = new PathGeometry();
        addCountryPath(geometry, countryFix);
        GeometryByCountry[countryFix.Id] = geometry;
      }

      InvalidateVisual();
      mapOverlayControl.MapChanged();
      mapInfoWindow.MapChanged();
      if (options.IsRandomOptions) {
        advancedOptionsWindow = new AdvancedOptionsWindow(options.MountainsPercentage, options.ArmiesInBiggestCountry,
        options.ArmyGrowthFactor, options.ProtectionFactor, options.AttackFactor, options.AttackBenefitFactor, options.IsRandomOptions, 
        isReadOnly: true);
        advancedOptionsWindow.Show();
      }

      isGuiMoveAwaited = true;
      if (isAutoPlaySelected && !options.IsHumanPlaying) {
        autoPlayTimer.Start();
      }
    }


    private void gameChanged(Game game, Player? player) {
      if (!Dispatcher.CheckAccess()) {
        // We're not in the UI thread, so we need to call BeginInvoke
        Dispatcher.BeginInvoke(new Action<Game, Player>(gameChanged), game, player);
        return;
      }

      // on the WPF UI thread
      Game = game;
      GuiPlayer = player;
      mapInfoWindow.GameChanged();

      if (game.HasGameFinished) {
        mapFinishedOverlayControl.ShowResult(options.IsHumanPlaying, game.WinnerPlayerId);
        autoPlayTimer.Stop();
      } else {
        mapFinishedOverlayControl.Hide();
      }

      isGuiMoveAwaited = true;
      if (isTimerWaiting) {
        isTimerWaiting = false;
        ControllerMove(Move.NoMove);
      }
      InvalidateVisual();
    }


    const bool useStroke = true;


    private void addCountryPath(PathGeometry geometry, CountryFix countryFix) {
      var pathFigure = new PathFigure();
      var oldCoordinate = countryFix.BorderCoordinates[0];
      pathFigure.StartPoint = new Point(oldCoordinate.X*scale, oldCoordinate.Y*scale);
      pathFigureSegmentsStart(pathFigure.StartPoint);
      for (var coordinateIndex = 1; coordinateIndex < countryFix.BorderCoordinates.Count; coordinateIndex++) {
        var coordinate = countryFix.BorderCoordinates[coordinateIndex];
        if (coordinate.IsCrossingBorder(oldCoordinate)) {
          var lastCoordinates = addPartialCountries(geometry, countryFix, ref coordinateIndex);
          if (lastCoordinates.Count>0) {
            addCornerIfNeeded(oldCoordinate, lastCoordinates[0], pathFigure);
            foreach (var lastCoordinate in lastCoordinates) {
              pathFigureSegmentsAdd(pathFigure, toPoint(lastCoordinate));
            }
          }else {
            //lastCoordinates is empty, the last coordinate was at a different window border than the very 
            //first one. Presently the pathFigure of the first coordinates gets completed, but nothing needs
            //to be added
            addCornerIfNeeded(oldCoordinate, countryFix.BorderCoordinates[0], pathFigure);
          }
        } else {
          pathFigureSegmentsAdd(pathFigure, toPoint(coordinate));
        }
        oldCoordinate = coordinate;
      }
      pathFigureSegmentsAdd(pathFigure, pathFigure.StartPoint);
      geometry.Figures.Add(pathFigure);
    }


    #if IsDebuggingFigureSegments
    Point lastPoint;
    #endif
    readonly RingBuffer<Point> pointRingBuffer = new(100);


    private void pathFigureSegmentsStart(Point startPoint) {
      pointRingBuffer.Clear();
      pointRingBuffer.Add(startPoint);
      #if IsDebuggingFigureSegments
      lastPoint = startPoint;
      #endif
    }


    #pragma warning disable CA1822 // Mark members as static
    private void pathFigureSegmentsAdd(PathFigure pathFigure, Point point) {
    #pragma warning restore CA1822
      #if IsDebuggingFigureSegments
      Tracer.Trace(point.ToString());
      var distance = lastPoint-point;
      if (distance.Length>600) {
        var  messages = Tracer.GetTrace();
        var sb = new StringBuilder();
        if (messages.Length>0) {
          var minMessageIndex = Math.Max(0, messages.Length-101);
          for (var messageIndex = messages.Length-1; messageIndex > minMessageIndex; messageIndex--) {
            sb.AppendLine(messages[messageIndex].ToString());
          }
          string s = sb.ToString();
        }
      }
      lastPoint = point;
      #endif
      pathFigure.Segments.Add(new LineSegment(point, useStroke));
    }



    /// <summary>
    /// Adds a part of a country to the geometry which lays at the opposite end of the map as the very
    /// first border point of the country. Each time the border gets crossed, the country part gets
    /// added, except the points after the last crossing gets returned as a list of Point.
    /// </summary>
    private List<Coordinate> addPartialCountries(PathGeometry geometry, CountryFix countryFix, ref int coordinateIndex) {
      var nextBorderCoordinates = new List<Coordinate>();
      var startCoordinate = countryFix.BorderCoordinates[coordinateIndex];
      nextBorderCoordinates.Add(startCoordinate);
      var oldCoordinate = startCoordinate;
      coordinateIndex++;
      var lastCoordinateIndex = countryFix.BorderCoordinates.Count - 1;
      var isBorderCrossingAtEnd = false;
      for (; coordinateIndex < countryFix.BorderCoordinates.Count; coordinateIndex++) {
        var coordinate = countryFix.BorderCoordinates[coordinateIndex];
        if (coordinateIndex==lastCoordinateIndex) {
          //is the last and the first border point at different window borders ?
          isBorderCrossingAtEnd = coordinate.IsCrossingBorder(countryFix.BorderCoordinates[0]);
          if (isBorderCrossingAtEnd) {
            nextBorderCoordinates.Add(coordinate);
            oldCoordinate = coordinate;
          }
        }
        if (coordinate.IsCrossingBorder(oldCoordinate) || isBorderCrossingAtEnd) {
          var pathFigure = new PathFigure {StartPoint = toPoint(nextBorderCoordinates[0])};
          for (var nextBorderCoordinatesIndex = 1; nextBorderCoordinatesIndex < nextBorderCoordinates.Count; nextBorderCoordinatesIndex++) {
            pathFigure.Segments.Add(new LineSegment(toPoint(nextBorderCoordinates[nextBorderCoordinatesIndex]), useStroke));
          }
          addCornerIfNeeded(nextBorderCoordinates[0], oldCoordinate, pathFigure);
          pathFigure.Segments.Add(new LineSegment(pathFigure.StartPoint, useStroke));
          geometry.Figures.Add(pathFigure);
          nextBorderCoordinates.Clear();
        }
        if (!isBorderCrossingAtEnd) {
          nextBorderCoordinates.Add(coordinate);
        }
        oldCoordinate = coordinate;
      }
      return nextBorderCoordinates;
    }


    private Point toPoint(Coordinate coordinate) {
      return new Point(coordinate.X*scale, coordinate.Y*scale);
    }


    private void addCornerIfNeeded(Coordinate coordinate1, Coordinate coordinate2, PathFigure pathFigure) {
      var isX1Zero = coordinate1.X==0;
      var isX1Max = coordinate1.X==PixelMap.XMax;
      var isX1Border = isX1Zero || isX1Max;

      var isY1Zero = coordinate1.Y==0;
      var isY1Max = coordinate1.Y==PixelMap.YMax;
      var isY1Border = isY1Zero || isY1Max;

      var isX2Zero = coordinate2.X==0;
      var isX2Max = coordinate2.X==PixelMap.XMax;
      var isX2Border = isX2Zero || isX2Max;

      var isY2Zero = coordinate2.Y==0;
      var isY2Max = coordinate2.Y==PixelMap.YMax;
      var isY2Border = isY2Zero || isY2Max;

      if (!isX1Border && !isY1Border) {
        return;
      }
      if (!isX2Border && !isY2Border) {
        return;
      }

      if (isX1Border && isY1Border)
        return; //Point1 is a corner, nothing needs to be added
      if (isX2Border && isY2Border)
        return; //Point2 is a corner, nothing needs to be added

      //different map-borders get crossed -> add map corner
      //bool isXZero = coordinate1.X==0 || coordinate2.X==0;
      //bool isYZero = coordinate1.Y==0 || coordinate2.Y==0;
      if ((isX1Zero && isY2Zero) || (isX2Zero && isY1Zero)) {
        //add top left corner
        pathFigureSegmentsAdd(pathFigure, new Point(0, 0));
      } else if ((isX1Max && isY2Zero) || (isX2Max && isY1Zero)) {
        //add top right corner
        pathFigureSegmentsAdd(pathFigure, new Point(PixelMap.XMax*scale, 0));
      } else if ((isX1Zero && isY2Max) || (isX2Zero && isY1Max)) {
        //add bottom left corner
        pathFigureSegmentsAdd(pathFigure, new Point(0, PixelMap.YMax*scale));
      } else if ((isX1Max && isY2Max) || (isX2Max && isY1Max)) {
        //add bottom right corner
        pathFigureSegmentsAdd(pathFigure, new Point(PixelMap.XMax*scale, PixelMap.YMax*scale));
      }
    }


    private void draw1or2Arrows(DrawingContext drawingContext, Coordinate fromCoordinate, Coordinate toCoordinate, Brush brush, Pen pen) {
      var deltaX = Math.Abs(fromCoordinate.X - toCoordinate.X);
      var deltaY = Math.Abs(fromCoordinate.Y - toCoordinate.Y);
      if (deltaX<PixelMap.XMax/2 && deltaY<PixelMap.YMax/2) {
        //only 1 arrow needed
        drawArrow(drawingContext, fromCoordinate.X, fromCoordinate.Y, toCoordinate.X, toCoordinate.Y, brush, pen);
      } else {
        //the shortest connection is across the Window border. Draw 2 arrows
        int fromOutsideX;
        int fromOutsideY;
        int toOutsideX;
        int toOutsideY;
        if (deltaX<PixelMap.XMax/2) {
          fromOutsideX = fromCoordinate.X;
          toOutsideX = toCoordinate.X;
        } else {
          if (fromCoordinate.X > toCoordinate.X) {
            fromOutsideX = fromCoordinate.X - PixelMap.XMax;
            toOutsideX = toCoordinate.X + PixelMap.XMax;
          } else {
            fromOutsideX = fromCoordinate.X + PixelMap.XMax;
            toOutsideX = toCoordinate.X - PixelMap.XMax;
          }
        }
        if (deltaY<PixelMap.YMax/2) {
          fromOutsideY = fromCoordinate.Y;
          toOutsideY = toCoordinate.Y;
        } else {
          if (fromCoordinate.Y > toCoordinate.Y) {
            fromOutsideY = fromCoordinate.Y - PixelMap.YMax;
            toOutsideY = toCoordinate.Y + PixelMap.YMax;
          } else {
            fromOutsideY = fromCoordinate.Y + PixelMap.YMax;
            toOutsideY = toCoordinate.Y - PixelMap.YMax;
          }
        }
        drawArrow(drawingContext, fromCoordinate.X, fromCoordinate.Y, toOutsideX, toOutsideY, brush, pen);
        drawArrow(drawingContext, fromOutsideX, fromOutsideY, toCoordinate.X, toCoordinate.Y, brush, pen);
      }
    }


    private void drawArrow(DrawingContext drawingContext, int fromX, int fromY, int toX, int toY, Brush brush, Pen pen) {
      var geometry = new PathGeometry();
      var pathFigure = new PathFigure();
      var deltaX = fromX>toX ? fromX - toX : toX - fromX;
      var deltaY = fromY>toY ? fromY - toY : toY - fromY;
      var distance = Math.Sqrt(deltaX*deltaX + deltaY*deltaY);
      var angle = Math.Acos(deltaX/distance);
      double arrowWidth = 10;
      var sinWidth = Math.Sin(angle)*arrowWidth;
      var cosWidth = Math.Cos(angle)*arrowWidth;
      double height = 20;
      double centerX;
      double fromX1;
      double fromY1;
      double toX1;
      double toY1;
      double padding = 10;
      if (toX > fromX) {
        fromX1 = fromX + padding * Math.Cos(angle);
        toX1 = toX - padding * Math.Cos(angle);
        centerX = toX1 - height * Math.Cos(angle);
      } else {
        fromX1 = fromX - padding * Math.Cos(angle);
        toX1 = toX + padding * Math.Cos(angle);
        centerX = toX1 + height * Math.Cos(angle);
        sinWidth = -sinWidth;
      }
      double centerY;
      if (toY > fromY) {
        fromY1 = fromY + padding * Math.Sin(angle);
        toY1 = toY - padding * Math.Sin(angle);
        centerY = toY1 - height * Math.Sin(angle);
      } else {
        fromY1 = fromY - padding * Math.Sin(angle);
        toY1 = toY + padding * Math.Sin(angle);
        centerY = toY1 + height * Math.Sin(angle);
        cosWidth = -cosWidth;
      }

      pathFigure.StartPoint = new Point((fromX1 + sinWidth/2) * scale, (fromY1 -cosWidth/2) * scale);
      pathFigure.Segments.Add(new LineSegment(new Point((centerX + sinWidth/2) * scale, (centerY - cosWidth/2) * scale), true));
      pathFigure.Segments.Add(new LineSegment(new Point((centerX + sinWidth) * scale, (centerY - cosWidth) * scale), true));
      pathFigure.Segments.Add(new LineSegment(new Point(toX1 * scale, toY1 * scale), true));
      pathFigure.Segments.Add(new LineSegment(new Point((centerX - sinWidth) * scale, (centerY + cosWidth) * scale), true));
      pathFigure.Segments.Add(new LineSegment(new Point((centerX - sinWidth/2) * scale, (centerY + cosWidth/2) * scale), true));
      pathFigure.Segments.Add(new LineSegment(new Point((fromX1 - sinWidth/2) * scale, (fromY1 + cosWidth/2) * scale), true));
      pathFigure.Segments.Add(new LineSegment(pathFigure.StartPoint, true));
      geometry.Figures.Add(pathFigure);
      drawingContext.DrawGeometry(brush, pen, geometry);
    }


    internal Brush[] PlayerBrushes { get; private set; }
    internal Brush[,] PlayerBrushes2 { get; private set; }
    const int shadeCount = 3;


    void initialisePlayerBrushes(int playersCount) {
      PlayerBrushes = new Brush[playersCount];
      PlayerBrushes2 = new Brush[playersCount, shadeCount];
      for (var playerIdIndex = 0; playerIdIndex < playersCount; playerIdIndex++) {
        var colorIndex = options.IsHumanPlaying ? playerIdIndex : playerIdIndex+1;
        var color = Color.FromArgb(options.Colors[colorIndex, 0], options.Colors[colorIndex, 1], options.Colors[colorIndex, 2], options.Colors[colorIndex, 3]);
        PlayerBrushes[playerIdIndex] = new SolidColorBrush(color);
        var rDiff = 0xFF - color.R;
        var gDiff = 0xFF - color.G;
        var bDiff = 0xFF - color.B;
        var shadeStep = shadeCount + 6;
        var rDelta = rDiff / shadeStep;
        var gDelta = gDiff / shadeStep;
        var bDelta = bDiff / shadeStep;
        for (var shadeIndex = 0; shadeIndex < shadeCount; shadeIndex++) {
          if (shadeIndex==0) {
            PlayerBrushes2[playerIdIndex, shadeIndex] = PlayerBrushes[playerIdIndex];
          } else {
            var r = (byte)(color.R + rDelta * shadeIndex);
            var g = (byte)(color.G + gDelta * shadeIndex);
            var b = (byte)(color.B + bDelta * shadeIndex);
            PlayerBrushes2[playerIdIndex, shadeIndex] = new SolidColorBrush(Color.FromRgb(r, g, b));
          }
        }
      }
    }


    public static Color ToColour(int number) {
      return (number % 20) switch {
         0 => Color.FromRgb(0xFF, 0x80, 0x80),
         1 => Color.FromRgb(0xFF, 0xFF, 0x30),
         2 => Color.FromRgb(0x80, 0xEF, 0x80),
         3 => Color.FromRgb(0x80, 0xEF, 0xEF),
         4 => Color.FromRgb(0x80, 0x80, 0xEF),
         5 => Color.FromRgb(0xEF, 0x80, 0xEF),
         6 => Color.FromRgb(0xE0, 0xE0, 0x70),
         7 => Color.FromRgb(0xE0, 0xE0, 0xE0),
         8 => Colors.DeepPink,
         9 => Colors.Firebrick,
        10 => Colors.ForestGreen,
        11 => Colors.Fuchsia,
        12 => Colors.Gold,
        13 => Colors.LightGreen,
        14 => Colors.GreenYellow,
        15 => Colors.HotPink,
        16 => Colors.IndianRed,
        17 => Colors.Gray,
        18 => Colors.Khaki,
        19 => Colors.Lime,
        _ => throw new NotSupportedException(),
      };
    }
#endregion
  }
}
