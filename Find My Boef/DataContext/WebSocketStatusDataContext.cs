using System.Windows.Controls;
using System.Windows.Media;

namespace Find_My_Boef.DataContext
{
    public class WebSocketStatusDataContext
    {
        public TextBlock Target;

        private string _statusMessage;

        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                _statusMessage = value;
                Target.Text = value;
            }
        }

        private SolidColorBrush _statusColor;

        public SolidColorBrush StatusColor
        {
            get
            {
                return _statusColor;
            }
            set
            {
                _statusColor = value;
                Target.Foreground = value;
            }
        }
    }
}
