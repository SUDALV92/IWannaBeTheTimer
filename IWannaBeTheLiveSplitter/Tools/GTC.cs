using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWannaBeTheLiveSplitter.Model;

namespace IWannaBeTheLiveSplitter.Tools
{
    public static class GTC
    {
        public static ComplexTime CalculateDifference(ComplexTime fewerTime, ComplexTime greaterTime)
        {
            if (fewerTime == null || greaterTime == null)
                return new ComplexTime();
            int? fewerHours = fewerTime.Hours;
            int? fewerMinutes = fewerTime.Minutes;
            int? fewerSeconds = fewerTime.Seconds;
            int? greaterHours = greaterTime.Hours;
            int? greaterMinutes = greaterTime.Minutes;
            int? greaterSeconds = greaterTime.Seconds;

            if (!fewerSeconds.HasValue || !fewerMinutes.HasValue || !fewerHours.HasValue || !greaterMinutes.HasValue ||
                !greaterHours.HasValue || !greaterSeconds.HasValue)
                return new ComplexTime();
            var resultHours = greaterHours - fewerHours;
            int? resultMinutes;
            int? resultSeconds;
            if (greaterMinutes >= fewerMinutes)
            {
                resultMinutes = greaterMinutes - fewerMinutes;
            }
            else
            {
                resultHours -= 1;
                resultMinutes = 60 - Math.Abs(greaterMinutes.Value - fewerMinutes.Value);
            }
            if (greaterSeconds >= fewerSeconds)
            {
                resultSeconds = greaterSeconds - fewerSeconds;
            }
            else
            {
                if (resultMinutes > 0)
                    resultMinutes -= 1;
                else
                {
                    resultMinutes = 59;
                    resultHours -= 1;
                }
                resultSeconds = 60 - Math.Abs(greaterSeconds.Value - fewerSeconds.Value);
            }
            return new ComplexTime { Hours = resultHours, Minutes = resultMinutes, Seconds = resultSeconds }; //$"{resultHours:0}:{resultMinutes:00}:{resultSeconds:00}";
        }

        public static ComplexTime CalculateSum(ComplexTime time1, ComplexTime time2)
        {
            if (time1 == null || time2 == null)
                return null;
            int? fewerHours = time1.Hours;
            int? fewerMinutes = time1.Minutes;
            int? fewerSeconds = time1.Seconds;
            int? greaterHours = time2.Hours;
            int? greaterMinutes = time2.Minutes;
            int? greaterSeconds = time2.Seconds;

            if (!fewerSeconds.HasValue || !fewerMinutes.HasValue || !fewerHours.HasValue || !greaterMinutes.HasValue ||
                !greaterHours.HasValue || !greaterSeconds.HasValue)
                return null;

            ComplexTime resultTime = new ComplexTime { Hours = 0, Minutes = 0, Seconds = 0 };

            resultTime.Hours = time1.Hours + time2.Hours;

            var tempSec = time1.Seconds + time2.Seconds;
            if (tempSec > 59)
            {
                resultTime.Minutes += 1;
                resultTime.Seconds = tempSec - 60;
            }
            else
            {
                resultTime.Seconds = tempSec;
            }

            var tempMin = time1.Minutes + time2.Minutes + resultTime.Minutes;
            if (tempMin > 59)
            {
                resultTime.Hours += 1;
                resultTime.Minutes = tempMin - 60;
            }
            else
            {
                resultTime.Minutes = tempMin;
            }

            return resultTime;
        }
    }
}
