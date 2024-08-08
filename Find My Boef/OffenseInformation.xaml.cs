using Find_My_Boef.Controller;
using Find_My_Boef.DataContext;
using Find_My_Boef.Model;
using Find_My_Boef.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for OffenseInformation.xaml
    /// </summary>
    public partial class OffenseInformation : Window
    {
        private SocialMediaData _twitterData { get; set; }
        private int _offenseId { get; set; }
        public OffenseDataContext OffenseDataContext;
        private bool isEdit { get; set; }

        /// <summary> 
        /// Constructor
        /// </summary>
        public OffenseInformation(int offenseId)
        {
            OffenseDataContext = new();
            DataContext = OffenseDataContext;
            OffenseDataContext.RefreshAssignedOfficerList(offenseId);
            Offense offense = OffenseDataContext.CurrentHistoryWindow.GetHistoryOffensesFromID(offenseId);
            _twitterData = new SocialMediaData();
            _ = AddTwitterDataAsync(offense.Location.Lat, offense.Location.Lng);
            _offenseId = offenseId;
            InitializeComponent();
        }




        /// <summary>
        /// Add twitterdata to the OffenseInformation window
        /// </summary>
        /// <param name="latitude">Latitude for social media data</param>
        /// <param name="longitude">Longitude for social media data</param>
        /// <returns>none</returns>
        private async Task AddTwitterDataAsync(double latitude, double longitude)
        {
            //get twitter data
            IAsyncEnumerable<SocialMediaPost> posts = _twitterData.GetSocialMediaData(latitude, longitude);
            int margin = 0;

            if (posts != null)
            {
                await foreach (SocialMediaPost post in posts)
                {
                    DrawSocialMediaPostBox(post, margin);
                    margin += 250;
                }
            }
        }

        /// <summary>
        /// Draw the Postbox
        /// </summary>
        /// <param name="post">This is the post data</param>
        /// <param name="margin">Margin given for drawing the box</param>
        private void DrawSocialMediaPostBox(SocialMediaPost post, int margin)
        {
            //Create border
            Border border = new Border();
            border.BorderBrush = Brushes.White;
            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F2223"));
            border.BorderThickness = new Thickness(1);
            border.HorizontalAlignment = HorizontalAlignment.Left;
            border.Margin = new Thickness(margin, 0, 0, 0);
            border.Height = 244;
            border.Width = 200;
            border.VerticalAlignment = VerticalAlignment.Top;

            //Add grid
            Grid grid = new Grid();
            grid.Margin = new Thickness(0, 0, -1, -1);
            grid.Height = 244;

            //Time label
            Label timeLabel = new Label();
            timeLabel.Content = "Tijd:";
            timeLabel.HorizontalAlignment = HorizontalAlignment.Left;
            timeLabel.Margin = new Thickness(3, 40, 0, 0);
            timeLabel.VerticalAlignment = VerticalAlignment.Top;
            timeLabel.FontSize = 12;
            timeLabel.Foreground = Brushes.White;
            timeLabel.FontWeight = FontWeight.FromOpenTypeWeight(700);
            timeLabel.Padding = new Thickness(1, 1, 1, 1);
            grid.Children.Add(timeLabel);


            //Time of publishing
            TextBlock time = new TextBlock();
            time.HorizontalAlignment = HorizontalAlignment.Left;
            time.Margin = new Thickness(50, 40, 0, 0);
            time.TextWrapping = TextWrapping.Wrap;
            time.VerticalAlignment = VerticalAlignment.Top;
            time.FontSize = 12;
            time.Foreground = Brushes.White;
            time.Text = post.CreatedAt.LocalDateTime.ToShortTimeString();
            grid.Children.Add(time);

            //Date label
            Label dateLabel = new Label();
            dateLabel.Content = "Datum:";
            dateLabel.HorizontalAlignment = HorizontalAlignment.Left;
            dateLabel.Margin = new Thickness(3, 58, 0, 0);
            dateLabel.VerticalAlignment = VerticalAlignment.Top;
            dateLabel.FontSize = 12;
            dateLabel.Foreground = Brushes.White;
            dateLabel.FontWeight = FontWeight.FromOpenTypeWeight(700);
            dateLabel.Padding = new Thickness(1, 1, 1, 1);
            grid.Children.Add(dateLabel);

            //Date of publisheing
            TextBlock dateText = new TextBlock();
            dateText.HorizontalAlignment = HorizontalAlignment.Left;
            dateText.Margin = new Thickness(50, 58, 0, 0);
            dateText.TextWrapping = TextWrapping.Wrap;
            dateText.VerticalAlignment = VerticalAlignment.Top;
            dateText.FontSize = 12;
            dateText.Foreground = Brushes.White;
            dateText.Text = post.CreatedAt.LocalDateTime.ToShortDateString();
            grid.Children.Add(dateText);

            //Twitter logo
            Image img = new();
            img.Source = new BitmapImage(new Uri(post.TopImageURL, UriKind.RelativeOrAbsolute));
            img.HorizontalAlignment = HorizontalAlignment.Left;
            img.Height = 35;
            img.Width = 35;
            img.VerticalAlignment = VerticalAlignment.Top;
            grid.Children.Add(img);

            //Author of tweet
            Label author = new Label();
            author.Content = "Geschreven door: " + post.CreatedBy.ScreenName;
            author.HorizontalAlignment = HorizontalAlignment.Left;
            author.Margin = new Thickness(3, 80, 0, 0);
            author.VerticalAlignment = VerticalAlignment.Top;
            author.FontSize = 12;
            author.Foreground = Brushes.White;
            author.FontWeight = FontWeight.FromOpenTypeWeight(700);
            author.Padding = new Thickness(1, 1, 1, 1);
            grid.Children.Add(author);

            //Scrollviewer for message
            ScrollViewer scrollviewer = new ScrollViewer();
            scrollviewer.HorizontalAlignment = HorizontalAlignment.Left;
            scrollviewer.Height = 148;
            scrollviewer.Margin = new Thickness(3, 96, 3, 0);
            scrollviewer.VerticalAlignment = VerticalAlignment.Top;
            scrollviewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            grid.Children.Add(scrollviewer);

            //Message of tweet
            TextBlock message = new();
            message.TextWrapping = TextWrapping.Wrap;
            message.VerticalAlignment = VerticalAlignment.Top;
            message.HorizontalAlignment = HorizontalAlignment.Left;

            string[] words = Regex.Replace(post.Text, @"\r\n?|\n?", "").Split(" ");
            bool lastWordIsAt = false;
            foreach (string word in words)
            {
                TextBlock wordLable = new();
                wordLable.FontSize = 12;
                wordLable.Text = word;
                wordLable.Margin = new Thickness(1, 0, 1, 0);

                if (word.StartsWith('#') || word.StartsWith('@') || lastWordIsAt)
                {
                    wordLable.Foreground = Brushes.LightBlue;

                    if (lastWordIsAt)
                    {
                        lastWordIsAt = false;
                    }
                }
                else
                {
                    wordLable.Foreground = Brushes.White;
                }

                // make link clickable link
                if (word.StartsWith("http"))
                {
                    Hyperlink link = new();
                    link.IsEnabled = true;
                    link.Inlines.Add(word);
                    link.NavigateUri = new Uri(word);
                    link.Click += new RoutedEventHandler((s, e) => link_Click(s, e, word));
                    link.FontSize = 12;

                    message.Inlines.Add(link);

                    continue;
                }

                // if a single space is used after an @, the next word will be the username
                if (word == "@" || word == "@ ")
                {
                    lastWordIsAt = true;
                }
                message.Inlines.Add(wordLable);
            }
            scrollviewer.Content = message;

            //Add image to scrollviewer
            if (post.Media != null)
            {
                Image tweetImg = new();
                foreach (var media in post.Media)
                {
                    tweetImg.Source = new BitmapImage(new Uri(media.MediaURL));
                    tweetImg.HorizontalAlignment = HorizontalAlignment.Left;
                    tweetImg.Width = 194;
                    tweetImg.VerticalAlignment = VerticalAlignment.Top;
                    message.Inlines.Add(tweetImg);
                }
            }

            //add components to scrollpane
            border.Child = grid;
            Tweetbox.Children.Add(border);
        }
        public void link_Click(object sender, RoutedEventArgs args, string link)
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }

        /// <summary>
        /// Route to offense screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditClick(object sender, RoutedEventArgs e)
        {
            OffenseDataContext.CreateEditOffenseWindow(_offenseId, this);
            OffenseDataContext.AmountOfOffenseInformationScreensOpen--;
            isEdit = true;
            Close();
        }

        private void OffenseInformation_Closed(object sender, EventArgs e)
        {
            OffenseDataContext.AmountOfOffenseInformationScreensOpen = 0;
            if (OffenseDataContext.CurrentHistoryWindow != null && isEdit == false)
            {
                OffenseDataContext.DisplayOffenseHistoryWindow();
            }
            Close();
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            OffenseDataContext.DeleteOffense(_offenseId);
            OffenseDataContext.AmountOfOffenseInformationScreensOpen--;
            if (OffenseDataContext.CurrentHistoryWindow != null) OffenseDataContext.CurrentHistoryWindow.ReloadOffenses();
            MainInstance.ReloadOffenses();
            Visualization.ReloadHeatmaps();
            Close();
        }
    }
}
