using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FTPClient
{
    public class Parameter
    {
        public string Host_val { get; set; }
        public string UserName_val { get; set; }
        public string Password_val { get; set; }
        public string Local_Path_val { get; set; }
        public string Ftp_Path_val { get; set; }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        public string TextBoxDispatcherGetValue(TextBox control)
        {
            string name = "";
            this.Dispatcher.Invoke(() =>
            {
                name = control.Text;
            });
            return name;
        }

        public void TextBoxDispatcherSetValue(string name, TextBox control)
        {
            this.Dispatcher.Invoke(() =>
            {
                control.Text = name;
            });
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<Parameter> Parameter_info = Config.Load();
            Host.Text = Parameter_info[0].Host_val;
            UserName.Text = Parameter_info[0].UserName_val;
            Password.Text = Parameter_info[0].Password_val;
            Local_Path.Text = Parameter_info[0].Local_Path_val;
            Ftp_Path.Text = Parameter_info[0].Ftp_Path_val;
        }
        BaseLogRecord Logger = new BaseLogRecord();
        BaseConfig<Parameter> Config = new BaseConfig<Parameter>();
        FTP Do = new FTP();
        #endregion

        #region Main Screen
        private void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(FileUpload):
                    {
                        if (System.IO.Path.GetFileNameWithoutExtension(Local_Path.Text) == "Project")
                        {
                            Task.Run(() =>
                            {
                                Do.ftpUser = TextBoxDispatcherGetValue(UserName);
                                Do.ftpPassword = TextBoxDispatcherGetValue(Password);
                                Do.ftpRootURL = "ftp://" + TextBoxDispatcherGetValue(Host);
                                string Ftp_Path_Invokval = TextBoxDispatcherGetValue(Ftp_Path);
                                if (Directory.Exists(TextBoxDispatcherGetValue(Local_Path)))
                                {
                                    foreach (var (dirPath, subDirs, files) in Do.Walk(TextBoxDispatcherGetValue(Local_Path)))
                                    {
                                        bool dirstate = Do.FolderCheckExist(Ftp_Path_Invokval, Regex.Split(dirPath, "Project")[1].Replace("\\", "/"));
                                        if (dirstate)
                                        {
                                            Console.WriteLine($"FTP建立{Regex.Split(dirPath, "Project")[1].Replace("\\", "/")}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"FTP已有{Regex.Split(dirPath, "Project")[1].Replace("\\", "/")}");
                                        }
                                        //Console.WriteLine("目錄：" + Regex.Split(dirPath, "Project")[1].Replace("\\", "/"));
                                        foreach (string filePath in files)
                                        {
                                            bool filestate = Do.FileCheckExist(Ftp_Path_Invokval, Regex.Split(filePath, "Project")[1].Replace("\\", "/"));
                                            if (filestate)
                                            {
                                                Console.WriteLine($"FTP已有{Regex.Split(filePath, "Project")[1].Replace("\\", "/") }");
                                            }
                                            else
                                            {
                                                Do.FileUpload(new FileInfo(filePath), Ftp_Path_Invokval, Regex.Split(filePath, "Project")[1].Replace("\\", "/"));
                                                Console.WriteLine($"FTP建立{Regex.Split(filePath, "Project")[1].Replace("\\", "/") }");
                                            }
                                            //Console.WriteLine("文件：" + Regex.Split(filePath, "Project")[1].Replace("\\", "/"));
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("請確認本端路徑是否存在!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                }
                            });
                        }
                        else
                        {
                            MessageBox.Show("請確認上傳路徑是.../Project!", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        }
                        break;
                    }
                case nameof(Open_Dir):
                    {
                        System.Windows.Forms.FolderBrowserDialog path = new System.Windows.Forms.FolderBrowserDialog();
                        path.ShowDialog();
                        Local_Path.Text = path.SelectedPath;
                        break;
                    }
                case nameof(Save_Config):
                    {
                        List<Parameter> Student_config = new List<Parameter>()
                        {
                            new Parameter() {Host_val = Host.Text,
                                             UserName_val = UserName.Text,
                                             Password_val=Password.Text,
                                             Local_Path_val=Local_Path.Text,
                                             Ftp_Path_val= Ftp_Path.Text
                                             }
                        };
                        Config.Save(Student_config);
                        Logger.WriteLog("儲存參數!", 1, richTextBoxGeneral);
                        break;
                    }

            }
        }
        #endregion





    }
}
