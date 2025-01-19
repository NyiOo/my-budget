
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SQLite;
using System.Linq;

namespace MyBudget.Model
{
    internal sealed class SQLiteDb  : IModel
    {        

        static readonly SQLiteDb db = new SQLiteDb() ;

        Decimal income, balance, outcome, lastMonthRemain, otherIncome ;
        readonly SQLiteDataAdapter[] adapters = new SQLiteDataAdapter[4];
        readonly SQLiteCommandBuilder[] cmdBuilders = new SQLiteCommandBuilder[4];
        DataSet myDataSet;

        public static String currentPath, oldPath;
        public static SQLiteDb GetSqliteDB
        {
            get { return db; }
        }        
        
       
          
        private  SQLiteDb()
        {
           
            myDataSet = new DataSet();

            for(int i=0; i< adapters.Length ; i++)
            {
                adapters[i] = new SQLiteDataAdapter();                      // sqliteDataAdapter can hold one table, that why it have been created according by tables.
                cmdBuilders[i] = new SQLiteCommandBuilder(adapters[i]);     //create command builder to auto translate insert,delete and update command   
            }
          
        }           
        

        /// <summary>
        /// Create Database File and Tables
        /// </summary>
        /// <returns></returns>
        public String CreateNewDatabase()
        {
            var save_file = new SaveFileDialog();
            save_file.Filter = "Database File|*.db";
            String fileName = String.Empty;

            if (save_file.ShowDialog() == true)
            {
               fileName = save_file.FileName; 

                if(fileName != String.Empty)
                {
                    CreateSQLiteDatabase(fileName);
                }                
                               
            }

            return fileName;
        }
        /// <summary>
        /// Create Expenditure Memory Table
        /// </summary>
        /// <param name="datetime">DateTime</param>
        public void LoadMemoryTableForExpenditure( DateTime datetime)
        {          

            var date = ConvertToSqliteDate(datetime.Date);

            SQLiteConnection conn = new SQLiteConnection(@"DataSource=" + SQLiteDb.currentPath);

            //Remark...dont use "using( ){...}" because it will release all resourses
            //if all resourse have been released, no more can use sqliteadapter...

            conn.Open();

            String query = "select * from Expenditure As e where strftime('%m%Y',e.Date) = strftime('%m%Y','" + date + "') order by e.Date";


            var command = new SQLiteCommand(query, conn);
            

            //If main table have already created at first time, it should be clear data to avoid duplicated data...
            if (!myDataSet.Tables.Contains("main_table"))
            {
                myDataSet.Tables.Add("main_table");              

            }
            else
            {
                myDataSet.Tables["main_table"].Clear();   
            }

            adapters[0].SelectCommand = command;
            adapters[0].Fill(myDataSet, "main_table");

            conn.Close();
                
        }
        /// <summary>
        /// Create Balance Memory Table
        /// </summary>
        public void LoadMemoryTableForBalance()
        {
            oldPath = currentPath;

            SQLiteConnection conn = new SQLiteConnection(@"DataSource=" + SQLiteDb.currentPath);
            conn.Open();

            var command = new SQLiteCommand("SELECT * FROM TotalBalance as b order by b.Date", conn);
            

            if(!myDataSet.Tables.Contains("balance_table"))
            {
                myDataSet.Tables.Add("balance_table");               
                myDataSet.Tables.Add("datagrid_table");
            }
            else
            {
                myDataSet.Tables["balance_table"].Clear();               
                myDataSet.Tables["datagrid_table"].Clear();
            }           

            adapters[1].SelectCommand = command;
            adapters[1].Fill(myDataSet, "balance_table");            

            conn.Close();
            
        }
        /// <summary>
        /// Create Category Memory Table
        /// </summary>
        public void LoadMemoryTableForCategory()
        {
            SQLiteConnection conn = new SQLiteConnection(@"DataSource=" + SQLiteDb.currentPath);
            conn.Open();
            
            var command = new SQLiteCommand("select * from Category", conn);

            if (!myDataSet.Tables.Contains("category_table"))
            {               
                myDataSet.Tables.Add("category_table");               
            }
            else
            {               
                myDataSet.Tables["category_table"].Clear();               
            }

            adapters[2].SelectCommand = command;
            adapters[2].Fill(myDataSet, "category_table");

            conn.Close();

        }


