using System.Configuration;

namespace Find_My_Boef.Controller
{
    public class TwitterApi
    {
        public static Tweetinvi.TwitterClient TwitterClient { get; set; }

        /// <summary>
        /// Constructor setting the twitter API Client.
        /// </summary>
        public TwitterApi()
        {
            string consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
            string consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
            string accessToken = ConfigurationManager.AppSettings["AccessToken"];
            string accessTokenSecret = ConfigurationManager.AppSettings["AccessTokenSecret"];
            TwitterClient = new Tweetinvi.TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }
    }
}
