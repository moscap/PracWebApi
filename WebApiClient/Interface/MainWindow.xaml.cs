using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using Recognition;

namespace Interface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ModelItf Itf;               
        public MainWindow()
        {
            InitializeComponent();
            Itf = new ModelItf();
            MainGrid.DataContext = Itf;
            Itf.ErrNotification += EventNotifier;
        }
        
        void EventNotifier(string message)
        {
            MessageBox.Show(message);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var dg = new System.Windows.Forms.FolderBrowserDialog();
            if (dg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Itf.Set_Samples_Path(dg.SelectedPath);
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (Itf.Are_Files_Set())
            {
                Itf.Run();
            }
            else
            {
                MessageBox.Show("Задайте путь до папки!");
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Itf.Stop();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Factory.StartNew(() => { 
                var server_ans = Itf.Model.Clear_Database();
                var ans = server_ans.Result;
                MessageBox.Show(ans);
            });
            Itf.collection.Clear();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            string msg = "";
            var task = Task.Factory.StartNew(() => { 
                var statistic = Itf.Model.Get_Stat();
                
                var stats = statistic.Result;
                foreach (string stat in stats)
                {
                    msg += stat;
                }

                MessageBox.Show(msg, "Statistics");
            });
        }
    }
}
