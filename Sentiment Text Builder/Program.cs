using Find_My_Boef.Controller;
using Find_My_Boef.Model;
using System.Configuration;
using System.Text.RegularExpressions;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

string FOLDERPATH = "..\\..\\..\\..\\Find My Boef\\Controller\\Training\\";
string TRAININGTXT = string.Format("{0}\\TrainingData.txt", FOLDERPATH);
string Keywords = "";
string[] AllCurrentText;
List<string> NewText = new();
TwitterClient TwitterClient;

// driver logic
if (File.Exists(TRAININGTXT))
{
    Console.WriteLine("write \"save\" to save, \"train\" to train the model, anything else will fetch tweets with the input as keywords");
    Console.WriteLine("write \"1\" or \"y\" if the text is relevant");
    string ans = Console.ReadLine();

    Init();

    if (ans == "train")
    {
        SentimentTraining.Train(TRAININGTXT);
        Console.WriteLine("Training done");
        Environment.Exit(1);
    }

    Keywords = ans;

    if (ans == "")
    {
        Keywords = "*";
    }

    // if somthing goes wrong, save progress
    try
    {
        MainLoop().Wait();
    }
    catch (Exception e)
    {
        Console.WriteLine(string.Format("Error: {0}", e.Message));
        SaveAll();
    }
}
else
{
    Console.WriteLine("Text file could not be read");
    Environment.Exit(1);
}
async Task MainLoop()
{
    while (true)
    {
        IAsyncEnumerable<SocialMediaPost> posts = SocialMediaData.GetSocialMediaData(Keywords);

        if (posts != null)
        {
            await foreach (SocialMediaPost post in posts)
            {
                string text = CleanText(post.Text);

                if (TweetExistsinFile(text))
                {
                    continue;
                }

                Query(text);
            }
        }
    }
}

// displays fetched tweet and add to list
void Query(string text)
{
    Console.WriteLine(text);

    string ans = Console.ReadLine();

    if (ans == "save")
    {
        SaveAll();
    }

    NewText.Add(string.Format("{0},{1}", (ans == "1" || ans == "y") ? "1" : "0", text));
}
// fetch tweets with Tweetinvi 
void Init()
{
    //Retrieve config
    ExeConfigurationFileMap configMap = new()
    {
        ExeConfigFilename = "..\\..\\..\\..\\Find My Boef\\App.config"
    };
    Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
    KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;
    foreach (KeyValueConfigurationElement setting in settings)
    {
        ConfigurationManager.AppSettings.Set(setting.Key, setting.Value);
    }

    string consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
    string consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
    string accessToken = ConfigurationManager.AppSettings["AccessToken"];
    string accessTokenSecret = ConfigurationManager.AppSettings["AccessTokenSecret"];
    TwitterClient = new TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);

    AllCurrentText = File.ReadAllLines(TRAININGTXT);
}

// check if tweet is already in text file
bool TweetExistsinFile(string txt)
{
    bool r = false;

    foreach (string tweet in AllCurrentText)
    {
        // remove first 2 chars
        if (tweet.Remove(0, 2) == txt)
        {
            r = true;
        }
    }
    foreach (string s in NewText)
    {
        if (s.Remove(0, 2) == txt)
        {
            r = true;
        }
    }

    return r;
}

// save all text in list to text file
void SaveAll()
{
    foreach (string s in NewText)
    {
        File.AppendAllText(TRAININGTXT, Environment.NewLine + s);
    }
    Environment.Exit(1);
}

// remove newlines
string CleanText(string text)
{
    if (text == null)
    {
        return "";
    }

    text = Regex.Replace(text, @"[\r\n|\b|\n]", " ", RegexOptions.IgnoreCase);

    return text;
}