using System.Windows.Controls;
using System.Windows.Input;

namespace CSharpZapoctak.Views
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

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            int max = 250;
            int min = 1;

            if (((TextBox)sender).Name == "NumberTextBox" || ((TextBox)sender).Name == "EditNumberTextBox") { max = 99; }

            //do not allow futher incorrect typing
            e.Handled = !(int.TryParse(((TextBox)sender).Text + e.Text, out int i) && i >= min && i <= max);
        }

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
