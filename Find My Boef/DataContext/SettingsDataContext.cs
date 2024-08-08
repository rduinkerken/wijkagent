using System.Configuration;

namespace Find_My_Boef.DataContext
{
    public class SettingsDataContext
    {
        public int CurrentTimerSetting { get; set; }
        public double CurrentHeatMapSetting { get; set; }
        public int CurrentZoomValue { get; set; }

        public static SettingsDataContext GetDataContext()
        {
            var sdc = new SettingsDataContext()
            {
                CurrentTimerSetting = int.Parse(ConfigurationManager.AppSettings["Timer_Interval"]) / 1000,
                CurrentHeatMapSetting = double.Parse(ConfigurationManager.AppSettings["Heat_Map_Ratio"]),
                CurrentZoomValue = int.Parse(ConfigurationManager.AppSettings["Zoom_Value"])
            };
            return sdc;
        }
    }
}
