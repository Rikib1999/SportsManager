using System.Windows.Controls;
using System.Windows.Input;

namespace CSharpZapoctak.Views
{
    /// <summary>
    /// Interaction logic for AddMatchView.xaml
    /// </summary>
    public partial class AddMatchView : UserControl
    {
        public AddMatchView()
        {
            InitializeComponent();
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        }

        private void ValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            int max = 1;
            int min = 0;
            switch (((TextBox)sender).Name)
            {
                case "Hours":
                    max = 23;
                    break;
                case "Minutes":
                    max = 59;
                    break;
                case "Periods":
                    max = 12;
                    break;
                case "PeriodDuration":
                    max = 600;
                    break;
                case "PeriodMinute":
                    max = 700;
                    break;
                case "PeriodSecond":
                    max = 59;
                    break;
                case "EndPeriodMinute":
                    max = 700;
                    break;
                case "EndPeriodSecond":
                    max = 59;
                    break;
                case "ShootoutSeries":
                    max = 99;
                    break;
                default:
                    break;
            }

            //do not allow futher incorrect typing
            e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int i) && i >= min && i <= max);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int max = 1;
            int min = 0;
            switch (((TextBox)sender).Name)
            {
                case "Hours":
                    max = 23;
                    break;
                case "Minutes":
                    max = 59;
                    break;
                case "Periods":
                    max = 12;
                    break;
                case "PeriodDuration":
                    max = 600;
                    break;
                case "PeriodMinute":
                    max = 700;
                    break;
                case "PeriodSecond":
                    max = 59;
                    break;
                case "EndPeriodMinute":
                    max = 700;
                    break;
                case "EndPeriodSecond":
                    max = 59;
                    break;
                case "ShootoutSeries":
                    max = 99;
                    break;
                default:
                    break;
            }

            if (!int.TryParse(((TextBox)sender).Text, out int j) || j < min || j > max)
            {
                //delete incoret input
                ((TextBox)sender).Text = "";
            }
            else
            {
                //delete leading zeros
                ((TextBox)sender).Text = j.ToString();
            }
        }
    }
}
