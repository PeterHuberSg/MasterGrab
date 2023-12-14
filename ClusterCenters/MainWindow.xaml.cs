using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ClusterCenters {


  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window {


    readonly Cluster[] clusters = new Cluster[16];

    readonly (string Name, PercentPoint[] Points)[] clustersConfiguration = {
      ("2 clusters horizontal", new []{
      new PercentPoint(1/2.0, 1/2.0),
      new PercentPoint(1/2.0, 0/2.0)}),

      ("2 clusters vertical", new []{
      new PercentPoint(1/2.0, 1/2.0),
      new PercentPoint(0/2.0, 1/2.0)}),

      ("2 clusters compact, same as vertical", new []{
      new PercentPoint(1.0/2, 1/2.0),
      new PercentPoint(0/2.0, 1/2.0)}),


      ("3 clusters horizontal", new []{
      new PercentPoint(3/6.0, 3/6.0),
      new PercentPoint(3/6.0, 5/6.0),
      new PercentPoint(3/6.0, 1/6.0)}),

      ("3 clusters vertical", new []{
      new PercentPoint(3/6.0, 3/6.0),
      new PercentPoint(5/6.0, 3/6.0),
      new PercentPoint(1/6.0, 3/6.0)}),

      ("3 clusters diagonal", new []{
      new PercentPoint(3/6.0, 3/6.0),
      new PercentPoint(1/6.0, 1/6.0),
      new PercentPoint(5/6.0, 5/6.0)}),


     ("4 clusters horizontal", new []{
      new PercentPoint(2/4.0, 2/4.0),
      new PercentPoint(2/4.0, 3/4.0),
      new PercentPoint(2/4.0, 0/4.0),
      new PercentPoint(2/4.0, 1/4.0)}),

     ("4 clusters vertical", new []{
      new PercentPoint(2/4.0, 2/4.0),
      new PercentPoint(3/4.0, 2/4.0),
      new PercentPoint(0/4.0, 2/4.0),
      new PercentPoint(1/4.0, 2/4.0)}),

     ("4 clusters diagonal", new []{
      new PercentPoint(2/4.0, 2/4.0),
      new PercentPoint(3/4.0, 3/4.0),
      new PercentPoint(0/4.0, 0/4.0),
      new PercentPoint(1/4.0, 1/4.0)}),

     ("4 clusters compact", new []{
      new PercentPoint(2/4.0, 2/4.0),
      new PercentPoint(0/4.0, 3/4.0),
      new PercentPoint(1/4.0, 0/4.0),
      new PercentPoint(3/4.0, 1/4.0)}),


      ("5 clusters horizontal", new []{
      new PercentPoint(5/10.0, 5/10.0),
      new PercentPoint(5/10.0, 7/10.0),
      new PercentPoint(5/10.0, 9/10.0),
      new PercentPoint(5/10.0, 1/10.0),
      new PercentPoint(5/10.0, 3/10.0)}),

      ("5 clusters vertical", new []{
      new PercentPoint(5/10.0, 5/10.0),
      new PercentPoint(7/10.0, 5/10.0),
      new PercentPoint(9/10.0, 5/10.0),
      new PercentPoint(1/10.0, 5/10.0),
      new PercentPoint(3/10.0, 5/10.0)}),

      ("5 clusters diagonal", new []{
      new PercentPoint(5/10.0, 5/10.0),
      new PercentPoint(7/10.0, 7/10.0),
      new PercentPoint(9/10.0, 9/10.0),
      new PercentPoint(1/10.0, 1/10.0),
      new PercentPoint(3/10.0, 3/10.0)}),

      ("5 clusters compact", new []{
      new PercentPoint(5/10.0, 5/10.0),
      new PercentPoint(7/10.0, 9/10.0),
      new PercentPoint(9/10.0, 3/10.0),
      new PercentPoint(1/10.0, 7/10.0),
      new PercentPoint(3/10.0, 1/10.0)}),


       ("6 clusters horizontal", new []{
      new PercentPoint(3/6.0, 3/6.0),
      new PercentPoint(3/6.0, 4/6.0),
      new PercentPoint(3/6.0, 5/6.0),
      new PercentPoint(3/6.0, 0/6.0),
      new PercentPoint(3/6.0, 1/6.0),
      new PercentPoint(3/6.0, 2/6.0)}),

       ("6 clusters vertical", new []{
      new PercentPoint(3/6.0, 3/6.0),
      new PercentPoint(4/6.0, 3/6.0),
      new PercentPoint(5/6.0, 3/6.0),
      new PercentPoint(0/6.0, 3/6.0),
      new PercentPoint(1/6.0, 3/6.0),
      new PercentPoint(2/6.0, 3/6.0)}),

       ("6 clusters diagonal old", new []{
      new PercentPoint(3/6.0, 3/6.0),
      new PercentPoint(4/6.0, 4/6.0),
      new PercentPoint(5/6.0, 5/6.0),
      new PercentPoint(0/6.0, 0/6.0),
      new PercentPoint(1/6.0, 1/6.0),
      new PercentPoint(2/6.0, 2/6.0)}),

       ("6 clusters diagonal", new []{
      new PercentPoint(3/6.0, 3/6.0),
      new PercentPoint(4/6.0, 5/6.0),
      new PercentPoint(5/6.0, 1/6.0),
      new PercentPoint(0/6.0, 3/6.0),
      new PercentPoint(1/6.0, 5/6.0),
      new PercentPoint(2/6.0, 1/6.0)}),

       ("6 clusters compact", new []{
      new PercentPoint(3/6.0, 3/6.0),
      new PercentPoint(4/6.0, 4/6.0),
      new PercentPoint(5/6.0, 1/6.0),
      new PercentPoint(0/6.0, 2/6.0),
      new PercentPoint(1/6.0, 5/6.0),
      new PercentPoint(2/6.0, 0/6.0)}),


     ("7 clusters horizontal", new []{
      new PercentPoint(7/14.0, 07/14.0),
      new PercentPoint(7/14.0, 09/14.0),
      new PercentPoint(7/14.0, 11/14.0),
      new PercentPoint(7/14.0, 13/14.0),
      new PercentPoint(7/14.0, 01/14.0),
      new PercentPoint(7/14.0, 03/14.0),
      new PercentPoint(7/14.0, 05/14.0)}),

     ("7 clusters vertical", new []{
      new PercentPoint(07/14.0, 7/14.0),
      new PercentPoint(09/14.0, 7/14.0),
      new PercentPoint(11/14.0, 7/14.0),
      new PercentPoint(13/14.0, 7/14.0),
      new PercentPoint(01/14.0, 7/14.0),
      new PercentPoint(03/14.0, 7/14.0),
      new PercentPoint(05/14.0, 7/14.0)}),

     ("7 clusters diagonal", new []{
      new PercentPoint(07/14.0, 07/14.0),
      new PercentPoint(09/14.0, 09/14.0),
      new PercentPoint(11/14.0, 11/14.0),
      new PercentPoint(13/14.0, 13/14.0),
      new PercentPoint(01/14.0, 01/14.0),
      new PercentPoint(03/14.0, 03/14.0),
      new PercentPoint(05/14.0, 05/14.0)}),

     ("7 clusters compact", new []{
      new PercentPoint(07/14.0, 07/14.0),
      new PercentPoint(09/14.0, 13/14.0),
      new PercentPoint(11/14.0, 05/14.0),
      new PercentPoint(13/14.0, 11/14.0),
      new PercentPoint(01/14.0, 03/14.0),
      new PercentPoint(03/14.0, 09/14.0),
      new PercentPoint(05/14.0, 01/14.0)}),


     ("8 clusters horizontal", new []{
      new PercentPoint(4/8.0, 4/8.0),
      new PercentPoint(4/8.0, 5/8.0),
      new PercentPoint(4/8.0, 6/8.0),
      new PercentPoint(4/8.0, 7/8.0),
      new PercentPoint(4/8.0, 0/8.0),
      new PercentPoint(4/8.0, 1/8.0),
      new PercentPoint(4/8.0, 2/8.0),
      new PercentPoint(4/8.0, 3/8.0)}),

     ("8 clusters vertical", new []{
      new PercentPoint(4/8.0, 4/8.0),
      new PercentPoint(5/8.0, 4/8.0),
      new PercentPoint(6/8.0, 4/8.0),
      new PercentPoint(7/8.0, 4/8.0),
      new PercentPoint(0/8.0, 4/8.0),
      new PercentPoint(1/8.0, 4/8.0),
      new PercentPoint(2/8.0, 4/8.0),
      new PercentPoint(3/8.0, 4/8.0)}),

     ("8 clusters diagonal", new []{
      new PercentPoint(4/8.0, 4/8.0),
      new PercentPoint(5/8.0, 5/8.0),
      new PercentPoint(6/8.0, 6/8.0),
      new PercentPoint(7/8.0, 7/8.0),
      new PercentPoint(0/8.0, 0/8.0),
      new PercentPoint(1/8.0, 1/8.0),
      new PercentPoint(2/8.0, 2/8.0),
      new PercentPoint(3/8.0, 3/8.0)}),

      ("8 clusters compact", new []{
      new PercentPoint(4/8.0, 4/8.0),
      new PercentPoint(5/8.0, 7/8.0),
      new PercentPoint(6/8.0, 2/8.0),
      new PercentPoint(7/8.0, 5/8.0),
      new PercentPoint(0/8.0, 0/8.0),
      new PercentPoint(1/8.0, 3/8.0),
      new PercentPoint(2/8.0, 6/8.0),
      new PercentPoint(3/8.0, 1/8.0)}),


      ("9 clusters horizontal", new []{
      new PercentPoint(9/18.0, 09/18.0),
      new PercentPoint(9/18.0, 11/18.0),
      new PercentPoint(9/18.0, 13/18.0),
      new PercentPoint(9/18.0, 15/18.0),
      new PercentPoint(9/18.0, 17/18.0),      
      new PercentPoint(9/18.0, 01/18.0),
      new PercentPoint(9/18.0, 03/18.0),
      new PercentPoint(9/18.0, 05/18.0),
      new PercentPoint(9/18.0, 07/18.0)}),

      ("9 clusters vertical", new []{
      new PercentPoint(09/18.0, 9/18.0),
      new PercentPoint(11/18.0, 9/18.0),
      new PercentPoint(13/18.0, 9/18.0),
      new PercentPoint(15/18.0, 9/18.0),
      new PercentPoint(17/18.0, 9/18.0),
      new PercentPoint(01/18.0, 9/18.0),
      new PercentPoint(03/18.0, 9/18.0),
      new PercentPoint(05/18.0, 9/18.0),
      new PercentPoint(07/18.0, 9/18.0)}),

      //("9 clusters 0 Deviation: 0", new []{
      //new PercentPoint(0/18.0, 0/18.0),
      //new PercentPoint(1/18.0, 1/18.0),
      //new PercentPoint(2/18.0, 2/18.0),
      //new PercentPoint(3/18.0, 3/18.0),
      //new PercentPoint(4/18.0, 4/18.0),
      //new PercentPoint(5/18.0, 5/18.0),
      //new PercentPoint(6/18.0, 6/18.0),
      //new PercentPoint(7/18.0, 7/18.0),
      //new PercentPoint(8/18.0, 8/18.0)}),

      ("9 clusters diagonal", new []{
      new PercentPoint(09/18.0, 09/18.0),
      new PercentPoint(11/18.0, 13/18.0),
      new PercentPoint(13/18.0, 17/18.0),
      new PercentPoint(15/18.0, 03/18.0),
      new PercentPoint(17/18.0, 07/18.0),
      new PercentPoint(01/18.0, 11/18.0),
      new PercentPoint(03/18.0, 15/18.0),
      new PercentPoint(05/18.0, 01/18.0),
      new PercentPoint(07/18.0, 05/18.0)}),

      ("9 clusters compact", new []{
      new PercentPoint(09/18.0, 09/18.0),
      new PercentPoint(11/18.0, 17/18.0),
      new PercentPoint(13/18.0, 07/18.0),
      new PercentPoint(15/18.0, 15/18.0),
      new PercentPoint(17/18.0, 05/18.0),
      new PercentPoint(01/18.0, 13/18.0),
      new PercentPoint(03/18.0, 03/18.0),
      new PercentPoint(05/18.0, 11/18.0),
      new PercentPoint(07/18.0, 01/18.0)}),


            ("10 clusters 0 Deviation: 0", new []{
      new PercentPoint(0/10.0, 0/10.0),
      new PercentPoint(1/10.0, 1/10.0),
      new PercentPoint(2/10.0, 4/10.0),
      new PercentPoint(3/10.0, 5/10.0),
      new PercentPoint(4/10.0, 8/10.0),
      new PercentPoint(5/10.0, 9/10.0),
      new PercentPoint(6/10.0, 2/10.0),
      new PercentPoint(7/10.0, 3/10.0),
      new PercentPoint(8/10.0, 6/10.0),
      new PercentPoint(9/10.0, 7/10.0)}),

      ("10 clusters 1 Deviation: 0", new []{
      new PercentPoint(0/10.0, 0/10.0),
      new PercentPoint(1/10.0, 1/10.0),
      new PercentPoint(2/10.0, 6/10.0),
      new PercentPoint(3/10.0, 7/10.0),
      new PercentPoint(4/10.0, 2/10.0),
      new PercentPoint(5/10.0, 3/10.0),
      new PercentPoint(6/10.0, 8/10.0),
      new PercentPoint(7/10.0, 9/10.0),
      new PercentPoint(8/10.0, 4/10.0),
      new PercentPoint(9/10.0, 5/10.0)}),

      ("10 clusters 2 Deviation: 0", new []{
      new PercentPoint(0/10.0, 0/10.0),
      new PercentPoint(1/10.0, 3/10.0),
      new PercentPoint(2/10.0, 4/10.0),
      new PercentPoint(3/10.0, 7/10.0),
      new PercentPoint(4/10.0, 8/10.0),
      new PercentPoint(5/10.0, 1/10.0),
      new PercentPoint(6/10.0, 2/10.0),
      new PercentPoint(7/10.0, 5/10.0),
      new PercentPoint(8/10.0, 6/10.0),
      new PercentPoint(9/10.0, 9/10.0)}),

      ("10 clusters 3 Deviation: 0", new []{
      new PercentPoint(0/10.0, 0/10.0),
      new PercentPoint(1/10.0, 5/10.0),
      new PercentPoint(2/10.0, 4/10.0),
      new PercentPoint(3/10.0, 9/10.0),
      new PercentPoint(4/10.0, 8/10.0),
      new PercentPoint(5/10.0, 3/10.0),
      new PercentPoint(6/10.0, 2/10.0),
      new PercentPoint(7/10.0, 7/10.0),
      new PercentPoint(8/10.0, 6/10.0),
      new PercentPoint(9/10.0, 1/10.0)}),

      ("10 clusters 4 Deviation: 0", new []{
      new PercentPoint(0/10.0, 0/10.0),
      new PercentPoint(1/10.0, 5/10.0),
      new PercentPoint(2/10.0, 6/10.0),
      new PercentPoint(3/10.0, 1/10.0),
      new PercentPoint(4/10.0, 2/10.0),
      new PercentPoint(5/10.0, 7/10.0),
      new PercentPoint(6/10.0, 8/10.0),
      new PercentPoint(7/10.0, 3/10.0),
      new PercentPoint(8/10.0, 4/10.0),
      new PercentPoint(9/10.0, 9/10.0)}),

      ("10 clusters 5 Deviation: 0", new []{
      new PercentPoint(0/10.0, 0/10.0),
      new PercentPoint(1/10.0, 7/10.0),
      new PercentPoint(2/10.0, 6/10.0),
      new PercentPoint(3/10.0, 3/10.0),
      new PercentPoint(4/10.0, 2/10.0),
      new PercentPoint(5/10.0, 9/10.0),
      new PercentPoint(6/10.0, 8/10.0),
      new PercentPoint(7/10.0, 5/10.0),
      new PercentPoint(8/10.0, 4/10.0),
      new PercentPoint(9/10.0, 1/10.0)}),

      ("10 clusters 6 Deviation: 0", new []{
      new PercentPoint(0/10.0, 0/10.0),
      new PercentPoint(1/10.0, 9/10.0),
      new PercentPoint(2/10.0, 4/10.0),
      new PercentPoint(3/10.0, 3/10.0),
      new PercentPoint(4/10.0, 8/10.0),
      new PercentPoint(5/10.0, 7/10.0),
      new PercentPoint(6/10.0, 2/10.0),
      new PercentPoint(7/10.0, 1/10.0),
      new PercentPoint(8/10.0, 6/10.0),
      new PercentPoint(9/10.0, 5/10.0)}),

      ("10 clusters 7 Deviation: 0", new []{
      new PercentPoint(0/10.0, 0/10.0),
      new PercentPoint(1/10.0, 9/10.0),
      new PercentPoint(2/10.0, 6/10.0),
      new PercentPoint(3/10.0, 5/10.0),
      new PercentPoint(4/10.0, 2/10.0),
      new PercentPoint(5/10.0, 1/10.0),
      new PercentPoint(6/10.0, 8/10.0),
      new PercentPoint(7/10.0, 7/10.0),
      new PercentPoint(8/10.0, 4/10.0),
      new PercentPoint(9/10.0, 3/10.0)}),


      ("11 clusters 0 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 1/11.0),
      new PercentPoint(2/11.0, 2/11.0),
      new PercentPoint(3/11.0, 3/11.0),
      new PercentPoint(4/11.0, 4/11.0),
      new PercentPoint(5/11.0, 5/11.0),
      new PercentPoint(6/11.0, 6/11.0),
      new PercentPoint(7/11.0, 7/11.0),
      new PercentPoint(8/11.0, 8/11.0),
      new PercentPoint(9/11.0, 9/11.0),
      new PercentPoint(10/11.0, 10/11.0)}),

      ("11 clusters 1 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 2/11.0),
      new PercentPoint(2/11.0, 4/11.0),
      new PercentPoint(3/11.0, 6/11.0),
      new PercentPoint(4/11.0, 8/11.0),
      new PercentPoint(5/11.0, 10/11.0),
      new PercentPoint(6/11.0, 1/11.0),
      new PercentPoint(7/11.0, 3/11.0),
      new PercentPoint(8/11.0, 5/11.0),
      new PercentPoint(9/11.0, 7/11.0),
      new PercentPoint(10/11.0, 9/11.0)}),

      ("11 clusters 2 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 3/11.0),
      new PercentPoint(2/11.0, 6/11.0),
      new PercentPoint(3/11.0, 9/11.0),
      new PercentPoint(4/11.0, 1/11.0),
      new PercentPoint(5/11.0, 4/11.0),
      new PercentPoint(6/11.0, 7/11.0),
      new PercentPoint(7/11.0, 10/11.0),
      new PercentPoint(8/11.0, 2/11.0),
      new PercentPoint(9/11.0, 5/11.0),
      new PercentPoint(10/11.0, 8/11.0)}),

      ("11 clusters 3 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 4/11.0),
      new PercentPoint(2/11.0, 8/11.0),
      new PercentPoint(3/11.0, 1/11.0),
      new PercentPoint(4/11.0, 5/11.0),
      new PercentPoint(5/11.0, 9/11.0),
      new PercentPoint(6/11.0, 2/11.0),
      new PercentPoint(7/11.0, 6/11.0),
      new PercentPoint(8/11.0, 10/11.0),
      new PercentPoint(9/11.0, 3/11.0),
      new PercentPoint(10/11.0, 7/11.0)}),

      ("11 clusters 4 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 5/11.0),
      new PercentPoint(2/11.0, 10/11.0),
      new PercentPoint(3/11.0, 4/11.0),
      new PercentPoint(4/11.0, 9/11.0),
      new PercentPoint(5/11.0, 3/11.0),
      new PercentPoint(6/11.0, 8/11.0),
      new PercentPoint(7/11.0, 2/11.0),
      new PercentPoint(8/11.0, 7/11.0),
      new PercentPoint(9/11.0, 1/11.0),
      new PercentPoint(10/11.0, 6/11.0)}),

      ("11 clusters 5 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 6/11.0),
      new PercentPoint(2/11.0, 1/11.0),
      new PercentPoint(3/11.0, 7/11.0),
      new PercentPoint(4/11.0, 2/11.0),
      new PercentPoint(5/11.0, 8/11.0),
      new PercentPoint(6/11.0, 3/11.0),
      new PercentPoint(7/11.0, 9/11.0),
      new PercentPoint(8/11.0, 4/11.0),
      new PercentPoint(9/11.0, 10/11.0),
      new PercentPoint(10/11.0, 5/11.0)}),

      ("11 clusters 6 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 7/11.0),
      new PercentPoint(2/11.0, 3/11.0),
      new PercentPoint(3/11.0, 10/11.0),
      new PercentPoint(4/11.0, 6/11.0),
      new PercentPoint(5/11.0, 2/11.0),
      new PercentPoint(6/11.0, 9/11.0),
      new PercentPoint(7/11.0, 5/11.0),
      new PercentPoint(8/11.0, 1/11.0),
      new PercentPoint(9/11.0, 8/11.0),
      new PercentPoint(10/11.0, 4/11.0)}),

      ("11 clusters 7 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 8/11.0),
      new PercentPoint(2/11.0, 5/11.0),
      new PercentPoint(3/11.0, 2/11.0),
      new PercentPoint(4/11.0, 10/11.0),
      new PercentPoint(5/11.0, 7/11.0),
      new PercentPoint(6/11.0, 4/11.0),
      new PercentPoint(7/11.0, 1/11.0),
      new PercentPoint(8/11.0, 9/11.0),
      new PercentPoint(9/11.0, 6/11.0),
      new PercentPoint(10/11.0, 3/11.0)}),

      ("11 clusters 8 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 9/11.0),
      new PercentPoint(2/11.0, 7/11.0),
      new PercentPoint(3/11.0, 5/11.0),
      new PercentPoint(4/11.0, 3/11.0),
      new PercentPoint(5/11.0, 1/11.0),
      new PercentPoint(6/11.0, 10/11.0),
      new PercentPoint(7/11.0, 8/11.0),
      new PercentPoint(8/11.0, 6/11.0),
      new PercentPoint(9/11.0, 4/11.0),
      new PercentPoint(10/11.0, 2/11.0)}),

      ("11 clusters 9 Deviation: 0", new []{
      new PercentPoint(0/11.0, 0/11.0),
      new PercentPoint(1/11.0, 10/11.0),
      new PercentPoint(2/11.0, 9/11.0),
      new PercentPoint(3/11.0, 8/11.0),
      new PercentPoint(4/11.0, 7/11.0),
      new PercentPoint(5/11.0, 6/11.0),
      new PercentPoint(6/11.0, 5/11.0),
      new PercentPoint(7/11.0, 4/11.0),
      new PercentPoint(8/11.0, 3/11.0),
      new PercentPoint(9/11.0, 2/11.0),
      new PercentPoint(10/11.0, 1/11.0)}),
    };



    public MainWindow() {
      InitializeComponent();

      for (int clusterIndex=0; clusterIndex<clusters.Length; clusterIndex++) {
        clusters[clusterIndex] = new Cluster(clusterIndex, this, MainCanvas);
      }
      setupClusters(clustersConfiguration.Length-1);
      foreach (var clusterConfiguration in clustersConfiguration) {
        ConfigCombobox.Items.Add(new ComboBoxItem() { Content = clusterConfiguration.Name });
      }

      MainCanvas.SizeChanged += MainCanvas_SizeChanged;
      ConfigCombobox.SelectionChanged += ConfigCombobox_SelectionChanged;
      NextButton.Click += NextButton_Click;
    }


    int clustersUsedCount = int.MinValue;


    private void setupClusters(int clustersConfigurationIndex) {
      var clusterConfiguration = clustersConfiguration[clustersConfigurationIndex];
      ConfigCombobox.SelectedIndex = clustersConfigurationIndex;
      clustersUsedCount = clusterConfiguration.Points.Length;

      ClusterGrid.Children.Clear();
      ClusterGrid.ColumnDefinitions.Clear();
      int clusterIndex = 0;
      for (; clusterIndex < clustersUsedCount; clusterIndex++) {
        var cluster = clusters[clusterIndex];
        cluster.PercentCenter = clusterConfiguration.Points[clusterIndex];
        cluster.ClusterTextBlock.Visibility = Visibility.Visible;

        if (clusterIndex>0) {
          ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
        }
        ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition());
        ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition{ Width = GridLength.Auto });
        ClusterGrid.ColumnDefinitions.Add(new ColumnDefinition());

        FrameworkElement control;

        var columnId = 5*clusterIndex;
        if (clusterIndex%2==1) {
          control = new Rectangle { Fill=Brushes.DarkGray };
          Grid.SetColumn(control, columnId);
          Grid.SetRowSpan(control, 4);
          Grid.SetColumnSpan(control, 4);
          ClusterGrid.Children.Add(control);
        }


        //row 0: cluster number
        control = new TextBlock(new Run(clusterIndex.ToString()) {FontWeight = FontWeights.Bold});
        Grid.SetColumn(control, columnId);
        ClusterGrid.Children.Add(control);

        //row 1: pixel coordinates integer
        control = new TextBlock(new Run("X:") { FontWeight = FontWeights.Bold });
        Grid.SetRow(control, 1);
        Grid.SetColumn(control, columnId);
        ClusterGrid.Children.Add(control);

        var textBox = new TextBox { Tag = cluster};
        textBox.PreviewLostKeyboardFocus += IntCenterXTextBox_PreviewLostKeyboardFocus;
        Grid.SetRow(textBox, 1);
        Grid.SetColumn(textBox, columnId + 1);
        ClusterGrid.Children.Add(textBox);
        cluster.IntCenterXTextBox = textBox;

        control = new TextBlock(new Run("Y:") { FontWeight = FontWeights.Bold });
        Grid.SetRow(control, 1);
        Grid.SetColumn(control, columnId + 2);
        ClusterGrid.Children.Add(control);

        textBox = new TextBox { Tag = cluster };
        textBox.PreviewLostKeyboardFocus += IntCenterYTextBox_PreviewLostKeyboardFocus;
        Grid.SetRow(textBox, 1);
        Grid.SetColumn(textBox, columnId + 3);
        ClusterGrid.Children.Add(textBox);
        cluster.IntCenterYTextBox = textBox;

        //row 2: pixel coordinates percentage
        control = new TextBlock(new Run("X:") { FontWeight = FontWeights.Bold });
        Grid.SetRow(control, 2);
        Grid.SetColumn(control, columnId);
        ClusterGrid.Children.Add(control);

        textBox = new TextBox { Tag = cluster };
        textBox.PreviewLostKeyboardFocus += PercentCenterXTextBox_PreviewLostKeyboardFocus;
        Grid.SetRow(textBox, 2);
        Grid.SetColumn(textBox, columnId + 1);
        ClusterGrid.Children.Add(textBox);
        cluster.PercentCenterXTextBox = textBox;

        control = new TextBlock(new Run("Y:") { FontWeight = FontWeights.Bold });
        Grid.SetRow(control, 2);
        Grid.SetColumn(control, columnId + 2);
        ClusterGrid.Children.Add(control);

        textBox = new TextBox { Tag = cluster };
        textBox.PreviewLostKeyboardFocus += PercentCenterYTextBox_PreviewLostKeyboardFocus;
        Grid.SetRow(textBox, 2);
        Grid.SetColumn(textBox, columnId + 3);
        ClusterGrid.Children.Add(textBox);
        cluster.PercentCenterYTextBox = textBox;

        //row 3: area percentage
        control = new TextBlock(new Run("‰:") { FontWeight = FontWeights.Bold });
        Grid.SetRow(control, 3);
        Grid.SetColumn(control, columnId);
        ClusterGrid.Children.Add(control);

        textBox = new TextBox { IsReadOnly = true, IsEnabled = false};
        Grid.SetRow(textBox, 3);
        Grid.SetColumn(textBox, columnId + 1);
        ClusterGrid.Children.Add(textBox);
        cluster.PercentTextBox = textBox;
      }

      //hide unused clusters
      for (; clusterIndex < clusters.Length; clusterIndex++) {
        var cluster = clusters[clusterIndex];
        cluster.PercentCenter = PercentPoint.Undef;
        cluster.ClusterTextBlock.Visibility = Visibility.Hidden;
      }
      AverageTextBlock.Text = (1000 / clustersUsedCount).ToString();
    }


    #region Event Handlers
    //      --------------

    //drawing area measured in pixels
    int pixelsWidth;
    int pixelsWidthHalf;
    int pixelsHeight;
    int pixelsHeightHalf;
    int rawStride;
    int[] pixels;


    DpiScale dpiScale;
    PixelFormat pixelFormat = PixelFormats.Bgr32;


    private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e) {
      //use MainCanvas to detect size change. MainCanvas has the same size like MainImage, which can be empty and then
      //has size 0.
      dpiScale =  VisualTreeHelper.GetDpi(this);
      pixelsWidth = (int)(e.NewSize.Width*dpiScale.DpiScaleX);
      pixelsWidthHalf = pixelsWidth / 2;
      pixelsHeight = (int)(e.NewSize.Height*dpiScale.DpiScaleX);
      pixelsHeightHalf = pixelsHeight / 2;
      rawStride = (pixelsWidth * pixelFormat.BitsPerPixel + 7) / 8;
      pixels = new int[rawStride/4 * pixelsHeight];
      reDraw();
    }


    private void ConfigCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      setupClusters(ConfigCombobox.SelectedIndex);
      reDraw();
    }


    private void NextButton_Click(object sender, RoutedEventArgs e) {
      //nothing happens when ConfigCombobox.SelectedIndex++ and SelectedIndex has reached the last available item
      if (ConfigCombobox.SelectedIndex==clustersConfiguration.Length-1) {
        ConfigCombobox.SelectedIndex = 0;
      } else {
        ConfigCombobox.SelectedIndex++;
      }
    }


    private void IntCenterXTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e) {
      var textBox = (TextBox)sender;
      var number = getIntNumber(textBox, pixelsWidth);
      if (number<0) {
        e.Handled = true;
        return;
      }

      var cluster = (Cluster)textBox.Tag;
      cluster.IntCenter = new IntPoint(number, cluster.IntCenter.Y);
      cluster.PercentCenter = new PercentPoint((double)number/pixelsWidth, cluster.PercentCenter.Y);
      reDraw();
    }


    private void IntCenterYTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e) {
      var textBox = (TextBox)sender;
      var number = getIntNumber(textBox, pixelsHeight);
      if (number<0) {
        e.Handled = true;
        return;
      }

      var cluster = (Cluster)textBox.Tag;
      cluster.IntCenter = new IntPoint(cluster.IntCenter.X, number);
      cluster.PercentCenter = new PercentPoint(cluster.PercentCenter.X, (double)number/pixelsHeight);
      reDraw();
    }


    private int getIntNumber(TextBox textBox, int max) {
      try {
        var number = int.Parse(textBox.Text);
        if (number<0) {
          MessageBox.Show($"'{number}' must be >=0.");
          return -1;
        }
        if (number>max) {
          MessageBox.Show($"'{number}' must be <={max}.");
          return -1;
        }
        return number;

      } catch (Exception) {
        MessageBox.Show($"'{textBox.Text}' is not a number.");
        return -1;
      }
    }


    private void PercentCenterXTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e) {
      var textBox = (TextBox)sender;
      var number = getPercentNumber(textBox);
      if (number<0) {
        e.Handled = true;
        return;
      }

      var cluster = (Cluster)textBox.Tag;
      cluster.PercentCenter = new PercentPoint(number, cluster.PercentCenter.Y);
      cluster.IntCenter = new IntPoint((int)(number*pixelsWidth), cluster.IntCenter.Y);
      reDraw();
    }


    private void PercentCenterYTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e) {
      var textBox = (TextBox)sender;
      var number = getPercentNumber(textBox);
      if (number<0) {
        e.Handled = true;
        return;
      }

      var cluster = (Cluster)textBox.Tag;
      cluster.PercentCenter = new PercentPoint(cluster.PercentCenter.X, number);
      cluster.IntCenter = new IntPoint(cluster.IntCenter.X, (int)(number*pixelsHeight));
      reDraw();
    }


    private double getPercentNumber(TextBox textBox) {
      try {
        var number = double.Parse(textBox.Text);
        if (number<0) {
          MessageBox.Show($"'{number}' must be >=0.");
          return -1;
        }
        if (number>=1000) {
          MessageBox.Show($"'{number}' must be smaller 1000.");
          return -1;
        }
        return number/1000;

      } catch (Exception) {
        MessageBox.Show($"'{textBox.Text}' is not a number.");
        return -1;
      }
    }
    #endregion


    #region Cluster methods
    //      ---------------

    public (double X, double Y) MoveClusterCenter(int clusterId, double deltaX, double deltaY) {
      var cluster = clusters[clusterId];
      var newX = (int)(cluster.IntCenter.X + dpiScale.DpiScaleX * deltaX);
      if (newX>pixelsWidth) {
        newX -= pixelsWidth;
      } else if (newX<0) {
        newX += pixelsWidth;
      }
      var newY = (int)(cluster.IntCenter.Y + dpiScale.DpiScaleY * deltaY);
      if (newY>pixelsHeight) {
        newY -= pixelsHeight;
      } else if (newY<0) {
        newY += pixelsHeight;
      }
      cluster.IntCenter = new IntPoint(newX, newY);
      cluster.PercentCenter = new PercentPoint((double)newX/pixelsWidth, (double)newY/pixelsHeight);
      reDraw();
      return (newX / dpiScale.DpiScaleX, newY / dpiScale.DpiScaleY);
    }


    private int calcDistance(IntPoint point0, IntPoint point1) { 
      var xDif = Math.Abs(point0.X - point1.X);
      if (xDif>pixelsWidthHalf) {
        xDif = pixelsWidth - xDif;
      }
      var yDif = Math.Abs(point0.Y - point1.Y);
      if (yDif>pixelsHeightHalf) {
        yDif = pixelsHeight - yDif;
      }
      return xDif*xDif + yDif*yDif;
    }
    #endregion


    #region Drawing
    //      -------

    readonly StringBuilder sb0 = new StringBuilder();
    readonly StringBuilder sb1 = new StringBuilder();
    readonly StringBuilder sb2 = new StringBuilder();
    readonly StringBuilder sb3 = new StringBuilder();


    private void reDraw() {
      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
        var cluster = clusters[clusterIndex];
        cluster.IntCenter =
          new IntPoint((int)(cluster.PercentCenter.X*pixelsWidth), (int)(cluster.PercentCenter.Y*pixelsHeight));
        cluster.PixelsCount = 0;
      }

      _ = Parallel.For(0, pixelsHeight, (y) => {
        var offset = y * pixelsWidth;
        var pixelCounts = new int[clustersUsedCount];
        for (int x = 0; x<pixelsWidth; x++) {
          var distance = int.MaxValue;
          var clusterIndexFound = int.MaxValue;
          for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
            var newDistance = calcDistance(clusters[clusterIndex].IntCenter, new IntPoint(x, y));
            if (distance>newDistance) {
              distance = newDistance;
              clusterIndexFound = clusterIndex;
            }
          }
          pixels[offset++] = clusters[clusterIndexFound].Colour;
          pixelCounts[clusterIndexFound]++;
        }
        lock (clusters) {
          for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
            clusters[clusterIndex].PixelsCount += pixelCounts[clusterIndex];
          }
        }
      });

      var total = 0;
      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
        var cluster = clusters[clusterIndex];
        drawCross(cluster.IntCenter);
        total += cluster.PixelsCount;
      }
      if (total!=pixels.Length) System.Diagnostics.Debugger.Break();

      sb0.Clear();
      sb1.Clear();
      sb2.Clear();
      sb3.Clear();
      const int w = 20;
      total /= 1000;
      for (int clusterIndex = 0; clusterIndex < clustersUsedCount; clusterIndex++) {
        sb0.Append(clusterIndex.ToString().PadRight(w));
        var cluster = clusters[clusterIndex];
        //System.Diagnostics.Debug.WriteLine($"{cluster.Id}: {cluster.PixelsCount/total}");
        System.Diagnostics.Debug.WriteLine(cluster.PercentCenter);
        var textBlock = cluster.ClusterTextBlock;
        var percentage = cluster.PixelsCount/total;
        textBlock.Text = $"{cluster.Id}: {percentage}";
        Canvas.SetLeft(textBlock, cluster.IntCenter.X/dpiScale.DpiScaleX);
        Canvas.SetTop(textBlock, cluster.IntCenter.Y/dpiScale.DpiScaleY);
        sb1.Append(cluster.IntCenter.ToString().PadRight(w));
        sb2.Append(cluster.PercentCenter.ToString().PadRight(w));
        sb3.Append((percentage.ToString()+"‰").PadRight(w));

        cluster.IntCenterXTextBox.Text = cluster.IntCenter.X.ToString();
        cluster.IntCenterYTextBox.Text = cluster.IntCenter.Y.ToString();
        cluster.PercentCenterXTextBox.Text = (cluster.PercentCenter.X*1000).ToString("N0");
        cluster.PercentCenterYTextBox.Text = (cluster.PercentCenter.Y*1000).ToString("N0");
        cluster.PercentTextBox.Text = percentage.ToString();
      }
      CoordinatesTextBox.Text = 
        sb0.ToString() + Environment.NewLine + 
        sb1.ToString() + Environment.NewLine +
        sb2.ToString() + Environment.NewLine + 
        sb3.ToString();
      BitmapSource bitmap = BitmapSource.Create(pixelsWidth, pixelsHeight,
          dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchX, pixelFormat, null,
          pixels, rawStride);
      MainImage.Source = bitmap;
    }


    private void testBorderDrawing() {
      //test if drawArea() paints precisely into border
      drawArea(new IntPoint(0, 0), new IntPoint(100, 100));
      drawArea(new IntPoint(pixelsWidth-100, 0), new IntPoint(pixelsWidth, 100));
      drawArea(new IntPoint(0, pixelsHeight-100), new IntPoint(100, pixelsHeight));
      drawArea(new IntPoint(pixelsWidth-100, pixelsHeight-100), new IntPoint(pixelsWidth, pixelsHeight));
    }


    private void testCrossesDrawing() {
      int xLeft = 3;
      int yTop = 7;
      int xMid = pixelsWidth/2;
      int yMid = pixelsHeight/2;
      int xRight = pixelsWidth - 3;
      int yBottom = pixelsHeight - 7;
      IntPoint[] points = {
        new IntPoint(xLeft, yTop),         //top left corner
        new IntPoint(xLeft, yBottom),      //bottom left corner 
        new IntPoint(xLeft, yMid),         //middle left border
        new IntPoint(xRight, yTop),        //top right corner
        new IntPoint(xRight, yBottom),     //bottom right corner
        new IntPoint(xRight, yMid),        //middle right border
        new IntPoint(xMid, yTop),          //middle top border
        new IntPoint(xMid, yBottom),       //middle bottom border
        new IntPoint(xMid, yMid)};         //center

      foreach (var point in points) {
        drawCross(point);
      }
    }


    private void drawCross(IntPoint point) {
      //const int length = 40; //used for testing drawing of crosses
      //const int width = 12;
      const int length = 20;
      const int width = 6;
      drawRectangle(new IntPoint(point.X-length, point.Y-width), new IntPoint(point.X+length, point.Y+width));
      drawRectangle(new IntPoint(point.X-width, point.Y-length), new IntPoint(point.X+width, point.Y+length));
    }


    /// <summary>
    /// Draws a rectangle which might cross a border. If it does, the original rectangle will be drawn as
    /// 2 recttangles at the opposite sides of the border (wrap around)'
    /// </summary>
    private void drawRectangle(IntPoint minPoint, IntPoint maxPoint) {
      if (minPoint.X>maxPoint.X || minPoint.Y>maxPoint.Y) throw new ArgumentException();

      if (minPoint.X<0) {
        if (minPoint.Y<0) {
          //top left corner
          draw4Corners(maxPoint.X, maxPoint.Y, minPoint.X + pixelsWidth, minPoint.Y+pixelsHeight);
        } else if (maxPoint.Y>pixelsHeight) {
          //bottom left corner
          draw4Corners(maxPoint.X, maxPoint.Y-pixelsHeight, minPoint.X + pixelsWidth, minPoint.Y);
        } else {
          //left border
          drawArea(new IntPoint(0, minPoint.Y), maxPoint);
          drawArea(new IntPoint(pixelsWidth + minPoint.X, minPoint.Y), new IntPoint(pixelsWidth, maxPoint.Y));
        }
      } else if (maxPoint.X>pixelsWidth) {
        if (minPoint.Y<0) {
          //top right corner
          draw4Corners(maxPoint.X-pixelsWidth, maxPoint.Y, minPoint.X, minPoint.Y+pixelsHeight);
        } else if (maxPoint.Y>pixelsHeight) {
          //bottom right corner
          draw4Corners(maxPoint.X-pixelsWidth, maxPoint.Y-pixelsHeight, minPoint.X, minPoint.Y);
        } else {
          //right border
          drawArea(new IntPoint(0, minPoint.Y), new IntPoint(maxPoint.X-pixelsWidth, maxPoint.Y));
          drawArea(new IntPoint(minPoint.X, minPoint.Y), new IntPoint(pixelsWidth, maxPoint.Y));
        }
      }else if (minPoint.Y<0) {
        //top border
        drawArea(new IntPoint(minPoint.X, 0), new IntPoint(maxPoint.X, maxPoint.Y));
        drawArea(new IntPoint(minPoint.X, minPoint.Y+pixelsHeight), new IntPoint(maxPoint.X, pixelsHeight));
      } else if (maxPoint.Y>pixelsHeight) {
        //bottom border
        drawArea(new IntPoint(minPoint.X, 0), new IntPoint(maxPoint.X, maxPoint.Y-pixelsHeight));
        drawArea(new IntPoint(minPoint.X, minPoint.Y), new IntPoint(maxPoint.X, pixelsHeight));
      } else {
        //inside of map, not crossing any border
        drawArea(minPoint, maxPoint);
      }
    }


    private void draw4Corners(int xLeft, int yTop, int xRight, int yBottom) {
      drawArea(new IntPoint(0, 0), new IntPoint(xLeft, yTop));
      drawArea(new IntPoint(xRight, 0), new IntPoint(pixelsWidth, yTop));
      drawArea(new IntPoint(0, yBottom), new IntPoint(xLeft, pixelsHeight));
      drawArea(new IntPoint(xRight, yBottom), new IntPoint(pixelsWidth, pixelsHeight));
    }


    /// <summary>
    /// Draws also a rectangle, one that does not cross any border
    /// </summary>
    private void drawArea(IntPoint minPoint, IntPoint maxPoint) {
      var lineOffset = minPoint.Y*pixelsWidth + minPoint.X;
      for (int y = minPoint.Y; y < maxPoint.Y; y++) {
        var pixelOffset = lineOffset;
        for (int x = minPoint.X; x < maxPoint.X; x++) {
          pixels[pixelOffset++] = 0;
        }
        lineOffset += pixelsWidth;
      }
    }
    #endregion
  }
}
