/********************************************************************************************************

MasterGrab.BL.GameException
===========================

Allows easy filtering out of MasterGrab specific exceptions

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
  /// Allows easy filtering out of MasterGrab specific exceptions
  /// </summary>
  public class GameException: Exception {

    #region Properties
    //     -----------


    public readonly Game? Game;
    public readonly int CountryId;
    public readonly IReadOnlyList<int>? CountryIds;
    #endregion

    #region Constructor
    //      -----------

    /// <summary>
    /// default constructor, without data
    /// </summary>
    public GameException(): this(null, int.MinValue, null!) {
    }


    /// <summary>
    /// GameException with Game data and countries involved
    /// </summary>
    public GameException(Game? game, int countryId, IReadOnlyList<int>? countryIds): base() {
      Game = game;
      CountryId = countryId;
      CountryIds = countryIds;
    }


    /// <summary>
    /// GameException with Game data, countries involved and error message
    /// </summary>
    public GameException(Game game, int countryId, IReadOnlyList<int> countryIds, string message): base(message) {
      Game = game;
      CountryId = countryId;
      CountryIds = countryIds;
    }


    /// <summary>
    /// GameException with Game data, countries involved, error message and error message arguments
    /// </summary>
    public GameException(Game? game, int countryId, IReadOnlyList<int> countryIds, string message, 
      params object[] args): base(string.Format(message, args)) 
    {
      Game = game;
      CountryId = countryId;
      CountryIds = countryIds;
    }
    #endregion

  }
}
