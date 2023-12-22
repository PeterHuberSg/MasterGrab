/********************************************************************************************************

MasterGrab.BL.Player
====================

MasterGrab is usually played by one human (GuiPlayer) and some robots (executed by computer), which are 
all called Player.

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

using System.Collections.Generic;


namespace MasterGrab {

  /// <summary>
  /// The game is usually played by one human (GuiPlayer) and some robots. The game stores for each a Player in 
  /// Game.players.
  /// </summary>
  public class Player {

    #region Properties
    //      ----------

    /// <summary>
    /// Unique identification number of player, starting from 0, which identifies the GuiPlayer.
    /// </summary>
    public readonly int Id;


    /// <summary>
    /// Player's name
    /// </summary>
    public readonly string Name;


    /// <summary>
    /// Game the Player is created for.
    /// </summary>
    public readonly Game Game;


    /// <summary>
    /// Countries owned by this player
    /// </summary>
    public IReadOnlyList<int> CountryIds => countryIds;
    readonly List<int> countryIds;
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor using id and name. The caller has to ensure that id is unique.
    /// </summary>
    public Player(int id, string name, Game game) {
      Id = id;
      Name = name;
      Game = game;

      countryIds = new List<int>();
    }


    /// <summary>
    /// Clone constructor, creating a copy of player, i.e. the clone and the original do not share any objects. The caller 
    /// must add the countries later, because if Player clones countries, they will not match with the countries cloned by 
    /// Map.
    /// </summary>
    public Player(Game game, Player player) : this(player.Id, player.Name, game) {
      foreach (var countryId in player.CountryIds) {
        countryIds.Add(countryId);
      }
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// add a country newly owned by this player
    /// </summary>
    internal void AddCountry(int countryId) {
      if (!countryIds.Contains(countryId)) {
        countryIds.Add(countryId);
      }
    }


    /// <summary>
    /// Remove Country from Player. He no longer owns it.
    /// </summary>
    internal void RemoveCountry(int countryId) {
      countryIds.Remove(countryId);
    }


    /// <summary>
    /// Return the Player data in a string
    /// </summary>
    public override string ToString() {
      var returnString = "Id: " + Id + "; Countries: ";
      foreach (var countryId in countryIds) {
        returnString += countryId + ", ";
      }
      return returnString;
    }
    #endregion
  }
}