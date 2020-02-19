using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using IWannaBeTheLiveSplitter.Model;
using IWannaBeTheLiveSplitter.Tools;

namespace IWannaBeTheLiveSplitter.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private Visibility processesVisibility;
        string gameName;
        private static readonly string folder = App.Folder + "/";
        bool lastStatRemoved = false;
        public static ObservableCollection<WindowModel> Windows { get; set; }

        public Stats Stats
        {
            get { return stats; }
            set { stats = value; OnPropertyChanged(); }
        }

        public string Last
        {
            get { return last; }
            set { last = value; OnPropertyChanged(); }
        }

        public string Current
        {
            get { return current; }
            set { current = value; OnPropertyChanged(); }
        }

        public Visibility ProcessesVisibility
        {
            get { return processesVisibility; }
            set { processesVisibility = value; OnPropertyChanged(); }
        }

        //Server server = new Server();

        internal void Initialize()
        {

        }

        public Visibility GameTimeVisibility
        {
            get { return gameTimeVisibility; }
            set { gameTimeVisibility = value; OnPropertyChanged(); }
        }

        public WindowModel Window { get; set; }

        DispatcherTimer Timer;
        private string gameTime;
        private Visibility gameTimeVisibility;
        private Visibility deathsRegexVisibility;
        private string deaths;
        private Visibility newParallelStatVisibility;
        private Stats stats;
        private string last;
        private string current;
        private Visibility parallelsVisibility;
        private Visibility renameStatVisibility;

        public string GameTime
        {
            get { return gameTime; }
            set { gameTime = value; OnPropertyChanged(); }
        }

        public string Deaths
        {
            get { return deaths; }
            set { deaths = value; OnPropertyChanged(); }
        }

        public string DeathsRegex { get; set; }

        public Visibility DeathsRegexVisibility
        {
            get { return deathsRegexVisibility; }
            set { deathsRegexVisibility = value; OnPropertyChanged(); }
        }

        public Visibility NewParallelStatVisibility
        {
            get { return newParallelStatVisibility; }
            set { newParallelStatVisibility = value; OnPropertyChanged(); }
        }

        public Visibility ParallelsVisibility
        {
            get { return parallelsVisibility; }
            set { parallelsVisibility = value; OnPropertyChanged(); }
        }

        public Visibility RenameStatVisibility
        {
            get { return renameStatVisibility; }
            set { renameStatVisibility = value; OnPropertyChanged(); }
        }


        protected delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        protected static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        protected static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        [DllImport("user32.dll")]
        protected static extern bool IsWindowVisible(IntPtr hWnd);

        protected static bool GetWindow(IntPtr hWnd, IntPtr lParam)
        {
            int size = GetWindowTextLength(hWnd);
            if (size++ > 0 && IsWindowVisible(hWnd))
            {
                StringBuilder sb = new StringBuilder(size);
                GetWindowText(hWnd, sb, size);
                if (Regex.IsMatch(sb.ToString(), "([0-9]+\\s*:\\s*[0-9]+\\s*:\\s*[0-9]+)|(Time\\s*\\[\\s*[0-9]\\s*\\]\\s*:\\s*[0-9]+)"))
                    Windows.Add(new WindowModel { hwnd = hWnd, Title = sb.ToString() });
            }
            return true;
        }


        public MainViewModel()
        {
            Timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            Timer.Tick += Timer_Tick;
            Windows = new ObservableCollection<WindowModel>();
            DeathsRegex = "(Death\\s*:\\s*[0-9]+)" +
                          "|(Death\\s*\\[\\s*[0-9]\\s*\\]\\s*:\\s*[0-9]+)" +
                          "|(Deaths\\s*:\\s*[0-9]+)" +
                          "|(Deaths\\s*\\[\\s*[0-9]\\s*\\]\\s*:\\s*[0-9]+)" +
                          "|(Death\\s*\\[\\s*[0-9]+\\s*\\])" +
                          "|(Deaths\\s*\\[\\s*[0-9]+\\s*\\])" +
                          "|(\\[\\s*Death\\s*\\]\\s*[0-9]+)" +
                          "|(Death\\s*:\\s*\\[\\s*[0-9]+\\s*\\])"
                          ;
            Timer.Start();
        } //\[?(\s)*D{1}(eath(s)?)?(\s)*?\]?(\s)*(\[(\s)*\d+(\s)*\])?(\s)*\:?(\s)*\[?\s*(\d+)\s*\]?

        public void LoadWindows()
        {
            DeathsRegexVisibility = Visibility.Collapsed;
            NewParallelStatVisibility = Visibility.Collapsed;
            ParallelsVisibility = Visibility.Collapsed;
            RenameStatVisibility = Visibility.Collapsed;
            Windows.Clear();
            EnumWindows(GetWindow, IntPtr.Zero);
            //Timer.Stop();
            if (Windows.Count == 1)
                SelectProcess(Windows.First());
            else if (Windows.Count > 1)
            {
                ProcessesVisibility = Visibility.Visible;
                GameTimeVisibility = Visibility.Collapsed;
            }

        }

        public void SelectProcess(WindowModel window)
        {
            ProcessesVisibility = Visibility.Collapsed;
            Window = window;
            GameTimeVisibility = Visibility.Visible;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Window == null)
            {
                LoadWindows();
                return;
            }

            int size = GetWindowTextLength(Window.hwnd);
            if (size++ > 0 && IsWindowVisible(Window.hwnd))
            {
                StringBuilder sb = new StringBuilder(size);
                GetWindowText(Window.hwnd, sb, size);
                var currentTime = Regex.Match(sb.ToString(), "([0-9]+\\s*:\\s*[0-9]+\\s*:\\s*[0-9]+)", RegexOptions.IgnoreCase).Value;
                if (string.IsNullOrEmpty(currentTime))
                {
                    currentTime = Regex.Match(sb.ToString(), "(Time\\s*\\[\\s*[0-9]\\s*\\]\\s*:\\s*[0-9]+)", RegexOptions.IgnoreCase).Value;
                }
                if (!string.IsNullOrEmpty(currentTime))
                {
                    GameTime = currentTime.Replace("Time", "");
                    var tempDeaths = Regex.Match(sb.ToString(), DeathsRegex, RegexOptions.IgnoreCase).Value;
                    var temp = Regex.Match(tempDeaths, "\\[[0-9]\\]").Value;
                    Deaths = !string.IsNullOrEmpty(temp) ? tempDeaths.Replace(temp, "") : tempDeaths;
                    Deaths = Deaths.ToUpper()
                        .Replace(" ","")
                        .Replace(":","")
                        .Replace("DEATHS", "")
                        .Replace("DEATH", "");
                    string tempGameName = sb.ToString().Replace(currentTime, "").Replace(tempDeaths, "");
                    
                    gameName = Regex.Replace(tempGameName, "[^A-Za-zА-Яа-я0-9 _.]", "_") + ".txt";
                    if (!File.Exists(folder + gameName))
                    {
                        var fs = File.Create(folder + gameName);
                        fs.Close();
                        Stats = new Stats { Values = new List<StatsValue>(), Parallels = new ObservableCollection<Parallels>() };
                        var json = Serializer.Serialize(Stats);
                        File.WriteAllText(folder + gameName, json);
                    }
                    if (Stats == null)
                    {
                        var json = File.ReadAllText(folder + gameName);
                        Stats = Serializer.Deserialize<Stats>(json);

                    }

                    var currentDeaths = GetDeaths();
                    foreach (var parallel in Stats.Parallels) // для ситуаций, когда текущее время меньше чем записаное в файл
                    {
                        if (parallel.OffTime > new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() })
                        {
                            parallel.OffTime = new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() };
                            parallel.ElapsedTime = GTC.CalculateSum(GTC.CalculateDifference(parallel.BackTime, parallel.OffTime), parallel.ElapsedTime);
                        }
                        if (parallel.BackTime > new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() })
                        {
                            parallel.BackTime = new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() };
                        }

                        if (parallel.OffDeaths > currentDeaths)
                        {
                            parallel.OffDeaths = currentDeaths.Value;
                            parallel.ElapsedDeaths = parallel.OffDeaths - parallel.BackDeaths + parallel.ElapsedDeaths;
                        }
                        if (parallel.BackDeaths > currentDeaths)
                        {
                            parallel.BackDeaths = currentDeaths.Value;
                        }
                        ParallelsVisibility = Visibility.Visible;
                    }
                    foreach (var statsValue in Stats.Values)
                    {
                        if (statsValue.Time > new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() })
                        {
                            statsValue.Time = new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() };
                        }
                        if (statsValue.Deaths.HasValue && statsValue.Deaths.Value > currentDeaths)
                        {
                            statsValue.Deaths = currentDeaths.Value;
                        }
                    }

                    foreach (var parallel in Stats.Parallels)
                    {
                        if (Stats.Values.Last().Time > parallel.OffTime)
                            ParallelsVisibility = Visibility.Collapsed;
                    }


                    if (Stats.SelectedParallel == null)
                    {

                        if (Stats.Values.Count == 0)
                            Current = $"Time: {GetHours():0}:{GetMinutes():00}:{GetSeconds():00}\nDeaths: {GetDeaths()}";
                        else
                        {
                            var idk = GTC.CalculateDifference(Stats.Values.Last().Time,
                                new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() });
                            Current =
                                $"Time: {idk.Hours:0}:{idk.Minutes:00}:{idk.Seconds:00}\nDeaths: {GetDeaths() - Stats.Values.Last().Deaths}";
                        }

                        if (Stats.Values.Count == 0)
                        {
                            Last = "";
                        }
                        else if (Stats.Values.Count == 1)
                        {
                            Last =
                                $"Time: {Stats.Values.Last().Time.Hours}:{Stats.Values.Last().Time.Minutes}:{Stats.Values.Last().Time.Seconds}\nDeaths: {Stats.Values.Last().Deaths}";
                        }
                        else
                        {
                            var idk = GTC.CalculateDifference(Stats.Values[Stats.Values.Count - 2].Time,
                                Stats.Values.Last().Time);
                            Last =
                                $"Time:{idk.Hours:0}:{idk.Minutes:00}:{idk.Seconds:00}\nDeaths: {Stats.Values.Last().Deaths - Stats.Values[Stats.Values.Count - 2].Deaths}";
                        }
                    }
                    else
                    {
                        var currentParallel = Stats.Parallels.FirstOrDefault(i => i.ID.Equals(Stats.SelectedParallel));
                        if (!currentParallel.Cleared)
                        {
                            var timenow = new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() };
                            var idk = GTC.CalculateSum(GTC.CalculateDifference(currentParallel.BackTime, timenow), currentParallel.ElapsedTime);
                            currentParallel.ElapsedTimeString = idk.Hours + ":" + idk.Minutes + ":" + idk.Seconds;
                            var idk2 = currentDeaths - currentParallel.BackDeaths + currentParallel.ElapsedDeaths;
                            currentParallel.ElapsedDeathsString = idk2.ToString();
                        }
                    }
                }
            }
            else
            {
                Stats = null;
                LoadWindows();
            }
        }

        public void HideDeathsRegex()
        {
            DeathsRegexVisibility = Visibility.Collapsed;
        }

        public void ShowDeathsRegex()
        {
            DeathsRegexVisibility = Visibility.Visible;
        }

        public void AddStat()
        {
            if (Stats.SelectedParallel == null)
            {
                Stats.Values.Add(new StatsValue
                {
                    ID = Stats.Values.LastOrDefault() == null ? 0 : Stats.Values.Last().ID + 1,
                    TimeString = GameTime,
                    Time = new ComplexTime
                    {
                        Hours = GetHours(),
                        Minutes = GetMinutes(),
                        Seconds = GetSeconds()
                    },
                    DeathsString = Deaths,
                    Deaths = GetDeaths(),
                    Name = Stats.CurrentStatName
                });
                Stats.CurrentStatName = string.Empty;
            }
            else
            {
                var currentParallel = Stats.Parallels.FirstOrDefault(i => i.ID.Equals(Stats.SelectedParallel));
                currentParallel.Cleared = true;
                if (Stats.Parallels.All(i => i.Cleared))
                {
                    Stats.SelectedParallel = null;
                }

            }
            var json = Serializer.Serialize(Stats);
            File.WriteAllText(folder + gameName, json);
            lastStatRemoved = false;
        }

        private int? ParseNullableInt(string input)
        {
            int idk;
            var success = int.TryParse(input, out idk);
            return success ? (int?)idk : null;
        }

        private int? GetHours()
        {
            var idk = Regex.Replace(GameTime, "[A-Za-z\\[\\] ]", "");
            idk = Regex.Replace(idk, "^:", "");
            var hours = Regex.Replace(idk, ":.*", "");
            return ParseNullableInt(hours);
        }

        private int? GetMinutes()
        {
            var idk = Regex.Replace(GameTime, "[A-Za-z\\[\\] ]", "");
            idk = Regex.Replace(idk, "^:", "");
            var minutes = Regex.Replace(idk, "^\\d+:", "");
            minutes = Regex.Replace(minutes, ":.*", "");
            return ParseNullableInt(minutes);
        }

        private int? GetSeconds()
        {
            var idk = Regex.Replace(GameTime, "[A-Za-z\\[\\] ]", "");
            idk = Regex.Replace(idk, "^:", "");
            var seconds = Regex.Replace(idk, "^.+:.+:", "");
            seconds = Regex.Replace(seconds, ":.*", "");
            return ParseNullableInt(seconds);
        }

        private int? GetDeaths()
        {
            var deathsString = Regex.Match(Deaths, "\\d+").Value;
            return ParseNullableInt(deathsString);
        }

        public void RemoveStat() // TODO: Надо будет сделать отдельную утилиту для редактирования таблички руками, чтобы
                                 // TODO: отменять действия или поправлять
        {
            if (!lastStatRemoved)
            {
                //var lines = File.ReadAllLines(gameName).ToList();
                //lines.RemoveAt(lines.Count - 1);
                //File.WriteAllLines(gameName, lines);
                lastStatRemoved = true;
            }
        }

        public void AddNewParallelStat(string parallelName)
        {
            var newID = Guid.NewGuid();
            Stats.Parallels.Add(new Parallels
            {
                BackTime = new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() },
                Cleared = false,
                ElapsedTime = new ComplexTime { Hours = 0, Minutes = 0, Seconds = 0 },
                ID = newID,
                OffTime = new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() },
                Name = parallelName,
                BackDeaths = GetDeaths(),
                OffDeaths = GetDeaths(),
                ElapsedDeaths = 0
            });
            var json = Serializer.Serialize(Stats);
            File.WriteAllText(folder + gameName, json);
            NewParallelStatVisibility = Visibility.Collapsed;
            ParallelsVisibility = Visibility.Visible;
            SelectParallel(newID);
        }

        public void SelectParallel(Guid parallelID)
        {
            var currentParallel = Stats.Parallels.FirstOrDefault(i => i.ID.Equals(Stats.SelectedParallel));
            if (currentParallel != null)
            {

                currentParallel.OffTime = new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() };
                currentParallel.ElapsedTime = GTC.CalculateSum(GTC.CalculateDifference(currentParallel.BackTime, currentParallel.OffTime), currentParallel.ElapsedTime);
                currentParallel.OffDeaths = GetDeaths();
                currentParallel.ElapsedDeaths = currentParallel.OffDeaths - currentParallel.BackDeaths + currentParallel.ElapsedDeaths;
            }

            Stats.SelectedParallel = parallelID;
            var selectedParallel = Stats.Parallels.FirstOrDefault(i => i.ID.Equals(Stats.SelectedParallel));
            selectedParallel.BackTime = new ComplexTime { Hours = GetHours(), Minutes = GetMinutes(), Seconds = GetSeconds() };
            selectedParallel.BackDeaths = GetDeaths();
            var json = Serializer.Serialize(Stats);
            File.WriteAllText(folder + gameName, json);
        }

        public void RenameCurrentStat(string newName)
        {
            Stats.CurrentStatName = newName;
            var json = Serializer.Serialize(Stats);
            File.WriteAllText(folder + gameName, json);
            RenameStatVisibility = Visibility.Collapsed;
        }
    }
}
