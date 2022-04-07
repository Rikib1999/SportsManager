using CSharpZapoctak.Commands;
using CSharpZapoctak.Others;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    public class TemplateSelectionDataGridViewModel<T> : NotifyPropertyChanged where T : IEntity
    {
        #region Commands
        public ICommand NavigateEntityCommand { get; set; }

        private ICommand checkNavigateEntityCommand;
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

        private Visibility photoVisibility = Visibility.Visible;
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
        public Visibility StatsVisibility
        {
            get => statsVisibility;
            set
            {
                statsVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public T SelectedEntity { get; set; }

        public ObservableCollection<T> Entities { get; set; }

        protected virtual void LoadData() { }

        protected void CheckNavigateEntity(object entity)
        {
            if (SelectedEntity != null)
            {
                NavigateEntityCommand.Execute(entity);
            }
        }
    }
}