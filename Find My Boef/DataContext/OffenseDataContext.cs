using Find_My_Boef.Controller;
using Find_My_Boef.Model;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using ToastNotifications.Messages;

namespace Find_My_Boef.DataContext
{
    public class OffenseDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Window Settings
        public static FontFamily Font = new("Calibri");
        public static int RegularTextFontSize { get; set; } = int.Parse(ConfigurationManager.AppSettings.Get("Regular_Text_Font_Size"));
        public static string WindowTitle { get; set; }
        #endregion

        #region Current Values
        public static int LoggedInOfficerId { get; set; } = SessionData.SessionOfficerId;

        public static int OffenseId { get; set; }

        public ObservableCollection<Officer> AssignedOfficers { get; set; } = new();

        public static int PartnerId { get; set; }
        public static string PartnerFullName { get; set; }

        public static string CurrentOffenseType { get; set; }
        public static string CurrentStatus { get; set; }
        public static string CurrentDescription { get; set; }
        // The two variables are needed to set the selected index in the ComboBox in WPF as the assigned english enumeration value.
        public static byte CurrentOffenseTypeIndex { get; set; }
        public static byte CurrentStatusIndex { get; set; }
        #endregion  

        public static EditOffenseWindow CurrentEditWindow { get; set; }
        public static OffenseInformation CurrentOffenseInformation { get; set; }
        public static OffenseHistory CurrentHistoryWindow { get; set; }
        public static int AmountOfEditScreensOpen { get; set; } = 0;
        public static int AmountOfOffenseInformationScreensOpen { get; set; } = 0;

        public static int AmountOfHistoryScreensOpen { get; set; } = 0;

        #region New Values
        public static byte NewOffenseTypeIndex { get; set; }
        public static byte NewStatusIndex { get; set; }
        public static string NewDescription { get; set; }
        #endregion

        public void RefreshAssignedOfficerList(int offenseId)
        {
            AssignedOfficers.Clear();

            List<int> officerIds = new List<int>();
            string query = "SELECT * FROM WijkagentDelict WHERE DelictId = @offenseId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter offenseIdParam = new("@offenseId", System.Data.SqlDbType.Int);
            offenseIdParam.Value = offenseId;
            command.Parameters.Add(offenseIdParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetInt32(0) != 0)
                {
                    officerIds.Add(reader.GetInt32(0)); // add all officerIds to list 
                }
            }

            query = "SELECT Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam FROM Werknemers";
            command = new(query, Database.Connection);
            command.Prepare();
            SqlDataReader selectReader = command.ExecuteReader();
            while (selectReader.Read())
            {
                if (officerIds.Contains(selectReader.GetInt32(0)))
                {
                    Officer newOfficer = new Officer();
                    newOfficer.FullName = selectReader.GetString(1) + " " + selectReader.GetString(2) + " " + selectReader.GetString(3);
                    AssignedOfficers.Add(newOfficer);
                }
            }

