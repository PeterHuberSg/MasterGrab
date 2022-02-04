/********************************************************************************************************

MasterGrab.Robots.SimpleRobot
=============================

Example of very simple Robot player for MasterGrab

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
  /// Simple Robot player using only 1 country to attack
  /// </summary>
  [Robot(name: "SimpleRobot", isUsedForDefault: false)]
  public class SimpleRobot: Robot {


    readonly List<Country> attackers;


    public SimpleRobot(): base() {
      attackers = new List<Country>();
    }


    protected override Move DoPlanMove() {
      Country? attacker = null;
      Country? target = null;
      double ratio = 0;
      //loop through all countries this robot owns
      //find the best neighbour to attack. SimpleRobot tries to optimise:
      //+ size of the neighbour country (the bigger the better)
      //+ how many defenders (the fewer the better)
      foreach (int countryId in RobotCountryIds) {
        var country = Map[countryId];
        double attackingArmies = country.ArmySize * Game.AttackFactor;
        //loop through every neighbour of this country
        foreach (int neighbourId in country.NeighbourIds) {
          var neighbour = Map[neighbourId];
          if (
            !neighbour.IsMountain && //cannot attack mountain
            neighbour.OwnerId!=Player.Id && //don't attack own country
            neighbour.ArmySize<attackingArmies) //is robot strong enough ?
          {
            //calculate how much stronger the attacker is then the defender
            double newRatio = attackingArmies / neighbour.ArmySize * neighbour.Size;
            if (ratio<newRatio) {
              //found a weaker country to attack
              ratio = newRatio;
              attacker = country;
              target = neighbour;
            }
          }
        }
      }
      Move move;
      if (attacker==null) {
        //if a Robot decides not to attack, he can select one country and move all armies
        //from countries he owns around that country into that country. See BasicRobot.cs 
        //for an example how this can be done
        move = Move.NoMove;
      } else {
        attackers.Clear();
        ////SimpleRobot is a bit stupid and uses only 1 of his countries to attack
        ////It would be better if he would use all of his countries which are neighbours of the attacked countries,
        ////like this:
        ////foreach (int targetNeighbourId in target.NeighbourIds) {
        ////  Country targetNeighbour = Map[targetNeighbourId];
        ////  if (targetNeighbour.OwnerId==Player.Id) {
        ////    attackers.Add(targetNeighbour);
        ////  }
        ////}
        attackers.Add(attacker);
        move = new Move(MoveTypeEnum.attack, Player, target!, attackers);
      }
      return move;
    }
  }
}
