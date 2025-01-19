using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Data;
using Caliburn.Micro;
using MyBudget.Framework;
using MyBudget.Model;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Data.SQLite;

namespace MyBudget
{
    [Export(typeof(IController))]
    public class CodeViewModel : Screen , IController
    {              
        public DataTable DataSource
        {
            get { return Model.GetDataSourceForCategoryView(); }
           
        }


        public IModel Model => SQLiteDb.GetSqliteDB;

        public CodeViewModel()
        {
            //_sqldb = SQLiteDb.GetSqliteDB;
        }      

        protected override void OnActivate()
        {
            NotifyOfPropertyChange(() => DataSource);         

        }

        protected override void OnDeactivate(bool close)
        {
            
            base.OnDeactivate(close);
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
                        Model.UpdateCategory();
                    }
                    else if (row.RowState == DataRowState.Added)
                    {
                        var id = Model.GetLastRowID();
                       
                        row.SetField(0, id + 1);

                        Model.UpdateCategory();
                    }
                }               
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);                    
                }
            }
            
        }       
      

        public void OnDelete(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {

                var result = MessageBox.Show("Are you sure you want to delete this row? All Data will be deleted.", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {

                    var dg = sender as DataGrid;

                    var view = dg.SelectedItem as DataRowView;

                    if (view != null)
                    {
                        var row = view.Row;

                        var id = (Int32) row.ItemArray[0];

                        try
                        {
                            var flag = Model.CheckByCategoryId(id);

                            if (flag)
                            {
                                row.Delete();
                                Model.UpdateCategory();
                            }
                            else
                            {
                                MessageBox.Show("You could't delete this category because it isn't empty.Try add another category name...");                               
                            }
                                



                        }
                        catch (DBConcurrencyException ex)
                        {
                            MessageBox.Show("You can't delete atonce after adding new row. Please delete later");
                        }

                    }
                }
               
                
            }
           
        }





    }
    
}
