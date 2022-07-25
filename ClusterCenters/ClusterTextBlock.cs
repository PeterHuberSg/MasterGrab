using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace ClusterCenters {


  public class ClusterTextBlock: TextBlock {


    public readonly Cluster Cluster;
    public readonly MainWindow mainWindow;
    readonly DispatcherTimer timer;


    public ClusterTextBlock(Cluster cluster, MainWindow mainWindow) {
      Cluster = cluster;
      this.mainWindow=mainWindow;
      PreviewMouseDown += ClusterTextBlock_MouseDown;
      MouseMove += ClusterTextBlock_MouseMove;
      MouseUp += ClusterTextBlock_MouseUp;
      timer = new DispatcherTimer();
      timer.Tick += Timer_Tick;
      timer.Interval = TimeSpan.FromMilliseconds(400);
    }


    Point mouseStartPoint;
    Point mousePoint;


    private void ClusterTextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      mouseStartPoint = e.GetPosition((IInputElement)Parent);
      Mouse.Capture(this);
    }


    private void ClusterTextBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
      if (e.LeftButton==MouseButtonState.Released) return;

      timer.Start();
      mousePoint = e.GetPosition((IInputElement)Parent);
    }


    private void Timer_Tick(object? sender, EventArgs e) {
      timer.Stop();
      moveClusterTextBlock();
    }


    private void ClusterTextBlock_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      Mouse.Capture(null);
      timer.Stop();
      mousePoint = e.GetPosition((IInputElement)Parent);
      moveClusterTextBlock();
    }


    private void moveClusterTextBlock() {
      var deltaX = mousePoint.X - mouseStartPoint.X;
      var deltaY = mousePoint.Y - mouseStartPoint.Y;
      if (deltaX==0 && deltaY==0) return;

      var (newX, newY) = mainWindow.MoveClusterCenter(Cluster.Id, deltaX, deltaY);
      Canvas.SetLeft(this, newX);
      Canvas.SetTop(this, newY);
      mouseStartPoint = mousePoint;
    }
  }
}
