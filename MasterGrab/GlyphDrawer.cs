/********************************************************************************************************

MasterGrab.MasterGrab.GlyphDrawer
=================================

Writes strings to a DrawingContext using one particular font.

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
using System.Windows;
using System.Windows.Media;


namespace MasterGrab {

  /// <summary>
  /// Draws glyphs to a DrawingContext. From the font information in the constructor, GlyphDrawer creates and stores the 
  /// GlyphTypeface, which is used every time for the drawing of the string.
  /// </summary>
  public class GlyphDrawer {

    #region Properties
    //      ----------

    /// <summary>
    /// Font specific properties used for writing with GlyphDrawer
    /// </summary>
    public GlyphTypeface GlyphTypeface => glyphTypeface;
    readonly GlyphTypeface glyphTypeface;


    /// <summary>
    /// Dots per inch to be used for writing
    /// </summary>
    public float PixelsPerDpi { get; set; }
    #endregion


    #region Constructor
    //      -----------

    readonly Typeface typeface;


    public GlyphDrawer(
      FontFamily fontFamily, 
      FontStyle fontStyle, 
      FontWeight fontWeight, 
      FontStretch fontStretch, 
      float pixelsPerDpi) 
    {
      typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
      if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
        throw new InvalidOperationException("No glyph typeface found");

      PixelsPerDpi = pixelsPerDpi;
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Writes a string to a DrawingContext, using the GlyphTypeface stored in the GlyphDrawer.
    /// </summary>
    /// <param name="drawingContext"></param>
    /// <param name="origin"></param>
    /// <param name="text"></param>
    /// <param name="size">same unit like FontSize: (em)</param>
    /// <param name="brush"></param>
    public void Write(DrawingContext drawingContext, Point origin, string text, double size, Brush brush) {
      if (text==null)
        return;

      var glyphIndexes = new ushort[text.Length];
      var advanceWidths = new double[text.Length];

      double totalWidth = 0;

      for (var charIndex = 0; charIndex<text.Length; charIndex++) {
        var glyphIndex = glyphTypeface.CharacterToGlyphMap[text[charIndex]];
        glyphIndexes[charIndex] = glyphIndex;

        var width = glyphTypeface.AdvanceWidths[glyphIndex] * size;
        advanceWidths[charIndex] = width;

        totalWidth += width;
      }

      var glyphRun = new GlyphRun(glyphTypeface, 0, false, size, PixelsPerDpi, glyphIndexes, origin, 
        advanceWidths, null, null, null, null, null, null);
      drawingContext.DrawGlyphRun(brush, glyphRun);
    }


    /// <summary>
    /// Writes a string to a DrawingContext, using the GlyphTypeface stored in the GlyphDrawer. The text will be right aligned. The
    /// last character will be at Origin, all other characters in front.
    /// </summary>
    /// <param name="drawingContext"></param>
    /// <param name="origin"></param>
    /// <param name="text"></param>
    /// <param name="size">same unit like FontSize: (em)</param>
    /// <param name="brush"></param>
    public void WriteRightAligned(DrawingContext drawingContext, Point origin, string text, double size, Brush brush) {

      var glyphIndexes = new ushort[text.Length];
      var advanceWidths = new double[text.Length];

      double totalWidth = 0;

      for (var charIndex = 0; charIndex<text.Length; charIndex++) {
        var glyphIndex = glyphTypeface.CharacterToGlyphMap[text[charIndex]];
        glyphIndexes[charIndex] = glyphIndex;

        var width = glyphTypeface.AdvanceWidths[glyphIndex] * size;
        advanceWidths[charIndex] = width;

        totalWidth += width;
      }

      var newOrigin = new Point(origin.X - totalWidth, origin.Y);
      var glyphRun = new GlyphRun(glyphTypeface, 0, false, size, PixelsPerDpi, glyphIndexes, newOrigin, 
        advanceWidths, null, null, null, null, null, null);
      drawingContext.DrawGlyphRun(brush, glyphRun);
    }


    /// <summary>
    /// Writes a string to a DrawingContext, using the GlyphTypeface stored in the GlyphDrawer. The text will be centered. The
    /// middle of text will be at Origin. The middle of the character heights will be on Origin.X
    /// </summary>
    public void WriteCentered(DrawingContext drawingContext, Point origin, string text, double size, Brush brush) {

      var glyphIndexes = new ushort[text.Length];
      var advanceWidths = new double[text.Length];

      double totalWidth = 0;

      for (var charIndex = 0; charIndex<text.Length; charIndex++) {
        var glyphIndex = glyphTypeface.CharacterToGlyphMap[text[charIndex]];
        glyphIndexes[charIndex] = glyphIndex;

        var width = glyphTypeface.AdvanceWidths[glyphIndex] * size;
        advanceWidths[charIndex] = width;

        totalWidth += width;
      }

      var newOrigin = new Point((int)(origin.X - totalWidth/2), (int)(origin.Y + glyphTypeface.AdvanceHeights[0]*12/2));
      var glyphRun = new GlyphRun(glyphTypeface, 0, false, size, PixelsPerDpi, glyphIndexes, newOrigin, 
        advanceWidths, null, null, null, null, null, null);
      drawingContext.DrawGlyphRun(brush, glyphRun);
    }


    /// <summary>
    /// Returns the length of the text using the GlyphTypeface stored in the GlyphDrawer. 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="size">same unit like FontSize: (em)</param>
    /// <returns></returns>
    public double GetLength(string text, double size) {
      double length = 0;

      for (var charIndex = 0; charIndex<text.Length; charIndex++) {
        var glyphIndex = glyphTypeface.CharacterToGlyphMap[text[charIndex]];
        var width = glyphTypeface.AdvanceWidths[glyphIndex] * size;
        length += width;
      }
      return length;
    }
    #endregion
  }
}
