/********************************************************************************************************

MasterGrab.BL.RingBuffer
========================

Ringbuffer stores the last x items added to it, the rest gets overwritten.

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
using System.Text;

namespace MasterGrab {


  /// <summary>
  /// Ringbuffer stores the last x items added to it, the rest gets overwritten.
  /// </summary>
  public class RingBuffer<T> where T : struct {
    readonly int size;
    readonly T[] buffer;
    int writePointer;
    bool isOverflow;


    public RingBuffer(int size) {
      this.size = size;
      buffer = new T[size];
      writePointer = 0;
      isOverflow = false;
    }


    public void Clear() {
      for (var bufferIndex = 0; bufferIndex < buffer.Length; bufferIndex++) {
        buffer[bufferIndex] = default;
      }
      writePointer = 0;
      isOverflow = false;
    }


    public void Add(T item) {
      buffer[writePointer++] = item;
      if (writePointer>=size) {
        writePointer = 0;
        isOverflow = true;
      }
    }


    /// <summary>
    /// Returns the last stored items T as string, each item on a line, the newest item first.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
      var stringBuilder = new StringBuilder();
      var readPointer = writePointer;
      do {
        var item = buffer[readPointer--];
        if (!EqualityComparer<T>.Default.Equals(item, default)) { //item!=default(T) will not compile
          stringBuilder.AppendLine(item.ToString());
        }
        if (readPointer<0) {
          if (isOverflow) {
            break;
          }
          readPointer = size - 1;
        }
      } while (readPointer!=writePointer);
      return stringBuilder.ToString();
    }

  }
}
