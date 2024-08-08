using Find_My_Boef.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Find_My_Boef.Controller
{
    public class MapFilter
    {
        public string Query { get; set; } = " WHERE";

        private DateTime FromDate { get; set; }

        private DateTime ToDate { get; set; }
        private string FromDateSQLString { get; set; }
        private string ToDateSQLString { get; set; }
        private string OffenseTypeString { get; set; }
        private string OffenseStatusString { get; set; }

        private OffenseType OffenseType { get; set; }
        private Status OffenseStatus { get; set; }


        //Constructor for filters
        public MapFilter(DateTime fromDate, DateTime toDate, string offenseTypeString, string offenseStatusString)
        {
            FromDate = fromDate;
            ToDate = toDate.AddHours(23.9999);
            OffenseTypeString = TranslateDutchToEngEnum(offenseTypeString);
            OffenseStatusString = TranslateDutchToEngEnum(offenseStatusString);
        }


        private bool CheckDates()
        {
            //Checks if date Range is valid
            if (FromDate.Date <= ToDate.Date && ToDate.Date < DateTime.Today.AddDays(1))
            {
                return true;
            }
            return false;

        }

        private void ConvertDatesToSQLFormat()
        {
            //Converts c# datetime object into SQL format Datetime Strings
            FromDateSQLString = FromDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
            ToDateSQLString = ToDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        //Translates the value given by the Dropdown menus into english
        private static string TranslateDutchToEngEnum(string DutchValue)
        {
            switch (DutchValue)
            {
                case "Onbezocht":
                    return "NotVisited";
                case "Bezig":
                    return "InProgress";
                case "Afgerond":
                    return "Processed";
                case "Ongeval":
                    return "Accident";
                case "Onbekend":
                    return "Unknown";
                case "Drugs":
                    return "Substance";
                case "Overval":
                    return "Assault";
                case "Alles":
                    return "All";
                case "Niet afgerond":
                    return "Not processed";
                default:
                    return "All";
            }
        }

        public int ConvertOffenseStatusToEnum()
        {
            //Tries parsing the OffenseStatusString into the 'Status' Enum
            if (Enum.TryParse<Status>(OffenseStatusString, out Status OffenseStatus))
            {
                return (int)OffenseStatus;
            }
            //Checks if user selected 'Niet afgerond'
            else if (OffenseStatusString == "Not processed")
            {
                return -1;
            }
            //Returns 999 if not found in enum -> meaning the user selected 'Alles'
            return 999;
        }

        public int ConvertOffenseTypeToEnum()
        {
            //Tries parsing the OffenseTypeString into the 'Status' Enum
            if (Enum.TryParse<OffenseType>(OffenseTypeString, out OffenseType OffenseType))
            {
                return (int)OffenseType;
            }
            //Returns 999 if not found in enum -> meaning the user selected 'Alles'

            return 999;
        }

        //Prepares query arguments and adds them to query string
        public string GetQueryArgsString()
        {
            Query = " WHERE";
            //Controls if dates are valid
            if (CheckDates())
            {
                ConvertDatesToSQLFormat();
                Query += $" '{FromDateSQLString}' < DatumTijd AND DatumTijd <= '{ToDateSQLString}'";
            }

            if (ConvertOffenseStatusToEnum() != 999)
            {
                if (ConvertOffenseStatusToEnum() == -1)
                {
                    Query += " AND NOT Status = 2";
                }
                else
                {
                    Query += $" AND Status = {ConvertOffenseStatusToEnum()}";
                }
            }

            if (ConvertOffenseTypeToEnum() != 999)
            {
                Query += $" AND Type = {ConvertOffenseTypeToEnum()}";
            }

            return Query;
        }

        //Filters offenses into 
        public void FilterOffenses()
        {
            List<int> FilteredOffenseIDs = OffenseData.LoadFilteredOffenses(GetQueryArgsString());

            foreach (Offense o in MainInstance.GetOffenses())
            {
                if (FilteredOffenseIDs.Any(x => x == o.ID))
                {
                    Offense offense = MainInstance.GetOffensesFromID(o.ID);
                    if (!offense.IsDrawn)
                    {
                        offense.Draw();
                    }
                }
                else
                {
                    MainInstance.GetOffensesFromID(o.ID).Remove();
                }
            }
        }


    }
}