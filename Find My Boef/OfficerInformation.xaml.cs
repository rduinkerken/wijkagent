using Find_My_Boef.Controller;
using Find_My_Boef.DataContext;
using Find_My_Boef.Model;
using GMap.NET;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using ToastNotifications.Messages;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for OfficerInformation.xaml
    /// </summary>
    public partial class OfficerInformation : Window
    {
        public static bool IsOfficerScreenOpen { get; set; }

        private OfficerDataContext _officerData = new();
        public OfficerInformation()
        {
            InitializeComponent();
        }

        public OfficerInformation(int officerId)
        {
            InitializeComponent();
            DataContext = _officerData;
            _officerData.CreateOfficerInformationWindow(officerId);
            _officerData.LoadData();
            EvalButtons();
        }

        public int RetrievePartnerIdFromOfficer(int officerId)
        {
            string query = "SELECT Partnernummer FROM Wijkagent WHERE Werknemersnummer = @officerId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter officerIdParam = new("@officerId", System.Data.SqlDbType.Int);
            officerIdParam.Value = officerId;
            command.Parameters.Add(officerIdParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                if (OfficerDataContext.IsExistingOfficerId(reader.GetInt32(0)))
                {
                    return reader.GetInt32(0);
                }
            }
            return -1;
        }

        private void ViewOfficerLocationButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int officerId = (int)button.Tag;
            if (!OfficerDataContext.IsExistingOfficerId(officerId))
            {
                MapWindow.Notifier.ShowError($"De agent waarvan u de locatie probeert te bekijken bestaat niet.");
                return;
            }
            string query = "SELECT HuidigeLocatie FROM Locatie WHERE Werknemersnummer = @officerId ORDER BY LaatsteUpdate DESC";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter officerIdParam = new("@officerId", System.Data.SqlDbType.Int);
            officerIdParam.Value = officerId;
            command.Parameters.Add(officerIdParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            PointLatLng location = new();
            bool isFirstRecord = true;
            if (reader.Read())
            {
                if (isFirstRecord)
                {
                    string pointString = reader.GetString(0);
                    String[] separator = { ";" };
                    String[] points = pointString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    double lat = double.Parse(points[0], System.Globalization.CultureInfo.InvariantCulture);
                    double lng = double.Parse(points[1], System.Globalization.CultureInfo.InvariantCulture);

                    location = new PointLatLng(lat, lng);
                    isFirstRecord = false;
                }
                else
                {
                    location = new PointLatLng(); // Reset values to 0, 0
                    return;
                }
            }
            else
            {
                MapWindow.Notifier.ShowError("De locatie van deze werknemer is onbekend.");
                return;
            }
            if (location.Lat == 0 && location.Lng == 0)
            {
                MapWindow.Notifier.ShowError("De locatie van deze werknemer is onbekend.");
                return;
            }
            MapWindow.ViewLocation(location);
            MapWindow.Notifier.ShowSuccess("Locatie wordt getoond.");
        }

        private void ViewOffenseButton_Click(object sender, RoutedEventArgs e)
        {

            int offenseId = 0;
            Button button = sender as Button;
            int officerId = (int)button.Tag;
            if (!OfficerDataContext.IsExistingOfficerId(officerId))
            {
                if (officerId == 0)
                {
                    MapWindow.Notifier.ShowError($"Werknemersnummer {officerId} bestaat niet.");
                    return;
                }
            }
            Officer officer = MainInstance.GetOfficerFromId(officerId);
            if (officer == null)
            {
                MapWindow.Notifier.ShowError("Deze werknemer bestaat niet.");
            }

            int partnerId = RetrievePartnerIdFromOfficer(officerId);

            string query = "SELECT DelictId FROM WijkagentDelict WHERE Wijkagentnummer = @officerId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter officerIdParam = new("@officerId", System.Data.SqlDbType.Int);
            officerIdParam.Value = officerId;
            command.Parameters.Add(officerIdParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (OffenseDataContext.IsExistingOffenseId(reader.GetInt32(0)))
                {
                    offenseId = reader.GetInt32(0);
                    OffenseDataContext.CreateOffenseWindow(offenseId, true);
                    return;
                }
                return;
            }
            if (OffenseDataContext.AmountOfOffenseInformationScreensOpen == 0 && OfficerDataContext.IsExistingOfficerId(partnerId))
            {
                string partnerQuery = "SELECT DelictId FROM WijkagentDelict WHERE Wijkagentnummer = @partnerId";
                SqlCommand partnerCommand = new(partnerQuery, Database.Connection);
                SqlParameter partnerIdParam = new("@partnerId", System.Data.SqlDbType.Int);
                partnerIdParam.Value = partnerId;
                partnerCommand.Parameters.Add(partnerIdParam);
                partnerCommand.Prepare();
                SqlDataReader partnerReader = partnerCommand.ExecuteReader();
                if (partnerReader.Read())
                {
                    if (OffenseDataContext.IsExistingOffenseId(partnerReader.GetInt32(0)))
                    {
                        offenseId = partnerReader.GetInt32(0);
                        OffenseDataContext.CreateOffenseWindow(offenseId, true);
                    }
                    else
                    {
                        MapWindow.Notifier.ShowError("De agent of partner zijn nog niet gekoppeld aan een delict.");
                    }
                }
                else
                {
                    MapWindow.Notifier.ShowError("De agent of partner zijn nog niet gekoppeld aan een delict.");
                }
            }
            else
            {
                MapWindow.Notifier.ShowError("De agent of partner zijn nog niet gekoppeld aan een delict.");
            }
        }

        private void OfficerInformation_Closed(object sender, EventArgs e)
        {
            IsOfficerScreenOpen = false;
            Close();
        }

        private void SearchAgents(object sender, RoutedEventArgs e)
        {
            if (AddPartnerSearchBox.Text.Length > 2)
            {
                _officerData.SearchAgents(AddPartnerSearchBox.Text);
            }
            else
            {
                _officerData.LoadData();
            }
        }

        private void AgentSelectChanged(object sender, RoutedEventArgs e)
        {
            if (AddPartnerBox.SelectedIndex == -1)
            {
                AddPartner.IsEnabled = false;
            }
            else
            {
                AddPartner.IsEnabled = true;
            }
        }

        private void RemovePartner_Click(object sender, RoutedEventArgs e)
        {
            _officerData.RemovePartner();
            RemovePartner.IsEnabled = false;
            AddPartnerPrompt.IsEnabled = true;
        }

        private void EvalButtons()
        {
            if (_officerData.CoopOfficerId == 0)
            {
                AddPartnerPrompt.IsEnabled = true;
                RemovePartner.IsEnabled = false;
            }
            else
            {
                AddPartnerPrompt.IsEnabled = false;
                RemovePartner.IsEnabled = true;
            }
        }

        private void AddPartner_Click(object sender, RoutedEventArgs e)
        {
            RemovePartner.IsEnabled = true;
            AddPartnerPrompt.IsEnabled = false;
            _officerData.AddPartner(AddPartnerBox.SelectedIndex);
            ChangeAddPartnerSectionVisibility(Visibility.Hidden);
        }

        private void AddPartnerPrompt_Click(object sender, RoutedEventArgs e)
        {
            _officerData.LoadData();
            AddPartnerSearchBox.Text = "";
            AddPartnerPrompt.IsEnabled = false;
            ChangeAddPartnerSectionVisibility(Visibility.Visible);
        }

        private void CancelPartner_Click(object sender, RoutedEventArgs e)
        {
            ChangeAddPartnerSectionVisibility(Visibility.Hidden);
            AddPartnerPrompt.IsEnabled = true;
        }

        private void ChangeAddPartnerSectionVisibility(Visibility v)
        {
            AddPartnerSearchBox.Visibility = v;
            AddPartnerBox.Visibility = v;
            AddPartner.Visibility = v;
            CancelPartner.Visibility = v;
        }
    }
}