        /// <summary>
        /// Get DataSource for ListView according by month of selected date
        /// </summary>
        /// <param name="date">DateTime</param>
        /// <returns></returns>
        public List<Expenditure> GetDataSourceForListView(DateTime datetime)
        {
            ResetMoney();           

            if (myDataSet.Tables["main_table"].Rows.Count == 0)
                return null;

            var expenditure_result = (from e in myDataSet.Tables["main_table"].AsEnumerable()
                                      where e.Field<DateTime>("Date").Year == datetime.Year && e.Field<DateTime>("Date").Month == datetime.Month
                                      join c in myDataSet.Tables["category_table"].AsEnumerable() on e.Field<Int32>("CategoryID") equals c.Field<Int32>("CategoryID")
                                      group e by e.Field<Int32>("CategoryID") into total_gp
                                      select new Expenditure()
                                      {
                                          Name = (from c in myDataSet.Tables["category_table"].AsEnumerable()
                                                  where c.Field<Int32>("CategoryID") == total_gp.Key
                                                  select c.Field<String>("Expense")).First(),

                                          StatusId = (from c in myDataSet.Tables["category_table"].AsEnumerable()
                                                      where c.Field<Int32>("CategoryID") == total_gp.Key
                                                      select c.Field<Int32>("StatusID")).First(),

                                          Money = total_gp.Sum(a => a.Field<Decimal>("Money"))

                                      }).ToList();

            foreach (var item in expenditure_result)
            {
                if (item.StatusId == 1)
                    outcome += item.Money;  
                else if (item.StatusId == 0)
                    income += item.Money;
                else if (item.StatusId == 2)
                    otherIncome += item.Money;                
               
                //System.Diagnostics.Debug.WriteLine(item.StatusId);               
            }


            expenditure_result.Sort(new Comparison<Expenditure>((a, b) =>
            {
                if (a.StatusId < b.StatusId)
                    return 1;
                else
                    return 0;
            }));



            CalculateBalance(datetime);


            return expenditure_result;

        }


        /// <summary>
        /// Getting Income, Outcome, Balance and Remain Cash
        /// </summary>
        /// <returns> income, outcome , balance </returns>
        public Decimal[] GetMoney()
        {
           return new Decimal[] { income, lastMonthRemain, outcome, balance , otherIncome};
        }



        /// <summary>
        /// Get DataSource for Combobox
        /// </summary>
        /// <param name="i">1 mean insert "All" and other mean nothing insert </param>
        /// <returns>List of Category Name</returns>
        public List<String> GetDataSourceForCombobox(Int32 i)
         {
             var lst = new List<String>();
            if (i == 1)
                lst.Add("All");

             var result = from e in myDataSet.Tables["category_table"].AsEnumerable()
                          select e.Field<String>("Expense");

             foreach (var name in result)
                 lst.Add(name);

            return lst;
         }
        /// <summary>
        /// Create New Row ,Insert Data and Update Table
        /// </summary>
        /// <param name="time">Selected DateTime</param>
        /// <param name="categoryName">Category Name</param>
        /// <param name="txt">Description</param>
        /// <param name="money">Money</param>
        public void InsertData(DateTime time, String categoryName, String txt, Decimal money)
        {
            var id = GetCategoryID(categoryName);          

            try
            {

                DataRow row = myDataSet.Tables["main_table"].NewRow();
                row["Date"] = time.Date;
                row["CategoryID"] = id;
                row["About"] = txt;
                row["Money"] = money;

                myDataSet.Tables["main_table"].Rows.Add(row);

                var statusId = CheckStatusID(id);

                UpdateBalance(time,statusId,money,0);

               
                adapters[0].Update(myDataSet, "main_table");                

                adapters[1].Update(myDataSet, "balance_table");


            }
             
            catch(SQLiteException ex)
            {
                throw ex;
            }
            
         }           
       
