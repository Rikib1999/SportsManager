using System.Windows.Controls;
using System.Windows.Input;

namespace SportsManager.Views
{
    /// <summary>
    /// Interaction logic for StandingsView.xaml
    /// </summary>
    public partial class StandingsView : UserControl
    {
        public StandingsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Overrides the page scrolling.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        }
    }
}