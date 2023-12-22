/********************************************************************************************************

MasterGrab.MasterGrab.MapOverlayControl
=======================================

Repaints country borders depending on the cursor's position.

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
using System.Windows.Input;
using System.Windows.Media;


namespace MasterGrab {


  public class MapOverlayControl: CustomControlBase {

    #region Constructor
    //      -----------

    readonly MapControl mapControl;
    readonly MediaPlayer mediaPlayer;

    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. countryStateByCountry gets only created in MapChanged() 
    public MapOverlayControl(MapControl mapControl) {
    #pragma warning restore CS8618
      this.mapControl = mapControl;

      mediaPlayer = new MediaPlayer();
      mediaPlayer.Open(new Uri("MetalBang.wav", UriKind.Relative));
      mediaPlayer.Volume = 100;
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void mapControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
      var point = e.GetPosition(this);
      if (!Coordinate.TryConvert(mapControl.PixelMap, (int)point.X, (int)point.Y, out var coordinate)) {
        return;
      }

      if (coordinate.Y<10) {
        mapControl.DisplayAllCountries();
        return;
      }

      //int clickCountryId = mapControl.Map[coordinate];
      mapControl.DisplayOneCountry(mapControl.PixelMap[coordinate]);
    }


    int mouseMoveCountryId;
    int clickCountryId = int.MinValue;
    readonly List<Country> selectedCountries = new();


    private void mapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      if (mapControl.Game.HasGameFinished) return;

      var point = e.GetPosition(this);
      if (!Coordinate.TryConvert(mapControl.PixelMap, (int)point.X, (int)point.Y, out var coordinate)) {
        clickCountryId = int.MinValue;
        return;
      }
      clickCountryId = mapControl.PixelMap[coordinate];
    }


    private void mapControl_MouseMove(object sender, MouseEventArgs e) {
      if (mapControl.Game.HasGameFinished) return;

      var mousePoint = e.GetPosition(this);
      if (!Coordinate.TryConvert(mapControl.PixelMap, (int)mousePoint.X, (int)mousePoint.Y, out var coordinate)) {
        clickCountryId = int.MinValue;
        return;
      }

      var countryId = mapControl.PixelMap[coordinate];
      if (clickCountryId!=countryId) {
        clickCountryId = int.MinValue;
      }

      if (mouseMoveCountryId!=countryId) {
        mouseMoveCountryId = countryId;
        for (var countryStateIndex = 0; countryStateIndex < countryStateByCountry.Length; countryStateIndex++) {
          var countryState = countryStateByCountry[countryStateIndex];
          if (countryState!=countryStateEnum.selected) {
            countryStateByCountry[countryStateIndex] = countryStateEnum.noState;
          }
        }
        var country = mapControl.Game.Map[countryId];
        if (!country.IsMountain) {
          if (countryStateByCountry[countryId]!=countryStateEnum.selected) {
            countryStateByCountry[countryId] = 
              country.OwnerId==mapControl.GuiPlayer?.Id ? countryStateEnum.mouseHoverOwner : countryStateEnum.mouseHoverOpponent;
          }
          foreach (var neighbourId in country.NeighbourIds) {
            markAsOwnerOrOpponent(neighbourId);
          }
        }
        InvalidateVisual();
      }
    }


    private void markAsOwnerOrOpponent(int countryId) {
      if (countryStateByCountry[countryId]==countryStateEnum.selected) return;

      var country = mapControl.Game.Map[countryId];
      countryStateByCountry[countryId] = 
        country.OwnerId==mapControl.GuiPlayer?.Id ? countryStateEnum.owner : countryStateEnum.opponent;
    }


    private void mapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
      if (mapControl.Game.HasGameFinished || mapControl.GuiPlayer is null) return;

      var point = e.GetPosition(this);
      if (!Coordinate.TryConvert(mapControl.PixelMap, (int)point.X, (int)point.Y, out var coordinate)) {
        return;
      }
      var countryId = mapControl.PixelMap[coordinate];
      if (countryId!=clickCountryId)
        return;//mouse was moved during double click to another country

      var country = mapControl.Game.Map[countryId];
      if (!country.IsMountain) {
        if (country.OwnerId==mapControl.GuiPlayer.Id) {
          //move command
          selectedCountries.Remove(country);
          if (selectedCountries.Count==0) {
            //no country is selected. Move the armies from all neighbour countries owned by the player to the
            //double clicked country
            foreach (var neighbourId in country.NeighbourIds) {
              var neighbour = mapControl.Game.Map[neighbourId];
              if (neighbour.OwnerId==mapControl.GuiPlayer.Id) {
                selectedCountries.Add(neighbour);
              }
            }
          }
          try {
            mapControl.ControllerMove(new Move(MoveTypeEnum.move, mapControl.GuiPlayer, country, selectedCountries));
          } catch (GameException ex) {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
          }

        } else {
          //attack command
          if (selectedCountries.Count==0) {
            foreach (var neighbourId in country.NeighbourIds) {
              var neighbour = mapControl.Game.Map[neighbourId];
              if (neighbour.OwnerId==mapControl.GuiPlayer?.Id) {
                selectedCountries.Add(neighbour);
              }
            }
            if (selectedCountries.Count>0) {
              try {
                mapControl.ControllerMove(new Move(MoveTypeEnum.attack, mapControl.GuiPlayer!, country, selectedCountries));
              } catch (GameException ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
              }
            }

          } else {
            if (country.CanBeAttacked(selectedCountries)) {
              try {
                mapControl.ControllerMove(new Move(MoveTypeEnum.attack, mapControl.GuiPlayer, country, selectedCountries));
              } catch (GameException ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
              }
            } else {
              mediaPlayer.Stop();
              mediaPlayer.Play();
            }
          }
        }
        countryStateByCountry[country.Id] = countryStateEnum.noState;
        foreach (var deselectCountry in selectedCountries) {
          countryStateByCountry[deselectCountry.Id] = countryStateEnum.noState;
        }
        selectedCountries.Clear();
        InvalidateVisual();
      }
    }
    #endregion


    #region Methods
    //      -------

    bool isFirstTime = true;


    internal void MapChanged() {
      countryStateByCountry = new countryStateEnum[mapControl.Game.Map.Count];
      foreach (var country in mapControl.Game.Map) {
        countryStateByCountry[country.Id] = countryStateEnum.noState;
      }

      if (isFirstTime) {
        isFirstTime = false;
        mapControl.MouseMove += mapControl_MouseMove;
        mapControl.MouseLeftButtonDown += mapControl_MouseLeftButtonDown;
        mapControl.MouseLeftButtonUp += mapControl_MouseLeftButtonUp;
        //mapControl.MouseRightButtonUp += mapControl_MouseRightButtonUp;
      }

      InvalidateVisual();
    }
    #endregion


    #region Graphic Overrides
    //      -----------------

    protected override Size MeasureContentOverride(Size constraint) {
      return constraint;
    }


    protected override Size ArrangeContentOverride(Rect arrangeRect) {
      return arrangeRect.Size;
    }


    enum countryStateEnum {
      noState,
      owner,
      opponent,
      victim,
      mouseHoverOwner,
      mouseHoverOpponent,
      selected,
    }
    countryStateEnum[] countryStateByCountry;

    readonly Pen ownerPen = new(new SolidColorBrush(Color.FromArgb(0xA0, 0x50, 0x50, 0x50)), 5);
    readonly Pen opponentPen = new(new SolidColorBrush(Color.FromArgb(0xA0, 0x70, 0x70, 0x70)), 5);
    readonly Pen hoverOwnerPen = new(new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)), 3);
    readonly Pen hoverOpponentPen = new(Brushes.Yellow, 3);
    readonly Pen selectedPen = new(new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)), 4);


    protected override void OnRenderContent(DrawingContext drawingContext, Size renderContentSize) {
      if (mapControl.PixelMap!=null) {
        //a map has been created

        //highlight first not selected borders
        foreach (var country in mapControl.Game.Map) {
          if (!country.IsMountain) {
            Brush countryBrush = Brushes.Transparent;
            switch (countryStateByCountry[country.Id]) {
            case countryStateEnum.noState:
              break;
            case countryStateEnum.owner:
              drawingContext.DrawGeometry(countryBrush, ownerPen, mapControl.GeometryByCountry[country.Id]);
              break;
            case countryStateEnum.opponent:
              drawingContext.DrawGeometry(countryBrush, opponentPen, mapControl.GeometryByCountry[country.Id]);
              break;
            case countryStateEnum.victim:
              throw new NotImplementedException();
            case countryStateEnum.mouseHoverOwner:
            case countryStateEnum.mouseHoverOpponent:
            case countryStateEnum.selected:
              break;
            default:
              throw new NotSupportedException();
            }
          }
        }

        //highlight selected countries after not selected countries, to paint over them
        foreach (var country in mapControl.Game.Map) {
          if (!country.IsMountain) {
            Brush countryBrush = Brushes.Transparent;
            switch (countryStateByCountry[country.Id]) {
            case countryStateEnum.noState:
            case countryStateEnum.owner:
            case countryStateEnum.opponent:
              break;
            case countryStateEnum.victim:
              throw new NotSupportedException();
            case countryStateEnum.mouseHoverOwner:
              drawingContext.DrawGeometry(countryBrush, hoverOwnerPen, mapControl.GeometryByCountry[country.Id]);
              break;
            case countryStateEnum.mouseHoverOpponent:
              drawingContext.DrawGeometry(countryBrush, hoverOpponentPen, mapControl.GeometryByCountry[country.Id]);
              break;
            case countryStateEnum.selected:
              drawingContext.DrawGeometry(countryBrush, selectedPen, mapControl.GeometryByCountry[country.Id]);
              break;
            default:
              throw new NotSupportedException();
            }
          }
        }
      }
    }
    #endregion
  }
}