            NotifyPropertyChanged();
        }

        /// <summary>
        /// OffenseId here is used to retrieve and update offense-information.
        /// </summary>
        /// <param name="updateData">this is a check asking if you want to update the data.</param>
        public static void CreateOffenseWindow(int offenseId, bool isUpdateData = false)
        {
            Debug.WriteLine("CreateOffenseWindow called");
            if (SessionData.IsLoggedIn())
            {
                Debug.WriteLine("WORDT GETOOND");
                OffenseId = offenseId;
                if (IsExistingOffenseId(offenseId))
                {
                    if (isUpdateData)
                    {
                        string selectOffenseQuery = "SELECT Type, Status, Beschrijving FROM Delict WHERE ID = @offenseId";
                        SqlCommand selectOffenseCommand = new(selectOffenseQuery, Database.Connection);
                        SqlParameter offenseIdParam = new("@offenseId", System.Data.SqlDbType.Int);
                        offenseIdParam.Value = offenseId;
                        selectOffenseCommand.Parameters.Add(offenseIdParam);
                        selectOffenseCommand.Prepare();
                        using (SqlDataReader reader = selectOffenseCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string currentDescription = reader.GetString(2);
                                CurrentOffenseTypeIndex = reader.GetByte(0);
                                CurrentStatusIndex = reader.GetByte(1);

                                OffenseType currentOffenseType = (OffenseType)CurrentOffenseTypeIndex;
                                Status currentStatus = (Status)CurrentStatusIndex;

                                CurrentOffenseType = Offense.ReturnDutchEnumValue(currentOffenseType);
                                CurrentStatus = Offense.ReturnDutchEnumValue(currentStatus);
                                CurrentDescription = currentDescription;
                            }
                        }
                    }
                }
                if (AmountOfOffenseInformationScreensOpen < 1)
                {
                    if (AmountOfHistoryScreensOpen > 0) CurrentHistoryWindow.Hide();
                    CurrentOffenseInformation = new OffenseInformation(offenseId)
                    {
                        FontFamily = Font,
                        Title = $"Delictinformatie - {CurrentOffenseType}"
                    };
                    DisplayOffenseInfoWindow(CurrentOffenseInformation);
                }
                else
                {
                    MapWindow.Notifier.ShowError("Dit delict bestaat niet.");
                }
            }
            else
            {
                MapWindow.Notifier.ShowError("U bent niet ingelogd.");
            }
        }


        private void NotifyPropertyChanged([CallerMemberName] String PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        /// <summary>
        /// Opens edit offense screen
        /// </summary>
        public static void CreateEditOffenseWindow(int offenseId, OffenseInformation offenseInformation)
        {
            if (AmountOfEditScreensOpen < 1)
            {
                WindowTitle = $"Delict bewerken - {CurrentOffenseType}";
                CurrentEditWindow = new EditOffenseWindow(offenseId, offenseInformation);
                CurrentEditWindow.FontFamily = Font;
                CurrentEditWindow.Title = WindowTitle;
                DisplayEditOffenseWindow(CurrentEditWindow);
            }
            else
            {
                MapWindow.Notifier.ShowError("Er is al een delict scherm open.");
            }
        }
        /// <summary>
        /// Display the offense window.
        /// </summary>
        /// <param name="offenseInfo">The window that needs to be shown</param>
        public static void DisplayOffenseInfoWindow(OffenseInformation offenseInfo)
        {
            if (AmountOfOffenseInformationScreensOpen < 1)
            {
                offenseInfo.Show();
                AmountOfOffenseInformationScreensOpen++;
            }
            else
            {
                MapWindow.Notifier.ShowError("Er is al een delict scherm open.");
            }
        }

        /// <summary>
        /// Display the edit delict window.
        /// </summary>
        /// <param name="editDelictWindow">The window that needs to be shown</param>
        public static void DisplayEditOffenseWindow(EditOffenseWindow editDelictWindow)
        {
            if (AmountOfEditScreensOpen < 1)
            {
                editDelictWindow.Show();
                AmountOfEditScreensOpen = 1;
            }
            else
            {
                MapWindow.Notifier.ShowError("Er is al een delict scherm open.");
            }
        }

        public static void CreateOffenseHistoryWindow()
        {
            if (AmountOfHistoryScreensOpen < 1 && AmountOfOffenseInformationScreensOpen < 1)
            {
                OffenseHistory historyWindow = new();
                CurrentHistoryWindow = historyWindow;
                DisplayOffenseHistoryWindow();
                AmountOfHistoryScreensOpen++;
            }
            else if (AmountOfOffenseInformationScreensOpen > 0)
            {
                MapWindow.Notifier.ShowError("Er is al een ander delict scherm open.");
            }
            else
            {
                MapWindow.Notifier.ShowError("Er is al een delict history scherm open.");
            }
        }

        public static void DisplayOffenseHistoryWindow()
        {
            double height = SystemParameters.WorkArea.Height;
            double width = SystemParameters.WorkArea.Width;
            CurrentHistoryWindow.Top = (height - CurrentHistoryWindow.Height) / 2;
            CurrentHistoryWindow.Left = (width - CurrentHistoryWindow.Width) / 2;
            CurrentHistoryWindow.Show();
        }

        /// <summary>
        /// Function for editing the offense-information into the database
        /// </summary> 
        ///  Returns true when the database contains the OffenseID (property)
        /// </summary> 
        /// <returns></returns> 
        public static bool IsExistingOffenseId(int offenseId)
        {
            string query = "SELECT * FROM Delict WHERE ID = @offenseId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter offenseIdParam = new("@offenseId", System.Data.SqlDbType.Int);
            offenseIdParam.Value = offenseId;
            command.Parameters.Add(offenseIdParam);
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

        public static void EditOffense(int offenseID)
        {
            if (IsExistingOffenseId(offenseID))
            {
                if (!MainInstance.Client.IsConnected())
                {
                    MapWindow.Notifier.ShowError("De wijzigingen zijn niet toegepast, er is geen verbinding met de websocket server");
                    return;
                }

                if (NewDescription == null)
                {
                    NewDescription = "";  // prevents null exception
                }

                string query = "UPDATE Delict SET Type = @newOffenseType, Status = @newStatus, Beschrijving = @newDescription WHERE ID = @offenseID";
                SqlCommand updateCommand = new SqlCommand(query, Database.Connection);
                SqlParameter newStatusParam = new SqlParameter("@newStatus", System.Data.SqlDbType.TinyInt);
                SqlParameter newOffenseTypeParam = new SqlParameter("@newOffenseType", System.Data.SqlDbType.TinyInt);
                SqlParameter newDescriptionParam = new SqlParameter("@newDescription", System.Data.SqlDbType.VarChar, 255);
                SqlParameter offenseIDParam = new SqlParameter("@offenseID", System.Data.SqlDbType.Int);

                offenseIDParam.Value = OffenseId;
                newStatusParam.Value = NewStatusIndex;
                newOffenseTypeParam.Value = NewOffenseTypeIndex;
                newDescriptionParam.Value = NewDescription;
                offenseIDParam.Value = OffenseId;

                updateCommand.Parameters.Add(newStatusParam);
                updateCommand.Parameters.Add(newOffenseTypeParam);
                updateCommand.Parameters.Add(newDescriptionParam);
                updateCommand.Parameters.Add(offenseIDParam);
                updateCommand.Prepare();

                if (updateCommand.ExecuteNonQuery() > 0)
                {
                    Offense offense = MainInstance.GetOffensesFromID(OffenseId);
                    offense.Status = (Status)NewStatusIndex;
                    offense.Description = NewDescription;
                    offense.Type = (OffenseType)NewOffenseTypeIndex;
                    offense.ReDraw();
                    ContentUpdate contentUpdate = new(UpdateType.Offense)
                    {
                        ContentUpdateOffense = new()
                        {
                            OffenseID = OffenseId,
                            NewStatus = (Status)NewStatusIndex,
                            NewOffenseType = (OffenseType)NewOffenseTypeIndex,
                            NewDescription = NewDescription,

                        },
                        AuthKey = SessionData.AuthKey
                    };
                    MainInstance.Client.SendMessage(contentUpdate);

                    MapWindow.Notifier.ShowSuccess("De wijzigingen zijn succesvol toegepast.");
                    if (CurrentHistoryWindow != null) DisplayOffenseHistoryWindow(); //if history tab exists, display history tab again
                    CurrentEditWindow.Close();
                    AmountOfEditScreensOpen = 0;
                }
                else
                {
                    MapWindow.Notifier.ShowError("De wijzigingen zijn niet toegepast.");
                }
            }
        }

        public static int AddNewOffense(PointLatLng point)
        {
            string location = point.Lat + ";" + point.Lng;
            location = location.Replace(',', '.');
            string query =
                "INSERT INTO Delict (DatumTijd, Locatie, Type, Beschrijving, Status)\r\nOUTPUT inserted.ID\r\nVALUES(GETDATE(), @Locatie, 0, '', 0)\r\n";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter locationParam = new("@Locatie", SqlDbType.VarChar, 255);
            locationParam.Value = location;
            command.Parameters.Add(locationParam);
            command.Prepare();

            return (int)command.ExecuteScalar();
        }

        public static void DeleteOffense(int id)
        {
            string query =
                "DELETE FROM Delict\r\nWHERE ID= @DelictId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter offenseIdParam = new("@DelictId", SqlDbType.VarChar, 255);
            offenseIdParam.Value = id;
            command.Parameters.Add(offenseIdParam);
            command.Prepare();
            command.ExecuteNonQuery();

            string deleteAssignmentsQuery = query = "DELETE FROM WijkagentDelict\r\nWHERE DelictId= @DelictId";
            SqlCommand deleteAssignmentsCommand = new(deleteAssignmentsQuery, Database.Connection);
            offenseIdParam = new("@DelictId", SqlDbType.VarChar, 255);
            offenseIdParam.Value = id;
            deleteAssignmentsCommand.Parameters.Add(offenseIdParam);
            deleteAssignmentsCommand.Prepare();
            deleteAssignmentsCommand.ExecuteNonQuery();

            ContentUpdate contentUpdate = new(UpdateType.NewOffense)
            {
                ContentUpdateNewOffense = new()
                {
                    OffenseID = id,
                },
                AuthKey = SessionData.AuthKey
            };
            MainInstance.Client.SendMessage(contentUpdate);
        }
    }
}
