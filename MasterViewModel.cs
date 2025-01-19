using System;
using System.Collections.Generic;
using System.Globalization;
using Caliburn.Micro;
using System.ComponentModel.Composition;
using MyBudget.Framework;
using MyBudget.Model;
using System.Windows;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace MyBudget
{

    [Export(typeof(IController))]
    public  class MasterViewModel : Screen, IController
    {       
        private BindableCollection<Expenditure> items;
        private Decimal[] money;
        private List<String> categoryList;
        private string kyats, description, categoryName;
        private DateTime dateTime;
        private Task creatTable;
       
        //========================================== ACCESSOR PROPERTIES  =================================
        public BindableCollection<Expenditure> Items
        {
            get { return items; }
            private set
            {
                if (items == value)
                    return;
                items = value;
                NotifyOfPropertyChange(() => Items);
            }
        }

        public Decimal[] Money { get { return money; }
            set
            {
                if (money == value)
                    return;
                money = value;
                NotifyOfPropertyChange(() => Money);
            }
        }

        public List<String> CategoryList { get { return categoryList; }
            set
            {
                if (categoryList == value)
                    return;
                categoryList = value;
                NotifyOfPropertyChange(() => CategoryList);
            }
        }

        public String Kyats
        {
            get { return kyats; }
            set
            {

                kyats = ConvertBurmeseToEnglish.ConvertFromMyanmarNumbers(value);

                NotifyOfPropertyChange(() => Kyats);
                NotifyOfPropertyChange(() => CanInsertData);
            }
        }

        public String StrText
        {
            get { return description; }
            set
            {

                description = value;
                NotifyOfPropertyChange(() => StrText);
                NotifyOfPropertyChange(() => CanInsertData);
            }
        }
        public DateTime Date { get { return dateTime; }
            set
            {
                if (dateTime == value) return;
                dateTime = value;
                NotifyOfPropertyChange(() => Date);
            }
        }

        public String CategoryName { get { return categoryName; }
            set
            {
                if (categoryName == value) return;
                categoryName = value;
                NotifyOfPropertyChange(() => CategoryName);
            }
        }

        public Boolean CanInsertData
        {
            get
            {
                if (dateTime != null && !String.IsNullOrWhiteSpace(description) && !NumberValidation.Validate(kyats))
                    return true;
                else
                    return false;

            }

        }

        public IModel Model => SQLiteDb.GetSqliteDB ;




        //========================================= CONSTRUCTOR ================================================
        public MasterViewModel()
        {
            Date = DateTime.Now;
        }
        //========================================= EVENT HANDLER ===============================================

        protected override void OnActivate()
        {
            if(SQLiteDb.oldPath != SQLiteDb.currentPath)
            {
                LoadMemoryBalance();                
            }

            LoadMemoryCategory();

            LoadMemoryTable();
        }

        private void LoadMemoryBalance()
        {
            System.Action action = Model.LoadMemoryTableForBalance;

            var asyncResult = action.BeginInvoke(_ => { }, null);

            while (!asyncResult.IsCompleted) { }

           

        }

        private void LoadMemoryTable()
        {
            Action<DateTime> mainAction = Model.LoadMemoryTableForExpenditure;

            creatTable = Task.Factory.FromAsync(mainAction.BeginInvoke(dateTime, asyncResult => { mainAction.EndInvoke(asyncResult); }, null), (nothing) => { });
                        
            BindListView();

            BindLabelMoney();
           
        }

        private void LoadMemoryCategory()
        {
            Model.LoadMemoryTableForCategory();

            BindMVCombobox();

            AssignInitialData();
        }
        protected override void OnInitialize()
        {           

             Items = new BindableCollection<Expenditure>();
        }

        protected override void OnDeactivate(bool close)
        {
            Items.Clear();
            Money = null;
            StrText = String.Empty;
            Kyats = String.Empty;            


        }


        public void InsertData()
        {
            try
            {
                var kyats = Convert.ToDecimal(this.kyats);

                Model.InsertData(dateTime, categoryName, description, kyats);

                BindListView();

                BindLabelMoney();
                
                StrText = String.Empty;
                Kyats = String.Empty;
            }
            catch (SQLiteException)
            {
                MessageBox.Show("Inserting Data Fail!", "Fail", MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
        }

        public void OnDateChange(object sender, RoutedPropertyChangedEventArgs<DateTime> args)
        {
            if(this.IsInitialized)
            {       
                var oldDate = args.OldValue;
                var newDate = args.NewValue;

                if (newDate.Month == oldDate.Month)
                    return;

                LoadMemoryTable();

                BindListView();

                BindLabelMoney();
            }
            
        }
        //========================================================= HELPER METHOD ========================================================

        private void BindListView()
        {
            Items.Clear();

            var task = creatTable.ContinueWith<List<Expenditure>>((o)=>Model.GetDataSourceForListView(dateTime));
            /*
            Func<DateTime, List<Expenditure>> func = _sqlitedb.GetDataSourceForListView;

            var asyncresult = func.BeginInvoke(_date, _ => { }, null);

            while (!asyncresult.IsCompleted) { }

            var result = func.EndInvoke(asyncresult);

            foreach (var item in result)
                Items.Add(item);
            */
            
            if ( task.Result != null)
            {
                foreach (var item in task.Result)
                {
                    if(item.StatusId == 1)                      //skip income and add status
                        Items.Add(item);
                   
                }
                   
            }

          

        }
        
       
        private void BindLabelMoney()
        {
            //Get Total Income, Outcome and Balance
            Money = Model.GetMoney();

          
        }

     
        private void BindMVCombobox()
        {
            CategoryList = Model.GetDataSourceForCombobox(0);
            
        }


        private void AssignInitialData()
        {           
            CategoryName = CategoryList[0];
        }

        
    }
}
