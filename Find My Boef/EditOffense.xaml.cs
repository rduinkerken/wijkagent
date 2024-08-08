using Find_My_Boef.Controller;
using Find_My_Boef.DataContext;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using ToastNotifications.Messages;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for EditOffense.xaml
    /// </summary>
    public partial class EditOffenseWindow : Window
    {
        public OffenseDataContext OffenseDataContext;
        private OffenseInformation _parentWindow;

        private int _offenseId { get; set; }
        public EditOffenseWindow(int offenseId, OffenseInformation offenseInformation)
        {
            _parentWindow = offenseInformation;
            OffenseDataContext = new();
            DataContext = OffenseDataContext;
            OffenseDataContext.RefreshAssignedOfficerList(offenseId);
            _offenseId = offenseId;
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            OffenseDataContext.EditOffense(OffenseDataContext.OffenseId);
        }


        private void EditOffenseWindow_Closed(object sender, EventArgs e)
        {
            OffenseDataContext.AmountOfEditScreensOpen = 0;
            OffenseDataContext.CreateOffenseWindow(_offenseId, true);
            Close();
        }

        private void OffenseType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            byte selectedIndex = (byte)comboBox.SelectedIndex;
            OffenseDataContext.NewOffenseTypeIndex = selectedIndex;
        }

        private void Status_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            byte selectedIndex = (byte)comboBox.SelectedIndex;
            OffenseDataContext.NewStatusIndex = selectedIndex;
        }

        private void Description_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox newDescriptionTextBox = (TextBox)sender;
            string description = newDescriptionTextBox.Text;
            OffenseDataContext.NewDescription = description;
        }

        public void ResetOfficerForOffense(int offenseId, int officerId)
        {
            if (OffenseDataContext.IsExistingOffenseId(offenseId))
            {
                if (!MainInstance.Client.IsConnected())
                {
                    MapWindow.Notifier.ShowError("Er is geen verbinding met de websocket server, de wijziging is niet toegepast.");
                    return;
                }

                string query = "DELETE FROM WijkagentDelict WHERE(Wijkagentnummer = @officerId AND DelictID = @offenseID)";
                SqlCommand updateCommand = new SqlCommand(query, Database.Connection);
                SqlParameter officerIdParam = new SqlParameter("@officerId", System.Data.SqlDbType.Int);
                SqlParameter offenseIdParam = new SqlParameter("@offenseId", System.Data.SqlDbType.Int);

                officerIdParam.Value = officerId;
                offenseIdParam.Value = offenseId;

                updateCommand.Parameters.Add(officerIdParam);
                updateCommand.Parameters.Add(offenseIdParam);
                updateCommand.Prepare();

                if (updateCommand.ExecuteNonQuery() > 0)
                {
                    MapWindow.Notifier.ShowSuccess("U bent niet meer toegewezen.");
                    OffenseDataContext.RefreshAssignedOfficerList(offenseId);
                }
                else
                {
                    MapWindow.Notifier.ShowSuccess("U staat niet toegewezen.");
                }
            }
        }

        public static bool IsExistingOfficerOffense(int officerId, int offenseId)
        {
            string query = "SELECT * FROM WijkagentDelict WHERE Wijkagentnummer = @officerId AND DelictId = @offenseId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter offenseIdParam = new("@offenseId", System.Data.SqlDbType.Int);
            SqlParameter officerIdParam = new("@officerId", System.Data.SqlDbType.Int);
            offenseIdParam.Value = offenseId;
            officerIdParam.Value = officerId;
            command.Parameters.Add(offenseIdParam);
            command.Parameters.Add(officerIdParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                return true;
            }
            else
            {
                reader.Close();
                return false;
            }
        }

        /// <summary>
        /// isAssign is to check if officerId will be reset or set to officerId
        /// </summary>
        /// 
        public void SetOfficerForOffense(int officerId, int offenseId)
        {
            if (OffenseDataContext.IsExistingOffenseId(offenseId))
            {
                if (!MainInstance.Client.IsConnected())
                {
                    MapWindow.Notifier.ShowError("Er is geen verbinding met de websocket server, de wijziging is niet toegepast.");
                    return;
                }
                if (!IsExistingOfficerOffense(officerId, offenseId))
                {
                    string query = "INSERT INTO WijkagentDelict VALUES (@officerId, @offenseId)";
                    SqlCommand insertCommand = new SqlCommand(query, Database.Connection);
                    SqlParameter officerIdParam = new SqlParameter("@officerId", System.Data.SqlDbType.Int);
                    SqlParameter offenseIdParam = new SqlParameter("@offenseId", System.Data.SqlDbType.Int);

                    officerIdParam.Value = officerId;
                    offenseIdParam.Value = offenseId;

                    insertCommand.Parameters.Add(officerIdParam);
                    insertCommand.Parameters.Add(offenseIdParam);
                    insertCommand.Prepare();

                    if (insertCommand.ExecuteNonQuery() > 0)
                    {
                        MapWindow.Notifier.ShowSuccess("U bent succesvol toegewezen.");
                        OffenseDataContext.RefreshAssignedOfficerList(offenseId);
                    }
                    else
                    {
                        MapWindow.Notifier.ShowError($"Toewijzen niet gelukt. (wijkagent {officerId})");
                    }
                }
                else
                {
                    MapWindow.Notifier.ShowError("U staat al toegewezen aan dit delict.");
                }
            }
            else
            {
                MapWindow.Notifier.ShowError("Dit delict is onbekend, controleer of dit delict nog bestaat.");
            }
        }

        private void BindMeToOffenseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!OfficerDataContext.IsExistingOfficerId(SessionData.SessionOfficerId))
            {
                if (SessionData.SessionOfficerId == 0)
                {
                    MapWindow.Notifier.ShowError("Uw toewijzing kan niet worden veranderd, u bent niet ingelogd.");
                    return;
                }
                throw new ArgumentException($"Werknemersnummer {SessionData.SessionOfficerId} bestaat niet in de werknemers tabel.");
            }
            SetOfficerForOffense(SessionData.SessionOfficerId, OffenseDataContext.OffenseId);
        }

        private void CancelBindToOffenseButton_Click(object sender, RoutedEventArgs e)
        {
            if (OffenseDataContext.IsExistingOffenseId(OffenseDataContext.OffenseId))
            {
                ResetOfficerForOffense(OffenseDataContext.OffenseId, SessionData.SessionOfficerId);
            }
            else
            {
                MapWindow.Notifier.ShowError("Dit delict is onbekend, controleer of dit delict nog bestaat.");
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
