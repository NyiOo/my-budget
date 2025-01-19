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

namespace MyBudget.Framework.CustomControl
{
    /// <summary>
    /// Interaction logic for CustomDate.xaml
    /// </summary>
    public partial class CustomDate : UserControl
    {

        #region Dependency Properties

        public static readonly DependencyProperty DateProperty;
        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        private static void OnDateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var customDate = (CustomDate)sender;

            var oldDate = (DateTime)e.OldValue;

            var newDate = (DateTime)e.NewValue;
            var month = GetMonthName(newDate.Month);

            customDate.ShortDate = String.Format("{0} {1}", month, newDate.Year);

            RoutedPropertyChangedEventArgs<DateTime> args = new RoutedPropertyChangedEventArgs<DateTime>(oldDate, newDate);
            args.RoutedEvent = DateChangedEvent;
            customDate.RaiseEvent(args);
           
        }

        public static readonly DependencyProperty ShortDateProperty;

        public String ShortDate
        {
            get { return (String)this.GetValue(ShortDateProperty); }
            set { this.SetValue(ShortDateProperty, value); }
        }

        #endregion


        #region Routed Event
        public static readonly RoutedEvent DateChangedEvent;

        public event RoutedPropertyChangedEventHandler<DateTime> DateChanged
        {
            add { AddHandler(DateChangedEvent, value); }
            remove { RemoveHandler(DateChangedEvent, value); }
        }

        #endregion

        static CustomDate()
        {
            DateChangedEvent = EventManager.RegisterRoutedEvent(nameof(DateChanged), RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<DateTime>), typeof(CustomDate));

            DateProperty = DependencyProperty.Register(nameof(Date), typeof(DateTime), typeof(CustomDate),
                new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDateChanged));

            ShortDateProperty = DependencyProperty.Register(nameof(ShortDate), typeof(String), typeof(CustomDate));
        }
        public CustomDate()
        {
            InitializeComponent();

            //Date = DateTime.Now;
        }


        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            Date = Date.AddMonths(-1);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Date = Date.AddMonths(1);
        }

        private static String GetMonthName(Int32 index)
        {
            switch (index)
            {
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "Aprial";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "Setember";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
                default: return "";
            }
        }

    }
}
