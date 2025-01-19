using Caliburn.Micro;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MyBudget.Framework;
using MyBudget.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SQLite;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;


namespace MyBudget
{
    [Export(typeof(IController))]
    public class DetailViewModel : Screen , IController
    {
        private ChoiceState state = ChoiceState.DataGrid;       
       
        private DateTime timeFrom, timeTo;       
        private List<String> categoryList, lables;
        private string categoryName;

        private DataRow selectedRow = null;
        private Decimal oldMoney;
        private Int32 oldIndex;

        //========================================== ACCESSOR PROPERTIES  =================================

        public ChoiceState State
        {
            get { return state; }
            set
            {
                if (state == value)
                    return;

                state = value;
                NotifyOfPropertyChange(() => State);
                if (state == ChoiceState.DataGrid) NotifyOfPropertyChange(() => DataSource);
            }
        }
       /// <summary>
       /// DataSource For DataGrid 
       /// </summary>
        public DataView DataSource
        {
            get
            {               
                return GetDataSourceForDataGrid();
            }
          
           
        }
        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime TimeFrom
        {
            get { return timeFrom; }
            set
            {
                if (timeFrom == value)
                    return;
                timeFrom = value;
                NotifyOfPropertyChange(() => TimeFrom);               
                NotifyOfPropertyChange(() => DataSource);

                if (state != ChoiceState.DataGrid)
                {
                    if (categoryName != "All")                                        // If Category List Index is not equal "All"
                    {
                        State = ChoiceState.BarChart;
                        NotifyOfPropertyChange(() => BarSeriesCollection);
                    }
                    else
                    {
                        State = ChoiceState.PieChart;
                        NotifyOfPropertyChange(() => PieSeriesCollection);
                    }
                }

            }
        }
        /// <summary>
        /// End Time
        /// </summary>
        public DateTime TimeTo
        {
            get { return timeTo; }
            set
            {
                if (timeTo == value)
                    return;
                timeTo = value;
                NotifyOfPropertyChange(() => TimeTo);               
                NotifyOfPropertyChange(() => DataSource);

                if (state != ChoiceState.DataGrid)
                {
                    if (categoryName != "All")
                    {
                        State = ChoiceState.BarChart;
                        NotifyOfPropertyChange(() => BarSeriesCollection);
                    }
                    else
                    {
                        State = ChoiceState.PieChart;
                        NotifyOfPropertyChange(() => PieSeriesCollection);
                    }
                }

            }
        }
        
        public String CategoryName
        {
            get { return categoryName; }
            set
            {
                if (categoryName == value) return;
                categoryName = value;
                NotifyOfPropertyChange(() => CategoryName);
                NotifyOfPropertyChange(() => DataSource);

                if (state != ChoiceState.DataGrid)
                {
                    if (categoryName != "All")
                    {
                        State = ChoiceState.BarChart;
                        NotifyOfPropertyChange(() => BarSeriesCollection);
                    }
                    else
                    {
                        State = ChoiceState.PieChart;
                        NotifyOfPropertyChange(() => PieSeriesCollection);
                    }

                }

            }
        }

        /// <summary>
        /// Binding List to Category Combobox
        /// </summary>
        public List<String> CategoryList
        {
            get { return categoryList; }
            set
            {
                if (categoryList == value)
                    return;
                categoryList = value;
                NotifyOfPropertyChange(() => CategoryList);
            }
        }
        /// <summary>
        /// Binding String Array to comobox of datagrid while editing state
        /// </summary>
        public String[] EditCategoryList
        {
            get
            {
                 var strArr = new string[categoryList.Count];
                 categoryList.CopyTo(1, strArr, 1, categoryList.Count-1);                
                 return strArr;

               
            }
        }

       

        public SeriesCollection BarSeriesCollection { get { return GetDataSourceForBarChart(); } }
        public List<String> Labels
        {
            get { return lables; }
            set
            {
                if (lables == value)
                    return;
                lables = value;
                NotifyOfPropertyChange(() => Labels);
            }
        }        
        public Func<Double,String> Formatter { get; set; }

        public SeriesCollection PieSeriesCollection { get { return GetDataSourceForPieChart(); } }

        public IModel Model => SQLiteDb.GetSqliteDB;


        //========================================= CONSTRUCTOR ================================================

        public DetailViewModel()
        {
            Labels = new List<string>();          
        }

        //========================================= EVENT HANDLER ===============================================

        protected override void OnActivate()
        {
            BindCombobox();

            AssignInitialData();

            NotifyOfPropertyChange(() => DataSource);            
            
        }

        protected override void OnDeactivate(bool close)
        {
            //_sqldb.UpdateTable(0);
        }

        public void DataGrid()
        {
            State = ChoiceState.DataGrid;            
        }  
        public void ChoiceChart(Int32 index)
        {
           if (index != 0)
            {
                State = ChoiceState.BarChart;
                NotifyOfPropertyChange(() => BarSeriesCollection);
            }
               

           else
            {
                State = ChoiceState.PieChart;
                NotifyOfPropertyChange(() => PieSeriesCollection);
            }
            
        }

