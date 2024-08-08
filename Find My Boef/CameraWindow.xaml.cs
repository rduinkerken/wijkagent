using Find_My_Boef.Controller;
using Find_My_Boef.Model;
using System.Text.RegularExpressions;
using System.Windows;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for CameraWindow.xaml
    /// </summary>
    public partial class CameraWindow : Window
    {
        private static Regex s_youTubeURLIDRegex = new(@"[\?&]v=(?<v>[^&]+)");

        public CameraWindow(int id)
        {
            InitializeComponent();

            Camera camera = MainInstance.GetCameraFromId(id);

            Display(camera.Url);
        }

        public void Display(string url)
        {
            CameraView.NavigateToString(GetEmbedHtml(url));
        }
        private static string GetEmbedHtml(string url)
        {
            bool isYoutube = url.ToLower().Contains("youtube");
            bool isBlob = url.ToLower().StartsWith("blob");
            string html = "<html>";

            html += "<head>";
            html += "<meta content='IE=Edge' http-equiv='X-UA-Compatible'/>";
            html += "</head>";
            html += "<body onload=\"makeFullScreen()\">";

            if (isYoutube)
            {
                Match m = s_youTubeURLIDRegex.Match(url);
                string id = m.Groups["v"].Value;
                html += string.Format("<iframe id='video' src='https://www.youtube.com/embed/{0}?autoplay=1&mute=1' width='100%' height='600px' frameborder='0' allow=\"autoplay; encrypted-media\" allowFullScreen></iframe>", id);
            }
            else if (isBlob)
            {
                html += string.Format("<video muted='muted' width='100%' height='600px' crossorigin='anonymous' preload='none' style='position: absolute' tabindex='-1' role='application' src='{0}'></video>", url);
            }
            else
            {
                // Some websites can't be iframed - they restrict that via a code added to the site's head section or require authentication.
                // https://stackoverflow.com/questions/14425790/some-sites-do-not-allow-embedding-via-iframe-is-there-any-possibility
                html += string.Format("<iframe id='video' src='{0}' width='100%' height='600px' frameborder='0' allow=\"autoplay; encrypted-media\" allowFullScreen></iframe>", url);
            }

            html += "</body>";
            html += "</html>";

            string fullScreenScript = "<script> function requestFullScreen(element) {     var requestMethod = element.requestFullScreen || element.webkitRequestFullScreen || element.mozRequestFullScreen || element.msRequestFullscreen;     if (requestMethod) {         requestMethod.call(element);     } else if (typeof window.ActiveXObject !== \"undefined\") {         var wscript = new ActiveXObject(\"WScript.Shell\");         if (wscript !== null) {             wscript.SendKeys(\"{ F11}\");         }     } } function makeFullScreen() {     document.getElementsByTagName(\"iframe\")[0].className = \"fullScreen\";     var elem = document.body;     requestFullScreen(elem); } </script>";
            string style = "<style>     iframe.fullScreen {         width: 100%;         height: 100%;         position: absolute;         top: 0;         left: 0;     } </style>";

            html += style;
            html += fullScreenScript;

            return html;
        }
    }
}