        /// <summary>
        /// Get DataSource for DataGrid Table 
        /// </summary>
        /// <param name="d1">Start Date</param>
        /// <param name="d2">End Date</param>
        /// <param name="index">Combobox Selected Index</param>
        /// <returns>DataView</returns>          
        public DataView GetDataSourceTable(DateTime d1, DateTime d2, String categoryName)
       {
           
            String query = null;
            SQLiteCommand command = new SQLiteCommand();

            var date1 = ConvertToSqliteDate(d1);
            var date2 = ConvertToSqliteDate(d2);
           

            if (categoryName == "All")
            {
                query = "SELECT * FROM Expenditure AS e WHERE e.Date BETWEEN @date1 AND  @date2 order by e.Date";

                command.CommandText = query;
                var para1 = new SQLiteParameter("@date1", date1);
                var para2 = new SQLiteParameter("@date2", date2);
               
                command.Parameters.AddRange(new[] { para1, para2});
            }
            else
            {
                var id = GetCategoryID(categoryName);
                query = "SELECT * FROM (SELECT * FROM Expenditure AS e WHERE e.Date BETWEEN @date1 AND  @date2 order by e.Date)"
                         + " As r WHERE r.CategoryId = @id";

                command.CommandText = query;
                var para1 = new SQLiteParameter("@date1", date1);
                var para2 = new SQLiteParameter("@date2", date2);
                var para3 = new SQLiteParameter("@id", id);
                command.Parameters.AddRange(new[] { para1, para2, para3 });

            }

            SQLiteConnection conn = new SQLiteConnection(@"DataSource=" + SQLiteDb.currentPath);

            conn.Open();
            
            command.Connection = conn;
                
            myDataSet.Tables["datagrid_table"].Clear();               

            adapters[3].SelectCommand = command;
            adapters[3].Fill(myDataSet, "datagrid_table");

            conn.Close();          

            return myDataSet.Tables["datagrid_table"].AsDataView();
       }


        /// <summary>
        /// Get DataSource For BarChart
        /// </summary>
        /// <param name="category">Combobox Selected Item Name</param>
        /// <param name="isYear">Year or Months</param>
        /// <param name="date1">Start DateTime</param>
        /// <param name="date2">End DateTime</param>
        /// <returns> Year or Month and Money</returns>
        public Dictionary<String, Decimal> BarChartDataSource(String cateogry, Boolean isYear ,DateTime date1, DateTime date2)
       {
            Dictionary<String, Decimal> result = new Dictionary<string, decimal>();

            var nothing = GetDataSourceTable(date1, date2, cateogry);


           var resultbyMonth = from e in myDataSet.Tables["datagrid_table"].AsEnumerable()                               
                                group e by e.Field<DateTime>("Date").Month into total
                                select new
                                {
                                    Month =  GetMonthName( total.Key),
                                    Money = total.Sum(a => a.Field<Decimal>("Money"))
                                };

           var resultbyYear = from e in myDataSet.Tables["datagrid_table"].AsEnumerable()                              
                              group e by e.Field<DateTime>("Date").Year into total
                              select new
                              {
                                  Year = total.Key.ToString(),                                  
                                  Money = total.Sum(a => a.Field<Decimal>("Money"))
                              };

           if (isYear)
           {              
               foreach(var item in resultbyYear)
               {
                   result.Add( item.Year , item.Money);
               }
           }
           else
           {
               foreach(var item in resultbyMonth)
               {
                   result.Add(item.Month, item.Money);
               }
           }


            if (result.Count > 0)
                return result;
            else
                return null;
           
       }
        /// <summary>
        /// Get DataSource For PipeChart
        /// </summary>
        /// <param name="date1">Start DateTime</param>
        /// <param name="date2">End DateTime</param>
        /// <returns>Income,Outcome and Balance Money</returns>
        public Dictionary<String,Decimal> PieChartDataSource(DateTime date1,DateTime date2)
       {          

            var dict = new Dictionary<String, Decimal>();

            var nothing = GetDataSourceTable(date1, date2, "All");


            var outcome = (from e in myDataSet.Tables["datagrid_table"].AsEnumerable()
                          join c in myDataSet.Tables["category_table"].AsEnumerable() on e.Field<Int32>("CategoryID") equals c.Field<Int32>("CategoryID")
                          where  c.Field<Int32>("StatusID") == 1
                           select e.Field<Decimal>("Money")).Sum();

            var income = (from e in myDataSet.Tables["datagrid_table"].AsEnumerable()
                          join c in myDataSet.Tables["category_table"].AsEnumerable() on e.Field<Int32>("CategoryID") equals c.Field<Int32>("CategoryID")
                          where c.Field<Int32>("StatusID") == 0
                          select e.Field<Decimal>("Money")).Sum();

            var getback =  (from e in myDataSet.Tables["datagrid_table"].AsEnumerable()
                            join c in myDataSet.Tables["category_table"].AsEnumerable() on e.Field<Int32>("CategoryID") equals c.Field<Int32>("CategoryID")
                            where  c.Field<Int32>("StatusID") == 2
                            select e.Field<Decimal>("Money")).Sum();
           
            outcome -= getback;            

            if(income == 0 || income < outcome)
            {
                income = outcome + balance;
            }

            dict.Add("Income", income);
            dict.Add("Outcome", outcome);            
            dict.Add("Balance", balance);

           

            return dict;
        }               
       
