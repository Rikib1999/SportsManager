using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CSharpZapoctak.ViewModels
{
    class AddSeasonViewModel : ViewModelBase
    {
        #region Properties

        #region Variables
        private Season currentSeason;
        public Season CurrentSeason
        {
            get { return currentSeason; }
            set
            {
                currentSeason = value;
                OnPropertyChanged();
            }
        }

        private Team newTeam;
        public Team NewTeam
        {
            get { return newTeam; }
            set
            {
                newTeam = value;
                OnPropertyChanged();
            }
        }

        private Team existingTeam;
        public Team ExistingTeam
        {
            get { return existingTeam; }
            set
            {
                existingTeam = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> existingTeams;
        public ObservableCollection<Team> ExistingTeams
        {
            get { return existingTeams; }
            set
            {
                existingTeams = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> teams;
        public ObservableCollection<Team> Teams
        {
            get { return teams; }
            set
            {
                teams = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage bitmapSeason;
        public BitmapImage BitmapSeason
        {
            get { return bitmapSeason; }
            set
            {
                bitmapSeason = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage bitmapTeam;
        public BitmapImage BitmapTeam
        {
            get { return bitmapTeam; }
            set
            {
                bitmapTeam = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Country> countries;
        public ObservableCollection<Country> Countries
        {
            get { return countries; }
            set
            {
                countries = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> notSelectedTeams;
        public ObservableCollection<Team> NotSelectedTeams
        {
            get { return notSelectedTeams; }
            set
            {
                notSelectedTeams = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Group> groups;
        public ObservableCollection<Group> Groups
        {
            get { return groups; }
            set
            {
                groups = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        private ICommand loadSeasonImageCommand;
        public ICommand LoadSeasonImageCommand
        {
            get
            {
                if (loadSeasonImageCommand == null)
                {
                    loadSeasonImageCommand = new RelayCommand(param => LoadImage(CurrentSeason));
                }
                return loadSeasonImageCommand;
            }
        }

        private ICommand loadTeamImageCommand;
        public ICommand LoadTeamImageCommand
        {
            get
            {
                if (loadTeamImageCommand == null)
                {
                    loadTeamImageCommand = new RelayCommand(param => LoadImage(NewTeam));
                }
                return loadTeamImageCommand;
            }
        }

        private ICommand addNewTeamCommand;
        public ICommand AddNewTeamCommand
        {
            get
            {
                if (addNewTeamCommand == null)
                {
                    addNewTeamCommand = new RelayCommand(param => AddNewTeam());
                }
                return addNewTeamCommand;
            }
        }

        private ICommand addExistingTeamCommand;
        public ICommand AddExistingTeamCommand
        {
            get
            {
                if (addExistingTeamCommand == null)
                {
                    addExistingTeamCommand = new RelayCommand(param => AddExistingTeam());
                }
                return addExistingTeamCommand;
            }
        }

        private ICommand removeTeamCommand;
        public ICommand RemoveTeamCommand
        {
            get
            {
                if (removeTeamCommand == null)
                {
                    removeTeamCommand = new RelayCommand(param => RemoveTeam((Team)param));
                }
                return removeTeamCommand;
            }
        }

        private ICommand autoFillGroupsCommand;
        public ICommand AutoFillGroupsCommand
        {
            get
            {
                if (autoFillGroupsCommand == null)
                {
                    autoFillGroupsCommand = new RelayCommand(param => AutoFillGroups());
                }
                return autoFillGroupsCommand;
            }
        }

        private ICommand autoCompleteGroupsCommand;
        public ICommand AutoCompleteGroupsCommand
        {
            get
            {
                if (autoCompleteGroupsCommand == null)
                {
                    autoCompleteGroupsCommand = new RelayCommand(param => AutoCompleteGroups());
                }
                return autoCompleteGroupsCommand;
            }
        }

        private ICommand removeGroupCommand;
        public ICommand RemoveGroupCommand
        {
            get
            {
                if (removeGroupCommand == null)
                {
                    removeGroupCommand = new RelayCommand(param => RemoveGroup((Group)param));
                }
                return removeGroupCommand;
            }
        }

        private ICommand removeTeamFromGroupCommand;
        public ICommand RemoveTeamFromGroupCommand
        {
            get
            {
                if (removeTeamFromGroupCommand == null)
                {
                    removeTeamFromGroupCommand = new RelayCommand(param => RemoveTeamFromGroup(param));
                }
                return removeTeamFromGroupCommand;
            }
        }

        private ICommand addTeamToGroupCommand;
        public ICommand AddTeamToGroupCommand
        {
            get
            {
                if (addTeamToGroupCommand == null)
                {
                    addTeamToGroupCommand = new RelayCommand(param => AddTeamToGroup(param));
                }
                return addTeamToGroupCommand;
            }
        }

        private ICommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(param => Save());
                }
                return saveCommand;
            }
        }
        #endregion

        #region Format
        private string qualificationHeader = "Qualification";
        public string QualificationHeader
        {
            get { return qualificationHeader; }
            set
            {
                qualificationHeader = value;
                OnPropertyChanged();
            }
        }

        private bool qualificationSet = false;
        public bool QualificationSet
        {
            get { return qualificationSet; }
            set
            {
                qualificationSet = value;
                OnPropertyChanged();
            }
        }

        private string groupsHeader = "Groups";
        public string GroupsHeader
        {
            get { return groupsHeader; }
            set
            {
                groupsHeader = value;
                OnPropertyChanged();
            }
        }

        private bool groupsSet = false;
        public bool GroupsSet
        {
            get { return groupsSet; }
            set
            {
                groupsSet = value;
                OnPropertyChanged();
            }
        }

        private int groupsCount;
        public int GroupsCount
        {
            get { return groupsCount; }
            set
            {
                if (value == Groups.Count)
                {
                    groupsCount = value;
                }
                else
                {
                    int dif = value - groupsCount;
                    if (dif < 0)
                    {
                        for (int i = 0; i > dif; i--)
                        {
                            foreach (Team team in Groups[Groups.Count - 1].Teams)
                            {
                                NotSelectedTeams.Add(team);
                            }
                            Groups.RemoveAt(Groups.Count - 1);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dif; i++)
                        {
                            Groups.Add(new Group { Name = "New group", Teams = new ObservableCollection<Team>() });
                        }
                    }
                    groupsCount = value;
                }
                OnPropertyChanged();
            }
        }

        private bool playOffSet = false;
        public bool PlayOffSet
        {
            get { return playOffSet; }
            set
            {
                playOffSet = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #endregion

        #region Constructor
        public AddSeasonViewModel(NavigationStore navigationStore)
        {
            CurrentSeason = new Season();
            CurrentSeason.id = (int)EntityState.AddNew;
            NewTeam = new Team();
            NewTeam.id = (int)EntityState.AddNew;
            ExistingTeam = new Team();
            Countries = SportsData.countries;
            LoadExistingTeams();
            Teams = new ObservableCollection<Team>();
            Groups = new ObservableCollection<Group>();
            NotSelectedTeams = new ObservableCollection<Team>();
        }
        #endregion

        #region Methods
        private void LoadExistingTeams()
        {
            ExistingTeams = new ObservableCollection<Team>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id, name" +/*, info, status, country, date_of_creation*/ " FROM team", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow tm in dataTable.Rows)
                {
                    Team t = new Team
                    {
                        id = int.Parse(tm["id"].ToString()),
                        Name = tm["name"].ToString(),
                        //Info = tm["info"].ToString(),
                        //Status = bool.Parse(tm["status"].ToString()),
                        //DateOfCreation = DateTime.Parse(tm["date_of_creation"].ToString())
                    };
                    
                    //string country = tm["country"].ToString();
                    //t.Country = Countries.First(x => x.CodeTwo == country);

                    string[] imgPath = Directory.GetFiles(SportsData.TeamLogosPath, SportsData.sport.name + t.id + ".*");
                    if (imgPath.Length != 0)
                    {
                        t.LogoPath = imgPath.First();
                    }

                    ExistingTeams.Add(t);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadImage(Competition entity)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.DefaultExt = ".png";
            open.Filter = "Pictures (*.jpg;*.png)|*.jpg;*.png";

            if (open.ShowDialog() == true)
            {
                entity.LogoPath = open.FileName;

                MemoryStream ms = new MemoryStream();
                byte[] arrbytFileContent = File.ReadAllBytes(entity.LogoPath);
                ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                ms.Position = 0;

                if (entity.GetType() == typeof(Team))
                {
                    bitmapTeam = new BitmapImage();
                    bitmapTeam.BeginInit();
                    bitmapTeam.StreamSource = ms;
                    bitmapTeam.EndInit();
                    BitmapTeam = bitmapTeam;
                }
                else
                {
                    bitmapSeason = new BitmapImage();
                    bitmapSeason.BeginInit();
                    bitmapSeason.StreamSource = ms;
                    bitmapSeason.EndInit();
                    BitmapSeason = bitmapSeason;
                }
                GC.Collect();
            }
        }

        private void AddNewTeam()
        {
            if (string.IsNullOrWhiteSpace(NewTeam.Name))
            {
                return;
            }
            Team t = new Team(NewTeam);
            Teams.Add(t);
            NotSelectedTeams.Add(t);
            NewTeam = new Team();
            BitmapTeam = new BitmapImage();
        }

        private void AddExistingTeam()
        {
            if (ExistingTeam == null || ExistingTeam.id == (int)EntityState.NotSelected)
            {
                return;
            }
            if (!Teams.Any(x => x.id == ExistingTeam.id))
            {
                Team t = new Team(ExistingTeam);
                Teams.Add(t);
                NotSelectedTeams.Add(t);
            }
        }

        private void RemoveTeam(Team t)
        {
            Teams.Remove(t);
            NotSelectedTeams.Remove(t);
            foreach (Group g in Groups)
            {
                if (g.Teams.Contains(t))
                {
                    g.Teams.Remove(t);
                }
            }
        }

        private void AutoFillGroups()
        {
            throw new NotImplementedException();
        }

        private void AutoCompleteGroups()
        {
            throw new NotImplementedException();
        }

        private void RemoveTeamFromGroup(object param)
        {
            IList teamAndGroup = (IList)param;
            ((Group)teamAndGroup[1]).Teams.Remove((Team)teamAndGroup[0]);
            NotSelectedTeams.Add((Team)teamAndGroup[0]);
        }

        private void RemoveGroup(Group g)
        {
            foreach (Team t in g.Teams)
            {
                NotSelectedTeams.Add(t);
            }
            Groups.Remove(g);
            GroupsCount = Groups.Count;
        }

        private void AddTeamToGroup(object param)
        {
            IList teamAndGroup = (IList)param;
            if (teamAndGroup[0] == null || !NotSelectedTeams.Contains((Team)teamAndGroup[0]))
            {
                return;
            }
            ((Group)teamAndGroup[1]).Teams.Add((Team)teamAndGroup[0]);
            NotSelectedTeams.Remove((Team)teamAndGroup[0]);
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentSeason.Name))
            {
                return;
            }
            
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("INSERT INTO seasons(name ,info) VALUES ('" + CurrentSeason.Name + "', '" + CurrentSeason.Info + "')", connection);

            try
            {
                connection.Open();
                cmd.ExecuteReader();
                currentSeason.id = (int)cmd.LastInsertedId;
                connection.Close();

                if (string.IsNullOrWhiteSpace(CurrentSeason.LogoPath))
                {
                    return;
                }
                string filePath = SportsData.CompetitionLogosPath + "/" + SportsData.sport.name + CurrentSeason.id + Path.GetExtension(CurrentSeason.LogoPath);
                File.Copy(CurrentSeason.LogoPath, filePath);
                CurrentSeason.LogoPath = filePath;
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
