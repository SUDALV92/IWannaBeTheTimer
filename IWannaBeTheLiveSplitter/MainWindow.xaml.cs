using IWannaBeTheLiveSplitter.Model;
using IWannaBeTheLiveSplitter.Tools;
using IWannaBeTheLiveSplitter.ViewModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IWannaBeTheLiveSplitter
{
    public partial class MainWindow : Window
    {
        public MainViewModel MainViewModel { get; set; }
        LowLevelKeyboardListener listener;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            if (File.Exists("size.json"))
            {
                var json = File.ReadAllText("size.json");
                var array = Serializer.Deserialize<double[]>(json);
                Width = array[0];
                Height = array[1];
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var array = new double[2];
            array[0] = Width;
            array[1] = Height;
            var json = Serializer.Serialize(array);
            File.WriteAllText("size.json", json);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            listener = new LowLevelKeyboardListener();
            listener.OnKeyReleased += Listener_OnKeyReleased;
            listener.OnKeyPressed += Listener_OnKeyPressed;
            listener.HookKeyboard();
            MainViewModel = new MainViewModel();
            DataContext = MainViewModel;
            MainViewModel.Initialize();
            MainViewModel.LoadWindows();
        }

        bool ctrlPressed = false;
        private void Listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed.Equals(Key.LeftCtrl))
            {
                ctrlPressed = true;
            }
        }

        private void Listener_OnKeyReleased(object sender, KeyReleasedArgs e)
        {
            if (ctrlPressed && e.KeyReleased.Equals(Key.Enter))
            {
                MainViewModel.AddStat();
            }
            if (ctrlPressed && e.KeyReleased.Equals(Key.Back))
            {
                MainViewModel.RemoveStat();
            }
            if (e.KeyReleased.Equals(Key.LeftCtrl))
            {
                ctrlPressed = false;
            }
        }

        private void WindowSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                MainViewModel.SelectProcess(e.AddedItems[0] as WindowModel);
            }
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.R))
            {
                if (MainViewModel.NewParallelStatVisibility == Visibility.Collapsed && MainViewModel.RenameStatVisibility == Visibility.Collapsed)
                {
                    MainViewModel.LoadWindows();
                }
            }
            if (e.Key.Equals(Key.D))
            {
                if (MainViewModel.NewParallelStatVisibility == Visibility.Collapsed)
                {
                    MainViewModel.ShowDeathsRegex();
                }
            }
            if (e.Key.Equals(Key.Home))
            {
                MainViewModel.NewParallelStatVisibility = Visibility.Visible;
            }
            if (e.Key.Equals(Key.End))
            {
                MainViewModel.RenameStatVisibility = Visibility.Visible;
            }
        }

        private void DeathsRegexKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                MainViewModel.HideDeathsRegex();
                MainViewModel.DeathsRegex = (sender as TextBox).Text;
            }
        }

        private void NewParallelStatKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                MainViewModel.AddNewParallelStat(NewParallelName.Text);
            }
        }

        private void ParallelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var parallel = e.AddedItems[0] as Parallels;
                MainViewModel.SelectParallel(parallel.ID);
            }
        }

        private void RenameStatKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                MainViewModel.RenameCurrentStat(RenameStatTextBox.Text);
            }
        }
    }
}
