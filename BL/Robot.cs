/********************************************************************************************************

MasterGrab.BL.Robot
===================

A Robot is a computer based player in the game. 

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


namespace MasterGrab {


  /// <summary>
  /// A Robot is a computer based player in the game. It gets called by the GameController when it can make its next move.</para>
  /// 
  /// Inherit from Robot to make your own Robot, ideally in a different DLL. Overwrite DoPlanMove(). For convenience, some 
  /// variables are defined accessible through Player. After each time calling DoPlanMove(), they show completely
  /// new objects with the latest data. One cannot use Game or Country objects from a previous DoPlanMove() call in the 
  /// next. However, the Ids of the object stay the same. Also CountryFix objects do not change between calls.
  /// 
  /// Fresh copies of these objects are provide to each player to ensure that their information doesn't change when other
  /// Players make a move and to prevent cheating. The only way a Robot can influence the game is through the return value,
  /// a Move, of DoPlanMove().
  /// </summary>
  public class Robot {

    #region Properties
    //      ----------

    /// <summary>
    /// Player holds all the game information for this robot, i.e. Player.Id, link to Game and the country the Robot owns
    /// </summary>
    protected Player Player { get; private set; }

    /// <summary>
    /// Game holds all the game information, including links to all other Players.
    /// </summary>
    protected Game Game { get; private set; }

    /// <summary>
    /// Collection of all Countries
    /// </summary>
    protected Map Map { get; private set; }

    /// <summary>
    /// ContryIds of all Countries owned by the Robot. The country itself can be accessed through Map[countryId]
    /// </summary>
    protected IReadOnlyList<int> RobotCountryIds { get; private set; }

    /// <summary>
    /// Holds for each player the result of his last move
    /// </summary>
    protected IReadOnlyList<Result> Results { get; private set; }
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor
    /// </summary>
    //Game, Map, Player, RobotCountryIds, Results will not be null after PlanMove() is called
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. 
    public Robot() {
    #pragma warning restore CS8618
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Called by GameController, asking the Robot to make next Move. Overwrite DoPlanMove() to do so.
    /// </summary>
    public Move PlanMove(Player player) {
      Game = player.Game;
      Map = Game.Map;
      Player = player;
      RobotCountryIds = Player.CountryIds;
      Results = Game.Results;
      return DoPlanMove();
    }


    /// <summary>
    /// For inheritors to overrite. Use the information provided in the Robot variables to plan the next move. Notice that
    /// for each call of DoPlanMove(), a complete new set of objects are provided. Objects like Country from a previous
    /// call cannot be used in the new call. But Country.Id and CountryFix can be reused, they do not change between calls.
    /// </summary>
    protected virtual Move DoPlanMove() {
      return Move.NoMove;
    }
    #endregion
  }


  [AttributeUsage(AttributeTargets.Class)]
  public class RobotAttribute: Attribute {

    /// <summary>
    /// Name of Robot displayed to user
    /// </summary>
    public string Name { get; set; }


    /// <summary>
    /// Tooltip text of Robot displayed to user
    /// </summary>
    public string? Description { get; set; }


    /// <summary>
    /// Should robot be used for default options ?
    /// </summary>
    public bool IsUsedForDefault;


    public RobotAttribute(string name, string? description = null, bool isUsedForDefault = true) {
      Name = name;
      Description = description;
      IsUsedForDefault = isUsedForDefault;
    }
  }


  public record RobotInfo(int ID, Type Type, string Name, string? Description, bool IsUsedForDefault);
}
