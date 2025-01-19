using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace MyBudget.Detail
{
    /// <summary>
    /// Interaction logic for PipeChart.xaml
    /// </summary>
    public partial class PieChart : UserControl
    {
        public PieChart()
        {
            InitializeComponent();
        }

        private void PieChart_DataClick(object sender, ChartPoint chartPoint)
        {

            var chart = (LiveCharts.Wpf.PieChart)chartPoint.ChartView;

            foreach(PieSeries series in chart.Series)
            {
                series.PushOut=0;
            }

            var selectedSeries = (PieSeries) chartPoint.SeriesView;
            selectedSeries.PushOut = 8;

        }
    }
}
