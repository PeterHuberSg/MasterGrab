/********************************************************************************************************

MasterGrab.BL.GameController
============================

Controls when which player can execute a move. It runs on its own thread. Usually, the Gui thread 
constructs the GameController. 

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
using System.Threading;


namespace MasterGrab {


  /// <summary>
  /// Controls when which player can execute a move. It runs on its own thread. Usually, the Gui thread constructs the
  /// GameController. The constructor returns methods for the interaction between the class owning the GameController and 
  /// the Game. For details see constructor.</para>
  /// 
  /// GameController maps GuiPlayer and Robots to Players. GuiPlayer is player 0, the robots use 1 and higher.</para>
  /// 
  /// GameController -> GameFix -> Game -> Map -> Country
  /// </summary>
  public class GameController {


    #region Constructor
    //      -----------

    readonly Thread controllerThread;

    Options options;
    readonly Action<PixelMap, IReadOnlyList<CountryFix>, Game, Player?> mapChanged;
    readonly Action<Game, Player?> gameChanged;
    readonly Action<Exception> exceptionRaised;
    bool isReplay;
    bool isClosed;


    /// <summary>
    /// Constructor. The options define all parameters like how many players and countries. mapChanged, gameChanged, and
    /// exceptionRaised get called when those events occur, while move and close allow to control the game.
    /// The constructor returns quickly, since the Game gets constructed on a new controllerThread.
    /// </summary>
    //gameFix, executeReplay, executeMove, robots will be set in startNewGame()
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. 
    public GameController(
      Options options,
      Action<PixelMap, 
      IReadOnlyList<CountryFix>, Game, Player?> mapChanged, 
      Action<Game, Player?> gameChanged, 
      Action<Exception> exceptionRaised,
      out Action<Move> move, 
      out Action replay, 
      out Func<Options, Options> startNewGame, 
      out Action close) 
    {
    #pragma warning restore CS8618
      this.options = options;
      this.mapChanged = mapChanged;
      this.gameChanged = gameChanged;
      this.exceptionRaised = exceptionRaised;

      move = guiMove;
      replay = this.replay;
      startNewGame = this.startNewGame;
      close = this.close;

      isReplay = false;
      isClosed = false;
      controllerThread = new Thread(controllerMethod) {Name = "GameController"};
      controllerThread.Start();
    }
    #endregion


    #region Interface for class owner (GUI, human player)
    //      ---------------------------------------------

    Move? moveGui; //move that the GUI wants executed
    //signals the controllerThread that the Gui has changed some data
    readonly AutoResetEvent autoResetEvent = new(false); 


    private void guiMove(Move move) {
      //on WPF thread
      if (moveGui!=null) throw new Exception();
      //if GUI sends moves too quickly, moves get lost
      moveGui = move;
      autoResetEvent.Set();
    }


    private void replay() {
      //on WPF thread
      isReplay = true;
      autoResetEvent.Set();
    }


    private Options startNewGame(Options options) {
      //on WPF thread
#pragma warning disable IDE0045 // Convert to conditional expression
      //keep a local options copy, because startNewGameOptions might get set to null immediately because of autoResetEvent.Set()
      //when single stepping in debugger
      Options newOptions;
      if (options.IsRandomOptions) {
        newOptions = new Options(options);
      } else {
        newOptions = options;
      }
      startNewGameOptions = newOptions;
      #pragma warning restore IDE0045
      autoResetEvent.Set();
      return newOptions;
    }


    private void close() {
      //on WPF thread
      isClosed = true;
      autoResetEvent.Set();
    }
    #endregion


    #region GameController thread
    //      ---------------------

    Options? startNewGameOptions; //options used to create the game
    GameFix gameFix; //link to the Game
    Action executeReplay; //start the same game again
    Action<int/*playerId*/, Move> executeMove; //tell game which move to execute for which player
    Robot[] robots; //all robots
    bool isSkipWaiting;


    /// <summary>
    /// Main method executing on the controller thread. It creates a new game, then waits until the Gui sets an autoResetEvent 
    /// to then execute a Gui-Move, followed by one move for every robot and finally gameChanged() gets called. 
    /// The Gui can also command to restart a new game or to stop the thread. If an exception occurs, exceptionRaised() 
    /// gets called.
    /// </summary>
    private void controllerMethod() {
      startNewGame();

      do {
        try {
          //Thread.Sleep(500);
          if (isSkipWaiting) {
            isSkipWaiting = false;
          } else {
            autoResetEvent.WaitOne();
          }
          if (isClosed)
            return;

          if (isReplay) {
            isReplay = false;
            executeReplay();
            var gameCloned = gameFix.GetClonedGame();
            var guiPlayer = options.IsHumanPlaying ? gameCloned.Players[GameFix.GuiPlayerId] : null;
            gameChanged(gameCloned, guiPlayer);

          } else if (startNewGameOptions!=null) {
            options = startNewGameOptions;
            startNewGameOptions = null;
            startNewGame();

          } else {
            int humanPlayerOffset;
            if (options.IsHumanPlaying) {
              if (moveGui==null) throw new Exception();
              executeMove(GameFix.GuiPlayerId, moveGui);
              humanPlayerOffset = 1;

            } else {
              humanPlayerOffset = 0;
            }
            moveGui = null;

            for (var robotIndex = 0; robotIndex < robots.Length; robotIndex++) {
              try {
                var robot = robots[robotIndex];
                var gameRobot = gameFix.GetClonedGame();
                var move = robot.PlanMove(gameRobot.Players[robotIndex + humanPlayerOffset]);
                executeMove(robotIndex + humanPlayerOffset, move);
              } catch (Exception ex) {
                exceptionRaised(ex);
              }
            }

            var gameCloned = gameFix.GetClonedGame();
            var guiPlayer = options.IsHumanPlaying ? gameCloned.Players[GameFix.GuiPlayerId] : null;
            gameChanged(gameCloned, guiPlayer);
          }

        } catch (Exception ex) {
          exceptionRaised(ex);
        }
      } while (!isClosed);
    }


    //creates a new game and robots
    private void startNewGame() {
      robots = new Robot[options.Robots.Count];
      for (var robotIndex = 0; robotIndex < robots.Length; robotIndex++) {
        var constructorInfos = options.Robots[robotIndex].Type.GetConstructors();
        var constructorInfo = constructorInfos[0];
        robots[robotIndex] = (Robot)constructorInfo.Invoke(null);
      }

      try {
        gameFix = new GameFix(options, out executeReplay, out executeMove);
        //pass a copy of game to the GUI. Game values will change, but they should get reported to the Gui only in the next 
        //gameChanged event.
        var gameCloned = gameFix.GetClonedGame();
        var guiPlayer = options.IsHumanPlaying ? gameCloned.Players[GameFix.GuiPlayerId] : null;
        mapChanged(gameFix.PixelMap, gameFix.CountryFixArray, gameCloned, guiPlayer);

      } catch (Exception ex) {
        if (ex.Message.Contains("too many border points")) {
          //just create a new map. This is the easiest solution, since this exception occurs very seldom
          //Todo: Solve problem with too many border points
          //Tracer.TraceWarning("Exception occurred: " + ex.ToDetailString());
          startNewGameOptions = options;
          isSkipWaiting = true;
        } else {
          exceptionRaised(ex);
        }
      }
    }
    #endregion
  }
}
