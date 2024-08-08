using Find_My_Boef.Controller;
using Find_My_Boef.DataContext;
using Find_My_Boef.View;
using System.Configuration;
using System.Windows;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private bool _isDataDirty = false;

        private int _newTimerValue;
        private int _currentTimerValue = SettingsDataContext.GetDataContext().CurrentTimerSetting;

        private double _newHeatmapValue;
        private double _currentHeatmapvalue = SettingsDataContext.GetDataContext().CurrentHeatMapSetting;


        private int _currentMapZoom = SettingsDataContext.GetDataContext().CurrentZoomValue;
        private int _newMapZoom;

        public Settings()
        {
            InitializeComponent();
            DataContext = SettingsDataContext.GetDataContext();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isDataDirty = CheckDataDirty();
            // If data is dirty, prompt user and ask for a response
            if (_isDataDirty == true)
            {
                var result = MessageBox.Show("U heeft de instellingen nog niet opgeslagen. Weet u zeker dat u wilt afsluiten?",
                                             "Vraag",
                                             MessageBoxButton.YesNo);

                // User doesn't want to close, cancel closure
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }

            }
        }



        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TimerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _newTimerValue = (int)TimerSlider.Value;
        }

        private bool CheckDataDirty()
        {
            if (_newTimerValue != _currentTimerValue || _newHeatmapValue != _currentHeatmapvalue || _newMapZoom != _currentMapZoom)
            {
                return true;
            }
            return false;
        }

        private void HeatmapSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _newHeatmapValue = (double)HeatmapSlider.Value;
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _newMapZoom = (int)ZoomSlider.Value;
        }

        private void SaveBTN_Click(object sender, RoutedEventArgs e)
        {
            if (_newHeatmapValue != _currentHeatmapvalue)
            {
                double temp = _newHeatmapValue;
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings["Heat_Map_Ratio"].Value = temp.ToString();
                configuration.Save();

                ConfigurationManager.RefreshSection("appSettings");
                _currentHeatmapvalue = _newHeatmapValue;
                Visualization.setHeatmapRatio(_currentHeatmapvalue);
            }

            if (_newTimerValue != _currentTimerValue)
            {
                int temp = _newTimerValue * 1000;
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings["Timer_Interval"].Value = temp.ToString();
                configuration.Save();

                ConfigurationManager.RefreshSection("appSettings");
                _currentTimerValue = int.Parse(ConfigurationManager.AppSettings["Timer_Interval"]) / 1000;
            }

            if (_newMapZoom != _currentMapZoom)
            {
                int temp = _newMapZoom;
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configuration.AppSettings.Settings["Zoom_Value"].Value = temp.ToString();
                configuration.Save();

                ConfigurationManager.RefreshSection("appSettings");
                _currentMapZoom = _newMapZoom;
                MainInstance.GetControl().Zoom = _newMapZoom;
            }

            MainInstance.GetControl().ReloadMap();
            Visualization.ReloadHeatmaps();

        }
    }
}
