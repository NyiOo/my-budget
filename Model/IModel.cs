using System;
using System.Data.SQLite;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using Caliburn.Micro;
using MyBudget.Model;

namespace MyBudget.Model
{
   public interface IModel
    {
        #region MainWindow       
        String CreateNewDatabase();
        void LoadMemoryTableForExpenditure(DateTime datetime);
        void LoadMemoryTableForBalance();
        void LoadMemoryTableForCategory();
        List<Expenditure> GetDataSourceForListView(DateTime datetime);
        Decimal[] GetMoney();
        List<String> GetDataSourceForCombobox(Int32 i);
        void InsertData(DateTime time, String categoryName, String txt, Decimal money);

        #endregion

        #region Detail Window      
        DataView GetDataSourceTable(DateTime d1, DateTime d2, String name);
        Dictionary<String, Decimal> BarChartDataSource(String name, Boolean flag ,DateTime date1, DateTime date2);
        Dictionary<String, Decimal> PieChartDataSource(DateTime date1, DateTime date2);
        void UpdateDataGrid(DateTime dateTime, Int32 id,Decimal old_money, Decimal update_money);
        void UpdateDataGrid();
        Int32 GetStatusId(Int32 id);

        #endregion

        #region Code Window  
        DataTable GetDataSourceForCategoryView();
        void UpdateCategory();       
        Int32 GetLastRowID();
        Boolean CheckByCategoryId(Int32 id);

        #endregion


    }
}
