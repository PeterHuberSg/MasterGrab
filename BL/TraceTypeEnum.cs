/********************************************************************************************************

MasterGrab.BL.TraceTypeEnumExtension
====================================

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

namespace MasterGrab {


  public enum TraceTypeEnum {
    undef = 0,
    Trace,
    Warning,
    Error,
    Exception
  }


  public static class TraceTypeEnumExtension {
    public static string ShortString(this TraceTypeEnum tracerSource) {
      return tracerSource switch {
        TraceTypeEnum.Trace => "Trc",
        TraceTypeEnum.Warning => "War",
        TraceTypeEnum.Error => "Err",
        TraceTypeEnum.Exception => "Exc",
        TraceTypeEnum.undef => tracerSource.ToString(),
        _ => tracerSource.ToString(),
      };
    }
  }
}