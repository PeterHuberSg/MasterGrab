/********************************************************************************************************

MasterGrab.BL.OptionDef
=======================

OptionDef defines the values displayed to the user for one particular option.

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


namespace MasterGrab {

  /// <summary>
  /// OptionDef defines the values displayed to the user for one particular option.
  /// </summary>
  public class OptionDef {

    #region Properties
    //      ----------

    /// <summary>
    /// Name of the option
    /// </summary>
    public readonly string Name;


    /// <summary>
    /// Text explaining what the option is used for
    /// </summary>
    public readonly string ToolTip;


    /// <summary>
    /// Gives none generic access to OptionDef<>.DefaultValue
    /// </summary>
    public readonly string DefaultValueString;
    #endregion


    #region Constructor
    //      -----------

    public OptionDef(string name, string toolTip, string defaultValueString) {
      Name = name;
      ToolTip = toolTip;
      DefaultValueString = defaultValueString.Substring(0, Math.Min(defaultValueString.Length, 4));
    }

    #endregion
  }


  /// <summary>
  /// Defines for every options its meta data, like Name, Type, value range, etc.
  /// </summary>
  public class OptionDef<T>: OptionDef where T: IComparable<T> {


    #region Properties
    //      ----------

    /// <summary>
    /// Default value for option
    /// </summary>
    public readonly T DefaultValue;


    /// <summary>
    /// Smallest allowed value for option, if applicable
    /// </summary>
    public readonly T MinValue;


    /// <summary>
    /// Biggest allowed value for option, if applicable
    /// </summary>
    public readonly T MaxValue;


    /// <summary>
    /// Minimum value created by random generator
    /// </summary>
    public readonly T RandomOffset;


    /// <summary>
    /// (RandomOffset + RandomRange) is the biggest value the random generator can create
    /// </summary>
    public readonly T RandomRange;


    /// <summary>
    /// Value used to signal that this Option has no value assigned yet. For an object type this is null, otherwise something
    /// like int.MinValue.
    /// </summary>
    public readonly T NullValue;
    #endregion


    #region Constructor
    //      -----------

    /// <summary>
    /// Constructor
    /// </summary>
    public OptionDef(
      string name,
      string toolTip,
      T defaultValue,
      T minValue,
      T maxValue,
      T randomOffset,
      T randomRange,
      T nullValue): base(name, toolTip, defaultValue.ToString()!) 
    {
      DefaultValue = defaultValue;
      MinValue = minValue;
      MaxValue = maxValue;
      RandomOffset =randomOffset;
      RandomRange =randomRange;
      NullValue = nullValue;

      if (typeof(T)==typeof(int)) {
        var min = (int)(object)MinValue;
        var max = (int)(object)MaxValue;
        if (min>max) throw new ArgumentException($"MinValue {MinValue} should be smaller/equal MaxValue {MaxValue}.");

        var rOffset = (int)(object)RandomOffset;
        if (min>rOffset) throw new ArgumentException($"RandomOffset {RandomOffset} should be smaller/equal MinValue {MinValue}.");

        var rRange = (int)(object)RandomRange;
        if (rOffset+rRange>max) throw new ArgumentException($"rOffset+rRange {rOffset+rRange} should be smaller/equal MaxValue {MaxValue}.");

      } else if (typeof(T)==typeof(double)) {
        var min = (double)(object)MinValue;
        var max = (double)(object)MaxValue;
        if (min>max) throw new ArgumentException($"MinValue {MinValue} should be smaller/equal MaxValue {MaxValue}.");

        var rOffset = (double)(object)RandomOffset;
        if (min>rOffset) throw new ArgumentException($"RandomOffset {RandomOffset} should be smaller/equal MinValue {MinValue}.");

        var rRange = (double)(object)RandomRange;
        if (rOffset+rRange>max) throw new ArgumentException($"rOffset+rRange {rOffset+rRange} should be smaller/equal MaxValue {MaxValue}.");
      }
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// returns the meta data of an option in a ';' separated string
    /// </summary>
    public override string ToString() {
      return
      "Name: " + Name + "; " +
      "MinValue: " + MinValue + "; " +
      "MaxValue: " + MaxValue + "; " +
      "NullValue: " + NullValue + "; " +
      "ToolTip: " + ToolTip + "; ";
    }
    #endregion
  }
}
