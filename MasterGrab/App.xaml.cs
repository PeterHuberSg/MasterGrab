/********************************************************************************************************

MasterGrab.MasterGrab.App
=========================

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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;


namespace MasterGrab {


  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App: Application {

    public App() {
      Startup += App_Startup;  

    }

    private void App_Startup(object sender, StartupEventArgs e) {
      Window startupWindow;
      startupWindow = new MainWindow();
      //startupWindow = new HelpWindow();
      startupWindow.Show();
    }
  }
}
