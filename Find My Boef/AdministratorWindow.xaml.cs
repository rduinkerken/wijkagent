using Find_My_Boef.DataContext;
using System.Windows;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for AdministratorWindow.xaml
    /// </summary>
    public partial class AdministratorWindow : Window
    {
        private AdminDataContext adminData = new();
        public AdministratorWindow()
        {
            adminData.LoadData();

            InitializeComponent();
            FullGrid.DataContext = adminData;
        }

        private void AdminRemoveSelected(object sender, RoutedEventArgs e)
        {
            if (RemoveAdminBox.SelectedIndex == -1)
            {
                RemoveAdmin.IsEnabled = false;
            }
            else
            {
                RemoveAdmin.IsEnabled = true;
            }
        }
        private void AdminAddSelected(object sender, RoutedEventArgs e)
        {
            if (AddAdminBox.SelectedIndex == -1)
            {
                AddAdmin.IsEnabled = false;
            }
            else
            {
                AddAdmin.IsEnabled = true;
            }
        }

        private void RemoveAdmin_Click(object sender, RoutedEventArgs e)
        {
            adminData.RemoveAdmin(RemoveAdminBox.SelectedIndex);
        }

        private void AddAdmin_Click(object sender, RoutedEventArgs e)
        {
            adminData.AddAdmin(AddAdminBox.SelectedIndex);
        }

        private void SearchAdmin(object sender, RoutedEventArgs e)
        {
            if (AdminSearchBox.Text.Length > 2)
            {
                adminData.SearchEmployeesAdmin(AdminSearchBox.Text);
            }
            else
            {
                adminData.LoadData();
            }
        }
        private void OfficerRemoveSelected(object sender, RoutedEventArgs e)
        {
            if (RemoveOfficerBox.SelectedIndex == -1)
            {
                RemoveOfficer.IsEnabled = false;
            }
            else
            {
                RemoveOfficer.IsEnabled = true;
            }
        }
        private void OfficerAddSelected(object sender, RoutedEventArgs e)
        {
            if (AddOfficerBox.SelectedIndex == -1)
            {
                AddOfficer.IsEnabled = false;
            }
            else
            {
                AddOfficer.IsEnabled = true;
            }
        }
        private void SearchOfficer(object sender, RoutedEventArgs e)
        {
            if (OfficerSearchBox.Text.Length > 2)
            {
                adminData.SearchEmployeesOfficer(OfficerSearchBox.Text);
            }
            else
            {
                adminData.LoadData();
            }
        }

        private void AddOfficer_Click(object sender, RoutedEventArgs e)
        {
            adminData.AddOfficer(AddOfficerBox.SelectedIndex);
        }

        private void RemoveOfficer_Click(object sender, RoutedEventArgs e)
        {
            adminData.RemoveOfficer(RemoveOfficerBox.SelectedIndex);
        }
        private void SessionRemoveSelected(object sender, RoutedEventArgs e)
        {
            if (RemoveSessionBox.SelectedIndex == -1)
            {
                RemoveSession.IsEnabled = false;
            }
            else
            {
                RemoveSession.IsEnabled = true;
            }
        }

        private void RemoveSession_Click(object sender, RoutedEventArgs e)
        {
            adminData.RemoveSession(RemoveSessionBox.SelectedIndex);
        }

        private void SearchSession(object sender, RoutedEventArgs e)
        {
            if (SessionSearchBox.Text.Length > 2)
            {
                adminData.SearchSession(SessionSearchBox.Text);
            }
            else
            {
                adminData.LoadData();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