        /// <summary>
        /// Get DataSource For Category View
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetDataSourceForCategoryView()
        {           
            
            return myDataSet.Tables["category_table"];  
            
        }
        /// <summary>
        /// Update Category Table
        /// </summary>
        public void UpdateCategory()      
        {
            try
            {
               
                adapters[2].Update(myDataSet, "category_table");
            }
            catch (DBConcurrencyException ex)
            {
                throw ex;
            }
            
        }

        public Int32 GetLastRowID()
        {
            var table = myDataSet.Tables["category_table"];
            var lastRow = table.Rows[table.Rows.Count - 2];
            var id = lastRow.Field<Int32>("CategoryID");
            return id;                      

        }

        public bool CheckByCategoryId(Int32 id)
        {
            using (SQLiteConnection conn = new SQLiteConnection(@"DataSource=" + SQLiteDb.currentPath))
            {
                conn.Open();

                var query = "SELECT count(*) FROM Expenditure as e join Category as c on e.CategoryID ==" + id.ToString();

                var command = new SQLiteCommand(query, conn);

                var count = Convert.ToInt32(command.ExecuteScalar());

                if (count > 1)
                    return false;
                else
                    return true;
            }
        }


       


        /// <summary>
        /// Update Expenditure Table
        /// </summary>
        /// <param name="dateTime">Selected DateTime</param>
        /// <param name="id">Category ID</param>
        /// <param name="delta_money">Difference Money</param>
        public void UpdateDataGrid(DateTime dateTime,Int32 id ,Decimal old_money, Decimal delta_money)
        {
            UpdateBalance(dateTime, id,old_money, delta_money);           

            adapters[3].Update(myDataSet, "datagrid_table");  // Adapter translate SQL Command in runtime according by Row State 

            adapters[1].Update(myDataSet, "balance_table");


        }
        /// <summary>
        ///  Update Expenditure Table
        /// </summary>
        public void UpdateDataGrid()
        {          

            adapters[3].Update(myDataSet, "datagrid_table");
        }


        public Int32 GetStatusId(Int32 id)
        {
            return (int) CheckStatusID(id);
        }

        //========================================= Helper Method ============================================

