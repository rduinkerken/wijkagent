using System.ComponentModel;

namespace Find_My_Boef.DataContext
{
    public class NotificationDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static string TitleText { get; set; }
        public static string MessageText { get; set; }

        /// <summary>
        /// Default red = Colors.Salmon
        /// </summary>
        public static Notification CreateNotification(string windowTitle, string messageText)
        {
            TitleText = windowTitle;
            MessageText = messageText;
            Notification notification = new();
            DisplayNotification(notification);
            return notification;
        }

        public static void DisplayNotification(Notification notification)
        {
            notification.Show();
        }

        public static void HideNotification(Notification notification)
        {
            // A destroy might be neccesary instead of a .Hide() due to memory
            notification.Hide();
        }
    }
}
