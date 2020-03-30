using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recognition;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading;
using System.Collections.Concurrent;
using System.Windows.Threading;
using System.IO;

namespace Interface 
{
    public class ModelItf : INotifyPropertyChanged
    {
        public Model Model { get; set; }

        public ObservableCollection<Answer> collection { get; set; }

        public ConcurrentQueue<Answer> answers;

        public List<string> Cl_List = new List<string>();

        string Path_To_Folder = null;

        public delegate void ErrorNotifier(string message);

        public event ErrorNotifier ErrNotification;

        private int selected_index;
        public int Selected_Index {
            get
            {
                return selected_index;
            }
            set
            {
                selected_index = value;
                OnPropertyChanged(nameof(File_Names));
            }
        }

        Dispatcher dispatcher;


        public ModelItf()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            collection = new ObservableCollection<Answer>();
            Model = new Model();
            answers = Model.answers;

            Path_To_Folder = null;
            Selected_Index = -1;
            

            collection.CollectionChanged += MainCollectionChanged;
        }

        public IEnumerable<string> Classes
        {
            get
            {                
                return Cl_List;
            }
        }

        public IEnumerable<Answer> File_Names
        {
            get
            {
                
                if (Selected_Index >= 0)
                {
                    var classes = (from element in this.collection
                                   where element.Class == Selected_Index
                                   select element).ToList<Answer>();

                    return classes;
                }
                else
                {
                    return new List<Answer>();
                }
            }
        }

        public void Set_Samples_Path( string path)
        {
            Path_To_Folder = path;
        }

        public void Collection_Add(Dispatcher dispatcher)
        {
            while (true)
            {
                Answer ans;
                if (answers.TryDequeue(out ans))
                    if (ans != null)
                    {
                        if (ans.Class != -1)
                        {
                            dispatcher.BeginInvoke(new Action(() =>
                            {
                                collection.Add(ans);
                            })).Wait();
                        }
                        else
                        {
                            ErrNotification.Invoke(ans.File_Name);
                            return;
                        }                                            
                    }
                    else return;
            }
        }

        public void Run()
        {
            collection.Clear();
            Model = new Model();
            answers = Model.answers;

            if (Path_To_Folder != null)
            {
                var task_run = Task.Factory.StartNew(path =>
                {
                    Model.Run_Parallel((string)path);
                }, Path_To_Folder);

                var task = Task.Factory.StartNew(dispatcher =>
                {
                    Collection_Add((Dispatcher)dispatcher);
                }, dispatcher);
            }
            else
            {
                
            }
        }

        public void Stop()
        {
            Model.Stop_Tasks();
        }

        private void MainCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var classes = (from element in this.collection
                           group element by element.Class into cl
                           select cl.Key.ToString()).ToList<String>();
            if (!Cl_List.SequenceEqual(classes))
            {
                Cl_List = new List<string>(classes);
                OnPropertyChanged(nameof(Classes));
            }            
            OnPropertyChanged(nameof(File_Names));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public bool Are_Files_Set()
        {
            return Path_To_Folder != null;
        }
    }

}