        public void RowSelected(object sender, EventArgs e)
        {
            var dg = sender as DataGrid;

            var view = dg.SelectedItem as DataRowView;

            if (view != null)
            {
                selectedRow = view.Row;

                oldIndex = Convert.ToInt32(selectedRow.ItemArray[4]);
                
                if(selectedRow.ItemArray[2].GetType() != typeof(DBNull))
                    oldMoney = (Decimal)selectedRow.ItemArray[2];
            }

        }
        public void SaveChange(object sender, EventArgs e)
        {
            
            var dg = sender as DataGrid;

            var view = dg.SelectedItem as DataRowView;

            if (view != null)
            {
                var row = view.Row;

                try
                {
                    if (row.RowState == DataRowState.Modified)
                    {                       
                        var update_index = (Int32)row.ItemArray[4];   
                           

                        var date = (DateTime)row.ItemArray[1];
                        var newMoney = (Decimal)row.ItemArray[2];
                        int newId = Model.GetStatusId(update_index);
                        var oldId = Model.GetStatusId(oldIndex);

                        if (oldId != newId)
                        {
                            if(oldId == 2 || oldId== 0)
                            {
                                if (newId != 1)
                                    Model.UpdateDataGrid(date, newId, -oldMoney, newMoney);
                                else
                                    Model.UpdateDataGrid(date, newId, oldMoney, newMoney);
                            }                               
                            else
                                Model.UpdateDataGrid(date, newId, oldMoney, newMoney);
                        }

                        else if (!Decimal.Equals(oldMoney, newMoney))
                        {                                                 

                            Model.UpdateDataGrid(date, newId , 0 , newMoney - oldMoney);
                        }
                        else
                            Model.UpdateDataGrid();


                    }
                }
                catch(SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }

                

            }
            
        }
    
        public void KeyPressed(object sender, KeyEventArgs e)
       {
            if (e.Key == Key.Delete)
            {
                var result = MessageBox.Show("Are you sure you want to delete this row?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var delete_money = 0 - oldMoney;
                        var date = (DateTime)selectedRow.ItemArray[1];
                        var id = Convert.ToInt32(selectedRow.ItemArray[4]);
                        int statusId = Model.GetStatusId(id);

                        selectedRow.Delete();

                        Model.UpdateDataGrid(date, statusId ,delete_money,0);
                    }
                    catch (SQLiteException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }
                else if(result == MessageBoxResult.No)
                {
                    NotifyOfPropertyChange(() => DataSource);
                }
            }
           


        }
        //========================================== HELPER METHOD =================================================

        private DataView GetDataSourceForDataGrid()
        {
            Func<DateTime, DateTime, String, DataView> func = Model.GetDataSourceTable;            

            var asyncResult = func.BeginInvoke(timeFrom, timeTo, categoryName, _ => {  }, null);
            
            while (!asyncResult.IsCompleted) { }

            var result = func.EndInvoke(asyncResult);

            return result;
          
        }

        private SeriesCollection GetDataSourceForBarChart()
        {
            var collection = new SeriesCollection();
            var lst = new List<String>();

            var year_flag = timeFrom.Year < timeTo.Year ? true : false;    // if true, chart will show by year .. if false, chart will show by month  

            if (categoryName == "All")
                return null;

            Func<String, Boolean, DateTime, DateTime, Dictionary<String, Decimal>> func = Model.BarChartDataSource;

            var asyncResult = func.BeginInvoke(categoryName, year_flag, timeFrom, timeTo, _ => { }, null);

            while (!asyncResult.IsCompleted) 
            { 
                //do nothing
            }

            var result = func.EndInvoke(asyncResult);

            if (result != null)
            {
                var column = new ColumnSeries()
                {
                    Values = new ChartValues<Decimal>(result.Values),
                    DataLabels = true,
                    LabelPoint = (point) => String.Format("{0:#,##} Ks", point.Y),
                    Title = CategoryName                      
                };

                collection.Add(column);

                foreach(String name in result.Keys)
                {
                    lst.Add(name);
                }
            }

            Labels = lst; 
           

            return collection;

        }
        
        private SeriesCollection GetDataSourceForPieChart()
        {
            var collection = new SeriesCollection();           

            if (categoryName != "All")
                return null;            

            Func<ChartPoint, String> labelpoints = (chartpoint) => string.Format("{0} - {1:P}", chartpoint.Y, chartpoint.Participation);

            Func<DateTime, DateTime,Dictionary<String, Decimal>> func = Model.PieChartDataSource;

            var asyncResult = func.BeginInvoke(timeFrom, timeTo, _ => { }, null);

            while (!asyncResult.IsCompleted) { }

            var result = func.EndInvoke(asyncResult);

            if (result !=null)
            {
                foreach (var serie in result)
                {
                     var data = new CustomData()
                        {
                            Value1 = serie.Value,
                            Text1 = serie.Key  +" "+ serie.Value.ToString()+ " Ks"
                        };   

                    var pie = new PieSeries()
                    {
                        Title = serie.Key,
                        Values = new ChartValues<CustomData>() { data },
                        //Values = new ChartValues<Decimal>() { serie.Value },
                        DataLabels = true,
                        LabelPoint = labelpoints
                    };

                    collection.Add(pie);
                }
            }
            //let create a mapper so LiveCharts know how to plot our CustomerViewModel class
            var mapper = Mappers.Pie<CustomData>().Value(
               x=>  Convert.ToDouble(x.Value1));

            //lets save the mapper globally
            Charting.For<CustomData>(mapper);
            

            return collection;
        }

      

        private void AssignInitialData()
        {
            var now = DateTime.Now;

            TimeFrom = new DateTime(now.Year, now.Month, 1);
            TimeTo = new DateTime(now.Year, now.Month, now.Day);

            CategoryName = CategoryList[0];

        }

        private void BindCombobox()
        {
            CategoryList = Model.GetDataSourceForCombobox(1);
        }
        
        
    }
}
