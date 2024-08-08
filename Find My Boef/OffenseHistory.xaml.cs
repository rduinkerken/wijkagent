using Find_My_Boef.Controller;
using Find_My_Boef.DataContext;
using Find_My_Boef.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for OffenseHistory.xaml
    /// </summary>
    public partial class OffenseHistory : Window
    {
        private List<Offense> _offenses = new List<Offense>();
        private bool _hasInitialized = false; //Set to true when initialized to prevent Onchange events from happening

        /// <summary>
        /// Constructor
        /// </summary>
        public OffenseHistory()
        {
            this._offenses = OffenseData.LoadOffenses(false);
            this._offenses.Reverse();
            InitializeComponent();
            AddComponentData();
            _hasInitialized = true;
            LoadOffensesIntoGrid();
        }

        public void ReloadOffenses()
        {
            this._offenses = OffenseData.LoadOffenses(false);
            this._offenses.Reverse();
            LoadOffensesIntoGrid();
        }

        /// <summary>
        /// Add the component data to the window
        /// </summary>
        private void AddComponentData()
        {
            List<string> typeList = new List<string> { "Alle" };
            typeList.AddRange(Enum.GetNames(typeof(OffenseType)).Cast<string>().ToList());
            TypeBox.ItemsSource = typeList;
            TypeBox.SelectedIndex = 0;
            StartDate.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate.SelectedDate = StartDate.SelectedDate.Value.AddMonths(1).AddDays(-1);
        }

        private void Finished_Checked(object sender, RoutedEventArgs e)
        {
            LoadOffensesIntoGrid();
        }

        private void Doing_Checked(object sender, RoutedEventArgs e)
        {
            LoadOffensesIntoGrid();
        }

        private void Unfinished_Checked(object sender, RoutedEventArgs e)
        {
            LoadOffensesIntoGrid();
        }

        private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOffensesIntoGrid();
        }

        private void StartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOffensesIntoGrid();
        }

        private void EndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOffensesIntoGrid();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadOffensesIntoGrid();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e, int id)
        {
            OffenseDataContext.CreateOffenseWindow(id, true);
            Hide();
            double height = SystemParameters.WorkArea.Height;
            double width = SystemParameters.WorkArea.Width;
            this.Top = (height - this.Height) / 2;
            this.Left = (width - this.Width) / 2;
        }

        /// <summary>
        /// Filters the list based on filled in data
        /// </summary>
        /// <param name="offenses"></param>
        /// <returns>A filtered list of type Offense</returns>
        private List<Offense> FilterItems()
        {
            List<Offense> filteredOffenses = new List<Offense>();
            foreach (Offense offense in _offenses)
            {
                if (offense.Type.ToString() == TypeBox.SelectedValue.ToString() || TypeBox.SelectedValue.ToString() == "Alle")
                {
                    if (offense.Time.Date >= StartDate.SelectedDate.Value.Date && offense.Time.Date <= EndDate.SelectedDate.Value.Date)
                    {
                        if (offense.Description.ToLower().Contains(Search.Text.ToLower()) || offense.ID.ToString().Contains(Search.Text))
                        {
                            if (!Finished.IsChecked == true && offense.Status == Status.Processed) continue;
                            if (!Doing.IsChecked == true && offense.Status == Status.InProgress) continue;
                            if (!Unfinished.IsChecked == true && offense.Status == Status.NotVisited) continue;
                            filteredOffenses.Add(offense);
                        }
                    }
                }
            }
            return filteredOffenses;
        }

        /// <summary>
        /// Loads offenses into xaml
        /// </summary>
        /// <param name="offenses"></param>
        private void LoadOffensesIntoGrid()
        {
            if (_hasInitialized)
            {
                OffenseBox.Children.Clear();
                int Margin = 0;
                List<Offense> filteredOffenses = FilterItems();
                foreach (Offense offense in filteredOffenses)
                {
                    Border border = new Border();
                    border.Cursor = Cursors.Hand;
                    border.BorderBrush = Brushes.Black;
                    border.BorderThickness = new Thickness(1);
                    border.Height = 100;
                    border.VerticalAlignment = VerticalAlignment.Top;
                    border.Margin = new Thickness(0, Margin, 0, 0);
                    Margin += 100;
                    border.MouseDown += new System.Windows.Input.MouseButtonEventHandler((s, e) => Border_MouseDown(s, e, offense.ID));

                    Grid grid = new Grid();

                    Label label = new Label();
                    label.Content = "Delict";
                    label.HorizontalAlignment = HorizontalAlignment.Left;
                    label.VerticalAlignment = VerticalAlignment.Top;
                    label.FontSize = 16;
                    label.Height = 33;
                    label.Margin = new Thickness(-5, 0, 0, 0);
                    label.Foreground = Brushes.White;
                    label.FontWeight = FontWeights.Bold;
                    grid.Children.Add(label);

                    Label id = new Label();
                    id.Content = offense.ID;
                    id.HorizontalAlignment = HorizontalAlignment.Left;
                    id.VerticalAlignment = VerticalAlignment.Top;
                    id.FontSize = 16;
                    id.Height = 33;
                    id.Margin = new Thickness(45, 0, 0, 0);
                    id.Width = 80;
                    id.Foreground = Brushes.White;
                    id.FontWeight = FontWeights.Bold;
                    grid.Children.Add(id);

                    Label date = new Label();
                    date.Content = "Datum:";
                    date.HorizontalAlignment = HorizontalAlignment.Left;
                    date.VerticalAlignment = VerticalAlignment.Top;
                    date.FontSize = 16;
                    date.Height = 33;
                    date.Foreground = Brushes.White;
                    date.Margin = new Thickness(125, 0, 0, 0);
                    date.FontWeight = FontWeights.Bold;
                    grid.Children.Add(date);

                    Label dataData = new Label();
                    dataData.Content = offense.Time;
                    dataData.HorizontalAlignment = HorizontalAlignment.Left;
                    dataData.VerticalAlignment = VerticalAlignment.Top;
                    dataData.FontSize = 16;
                    dataData.Height = 33;
                    dataData.Foreground = Brushes.White;
                    dataData.Margin = new Thickness(185, 0, 0, 0);
                    grid.Children.Add(dataData);

                    Label type = new Label();
                    type.Content = "Type:";
                    type.HorizontalAlignment = HorizontalAlignment.Left;
                    type.VerticalAlignment = VerticalAlignment.Top;
                    type.FontSize = 16;
                    type.Height = 33;
                    type.Foreground = Brushes.White;
                    type.Margin = new Thickness(340, 0, 0, 0);
                    type.FontWeight = FontWeights.Bold;
                    grid.Children.Add(type);

                    Label typeData = new Label();
                    typeData.Content = offense.Type;
                    typeData.HorizontalAlignment = HorizontalAlignment.Left;
                    typeData.VerticalAlignment = VerticalAlignment.Top;
                    typeData.FontSize = 16;
                    typeData.Height = 33;
                    typeData.Foreground = Brushes.White;
                    typeData.Margin = new Thickness(385, 0, 0, 0);
                    grid.Children.Add(typeData);

                    Label status = new Label();
                    status.Content = "Status:";
                    status.HorizontalAlignment = HorizontalAlignment.Left;
                    status.VerticalAlignment = VerticalAlignment.Top;
                    status.FontSize = 16;
                    status.Height = 33;
                    status.Foreground = Brushes.White;
                    status.Margin = new Thickness(470, 0, 0, 0);
                    status.FontWeight = FontWeights.Bold;
                    grid.Children.Add(status);

                    Label statusData = new Label();
                    statusData.Content = offense.Status;
                    statusData.HorizontalAlignment = HorizontalAlignment.Left;
                    statusData.VerticalAlignment = VerticalAlignment.Top;
                    statusData.FontSize = 16;
                    statusData.Height = 33;
                    statusData.Foreground = Brushes.White;
                    statusData.Margin = new Thickness(525, 0, 0, 0);
                    grid.Children.Add(statusData);

                    TextBlock description = new TextBlock();
                    description.Text = offense.Description;
                    description.HorizontalAlignment = HorizontalAlignment.Left;
                    description.VerticalAlignment = VerticalAlignment.Top;
                    description.FontSize = 16;
                    description.Height = 70;
                    description.Width = 678;
                    description.Foreground = Brushes.White;
                    description.Margin = new Thickness(0, 25, 0, 0);
                    description.TextWrapping = TextWrapping.Wrap;
                    grid.Children.Add(description);

                    border.Child = grid;
                    OffenseBox.Children.Add(border);


                }
            }
        }
        public Offense GetHistoryOffensesFromID(int id)
        {
            foreach (Offense offense in _offenses)
            {
                if (offense.ID == id)
                {
                    return offense;
                }
            }

            return null;
        }

        private void OffenseHistory_Closed(object sender, EventArgs e)
        {
            OffenseDataContext.AmountOfHistoryScreensOpen = 0;
            OffenseDataContext.CurrentHistoryWindow = null;
            Close();
        }
    }
}