        /// <summary>
        /// Update Balance according by input parameters
        /// </summary>
        /// <param name="time">DateTime</param>
        /// <param name="id">Category ID</param>
        /// <param name="old_money">Difference Money</param>
        private void UpdateBalance(DateTime time, Int32 statusId, Decimal old_money,Decimal update_money)
        {
            Decimal update_balance = 0, old_balance = 0;

            //Need to declare PrimaryKey to work "Rows.Contain()  and Rows.Find() " functions...
            myDataSet.Tables["balance_table"].PrimaryKey = new DataColumn[] { myDataSet.Tables["balance_table"].Columns["Date"] };

            var Rows = myDataSet.Tables["balance_table"].Rows;

           

            if (!Rows.Contains(time.Date))
            {
                DataRow new_row = myDataSet.Tables["balance_table"].NewRow();

                new_row["Date"] = time.Date;

                var result = (from b in myDataSet.Tables["balance_table"].AsEnumerable()
                              where b.Field<DateTime>("Date") < time.Date
                              orderby b.Field<DateTime>("Date")
                              select b).ToList();

                if (result.Count > 0)
                {
                    var last = result.Last();
                    old_balance = last.Field<Decimal>("Balance");
                    update_balance = UpdateMoney(statusId , old_money, update_money, old_balance);
                    new_row["Balance"] = update_balance;
                }
                else
                    new_row["Balance"] = UpdateMoney(statusId , old_money , update_money , 0);

                myDataSet.Tables["balance_table"].Rows.Add(new_row);

                if (update_balance != 0)
                {
                    var difference = update_balance - old_balance;


                    var result1 = (from b in myDataSet.Tables["balance_table"].AsEnumerable()
                                   where b.Field<DateTime>("Date") > time.Date
                                   orderby b.Field<DateTime>("Date")
                                   select b).ToList();

                    if (result1.Count > 0)
                    {

                        foreach (var item in result1)
                        {
                            item.SetField<Decimal>(1, item.Field<Decimal>("Balance") + difference);
                        }
                    }
                }

               


            }
            else
            {

                var old_row = Rows.Find(time.Date);
                old_balance = old_row.Field<Decimal>("Balance");               

                update_balance = UpdateMoney(statusId , old_money, update_money , old_balance);

                old_row.SetField<Decimal>(1, update_balance);
               

                if(update_balance!=0)
                {
                   var difference = update_balance - old_balance;

                    var result1 = (from b in myDataSet.Tables["balance_table"].AsEnumerable()
                                   where b.Field<DateTime>("Date") > time.Date
                                   orderby b.Field<DateTime>("Date")
                                   select b).ToList();

                    if (result1.Count > 0)
                    {

                        foreach (var item in result1)
                        {
                            item.SetField<Decimal>(1, item.Field<Decimal>("Balance") + difference);
                        }
                    }
                }
                   
            }

           
        }

      

        /// <summary>
        /// Update Income,Outcome and Balance
        /// </summary>
        /// <param name="name">category name</param>
        /// <param name="old_money">cost</param>
        /// <returns></returns>
        private Decimal UpdateMoney(Int32 statusID, Decimal old_money, Decimal delta_money, Decimal balance)
        {           

            if (statusID == 0 || statusID == 2)             //"0" mean Income and "2" mean Get Back Loan
            {
                balance += old_money;
                balance += delta_money;
            }
            else
            {
                balance -= old_money;                          // "1" mean Outcome 
                balance -= delta_money;
            }

            return balance;
        }

        private Int32 CheckStatusID(Int32 id)
        {
            var statusID = (Int32)(from c in myDataSet.Tables["category_table"].AsEnumerable()
                                   where c.Field<Int32>("CategoryID") == id
                                   select c.Field<Int32>("StatusID")).First();

            return statusID;
        }

