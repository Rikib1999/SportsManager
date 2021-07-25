using System.Windows.Controls;
using System.Windows.Input;

namespace CSharpZapoctak.Views
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

        private void ValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            int max = 1;
            switch (((TextBox)sender).Name)
            {
                case "QualificationCountTextBox":
                    max = 16;
                    break;
                case "QualificationRoundsTextBox":
                    max = 10;
                    break;
                case "QualificationRoundOfTextBox":
                    max = 1024;
                    break;
                case "GroupCountTextBox":
                    max = 16;
                    break;
                case "PlayOffRoundsTextBox":
                    max = 10;
                    break;
                case "PlayOffBestOfTextBox":
                    max = 9;
                    break;
                case "PlayOffRoundOfTextBox":
                    max = 1024;
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
                e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int h) && h >= 1 && h <= max && h % 2 == 1);
            }
            else
            {
                e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int i) && i >= 1 && i <= max);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int max = 1;
            switch (((TextBox)sender).Name)
            {
                case "QualificationCountTextBox":
                    max = 16;
                    break;
                case "QualificationRoundsTextBox":
                    max = 10;
                    break;
                case "QualificationRoundOfTextBox":
                    max = 1024;
                    break;
                case "GroupCountTextBox":
                    max = 16;
                    break;
                case "PlayOffRoundsTextBox":
                    max = 10;
                    break;
                case "PlayOffBestOfTextBox":
                    max = 9;
                    break;
                case "PlayOffRoundOfTextBox":
                    max = 1024;
                    break;
                case "PlayOffFirstToTextBox":
                    max = 5;
                    break;
                default:
                    break;
            }

            if (!int.TryParse(((TextBox)sender).Text, out int j) || j < 1 || j > max || (((TextBox)sender).Name == "PlayOffBestOfTextBox" && j % 2 == 0))
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
