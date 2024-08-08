using Find_My_Boef.Model;
using Microsoft.ML;
using System.Collections.Generic;
using Tweetinvi.Core.Models.TwitterEntities;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Find_My_Boef.Controller
{
    public class SocialMediaData
    {
        public MLContext? MLContext;
        public ITransformer? Transformer;
        public PredictionEngine<SentimentIssue, SentimentPrediction> Engine;

        /// <summary>
        /// constructor
        /// </summary>
        public SocialMediaData()
        {
            InitializeData();
        }

        /// <summary>
        /// Check if tweet is relevant
        /// </summary>
        /// <param name="tweet">Tweet data</param>
        /// <returns>true or false</returns>
        public bool CheckPostRelevance(SocialMediaPost post)
        {
            var issue = new SentimentIssue { Text = post.Text };
            return Engine.Predict(issue).Prediction ? true : false;
        }

        /// <summary>
        /// Gets social media using the API and returns data based on relevancy
        /// </summary>
        /// <param name="latitude">Latidude for tweet daat</param>
        /// <param name="longitude">Longitude for tweet data</param>
        /// <returns>Relevant tweets</returns>
        public async IAsyncEnumerable<SocialMediaPost> GetSocialMediaData(double latitude, double longitude)
        {
            // fetch data from a social media api, in our case twitter
            var parameters = new SearchTweetsParameters(new Coordinates(latitude, longitude), 2, DistanceMeasure.Kilometers);
            var tweetResponse = await TwitterApi.TwitterClient.Search.SearchTweetsAsync(parameters);

            foreach (var tweet in tweetResponse)
            {
                // turn the tweet into an social media object
                SocialMediaPost post = TweetToGenericPost(tweet);

                if (CheckPostRelevance(post))
                {
                    yield return post;
                }
            }
        }
        public static async IAsyncEnumerable<SocialMediaPost> GetSocialMediaData(string keywords)
        {
            ITweet[] tweetResponse = await TwitterApi.TwitterClient.Search.SearchTweetsAsync(new SearchTweetsParameters(keywords)
            {
                Lang = LanguageFilter.Dutch
            });

            foreach (ITweet tweet in tweetResponse)
            {
                yield return TweetToGenericPost(tweet);
            }
        }

        public static SocialMediaPost TweetToGenericPost(ITweet tweet)
        {
            SocialMediaPost post = new()
            {
                Text = tweet.Text,
                CreatedAt = tweet.CreatedAt,
                CreatedBy = new SocialMediaPostUser
                {
                    Name = tweet.CreatedBy.Name,
                    ScreenName = tweet.CreatedBy.ScreenName,
                    Description = tweet.CreatedBy.Description
                },
                Media = new List<SocialMediaPostMedia>(),
                TopImageURL = @"Images/Tweet.png"
            };

            foreach (MediaEntity me in tweet.Media)
            {
                SocialMediaPostMedia temp = new();
                temp.MediaURL = me.MediaURL;
                post.Media.Add(temp);
            }

            return post;
        }

        /// <summary>
        /// Initialize the TrainingData
        /// </summary>
        public void InitializeData()
        {
            MLContext = new MLContext(seed: 1);
            Transformer = MLContext.Model.Load("..\\..\\..\\..\\Find my boef\\controller\\Training\\TrainData.ZIP", out _);
            Engine = MLContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(Transformer);
        }
    }
}