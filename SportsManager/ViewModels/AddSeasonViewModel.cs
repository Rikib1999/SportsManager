﻿using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using SportsManager.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SportsManager.ViewModels
{
    /// <summary>
    /// Viewmodel for creating a new season.
    /// </summary>
    public class AddSeasonViewModel : NotifyPropertyChanged
    {
        #region Properties

        #region Variables
        /// <summary>
        /// Current instance of the NavigationStore.
        /// </summary>
        private readonly NavigationStore ns;

        /// <summary>
        /// Index of the next created group.
        /// </summary>
        private int groupLetterCounter;

        /// <summary>
        /// Object for generating random numbers.
        /// </summary>
        private readonly Random r = new();

        /// <summary>
        /// Collecation of all competitions.
        /// </summary>
        public ObservableCollection<Competition> Competitions { get; set; }

        private Season currentSeason;
        /// <summary>
        /// Instance of the current new season.
        /// </summary>
        public Season CurrentSeason
        {
            get => currentSeason;
            set
            {
                currentSeason = value;
                OnPropertyChanged();
            }
        }

        private Team newTeam;
        /// <summary>
        /// Insatnce of a newly created team to be enlsited in the season.
        /// </summary>
        public Team NewTeam
        {
            get => newTeam;
            set
            {
                newTeam = value;
                OnPropertyChanged();
            }
        }

        private Team existingTeam;
        /// <summary>
        /// Instance of an existing team to be enlisted in the season.
        /// </summary>
        public Team ExistingTeam
        {
            get => existingTeam;
            set
            {
                existingTeam = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> existingTeams;
        /// <summary>
        /// Collection of all existing teams in the current sport.
        /// </summary>
        public ObservableCollection<Team> ExistingTeams
        {
            get => existingTeams;
            set
            {
                existingTeams = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> teams;
        /// <summary>
        /// Collection of enlisted teams in the current season.
        /// </summary>
        public ObservableCollection<Team> Teams
        {
            get => teams;
            set
            {
                teams = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage bitmapSeason;
        /// <summary>
        /// Bitmap for the seasons logo image.
        /// </summary>
        public BitmapImage BitmapSeason
        {
            get => bitmapSeason;
            set
            {
                bitmapSeason = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage bitmapTeam;
        /// <summary>
        /// Bitmap for the newly created teams logo image.
        /// </summary>
        public BitmapImage BitmapTeam
        {
            get => bitmapTeam;
            set
            {
                bitmapTeam = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Country> countries;
        /// <summary>
        /// Collection of all the countries in the world.
        /// </summary>
        public ObservableCollection<Country> Countries
        {
            get => countries;
            set
            {
                countries = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> notSelectedTeams;
        /// <summary>
        /// Collection of teams enlisted in the season, that are not seeded in any of the group or bracket.
        /// </summary>
        public ObservableCollection<Team> NotSelectedTeams
        {
            get => notSelectedTeams;
            set
            {
                notSelectedTeams = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands

        #region ImageCommands
        private ICommand loadSeasonImageCommand;
        /// <summary>
        /// When executed, it loads a logo image for the season into a bitmap.
        /// </summary>
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

        private ICommand removeSeasonImageCommand;
        /// <summary>
        /// When executed, it removes the logo image for the season from the bitmap.
        /// </summary>
        public ICommand RemoveSeasonImageCommand
        {
            get
            {
                if (removeSeasonImageCommand == null)
                {
                    removeSeasonImageCommand = new RelayCommand(param => RemoveImage(CurrentSeason));
                }
                return removeSeasonImageCommand;
            }
        }

        private ICommand loadTeamImageCommand;
        /// <summary>
        /// When executed, it loads a logo image for the newly created team into a bitmap.
        /// </summary>
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

        private ICommand removeTeamImageCommand;
        /// <summary>
        /// When executed, it removes the logo image for the newly created team from the bitmap.
        /// </summary>
        public ICommand RemoveTeamImageCommand
        {
            get
            {
                if (removeTeamImageCommand == null)
                {
                    removeTeamImageCommand = new RelayCommand(param => RemoveImage(NewTeam));
                }
                return removeTeamImageCommand;
            }
        }
        #endregion

        #region TeamCommands
        private ICommand addNewTeamCommand;
        /// <summary>
        /// When executed, it creates a new team and enlists it in the season.
        /// </summary>
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
        /// <summary>
        /// When executed, it enlists an existing team in the season.
        /// </summary>
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
        /// <summary>
        /// When executed, it removes the team enlistment in the season.
        /// </summary>
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
        #endregion

        #region QualificationCommands
        private ICommand autoFillQualificationCommand;
        /// <summary>
        /// When executed, it randomly seeds not seeded teams into the qualification brackets, overwritting the current seed.
        /// </summary>
        public ICommand AutoFillQualificationCommand
        {
            get
            {
                if (autoFillQualificationCommand == null)
                {
                    autoFillQualificationCommand = new RelayCommand(param => AutoFillQualification());
                }
                return autoFillQualificationCommand;
            }
        }

        private ICommand autoCompleteQualificationCommand;
        /// <summary>
        /// When executed, it randomly seeds not seeded teams into the qualification brackets.
        /// </summary>
        public ICommand AutoCompleteQualificationCommand
        {
            get
            {
                if (autoCompleteQualificationCommand == null)
                {
                    autoCompleteQualificationCommand = new RelayCommand(param => AutoCompleteQualification());
                }
                return autoCompleteQualificationCommand;
            }
        }
        #endregion

        #region GroupsCommands
        private ICommand autoFillGroupsCommand;
        /// <summary>
        /// When executed, it randomly seeds not seeded teams into the groups, overwritting the current seed.
        /// </summary>
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
        /// <summary>
        /// When executed, it randomly seeds not seeded teams into the groups.
        /// </summary>
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
        /// <summary>
        /// When executed, it removes the group.
        /// </summary>
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
        /// <summary>
        /// When executed, it removes the team from the group.
        /// </summary>
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
        /// <summary>
        /// When executed, it adds the team to the group.
        /// </summary>
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
        #endregion

        #region PlayOffCommands
        private ICommand autoFillPlayOffCommand;
        /// <summary>
        /// When executed, it randomly seeds not seeded teams into the play-off bracket, overwritting the current seed.
        /// </summary>
        public ICommand AutoFillPlayOffCommand
        {
            get
            {
                if (autoFillPlayOffCommand == null)
                {
                    autoFillPlayOffCommand = new RelayCommand(param => AutoFillPlayOff());
                }
                return autoFillPlayOffCommand;
            }
        }

        private ICommand autoCompletePlayOffCommand;
        /// <summary>
        /// When executed, it randomly seeds not seeded teams into the play-off bracket.
        /// </summary>
        public ICommand AutoCompletePlayOffCommand
        {
            get
            {
                if (autoCompletePlayOffCommand == null)
                {
                    autoCompletePlayOffCommand = new RelayCommand(param => AutoCompletePlayOff());
                }
                return autoCompletePlayOffCommand;
            }
        }

        private ICommand removeFirstTeamFromSerieCommand;
        /// <summary>
        /// When executed, it removes the first team from the serie.
        /// </summary>
        public ICommand RemoveFirstTeamFromSerieCommand
        {
            get
            {
                if (removeFirstTeamFromSerieCommand == null)
                {
                    removeFirstTeamFromSerieCommand = new RelayCommand(param => RemoveFirstTeamFromSerie(param));
                }
                return removeFirstTeamFromSerieCommand;
            }
        }

        private ICommand addFirstTeamToSerieCommand;
        /// <summary>
        /// When executed, it adds the first team to the serie.
        /// </summary>
        public ICommand AddFirstTeamToSerieCommand
        {
            get
            {
                if (addFirstTeamToSerieCommand == null)
                {
                    addFirstTeamToSerieCommand = new RelayCommand(param => AddFirstTeamToSerie(param));
                }
                return addFirstTeamToSerieCommand;
            }
        }

        private ICommand removeSecondTeamFromSerieCommand;
        /// <summary>
        /// When executed, it removes the second team from the serie.
        /// </summary>
        public ICommand RemoveSecondTeamFromSerieCommand
        {
            get
            {
                if (removeSecondTeamFromSerieCommand == null)
                {
                    removeSecondTeamFromSerieCommand = new RelayCommand(param => RemoveSecondTeamFromSerie(param));
                }
                return removeSecondTeamFromSerieCommand;
            }
        }

        private ICommand addSecondTeamToSerieCommand;
        /// <summary>
        /// When executed, it adds the second team to the serie.
        /// </summary>
        public ICommand AddSecondTeamToSerieCommand
        {
            get
            {
                if (addSecondTeamToSerieCommand == null)
                {
                    addSecondTeamToSerieCommand = new RelayCommand(param => AddSecondTeamToSerie(param));
                }
                return addSecondTeamToSerieCommand;
            }
        }
        #endregion

        #region SaveCommand
        private ICommand saveCommand;
        /// <summary>
        /// When executed, it saves the season into the database.
        /// </summary>
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

        #endregion

        #region Format

        #region Qualification
        private string qualificationHeader = "Qualification";
        /// <summary>
        /// Header for the qualification format section.
        /// </summary>
        public string QualificationHeader
        {
            get => qualificationHeader;
            set
            {
                qualificationHeader = value;
                OnPropertyChanged();
            }
        }

        private bool qualificationSet;
        /// <summary>
        /// True, if qualification is set. If true, it disables groups format. Sets the visibilities of qualification and groups. If false, it deletes the qualification brackets.
        /// </summary>
        public bool QualificationSet
        {
            get => qualificationSet;
            set
            {
                qualificationSet = value;
                if (!qualificationSet)
                {
                    QualificationVisibility = Visibility.Collapsed;
                    GroupsEnabled = true;
                    QualificationCount = 0;
                }
                else
                {
                    QualificationVisibility = Visibility.Visible;
                    GroupsEnabled = false;
                }
                OnPropertyChanged();
            }
        }

        private bool qualificationEnabled = true;
        /// <summary>
        /// If true, qualification format is enabled. It sets the qualification header.
        /// </summary>
        public bool QualificationEnabled
        {
            get => qualificationEnabled;
            set
            {
                qualificationEnabled = value;
                if (!qualificationEnabled)
                {
                    QualificationHeader = "Qualification (incompatible with groups)";
                }
                else
                {
                    QualificationHeader = "Qualification";
                }
                OnPropertyChanged();
            }
        }

        private Visibility qualificationVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the qualification section.
        /// </summary>
        public Visibility QualificationVisibility
        {
            get => qualificationVisibility;
            set
            {
                qualificationVisibility = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Bracket> qualificationBrackets;
        /// <summary>
        /// Collection of all qualification brackets.
        /// </summary>
        public ObservableCollection<Bracket> QualificationBrackets
        {
            get => qualificationBrackets;
            set
            {
                qualificationBrackets = value;
                OnPropertyChanged();
            }
        }

        private int qualificationCount;
        /// <summary>
        /// Number of qualififcation brackets. Creates new brackets if changed to bigger value and deletes brackets from end if changed to smaller value. Max number of possible teams is 256.
        /// </summary>
        public int QualificationCount
        {
            get => qualificationCount;
            set
            {
                if (value == QualificationBrackets.Count)
                {
                    qualificationCount = value;
                }
                else
                {
                    int newTeamCount = QualificationRoundOf * value;
                    if (newTeamCount > 256)
                    {
                        _ = MessageBox.Show("Qualification can hold up to 256 teams only. You have set " + newTeamCount + " teams.", "Qualification limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int dif = value - qualificationCount;
                    if (dif < 0)
                    {
                        for (int i = 0; i > dif; i--)
                        {
                            foreach (List<Serie> r in QualificationBrackets[^1].Series)
                            {
                                foreach (Serie s in r)
                                {
                                    if (Teams.Contains(s.FirstTeam))
                                    {
                                        NotSelectedTeams.Add(s.FirstTeam);
                                    }
                                    if (Teams.Contains(s.SecondTeam))
                                    {
                                        NotSelectedTeams.Add(s.SecondTeam);
                                    }
                                }
                            }
                            QualificationBrackets.RemoveAt(QualificationBrackets.Count - 1);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dif; i++)
                        {
                            Bracket b = new(QualificationRoundsCount);
                            b.Name = "Bracket " + (++qualificationCount);
                            QualificationBrackets.Add(b);
                        }
                    }
                }
                qualificationCount = value;
                OnPropertyChanged();
            }
        }

        private int qualificationRoundsCount;
        /// <summary>
        /// Number of rounds in qualififcation brackets. Creates new rounds if changed to bigger value and deletes rounds if changed to smaller value. Max number of possible teams is 256. Edits the QualificationRoundOf accordingly.
        /// </summary>
        public int QualificationRoundsCount
        {
            get => qualificationRoundsCount;
            set
            {
                if (value == qualificationRoundsCount) { return; }

                int newTeamCount = (int)Math.Pow(2, value) * QualificationCount;
                if (newTeamCount > 256)
                {
                    _ = MessageBox.Show("Qualification can hold up to 256 teams only. You have set " + newTeamCount + " teams.", "Qualification limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                qualificationRoundsCount = value;
                QualificationRoundOf = (int)Math.Pow(2, qualificationRoundsCount);
                foreach (Bracket b in QualificationBrackets)
                {
                    foreach (List<Serie> r in b.Series)
                    {
                        foreach (Serie s in r)
                        {
                            if (Teams.Contains(s.FirstTeam))
                            {
                                NotSelectedTeams.Add(s.FirstTeam);
                            }
                            if (Teams.Contains(s.SecondTeam))
                            {
                                NotSelectedTeams.Add(s.SecondTeam);
                            }
                        }
                    }
                }
                QualificationBrackets = new ObservableCollection<Bracket>();
                for (int i = 1; i <= QualificationCount; i++)
                {
                    Bracket b = new(QualificationRoundsCount);
                    b.Name = "Bracket " + i;
                    QualificationBrackets.Add(b);
                }
                OnPropertyChanged();
            }
        }

        private int qualificationRoundOf;
        /// <summary>
        /// Number of competitors in the first round in qualififcation brackets. Creates new rounds if changed to bigger value and deletes rounds if changed to smaller value. Max number of possible teams is 256. Edits the QualificationRoundsCount accordingly.
        /// </summary>
        public int QualificationRoundOf
        {
            get => qualificationRoundOf;
            set
            {
                if (value == qualificationRoundOf) { return; }

                int newTeamCount = value * QualificationCount;
                if (newTeamCount > 256)
                {
                    _ = MessageBox.Show("Qualification can hold up to 256 teams only. You have set " + newTeamCount + " teams.", "Qualification limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                QualificationRoundsCount = (int)Math.Log2(value);
                qualificationRoundOf = (int)Math.Pow(2, QualificationRoundsCount);
                foreach (Bracket b in QualificationBrackets)
                {
                    foreach (List<Serie> r in b.Series)
                    {
                        foreach (Serie s in r)
                        {
                            if (Teams.Contains(s.FirstTeam))
                            {
                                NotSelectedTeams.Add(s.FirstTeam);
                            }
                            if (Teams.Contains(s.SecondTeam))
                            {
                                NotSelectedTeams.Add(s.SecondTeam);
                            }
                        }
                    }
                }
                QualificationBrackets = new ObservableCollection<Bracket>();
                for (int i = 1; i <= QualificationCount; i++)
                {
                    Bracket b = new(QualificationRoundsCount);
                    b.Name = "Bracket " + i;
                    QualificationBrackets.Add(b);
                }
                OnPropertyChanged();
            }
        }
        #endregion

        #region Groups
        /// <summary>
        /// Collection of all groups in the season.
        /// </summary>
        private ObservableCollection<Group> groups;
        public ObservableCollection<Group> Groups
        {
            get => groups;
            set
            {
                groups = value;
                OnPropertyChanged();
            }
        }

        private string groupsHeader = "Groups";
        /// <summary>
        /// Header of the group stage section.
        /// </summary>
        public string GroupsHeader
        {
            get => groupsHeader;
            set
            {
                groupsHeader = value;
                OnPropertyChanged();
            }
        }

        private bool groupsSet;
        /// <summary>
        /// True, if group stage is selected. If enabled, it disables qualification section. If disabled, it resets all points for results settings and deletes all groups. It switches visibility of group stage and qualification section.
        /// </summary>
        public bool GroupsSet
        {
            get => groupsSet;
            set
            {
                groupsSet = value;
                if (!groupsSet)
                {
                    GroupsVisibility = Visibility.Collapsed;
                    QualificationEnabled = true;
                    GroupsCount = 0;
                    CurrentSeason.PointsForWin = null;
                    CurrentSeason.PointsForOTWin = null;
                    CurrentSeason.PointsForTie = null;
                    CurrentSeason.PointsForOTLoss = null;
                    CurrentSeason.PointsForLoss = null;
                }
                else
                {
                    GroupsVisibility = Visibility.Visible;
                    QualificationEnabled = false;
                }
                OnPropertyChanged();
            }
        }

        private bool groupsEnabled = true;
        /// <summary>
        /// If true, group stage is enabled. It switches the group stage section header.
        /// </summary>
        public bool GroupsEnabled
        {
            get => groupsEnabled;
            set
            {
                groupsEnabled = value;
                if (!groupsEnabled)
                {
                    GroupsHeader = "Groups (incompatible with qualification)";
                }
                else
                {
                    GroupsHeader = "Groups";
                }
                OnPropertyChanged();
            }
        }

        private Visibility groupsVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the group stage section.
        /// </summary>
        public Visibility GroupsVisibility
        {
            get => groupsVisibility;
            set
            {
                groupsVisibility = value;
                OnPropertyChanged();
            }
        }

        private int groupsCount;
        /// <summary>
        /// Number of groups in the season. I the value is changed to bigger, it adds new groups, if it is changed to a smaller value, it deletes groups from the end.
        /// </summary>
        public int GroupsCount
        {
            get => groupsCount;
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
                            foreach (Team team in Groups[^1].Teams)
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
                            Groups.Add(new Group { Name = GetGroupName(groupLetterCounter++), Teams = new ObservableCollection<Team>() });
                        }
                    }
                    groupsCount = value;
                }
                OnPropertyChanged();
            }
        }
        #endregion

        #region PlayOff
        /// <summary>
        /// Instance of the play-off bracket.
        /// </summary>
        public Bracket PlayOff { get; set; }

        private bool playOffSet;
        /// <summary>
        /// If true, the play-off is enabled. Sets play-off section visibility. If disabled, clears the play-off bracket.
        /// </summary>
        public bool PlayOffSet
        {
            get => playOffSet;
            set
            {
                playOffSet = value;
                if (playOffSet == false)
                {
                    PlayOffVisibility = Visibility.Collapsed;
                    PlayOffRoundOf = 0;
                }
                else
                {
                    PlayOffVisibility = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }

        private Visibility playOffVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the play-off section.
        /// </summary>
        public Visibility PlayOffVisibility
        {
            get => playOffVisibility;
            set
            {
                playOffVisibility = value;
                OnPropertyChanged();
            }
        }

        private int playOffRoundsCount;
        /// <summary>
        /// Number of rounds in play-off bracket. Creates new rounds if changed to bigger value and deletes rounds if changed to smaller value. Edits the PlayOffRoundOf accordingly.
        /// </summary>
        public int PlayOffRoundsCount
        {
            get => playOffRoundsCount;
            set
            {
                if (value == playOffRoundsCount)
                {
                    return;
                }
                playOffRoundsCount = value;
                PlayOffRoundOf = (int)Math.Pow(2, playOffRoundsCount);
                foreach (List<Serie> r in PlayOff.Series)
                {
                    foreach (Serie s in r)
                    {
                        if (Teams.Contains(s.FirstTeam))
                        {
                            NotSelectedTeams.Add(s.FirstTeam);
                        }
                        if (Teams.Contains(s.SecondTeam))
                        {
                            NotSelectedTeams.Add(s.SecondTeam);
                        }
                    }
                }
                PlayOff = new Bracket(PlayOffRoundsCount);
                OnPropertyChanged();
            }
        }

        private int playOffRoundOf;
        /// <summary>
        /// Number of competitors in the first round of play-off bracket. Creates new rounds if changed to bigger value and deletes rounds if changed to smaller value. Edits the PlayOffRoundsCount accordingly.
        /// </summary>
        public int PlayOffRoundOf
        {
            get => playOffRoundOf;
            set
            {
                if (value == playOffRoundOf)
                {
                    return;
                }
                PlayOffRoundsCount = (int)Math.Log2(value);
                playOffRoundOf = (int)Math.Pow(2, PlayOffRoundsCount);
                foreach (List<Serie> r in PlayOff.Series)
                {
                    foreach (Serie s in r)
                    {
                        if (Teams.Contains(s.FirstTeam))
                        {
                            NotSelectedTeams.Add(s.FirstTeam);
                        }
                        if (Teams.Contains(s.SecondTeam))
                        {
                            NotSelectedTeams.Add(s.SecondTeam);
                        }
                    }
                }
                PlayOff = new Bracket(PlayOffRoundsCount);
                OnPropertyChanged();
            }
        }

        private int playOffBestOf;
        /// <summary>
        /// Max number of matches that could be played in a play-off serie. Edits the PlayOffFirstTo accordingly.
        /// </summary>
        public int PlayOffBestOf
        {
            get => playOffBestOf;
            set
            {
                if (value == playOffBestOf)
                {
                    return;
                }
                playOffBestOf = value;
                PlayOffFirstTo = (playOffBestOf + 1) / 2;
                OnPropertyChanged();
            }
        }

        private int playOffFirstTo;
        /// <summary>
        /// Number of wins in a play-off serie needed for winning it. Edits the PlayOffBestOf accordingly.
        /// </summary>
        public int PlayOffFirstTo
        {
            get => playOffFirstTo;
            set
            {
                if (value == playOffFirstTo)
                {
                    return;
                }
                playOffFirstTo = value;
                PlayOffBestOf = (playOffFirstTo * 2) - 1;
                OnPropertyChanged();
            }
        }
        #endregion

        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Instatiates a new AddSeasonViewModel. Loads all needed data from database.
        /// </summary>
        /// <param name="navigationStore">Current instance of the NavigationStore.</param>
        public AddSeasonViewModel(NavigationStore navigationStore)
        {
            ns = navigationStore;
            CurrentSeason = new Season();
            NewTeam = new Team();
            ExistingTeam = new Team();

            Countries = SportsData.Countries;
            Task t1 = new(LoadExistingTeams);
            t1.Start();
            LoadCompetitions();

            CurrentSeason.Competition = SportsData.IsCompetitionSet() ? Competitions.First(x => x.ID == SportsData.COMPETITION.ID) : (new());
            Teams = new ObservableCollection<Team>();
            QualificationBrackets = new ObservableCollection<Bracket>();
            Groups = new ObservableCollection<Group>();
            PlayOff = new Bracket(0);
            NotSelectedTeams = new ObservableCollection<Team>();
        }
        #endregion

        #region Methods

        #region Loading
        /// <summary>
        /// Loads all existing competitions in the current sport from the database.
        /// </summary>
        private void LoadCompetitions()
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT id , name FROM competitions", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                Competitions = new ObservableCollection<Competition>();

                foreach (DataRow compet in dataTable.Rows)
                {
                    Competition c = new()
                    {
                        ID = int.Parse(compet["id"].ToString()),
                        Name = compet["name"].ToString(),
                    };
                    Competitions.Add(c);
                }
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Loads all existing teams in the current sport from the database.
        /// </summary>
        private void LoadExistingTeams()
        {
            ExistingTeams = new ObservableCollection<Team>();

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT id, name FROM team", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow tm in dataTable.Rows)
                {
                    Team t = new()
                    {
                        ID = int.Parse(tm["id"].ToString()),
                        Name = tm["name"].ToString(),
                    };

                    string[] imgPath = Directory.GetFiles(SportsData.TeamLogosPath, SportsData.SPORT.Name + t.ID + ".*");
                    if (imgPath.Length != 0)
                    {
                        t.ImagePath = imgPath.First();
                    }

                    if (t.ID != SportsData.NOID)
                    {
                        ExistingTeams.Add(t);
                    }
                }
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Loads an image to a bitmap from local file.
        /// </summary>
        /// <param name="entity"></param>
        private void LoadImage(Competition entity)
        {
            entity.ImagePath = ImageHandler.SelectImagePath();

            if (entity.ImagePath != null)
            {
                if (entity.GetType() == typeof(Team))
                {
                    BitmapTeam = ImageHandler.ImageToBitmap(entity.ImagePath);
                }
                else
                {
                    BitmapSeason = ImageHandler.ImageToBitmap(entity.ImagePath);
                }
                GC.Collect();
            }
        }
        #endregion

        #region Teams
        /// <summary>
        /// Creates a new team and enlists it for the season.
        /// </summary>
        private void AddNewTeam()
        {
            if (string.IsNullOrWhiteSpace(NewTeam.Name))
            {
                return;
            }
            if (NewTeam.Country == null)
            {
                _ = MessageBox.Show("Please select country.", "Country not selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Team t = new(NewTeam);
            t.SavedInDatabase = false;
            Teams.Add(t);
            NotSelectedTeams.Add(t);
            NewTeam = new Team();
            if (BitmapTeam != null && BitmapTeam.StreamSource != null) { BitmapTeam.StreamSource.Dispose(); }
            BitmapTeam = new BitmapImage();
        }

        /// <summary>
        /// Enlists an existing team for the season.
        /// </summary>
        private void AddExistingTeam()
        {
            if (ExistingTeam == null || ExistingTeam.ID == SportsData.NOID)
            {
                return;
            }
            if (!Teams.Any(x => x.ID == ExistingTeam.ID))
            {
                Team t = new(ExistingTeam);
                Teams.Add(t);
                NotSelectedTeams.Add(t);
            }
        }

        /// <summary>
        /// Removes a team enlistment for the season and removes the team from groups or brackets if it was seeded there.
        /// </summary>
        /// <param name="t"></param>
        private void RemoveTeam(Team t)
        {
            _ = Teams.Remove(t);
            _ = NotSelectedTeams.Remove(t);
            foreach (Group g in Groups)
            {
                if (g.Teams.Contains(t))
                {
                    _ = g.Teams.Remove(t);
                }
            }
            foreach (List<Serie> r in PlayOff.Series)
            {
                foreach (Serie s in r)
                {
                    if (s.FirstTeam == t)
                    {
                        (int, int) roundIndex = PlayOff.GetSerieRoundIndex(s);
                        PlayOff.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, -1);
                        s.FirstTeam = new Team();
                    }
                    else if (s.SecondTeam == t)
                    {
                        (int, int) roundIndex = PlayOff.GetSerieRoundIndex(s);
                        PlayOff.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, -1);
                        s.SecondTeam = new Team();
                    }
                }
            }
            foreach (Bracket b in QualificationBrackets)
            {
                foreach (List<Serie> r in b.Series)
                {
                    foreach (Serie s in r)
                    {
                        if (s.FirstTeam == t)
                        {
                            (int, int) roundIndex = b.GetSerieRoundIndex(s);
                            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, -1);
                            s.FirstTeam = new Team();
                        }
                        else if (s.SecondTeam == t)
                        {
                            (int, int) roundIndex = b.GetSerieRoundIndex(s);
                            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, -1);
                            s.SecondTeam = new Team();
                        }
                    }
                }
            }
        }
        #endregion

        #region Qualification
        /// <summary>
        /// Randomly seeds not selected teams into qualification brackets. It overwrites the old seeding.
        /// </summary>
        private void AutoFillQualification()
        {
            if (QualificationCount == 0)
            {
                return;
            }

            MessageBoxResult messageBoxResult = MessageBoxResult.Yes;
            bool willChange = false;
            foreach (Bracket b in QualificationBrackets)
            {
                if (willChange) { break; }
                foreach (List<Serie> round in b.Series)
                {
                    if (willChange) { break; }
                    foreach (Serie s in round)
                    {
                        if (Teams.Contains(s.FirstTeam) || Teams.Contains(s.SecondTeam))
                        {
                            messageBoxResult = MessageBox.Show("Current brackets will be changed. Do you wish to continue?", "Qualification auto-fill", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            willChange = true;
                            break;
                        }
                    }
                }
            }

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                //reset brackets
                int qCount = QualificationCount;
                QualificationCount = 0;
                QualificationCount = qCount;

                if (NotSelectedTeams.Count == 0) { return; }

                ShuffleList(NotSelectedTeams);

                foreach (Bracket b in QualificationBrackets)
                {
                    for (int i = 0; i < b.Series[0].Count; i++)
                    {
                        b.Series[0][i].FirstTeam = NotSelectedTeams[0];
                        NotSelectedTeams.RemoveAt(0);
                        b.IsEnabledTreeAfterInsertionAt(0, i, 1, 1);
                        if (NotSelectedTeams.Count == 0) { return; }

                        b.Series[0][i].SecondTeam = NotSelectedTeams[0];
                        NotSelectedTeams.RemoveAt(0);
                        b.IsEnabledTreeAfterInsertionAt(0, i, 2, 1);
                        if (NotSelectedTeams.Count == 0) { return; }
                    }
                }
            }
        }

        /// <summary>
        /// Randomly seeds not selected teams into qualification brackets.
        /// </summary>
        private void AutoCompleteQualification()
        {
            if (QualificationCount == 0 || NotSelectedTeams.Count == 0)
            {
                return;
            }

            ShuffleList(NotSelectedTeams);

            foreach (Bracket b in QualificationBrackets)
            {
                for (int i = 0; i < b.Series[0].Count; i++)
                {
                    if (!Teams.Contains(b.Series[0][i].FirstTeam) && b.Series[0][i].FirstIsEnabled == true)
                    {
                        b.Series[0][i].FirstTeam = NotSelectedTeams[0];
                        NotSelectedTeams.RemoveAt(0);
                        b.IsEnabledTreeAfterInsertionAt(0, i, 1, 1);
                        if (NotSelectedTeams.Count == 0) { return; }
                    }

                    if (!Teams.Contains(b.Series[0][i].SecondTeam) && b.Series[0][i].SecondIsEnabled == true)
                    {
                        b.Series[0][i].SecondTeam = NotSelectedTeams[0];
                        NotSelectedTeams.RemoveAt(0);
                        b.IsEnabledTreeAfterInsertionAt(0, i, 2, 1);
                        if (NotSelectedTeams.Count == 0) { return; }
                    }
                }
            }
        }
        #endregion

        #region Groups
        /// <summary>
        /// Gets the next group name created from capital letters ordered lexicographically.
        /// </summary>
        /// <param name="index">New group index.</param>
        /// <returns>The next group name created from capital letters ordered lexicographically.</returns>
        public static string GetGroupName(int index)
        {
            const byte BASE = 'Z' - 'A' + 1;
            string name = string.Empty;
            do
            {
                name = Convert.ToChar('A' + index % BASE) + name;
                index = index / BASE - 1;
            }
            while (index >= 0);
            return name;
        }

        /// <summary>
        /// Randomly seeds not selected teams into groups. It overwrites the old seeding.
        /// </summary>
        private void AutoFillGroups()
        {
            if (GroupsCount == 0)
            {
                return;
            }

            MessageBoxResult messageBoxResult = MessageBoxResult.Yes;
            foreach (Group g in Groups)
            {
                if (g.Teams.Count > 0)
                {
                    messageBoxResult = MessageBox.Show("Current groups will be changed. Do you wish to continue?", "Groups auto-fill", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    break;
                }
            }

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                //reset groups
                int gCount = GroupsCount;
                GroupsCount = 0;
                groupLetterCounter -= gCount;
                GroupsCount = gCount;

                ShuffleList(NotSelectedTeams);

                int groupIndex = 0;
                for (int i = NotSelectedTeams.Count - 1; i >= 0; i--)
                {
                    Groups[groupIndex].Teams.Add(NotSelectedTeams[i]);
                    NotSelectedTeams.RemoveAt(i);
                    groupIndex = (groupIndex + 1) % gCount;
                }
            }
        }

        /// <summary>
        /// Randomly seeds not selected teams into groups.
        /// </summary>
        private void AutoCompleteGroups()
        {
            if (GroupsCount == 0)
            {
                return;
            }

            ShuffleList(NotSelectedTeams);

            int maxTeamCount = 0;
            foreach (Group g in Groups)
            {
                if (maxTeamCount < g.Teams.Count)
                {
                    maxTeamCount = g.Teams.Count;
                }
            }

            int groupIndex = 0;
            int noChange = 0;
            while (NotSelectedTeams.Count > 0)
            {
                if (Groups[groupIndex].Teams.Count < maxTeamCount)
                {
                    Groups[groupIndex].Teams.Add(NotSelectedTeams[0]);
                    NotSelectedTeams.RemoveAt(0);
                    noChange = 0;
                }
                else
                {
                    noChange++;
                    if (noChange == GroupsCount)
                    {
                        noChange = 0;
                        maxTeamCount++;
                    }
                }
                groupIndex = (groupIndex + 1) % GroupsCount;
            }
        }

        /// <summary>
        /// Removes the team from the group. Edits the collection of not selected teams.
        /// </summary>
        /// <param name="param">Object of type IList containing team and group instance in that order.</param>
        private void RemoveTeamFromGroup(object param)
        {
            IList teamAndGroup = (IList)param;
            _ = ((Group)teamAndGroup[1]).Teams.Remove((Team)teamAndGroup[0]);
            NotSelectedTeams.Add((Team)teamAndGroup[0]);
        }

        /// <summary>
        /// Deletes the whole group from the season. Teams in it are inserted into the collection of not selected teams.
        /// </summary>
        /// <param name="g">Group instance to delete.</param>
        private void RemoveGroup(Group g)
        {
            foreach (Team t in g.Teams)
            {
                NotSelectedTeams.Add(t);
            }
            _ = Groups.Remove(g);
            GroupsCount = Groups.Count;
        }

        /// <summary>
        /// Adds the team to the group. Edits the collection of not selected teams.
        /// </summary>
        /// <param name="param">Object of type IList containing team and group instance in that order.</param>
        private void AddTeamToGroup(object param)
        {
            IList teamAndGroup = (IList)param;
            if (teamAndGroup[0] == null || !NotSelectedTeams.Contains((Team)teamAndGroup[0]))
            {
                return;
            }
            ((Group)teamAndGroup[1]).Teams.Add((Team)teamAndGroup[0]);
            _ = NotSelectedTeams.Remove((Team)teamAndGroup[0]);
        }
        #endregion

        #region PlayOff
        /// <summary>
        /// Randomly seeds not selected teams into play-off bracket. It overwrites the old seeding.
        /// </summary>
        private void AutoFillPlayOff()
        {
            if (PlayOffRoundOf == 0)
            {
                return;
            }

            MessageBoxResult messageBoxResult = MessageBoxResult.Yes;
            foreach (List<Serie> round in PlayOff.Series)
            {
                foreach (Serie s in round)
                {
                    if (Teams.Contains(s.FirstTeam) || Teams.Contains(s.SecondTeam))
                    {
                        messageBoxResult = MessageBox.Show("Current bracket will be changed. Do you wish to continue?", "Play-off auto-fill", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        break;
                    }
                }
            }

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                //reset bracket
                int roundOf = PlayOffRoundOf;
                PlayOffRoundOf = 0;
                PlayOffRoundOf = roundOf;

                if (NotSelectedTeams.Count == 0) { return; }

                ShuffleList(NotSelectedTeams);

                for (int i = 0; i < PlayOff.Series[0].Count; i++)
                {
                    PlayOff.Series[0][i].FirstTeam = NotSelectedTeams[0];
                    NotSelectedTeams.RemoveAt(0);
                    PlayOff.IsEnabledTreeAfterInsertionAt(0, i, 1, 1);
                    if (NotSelectedTeams.Count == 0) { return; }

                    PlayOff.Series[0][i].SecondTeam = NotSelectedTeams[0];
                    NotSelectedTeams.RemoveAt(0);
                    PlayOff.IsEnabledTreeAfterInsertionAt(0, i, 2, 1);
                    if (NotSelectedTeams.Count == 0) { return; }
                }
            }
        }

        /// <summary>
        /// Randomly seeds not selected teams into play-off bracket.
        /// </summary>
        private void AutoCompletePlayOff()
        {
            if (PlayOffRoundOf == 0 || NotSelectedTeams.Count == 0)
            {
                return;
            }

            ShuffleList(NotSelectedTeams);

            for (int i = 0; i < PlayOff.Series[0].Count; i++)
            {
                if (!Teams.Contains(PlayOff.Series[0][i].FirstTeam) && PlayOff.Series[0][i].FirstIsEnabled == true)
                {
                    PlayOff.Series[0][i].FirstTeam = NotSelectedTeams[0];
                    NotSelectedTeams.RemoveAt(0);
                    PlayOff.IsEnabledTreeAfterInsertionAt(0, i, 1, 1);
                    if (NotSelectedTeams.Count == 0) { return; }
                }

                if (!Teams.Contains(PlayOff.Series[0][i].SecondTeam) && PlayOff.Series[0][i].SecondIsEnabled == true)
                {
                    PlayOff.Series[0][i].SecondTeam = NotSelectedTeams[0];
                    NotSelectedTeams.RemoveAt(0);
                    PlayOff.IsEnabledTreeAfterInsertionAt(0, i, 2, 1);
                    if (NotSelectedTeams.Count == 0) { return; }
                }
            }
        }

        /// <summary>
        /// Removes the first team from the serie of the play-off. Edits the collection of not selected teams.
        /// </summary>
        /// <param name="param">Object of type IList containing serie and bracket instance in that order.</param>
        private void RemoveFirstTeamFromSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            Bracket b = (Bracket)serieAndBracket[1];

            NotSelectedTeams.Add(s.FirstTeam);
            s.FirstTeam = new Team();

            (int, int) roundIndex = b.GetSerieRoundIndex(s);
            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, -1);
        }

        /// <summary>
        /// Adds the first team to the serie of the play-off. Edits the collection of not selected teams.
        /// </summary>
        /// <param name="param">Object of type IList containing serie and bracket instance in that order.</param>
        private void AddFirstTeamToSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            Bracket b = (Bracket)serieAndBracket[1];

            if (s.FirstSelectedTeam == null || !NotSelectedTeams.Contains(s.FirstSelectedTeam))
            {
                return;
            }
            s.FirstTeam = s.FirstSelectedTeam;
            s.FirstSelectedTeam = new Team();
            _ = NotSelectedTeams.Remove(s.FirstTeam);

            (int, int) roundIndex = b.GetSerieRoundIndex(s);
            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 1, 1);
        }

        /// <summary>
        /// Removes the second team from the serie of the play-off. Edits the collection of not selected teams.
        /// </summary>
        /// <param name="param">Object of type IList containing serie and bracket instance in that order.</param>
        private void RemoveSecondTeamFromSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            Bracket b = (Bracket)serieAndBracket[1];

            NotSelectedTeams.Add(s.SecondTeam);
            s.SecondTeam = new Team();

            (int, int) roundIndex = b.GetSerieRoundIndex(s);
            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, -1);
        }

        /// <summary>
        /// Adds the second team to the serie of the play-off. Edits the collection of not selected teams.
        /// </summary>
        /// <param name="param">Object of type IList containing serie and bracket instance in that order.</param>
        private void AddSecondTeamToSerie(object param)
        {
            IList serieAndBracket = (IList)param;
            Serie s = (Serie)serieAndBracket[0];
            Bracket b = (Bracket)serieAndBracket[1];

            if (s.SecondSelectedTeam == null || !NotSelectedTeams.Contains(s.SecondSelectedTeam))
            {
                return;
            }
            s.SecondTeam = s.SecondSelectedTeam;
            s.SecondSelectedTeam = new Team();
            _ = NotSelectedTeams.Remove(s.SecondTeam);

            (int, int) roundIndex = b.GetSerieRoundIndex(s);
            b.IsEnabledTreeAfterInsertionAt(roundIndex.Item1, roundIndex.Item2, 2, 1);
        }
        #endregion

        #region Others
        /// <summary>
        /// Clears bitmap image.
        /// </summary>
        /// <param name="entity"></param>
        private void RemoveImage(Competition entity)
        {
            entity.ImagePath = "";
            if (entity.GetType() == typeof(Team))
            {
                BitmapTeam = new BitmapImage();
            }
            else
            {
                BitmapSeason = new BitmapImage();
            }
            GC.Collect();
        }

        /// <summary>
        /// Randomly shuffles a list.
        /// </summary>
        /// <param name="list"></param>
        private void ShuffleList(IList list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                object v = list[k];
                list[k] = list[n];
                list[n] = v;
            }
        }

        /// <summary>
        /// Validates and saves the season to the database.
        /// </summary>
        private void Save()
        {
            //validation
            if (CurrentSeason.Competition.ID == SportsData.NOID)
            {
                _ = MessageBox.Show("Please select competition.", "Competition not selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(CurrentSeason.Name))
            {
                _ = MessageBox.Show("Season name is required.", "Season name missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Teams.Count < 2)
            {
                _ = MessageBox.Show("At least two teams are needed.", "Too few teams", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!QualificationSet && !GroupsSet && !PlayOffSet)
            {
                _ = MessageBox.Show("Please select a format.", "Format not selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PlayOffSet && PlayOffRoundsCount < 1)
            {
                _ = MessageBox.Show("Please set the number of rounds of play-off.", "Play-off not set", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PlayOffSet && PlayOffBestOf < 1)
            {
                _ = MessageBox.Show("Please set the number of matches in series of play-off.", "Play-off series not set", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (QualificationSet && (QualificationCount < 1 || QualificationRoundOf < 1))
            {
                _ = MessageBox.Show("Please set the number of brackets and rounds of qualification.", "Qualification not set", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (GroupsSet && GroupsCount < 1)
            {
                _ = MessageBox.Show("Please set the number of groups.", "Groups not set", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (GroupsSet && (CurrentSeason.PointsForWin == null || CurrentSeason.PointsForOTWin == null || CurrentSeason.PointsForTie == null || CurrentSeason.PointsForOTLoss == null || CurrentSeason.PointsForLoss == null))
            {
                _ = MessageBox.Show("Please set the number of points for match results in groups section.", "Points not set", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!GroupsSet)
            {
                CurrentSeason.PointsForWin = 0;
                CurrentSeason.PointsForOTWin = 0;
                CurrentSeason.PointsForTie = 0;
                CurrentSeason.PointsForOTLoss = 0;
                CurrentSeason.PointsForLoss = 0;
            }
            foreach (Team t in Teams)
            {
                bool notAssigned = true;
                foreach (Group g in Groups)
                {
                    if (g.Teams.Contains(t))
                    {
                        notAssigned = false;
                        break;
                    }
                }
                if (notAssigned && PlayOffSet)
                {
                    foreach (List<Serie> r in PlayOff.Series)
                    {
                        if (!notAssigned) { break; }
                        foreach (Serie s in r)
                        {
                            if (s.FirstTeam == t || s.SecondTeam == t)
                            {
                                notAssigned = false;
                                break;
                            }
                        }
                    }
                }
                if (notAssigned)
                {
                    foreach (Bracket b in QualificationBrackets)
                    {
                        if (!notAssigned) { break; }
                        foreach (List<Serie> r in b.Series)
                        {
                            if (!notAssigned) { break; }
                            foreach (Serie s in r)
                            {
                                if (s.FirstTeam == t || s.SecondTeam == t)
                                {
                                    notAssigned = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (notAssigned)
                {
                    _ = MessageBox.Show("All teams need to be assigned. Please assign team " + t.Name + " to a qualification, group or play-off.", "Team not assigned", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string seasonInsertQuerry = "INSERT INTO seasons(competition_id, name, info, winner_id, qualification_count, qualification_rounds, group_count, play_off_rounds, play_off_best_of, " +
                                        "points_for_W, points_for_OW, points_for_T, points_for_OL, points_for_L, play_off_started) " +
                                        "VALUES (" + CurrentSeason.Competition.ID + ", '" + CurrentSeason.Name + "', '" + CurrentSeason.Info + "', " + -1 +
                                        ", " + QualificationCount + ", " + QualificationRoundsCount + ", " + GroupsCount + ", " + PlayOffRoundsCount + ", " + PlayOffBestOf +
                                        ", " + CurrentSeason.PointsForWin + ", " + CurrentSeason.PointsForOTWin + ", " + CurrentSeason.PointsForTie + ", " + CurrentSeason.PointsForOTLoss +
                                        ", " + CurrentSeason.PointsForLoss + ", '" + 0 + "')";

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                //season insertion
                cmd = new MySqlCommand(seasonInsertQuerry, connection)
                {
                    Transaction = transaction
                };
                _ = cmd.ExecuteNonQuery();
                currentSeason.ID = (int)cmd.LastInsertedId;

                if (!string.IsNullOrWhiteSpace(CurrentSeason.ImagePath))
                {
                    string filePath = SportsData.SeasonLogosPath + "/" + SportsData.SPORT.Name + CurrentSeason.ID + Path.GetExtension(CurrentSeason.ImagePath);
                    File.Copy(CurrentSeason.ImagePath, filePath);
                    CurrentSeason.ImagePath = filePath;
                }

                //new teams insertion
                foreach (Team t in Teams.Where(x => x.ID == SportsData.NOID))
                {
                    string teamInsertQuerry = "INSERT INTO team(name, info, status, country, date_of_creation) " +
                                              "VALUES ('" + t.Name + "', '" + t.Info + "', " + Convert.ToInt32(t.Status) + ", '" + t.Country.CodeTwo + "', '" + t.DateOfCreation.ToString("yyyy-MM-dd H:mm:ss") + "')";
                    cmd = new MySqlCommand(teamInsertQuerry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                    t.ID = (int)cmd.LastInsertedId;

                    if (!string.IsNullOrWhiteSpace(t.ImagePath))
                    {
                        string filePath = SportsData.TeamLogosPath + "/" + SportsData.SPORT.Name + t.ID + Path.GetExtension(t.ImagePath);
                        File.Copy(t.ImagePath, filePath);
                    }
                }

                //qualification insertion
                foreach (Bracket b in QualificationBrackets)
                {
                    string qualificationInsertQuerry = "INSERT INTO brackets(season_id, name) " +
                                              "VALUES (" + CurrentSeason.ID + ", '" + b.Name + "')";
                    cmd = new MySqlCommand(qualificationInsertQuerry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                    b.ID = (int)cmd.LastInsertedId;

                    for (int i = 0; i < b.Series.Count; i++)
                    {
                        for (int j = 0; j < b.Series[i].Count; j++)
                        {
                            int firstID = SportsData.NOID;
                            int secondID = SportsData.NOID;
                            if (Teams.Contains(b.Series[i][j].FirstTeam))
                            {
                                firstID = b.Series[i][j].FirstTeam.ID;
                                //team enlistment
                                string teamEnlistmentInsertQuerry = "INSERT INTO team_enlistment(team_id, season_id, group_id) " +
                                              "VALUES (" + firstID + ", " + CurrentSeason.ID + ", " + -1 + ")";
                                cmd = new MySqlCommand(teamEnlistmentInsertQuerry, connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();
                            }
                            if (Teams.Contains(b.Series[i][j].SecondTeam))
                            {
                                secondID = b.Series[i][j].SecondTeam.ID;
                                //team enlistment
                                string teamEnlistmentInsertQuerry = "INSERT INTO team_enlistment(team_id, season_id, group_id) " +
                                              "VALUES (" + secondID + ", " + CurrentSeason.ID + ", " + -1 + ")";
                                cmd = new MySqlCommand(teamEnlistmentInsertQuerry, connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();
                            }
                            if (firstID != SportsData.NOID || secondID != SportsData.NOID)
                            {
                                //match insertion
                                string macthInsertQuerry = "INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                              "VALUES (" + CurrentSeason.ID + ", 0, " + b.ID + ", " + j + ", " + i + ", -1, " + firstID + ", " + secondID + ", " + firstID + ")";
                                cmd = new MySqlCommand(macthInsertQuerry, connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                //groups insertion
                foreach (Group g in Groups)
                {
                    string groupInsertQuerry = "INSERT INTO groups(season_id, name) " +
                                              "VALUES (" + CurrentSeason.ID + ", '" + g.Name + "')";
                    cmd = new MySqlCommand(groupInsertQuerry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                    g.ID = (int)cmd.LastInsertedId;

                    //team enlistment
                    foreach (Team t in g.Teams)
                    {
                        string teamEnlistmentInsertQuerry = "INSERT INTO team_enlistment(team_id, season_id, group_id) " +
                                              "VALUES (" + t.ID + ", " + CurrentSeason.ID + ", " + g.ID + ")";
                        cmd = new MySqlCommand(teamEnlistmentInsertQuerry, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();
                    }
                }

                //play-off insertion
                if (PlayOffSet)
                {
                    for (int i = 0; i < PlayOff.Series.Count; i++)
                    {
                        for (int j = 0; j < PlayOff.Series[i].Count; j++)
                        {
                            int firstID = SportsData.NOID;
                            int secondID = SportsData.NOID;
                            if (Teams.Contains(PlayOff.Series[i][j].FirstTeam))
                            {
                                firstID = PlayOff.Series[i][j].FirstTeam.ID;
                                //team enlistment
                                string teamEnlistmentInsertQuerry = "INSERT INTO team_enlistment(team_id, season_id, group_id) " +
                                              "VALUES (" + firstID + ", " + CurrentSeason.ID + ", " + -1 + ")";
                                cmd = new MySqlCommand(teamEnlistmentInsertQuerry, connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();
                            }
                            if (Teams.Contains(PlayOff.Series[i][j].SecondTeam))
                            {
                                secondID = PlayOff.Series[i][j].SecondTeam.ID;
                                //team enlistment
                                string teamEnlistmentInsertQuerry = "INSERT INTO team_enlistment(team_id, season_id, group_id) " +
                                              "VALUES (" + secondID + ", " + CurrentSeason.ID + ", " + -1 + ")";
                                cmd = new MySqlCommand(teamEnlistmentInsertQuerry, connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();
                            }
                            if (firstID != SportsData.NOID || secondID != SportsData.NOID)
                            {
                                //match insertion
                                string macthInsertQuerry = "INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, home_competitor, away_competitor, bracket_first_team) " +
                                              "VALUES (" + CurrentSeason.ID + ", 0, -1, " + j + ", " + i + ", -1, " + firstID + ", " + secondID + ", " + firstID + ")";
                                cmd = new MySqlCommand(macthInsertQuerry, connection)
                                {
                                    Transaction = transaction
                                };
                                _ = cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                CurrentSeason.GroupCount = GroupsCount;
                CurrentSeason.PlayOffBestOf = PlayOffBestOf;
                CurrentSeason.PlayOffRounds = PlayOffRoundsCount;
                CurrentSeason.PlayOffStarted = false;
                CurrentSeason.QualificationCount = QualificationCount;
                CurrentSeason.QualificationRounds = QualificationRoundsCount;
                CurrentSeason.WinnerID = SportsData.NOID;
                CurrentSeason.WinnerName = "";
                if (BitmapSeason != null && BitmapSeason.StreamSource != null) { BitmapSeason.StreamSource.Dispose(); }
                if (BitmapTeam != null && BitmapTeam.StreamSource != null) { BitmapTeam.StreamSource.Dispose(); }

                transaction.Commit();
                connection.Close();

                new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new SeasonViewModel(ns))).Execute(CurrentSeason);
            }
            catch (Exception)
            {
                transaction.Rollback();
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
        #endregion

        #endregion
    }
}