using SportsManager.Commands;
using SportsManager.Others;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Template viewmodel for viewing all entites and theirs statistics in one sortable table with filtering. Serves for navigating to an entity detail viewmodel.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    public class TemplateSelectionDataGridViewModel<T> : NotifyPropertyChanged where T : IEntity
    {
        #region Commands
        /// <summary>
        /// After executing it, it navigates to the entity detail viewmodel.
        /// </summary>
        public ICommand NavigateEntityCommand { get; set; }

        private ICommand checkNavigateEntityCommand;
        /// <summary>
        /// Command that checks wheter entity was selected, so it can navigate its detail viewmodel, when executed.
        /// </summary>
        public ICommand CheckNavigateEntityCommand
        {
            get
            {
                if (checkNavigateEntityCommand == null)
                {
                    checkNavigateEntityCommand = new RelayCommand(param => CheckNavigateEntity(null));
                }
                return checkNavigateEntityCommand;
            }
        }

        private ICommand exportPDFCommand;
        /// <summary>
        /// When executed, it calls method Exports.ExportTable() for exporting the selection in PDF format.
        /// </summary>
        public ICommand ExportPDFCommand
        {
            get
            {
                if (exportPDFCommand == null)
                {
                    exportPDFCommand = new RelayCommand(param => Exports.ExportTable((System.Windows.Controls.DataGrid)param, "PDF", ExportTop));
                }
                return exportPDFCommand;
            }
        }

        private ICommand exportXLSXCommand;
        /// <summary>
        /// When executed, it calls method Exports.ExportTable() for exporting the selection in XLSX format.
        /// </summary>
        public ICommand ExportXLSXCommand
        {
            get
            {
                if (exportXLSXCommand == null)
                {
                    exportXLSXCommand = new RelayCommand(param => Exports.ExportTable((System.Windows.Controls.DataGrid)param, "XLSX", ExportTop));
                }
                return exportXLSXCommand;
            }
        }
        #endregion

        private int? exportTop;
        /// <summary>
        /// Number of top rows to be exported. If null, all rows will be exported.
        /// </summary>
        public int? ExportTop
        {
            get => exportTop;
            set
            {
                exportTop = value;
                OnPropertyChanged();
            }
        }

        #region Visibilities
        private bool showPhoto = true;
        /// <summary>
        /// Switches the visibility of photo column.
        /// </summary>
        public bool ShowPhoto
        {
            get => showPhoto;
            set
            {
                showPhoto = value;
                PhotoVisibility = showPhoto ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private bool showInfo = true;
        /// <summary>
        /// Switches the visibility of info columns.
        /// </summary>
        public bool ShowInfo
        {
            get => showInfo;
            set
            {
                showInfo = value;
                InfoVisibility = showInfo ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private bool showStats = true;
        /// <summary>
        /// Switches the visibility of statistics columns.
        /// </summary>
        public bool ShowStats
        {
            get => showStats;
            set
            {
                showStats = value;
                StatsVisibility = showStats ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private bool showExtendedStats = true;
        /// <summary>
        /// Switches the visibility of extended statistics columns.
        /// </summary>
        public bool ShowExtendedStats
        {
            get => showExtendedStats;
            set
            {
                showExtendedStats = value;
                ExtendedStatsVisibility = showExtendedStats ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private Visibility photoVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of photo column.
        /// </summary>
        public Visibility PhotoVisibility
        {
            get => photoVisibility;
            set
            {
                photoVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility infoVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of info columns.
        /// </summary>
        public Visibility InfoVisibility
        {
            get => infoVisibility;
            set
            {
                infoVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility statsVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of statistics columns.
        /// </summary>
        public Visibility StatsVisibility
        {
            get => statsVisibility;
            set
            {
                statsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility extendedStatsVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of extended statistics columns.
        /// </summary>
        public Visibility ExtendedStatsVisibility
        {
            get => extendedStatsVisibility;
            set
            {
                extendedStatsVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Currently selected entity from the list.
        /// </summary>
        public T SelectedEntity { get; set; }

        /// <summary>
        /// Collection of all entities to be shown.
        /// </summary>
        public ObservableCollection<T> Entities { get; set; }

        /// <summary>
        /// Virtual method for loading entities and their statistics and populating the Entities collection with them.
        /// </summary>
        protected virtual void LoadData() { }

        /// <summary>
        /// Checks wheter there is a selected entity.
        /// </summary>
        /// <param name="entity">Instance of the entity.</param>
        protected void CheckNavigateEntity(object entity)
        {
            if (SelectedEntity != null)
            {
                NavigateEntityCommand.Execute(entity);
            }
        }
    }
}