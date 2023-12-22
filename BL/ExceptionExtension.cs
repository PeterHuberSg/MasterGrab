/********************************************************************************************************

MasterGrab.BL.ExceptionExtension
================================

ExceptionExtension.ToDetailString(): Lists all details of an exception into a string

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
using System.Text;


namespace MasterGrab {

  /// <summary>
  /// Class for extension methods of Exception
  /// </summary>
  public static class ExceptionExtension {

		#region Methods
		//      -------

		/// <summary>
		/// Lists all details of an exception type into a string
		/// </summary>
		public static string ToDetailString(this Exception thisException){
			var exceptionInfo = new StringBuilder();
			int startPos;
			int titleLength; 
			
			// Loop through all exceptions
			var currentException = thisException;	// Temp variable to hold InnerException object during the loop.
			var exceptionCount = 1;				// Count variable to track the number of exceptions in the chain.
			do {
				// exception type and message as title
				startPos = exceptionInfo.Length;
				exceptionInfo.Append(currentException.GetType().FullName);
				titleLength = exceptionInfo.Length - startPos;
				exceptionInfo.Append("\r\n");
				if (exceptionCount==1) {
				    //main exception
					exceptionInfo.Append('=', titleLength);
				} else {
					//inner exceptions
					exceptionInfo.Append('-', titleLength);
				}
				
				exceptionInfo.Append("\r\n" + currentException.Message);
				// List the remaining properties of all other exceptions
				var propertiesArray = currentException.GetType().GetProperties();
				foreach (var property in propertiesArray) {
					// skip message, inner exception and stack trace
					if (property.Name is not "InnerException" and not "StackTrace" and not "Message" and not "TargetSite") {
						if (property.GetValue(currentException, null) is null) {
							//skip empty properties
						} else {
							exceptionInfo.AppendFormat("\r\n" + property.Name + ": " + property.GetValue(currentException, null));
						}
					}
				}

				// record the StackTrace with separate label.
				if (currentException.StackTrace != null) {
					exceptionInfo.Append("\r\n" + currentException.StackTrace + "\r\n");
				}
				exceptionInfo.Append("\r\n");

				// continue with inner exception
				currentException = currentException.InnerException;
				exceptionCount++;
			} while (currentException!=null);
			
			return exceptionInfo.ToString();
		}
    #endregion

  }
}