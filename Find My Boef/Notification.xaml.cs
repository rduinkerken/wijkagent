using System.Windows;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>    
    public partial class Notification : Window
    {
        public Notification()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
