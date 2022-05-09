using System.Windows.Controls;
using System.Windows.Input;

namespace SportsManager.Views
{
    /// <summary>
    /// Interaction logic for AddSeasonView.xaml
    /// </summary>
    public partial class AddSeasonView : UserControl
    {
        public AddSeasonView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Validates the textbox input.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void IntegerValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //do not allow futher incorrect typing
            e.Handled = !int.TryParse(((TextBox)sender).Text + e.Text, out _) && !int.TryParse(e.Text + ((TextBox)sender).Text, out _) && e.Text != "-";
        }

        /// <summary>
        /// Corrects the textbox input.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void IntegerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(((TextBox)sender).Text, out int j) && ((TextBox)sender).Text != "-")
            {
                //delete incoret input
                ((TextBox)sender).Text = "";
            }
            else
            {
                //delete leading zeros
                if (((TextBox)sender).Text != "-")
                {
                    ((TextBox)sender).Text = j.ToString();
                }
            }
        }

        /// <summary>
        /// Validates the textbox input.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void ValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            int max = 1;
            int min = 1;
            switch (((TextBox)sender).Name)
            {
                case "QualificationCountTextBox":
                    max = 16;
                    break;
                case "QualificationMaxCompetitorsTextBox":
                    max = 256;
                    break;
                case "QualificationRoundsTextBox":
                    max = 6;
                    break;
                case "QualificationRoundOfTextBox":
                    max = 64;
                    break;
                case "GroupCountTextBox":
                    max = 16;
                    break;
                case "PlayOffRoundsTextBox":
                    max = 8;
                    break;
                case "PlayOffBestOfTextBox":
                    max = 9;
                    break;
                case "PlayOffRoundOfTextBox":
                    max = 256;
                    min = 2;
                    break;
                case "PlayOffFirstToTextBox":
                    max = 5;
                    break;
                default:
                    break;
            }

            //do not allow futher incorrect typing
            if (((TextBox)sender).Name == "PlayOffBestOfTextBox")
            {
                e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int h) && h >= min && h <= max && h % 2 == 1);
            }
            else
            {
                e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int i) && i >= min && i <= max);
            }
        }

        /// <summary>
        /// Corrects the textbox input.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int max = 1;
            int min = 1;
            switch (((TextBox)sender).Name)
            {
                case "QualificationCountTextBox":
                    max = 16;
                    break;
                case "QualificationMaxCompetitorsTextBox":
                    max = 256;
                    break;
                case "QualificationRoundsTextBox":
                    max = 6;
                    break;
                case "QualificationRoundOfTextBox":
                    max = 64;
                    break;
                case "GroupCountTextBox":
                    max = 16;
                    break;
                case "PlayOffRoundsTextBox":
                    max = 8;
                    break;
                case "PlayOffBestOfTextBox":
                    max = 9;
                    break;
                case "PlayOffRoundOfTextBox":
                    max = 256;
                    min = 2;
                    break;
                case "PlayOffFirstToTextBox":
                    max = 5;
                    break;
                default:
                    break;
            }

            if (!int.TryParse(((TextBox)sender).Text, out int j) || j < min || j > max || (((TextBox)sender).Name == "PlayOffBestOfTextBox" && j % 2 == 0))
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
