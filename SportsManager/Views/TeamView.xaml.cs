using System.Windows.Controls;
using System.Windows.Input;

namespace SportsManager.Views
{
    /// <summary>
    /// Interaction logic for TeamView.xaml
    /// </summary>
    public partial class TeamView : UserControl
    {
        public TeamView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Overrides the page scrolling.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        /// <summary>
        /// Validates the textbox input.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void ValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            int max = 250;
            int min = 1;

            if (((TextBox)sender).Name == "NumberTextBox" || ((TextBox)sender).Name == "EditNumberTextBox") { max = 99; }

            //do not allow futher incorrect typing
            e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int i) && i >= min && i <= max);
        }

        /// <summary>
        /// Corrects the textbox input.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int max = 250;
            int min = 1;

            if (((TextBox)sender).Name == "NumberTextBox" || ((TextBox)sender).Name == "EditNumberTextBox") { max = 99; }

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
