using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace MyBudget.Framework.CustomControl
{
    /// <summary>
    /// Interaction logic for CustomLegend.xaml
    /// </summary>
    public partial class CustomLegend : UserControl, IChartLegend
    {
        private List<SeriesViewModel> _series;

        public CustomLegend()
        {
            InitializeComponent();

            

            DataContext = this;
        }

        public List<SeriesViewModel> Series
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged("Series");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
