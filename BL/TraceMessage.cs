/********************************************************************************************************

MasterGrab.BL.TraceMessage
==========================

The data of a message in MasterGrab's tracing system.

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
  /// The TraceMessage stores the actual message, a string, together with some tracing information, like when it was created,
  /// the trace type (warning, exception, ...), and a string which can be used to filter certain messages
  /// </summary>
  public class TraceMessage {
    public readonly TraceTypeEnum TraceType;
    public readonly DateTime Created;
    public readonly string? Message;
    public readonly string? FilterText;

    private string? asString;


    public TraceMessage(TraceTypeEnum tracrType, string? message, string? filterText = null) {
      TraceType = tracrType;
      Created = DateTime.Now;
      Message = message;
      FilterText = filterText;
    }


    public override string ToString() {
      if (asString==null) {
        asString = TraceType.ShortString() + Created.ToString(" HH:mm:ss.fff ") + Message;
      }
      return asString;
    }
  }
}
