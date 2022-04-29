using System.Windows.Controls;
using System.Windows.Input;

namespace SportsManager.Views
{
    /// <summary>
    /// Interaction logic for MatchesSelectionView.xaml
    /// </summary>
    public partial class MatchesSelectionView : UserControl
    {
        public MatchesSelectionView()
        {
            InitializeComponent();
        }

        private void ValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            int max = int.MaxValue;
            int min = 1;

            //do not allow futher incorrect typing
            e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int i) && i >= min && i <= max);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int max = int.MaxValue;
            int min = 1;

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