        /// <summary>
        /// Get Month Name
        /// </summary>
        /// <param name="index">Month</param>
        /// <returns></returns>
        private String GetMonthName(Int32 index)
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
                case 8:return "August";
                case 9: return "Setember";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
                default: return "";
            }
        }
        /// <summary>
        /// Convert DateTime Format to Sqlite Date Format
        /// </summary>
        /// <param name="date">DateTime</param>
        /// <returns></returns>
        private String ConvertToSqliteDate(DateTime date)
        {
            var year = date.Year;
            var month = date.Month < 10 ? "0"+date.Month.ToString() :  date.Month.ToString() ;
            var day = date.Day < 10 ? "0"+date.Day.ToString() :  date.Day.ToString() ;
            
            return String.Format("{0}-{1}-{2} 00:00:00", year, month, day);
        }

        /// <summary>
        /// Calculate Last Month Remain Money and Current Balance Money
        /// </summary>
        /// <param name="datetime">DateTime</param>
        private void CalculateBalance(DateTime datetime)
        {
            var year = datetime.Year;
            var month = datetime.Month;

            //Current Month  

            var result = from e in myDataSet.Tables["balance_table"].AsEnumerable()
                          where e.Field<DateTime>("Date").Year == year && e.Field<DateTime>("Date").Month == month
                          orderby e.Field<DateTime>("Date")
                          select e;
            if (result.Count() != 0)
            {
                var lastRow = result.Last();
                balance = lastRow.Field<Decimal>("Balance");
            }
            else
                balance = 0;

            //Last Month           

            if(month == 1)
            {
                month = 12;
                year -= 1;
            }
            else
            {
                month -= 1;
            }

            var result1 = from e in myDataSet.Tables["balance_table"].AsEnumerable()
                           where e.Field<DateTime>("Date").Year == year && e.Field<DateTime>("Date").Month == month
                           orderby e.Field<DateTime>("Date")
                           select e;

            if (result1.Count() != 0)
            {
                var lastRow = result.Last();
                lastMonthRemain = lastRow.Field<Decimal>("Balance");
            }
            else
                lastMonthRemain = 0;

            
        }
        /// <summary>
        /// Drop to zero line
        /// </summary>
        private void ResetMoney()
        {
            income = 0;
            outcome = 0;
            balance = 0;
            lastMonthRemain = 0;
            otherIncome = 0; 
        }


       
        /// <summary>
        /// Get CategoryID according by Name
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        private Int32 GetCategoryID(String name)
        {
           if (name == null || name == "") return 0;

           var result = (from c in myDataSet.Tables["category_table"].AsEnumerable()
                         where c.Field<String>("Expense") == name
                         select c.Field<Int32>("CategoryID")).First();

            return result;
        }

       
        /// <summary>
        /// Create Database
        /// </summary>
        /// <param name="fileName">File path</param>
        public static void CreateSQLiteDatabase(String fileName)
        {
            #region Create Command

            String balanceTB_command = "CREATE TABLE TotalBalance (Date DATE PRIMARY KEY ON CONFLICT ROLLBACK  NOT NULL, Balance DECIMAL NOT NULL); ";

            String categoryTB_comand = "CREATE TABLE Category (" +
                                       " CategoryID INT NOT NULL PRIMARY KEY ASC ON CONFLICT ROLLBACK," +
                                       "Expense TEXT NOT NULL," +
                                       "StatusID INT NOT NULL" +
                                       " REFERENCES StatusTable(StatusID));";

            String expenditureTB_command =  "CREATE TABLE Expenditure( ID INTEGER PRIMARY KEY ASC ON CONFLICT ROLLBACK AUTOINCREMENT NOT NULL," +
                                            "Date DATE NOT NULL REFERENCES TotalBalance (Date), Money DECIMAL NOT NULL, About TEXT NOT NULL," +
                                            "CategoryID INT REFERENCES Category NOT NULL);";
            String statusTB_command = "CREATE TABLE StatusTable (StatusID INT NOT NULL PRIMARY KEY ASC ON CONFLICT ROLLBACK, Status TEXT NOT NULL); ";

            #endregion


            #region Insert Command

            var d = DateTime.Now;


            String balanceTB = "INSERT INTO TotalBalance (Date, Balance) VALUES( @d ,0);";

            String categoryTB = "INSERT INTO Category (CategoryID, Expense, StatusID)" +
                                "VALUES(1,'ဝင်ငွေ',0);" +
                                "INSERT INTO Category (CategoryID, Expense, StatusID)" +
                                "VALUES(2,'လူသုံးကုန်ပစ္စည်း',1);" +
                                "INSERT INTO Category (CategoryID, Expense, StatusID)" +
                                "VALUES(3,'စားသောက်ကုန်ပစ္စည်း',1);" +
                                "INSERT INTO Category (CategoryID, Expense, StatusID)" +
                                "VALUES(4,'ချေးငွေ',1);" +
                                "INSERT INTO Category (CategoryID, Expense, StatusID)" +
                                "VALUES(5,'ချေးငွေပြန်ဆပ်ငွေ',2);";

            String statusTB = "INSERT INTO StatusTable (StatusID, Status ) VALUES(0,'Income');" +
                              "INSERT INTO StatusTable (StatusID, Status ) VALUES(1,'Outcome');" +
                              "INSERT INTO StatusTable (StatusID, Status ) VALUES(2,'Add');";
            #endregion

            using (SQLiteConnection connection = new SQLiteConnection(@"DataSource=" + fileName))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = categoryTB_comand + expenditureTB_command + statusTB_command + balanceTB_command;

                    command.ExecuteNonQuery();                   //Execute Create Command

                    command.CommandText = statusTB + categoryTB + balanceTB;

                    SQLiteParameter p = new SQLiteParameter("@d",d.Date);

                    command.Parameters.Add(p);
                  

                    command.ExecuteNonQuery();                  //Execute Insert Command
                }
            }
        }
      
        
    }
   
}
