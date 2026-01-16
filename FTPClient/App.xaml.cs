using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FTPClient
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex mutex;

        void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, "FTPClient", out ret);

            if (!ret)
            {
                MessageBox.Show("已有一個程式執行!");
                Environment.Exit(0);
            }

        }

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
        }

        
    }
}
