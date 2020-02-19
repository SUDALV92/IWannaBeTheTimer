using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IWannaBeTheLiveSplitter.Annotations;
using IWannaBeTheLiveSplitter.ViewModel;

namespace IWannaBeTheLiveSplitter.Model
{
    public class Stats
    {
        public List<StatsValue> Values { get; set; }
        public ObservableCollection<Parallels> Parallels { get; set; }
        public Guid? SelectedParallel { get; set; }
        public string CurrentStatName { get; set; }
    }

    public class Parallels : BaseViewModel
    {
        private ComplexTime elapsedTime;
        private string elapsedTimeString;
        private bool cleared;
        private int? elapsedDeaths;
        private string elapsedDeathsString;

        public string Name { get; set; }
        public Guid ID { get; set; }
        public int Group { get; set; }

        public ComplexTime ElapsedTime
        {
            get { return elapsedTime; }
            set { elapsedTime = value; OnPropertyChanged(nameof(ElapsedTimeString)); }
        } //сколько всего провели на этом параллельном сейве

        public string ElapsedTimeString
        {
            get { return elapsedTimeString; }
            set { elapsedTimeString = value; OnPropertyChanged(); }
        }

        public ComplexTime OffTime { get; set; }  //часы, минуты и секунды, когда переключились на другой параллельный сейв
        public ComplexTime BackTime { get; set; } //часы, минуты и секунды, когда переключились на этот параллельный сейв
        public int? OffDeaths { get; set; }
        public int? BackDeaths { get; set; }

        public int? ElapsedDeaths
        {
            get { return elapsedDeaths; }
            set { elapsedDeaths = value; OnPropertyChanged(); }
        }

        public string ElapsedDeathsString
        {
            get { return elapsedDeathsString; }
            set { elapsedDeathsString = value; OnPropertyChanged(); }
        }

        public bool Cleared
        {
            get { return cleared; }
            set { cleared = value; OnPropertyChanged(); }
        }
    }

    public class StatsValue
    {
        public int ID { get; set; }
        public string TimeString { get; set; }
        public ComplexTime Time { get; set; }
        public string DeathsString { get; set; }
        public int? Deaths { get; set; }
        public string Name { get; set; }
    }

    public class ComplexTime
    {
        public int? Hours { get; set; }
        public int? Minutes { get; set; }
        public int? Seconds { get; set; }

        public static bool operator >(ComplexTime ct1, ComplexTime ct2)
        {
            if (ct1.Hours > ct2.Hours) return true;
            if (ct1.Hours < ct2.Hours) return false;

            if (ct1.Minutes > ct2.Minutes) return true;
            if (ct1.Minutes < ct2.Minutes) return false;

            return ct1.Seconds > ct2.Seconds;
        }

        public static bool operator <(ComplexTime ct1, ComplexTime ct2)
        {
            if (ct1.Hours < ct2.Hours) return true;
            if (ct1.Hours > ct2.Hours) return false;

            if (ct1.Minutes < ct2.Minutes) return true;
            if (ct1.Minutes > ct2.Minutes) return false;

            return ct1.Seconds < ct2.Seconds;
        }
    }
}
