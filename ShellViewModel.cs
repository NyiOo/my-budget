using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows;
using System.IO;
using Microsoft.Win32;
using Caliburn.Micro;
using MyBudget.Framework;
using MyBudget.Model;
using Xceed.Wpf.Toolkit;

namespace MyBudget
{
    public interface IShell { }

    [Export(typeof(IShell))]
    public class ShellViewModel : Conductor<IController>.Collection.OneActive , IShell
    {
        private Byte flag = 2 ;
        private String file_path;
       

        [ImportingConstructor]
        public ShellViewModel([ImportMany]IEnumerable<IController> controllers)
        {
            DisplayName = "My Budget Software";

            Items.AddRange(controllers);

            file_path = Properties.Settings.Default.Database_File_Path;

            if(File.Exists(file_path))
            {
                SQLiteDb.currentPath = file_path;
            }           
            else
            {
                file_path = Environment.CurrentDirectory + "\\MyBudget_" + DateTime.Now.Year.ToString()+".db";
                var stream = File.Create(file_path);
                stream.Close();

                this.CreateDatabaseFile(file_path);                      
            }

            Overview();

          

        }

         /// <summary>
        /// Switch between Master View and Detail View
        /// </summary>
        public byte Flag
        {
            get { return flag; }
            set
            {
                flag = value;
                NotifyOfPropertyChange(() => CanCategory);
                NotifyOfPropertyChange(() => CanDetail);
                NotifyOfPropertyChange(() => CanOverview);
               
            }
        }     

        

        public String Status
        {
            get { return "File Path = " +file_path; }
            set
            {
                if (file_path == value)
                    return;
                file_path = value;
                NotifyOfPropertyChange(() => Status);
            }
        }
        public Boolean CanDetail
        {
            get
            {
                if (flag==1 || flag ==3)
                    return true;
                else
                    return false;
            }
        }
       

        public void Detail()
        {
            Flag = 2;

            ActivateItem(Items[1]);           
        }
        
        public Boolean CanOverview
        {
            get
            {
                if (flag==2 || flag == 3)
                    return true;
                else
                    return false;
            }
        }

        
        public void Overview()
        {
           Flag = 1;         

           ActivateItem(Items[2]);
        }

        public Boolean CanCategory
        {
            get
            {
                if (flag==1 || flag ==2)
                    return true;
                else
                    return false;
            }
        }

        public void Category()
        {
            Flag = 3;
           

            ActivateItem(Items[0]);
        }

        public void NewFile()
        {
            var save_file = new SaveFileDialog();
            save_file.Filter = "Database File|*.db";
            String fileName = String.Empty;

            if (save_file.ShowDialog() == true)
            {
                fileName = save_file.FileName;

                if (fileName != String.Empty)
                {
                    CreateDatabaseFile(fileName);

                    SQLiteDb.currentPath = fileName;

                    Status = fileName;

                    DeactivateItem(Items[2], false);                    

                    ActivateItem(Items[2]);
                }

            }

        }
        public void AssignPath()
        {
            OpenFileDialog open_file = new OpenFileDialog();
            open_file.Filter = "Database File|*.db";
            String file = String.Empty;

            if (open_file.ShowDialog() == true)
            {
                file = open_file.FileName;
                if (file != String.Empty)
                {
                    Properties.Settings.Default.Database_File_Path = file;

                    Properties.Settings.Default.Save();

                    SQLiteDb.currentPath = file;

                    Status = file;

                    DeactivateItem(Items[2],false);             //need to assign "false", because if assign with "true", active item will be closed

                    ActivateItem(Items[2]);             
                }
            }

           
        }

        protected override void OnDeactivate(bool close)
        {
          
               
        }

        
        private void CreateDatabaseFile(String path)
        {
            SQLiteDb.currentPath = path;
            Status = path;

            SQLiteDb.CreateSQLiteDatabase(path);

            Properties.Settings.Default.Database_File_Path = path;
            Properties.Settings.Default.Save();
        }
        
    }
}
