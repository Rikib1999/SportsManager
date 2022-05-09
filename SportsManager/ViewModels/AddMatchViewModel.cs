using SportsManager.Commands;
using SportsManager.Models;
using SportsManager.Others;
using SportsManager.Stores;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SportsManager.ViewModels
{
    #region Classes
    /// <summary>
    /// Class for representing a player in roster.
    /// </summary>
    public class PlayerInRoster : NotifyPropertyChanged
    {
        /// <summary>
        /// Identification number of the player.
        /// </summary>
        public int id;

        private string name;
        /// <summary>
        /// Name of the player.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string position;
        /// <summary>
        /// Position of the player.
        /// </summary>
        public string Position
        {
            get => position;
            set
            {
                position = value;
                OnPropertyChanged();
            }
        }

        private int? number;
        /// <summary>
        /// Number of the player.
        /// </summary>
        public int? Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        private bool present;
        /// <summary>
        /// True, if the player was playing the match. If set to false, it deletes all the match events of which the player was part of.
        /// </summary>
        public bool Present
        {
            get => present;
            set
            {
                present = value;

                ObservableCollection<PlayerInRoster> n = new();
                foreach (PlayerInRoster p in vm.HomePlayers)
                {
                    if (p.Present)
                    {
                        n.Add(p);
                    }
                }
                foreach (Period p in vm.Periods)
                {
                    p.HomeRoster = n;
                }
                vm.HomeRoster = n;

                n = new();
                foreach (PlayerInRoster p in vm.AwayPlayers)
                {
                    if (p.Present)
                    {
                        n.Add(p);
                    }
                }
                foreach (Period p in vm.Periods)
                {
                    p.AwayRoster = n;
                }
                vm.AwayRoster = n;

                foreach (Period p in vm.Periods)
                {
                    for (int i = p.Goals.Count - 1; i >= 0; i--)
                    {
                        if (!p.Goals[i].Scorer.Present || (p.Goals[i].Assist != null && p.Goals[i].Assist.Name != null && !p.Goals[i].Assist.Present))
                        {
                            p.Goals.RemoveAt(i);
                        }
                    }
                    for (int i = p.Penalties.Count - 1; i >= 0; i--)
                    {
                        if (!p.Penalties[i].Player.Present)
                        {
                            p.Penalties.RemoveAt(i);
                        }
                    }
                    for (int i = p.PenaltyShots.Count - 1; i >= 0; i--)
                    {
                        if (!p.PenaltyShots[i].Player.Present)
                        {
                            p.PenaltyShots.RemoveAt(i);
                        }
                    }
                    for (int i = p.GoalieShifts.Count - 1; i >= 0; i--)
                    {
                        if (!p.GoalieShifts[i].Player.Present)
                        {
                            p.GoalieShifts.RemoveAt(i);
                        }
                    }
                }

                foreach (ShootoutShot s in vm.Shootout)
                {
                    if (!s.Player.Present)
                    {
                        s.Player = new PlayerInRoster();
                    }
                    if (!s.Goalie.Present)
                    {
                        s.Goalie = new PlayerInRoster();
                    }
                }

                for (int i = 0; i < vm.Shootout.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        vm.Shootout[i].PlayerRoster = vm.HomeRoster;
                        vm.Shootout[i].GoalieRoster = vm.AwayRoster;
                    }
                    else
                    {
                        vm.Shootout[i].PlayerRoster = vm.AwayRoster;
                        vm.Shootout[i].GoalieRoster = vm.HomeRoster;
                    }
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Reference to the current viewmodel.
        /// </summary>
        public AddMatchViewModel vm;
    }

    /// <summary>
    /// Class representing a goaltender participation in the match.
    /// </summary>
    public class GoalieInMatch
    {
        /// <summary>
        /// Identification number of the goaltender.
        /// </summary>
        public int id;
        /// <summary>
        /// Team side of the goaltender.
        /// </summary>
        public string side;
        /// <summary>
        /// 1 if the goaltender was in the net at the start of the match, otherwise 0.
        /// </summary>
        public int started;
        /// <summary>
        /// 1 if the goaltender was relieved in the match, otherwise 0.
        /// </summary>
        public int relieved;
    }

    /// <summary>
    /// Class representing one period of a match with all its events.
    /// </summary>
    public class Period : NotifyPropertyChanged
    {
        private static readonly object _lock = new();

        /// <summary>
        /// Instantiates a new period.
        /// </summary>
        /// <param name="vm">Insatnce of the viewmodel for adding or editing the match, in which this period is.</param>
        /// <param name="overtime">True, if the period is overtime, otherwise false.</param>
        public Period(AddMatchViewModel vm, bool overtime = false)
        {
            this.vm = vm;
            this.overtime = overtime;
            Number = vm.Periods.Count + 1;
            duration = vm.PeriodDuration;
            if (overtime) { Number = 9999; }
            if (vm.Overtime) { Number--; }
            HomeRoster = vm.HomeRoster;
            AwayRoster = vm.AwayRoster;
            Goals = new();
            BindingOperations.EnableCollectionSynchronization(Goals, _lock);
            NewGoal = new Goal();
            GoalsRoster = new();
            BindingOperations.EnableCollectionSynchronization(GoalsRoster, _lock);
            Penalties = new();
            BindingOperations.EnableCollectionSynchronization(Penalties, _lock);
            NewPenalty = new Penalty();
            PenaltyRoster = new();
            BindingOperations.EnableCollectionSynchronization(PenaltyRoster, _lock);
            PenaltyShots = new();
            BindingOperations.EnableCollectionSynchronization(PenaltyShots, _lock);
            NewPenaltyShot = new PenaltyShot();
            PenaltyShotRoster = new();
            BindingOperations.EnableCollectionSynchronization(PenaltyShotRoster, _lock);
            GoalieShifts = new();
            BindingOperations.EnableCollectionSynchronization(GoalieShifts, _lock);
            NewGoalieShift = new GoalieShift();
            GoalieShiftRoster = new();
            BindingOperations.EnableCollectionSynchronization(GoalieShiftRoster, _lock);
            TimeOuts = new();
            BindingOperations.EnableCollectionSynchronization(TimeOuts, _lock);
            NewTimeOut = new TimeOut();
        }

        /// <summary>
        /// Insatnce of the viewmodel for adding or editing the match, in which this period is.
        /// </summary>
        public AddMatchViewModel vm;

        /// <summary>
        /// True, if the period is overtime, otherwise false.
        /// </summary>
        public readonly bool overtime;

        /// <summary>
        /// Name of the period.
        /// </summary>
        public string Name
        {
            get
            {
                if (overtime)
                {
                    return "Overtime";
                }
                else
                {
                    return "Period " + Number;
                }
            }
        }

        private int number;
        /// <summary>
        /// Number of the period.
        /// </summary>
        public int Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Duration of the period in minutes.
        /// </summary>
        public int duration;

        private ObservableCollection<PlayerInRoster> homeRoster;
        /// <summary>
        /// Roster of the home team.
        /// </summary>
        public ObservableCollection<PlayerInRoster> HomeRoster
        {
            get => homeRoster;
            set
            {
                homeRoster = value;
                if (GoalSide == "Home") { GoalsRoster = value; }
                if (PenaltySide == "Home") { PenaltyRoster = value; }
                if (PenaltyShotSide == "Home") { PenaltyShotRoster = value; }
                if (GoalieShiftSide == "Home") { GoalieShiftRoster = value; }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> awayRoster;
        /// <summary>
        /// Roster of the away team.
        /// </summary>
        public ObservableCollection<PlayerInRoster> AwayRoster
        {
            get => awayRoster;
            set
            {
                awayRoster = value;
                if (GoalSide == "Away") { GoalsRoster = value; }
                if (PenaltySide == "Away") { PenaltyRoster = value; }
                if (PenaltyShotSide == "Away") { PenaltyShotRoster = value; }
                if (GoalieShiftSide == "Away") { GoalieShiftRoster = value; }
                OnPropertyChanged();
            }
        }

        #region Goals
        private ObservableCollection<Goal> goals;
        /// <summary>
        /// Collection of all goal events in the period.
        /// </summary>
        public ObservableCollection<Goal> Goals
        {
            get => goals;
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private Goal newGoal;
        /// <summary>
        /// New goal event to be added to the period.
        /// </summary>
        public Goal NewGoal
        {
            get => newGoal;
            set
            {
                newGoal = value;
                OnPropertyChanged();
            }
        }

        private string goalSide;
        /// <summary>
        /// Team side of the new goal.
        /// </summary>
        public string GoalSide
        {
            get => goalSide;
            set
            {
                if (goalSide != value)
                {
                    goalSide = value;
                    NewGoal.Side = value;
                    NewGoal.Scorer = null;
                    NewGoal.Assist = null;
                    if (value == "Home")
                    {
                        GoalsRoster = HomeRoster;
                    }
                    else
                    {
                        GoalsRoster = AwayRoster;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PlayerInRoster> goalsRoster;
        /// <summary>
        /// Team roster for the goal by side.
        /// </summary>
        public ObservableCollection<PlayerInRoster> GoalsRoster
        {
            get => goalsRoster;
            set
            {
                goalsRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addGoalCommand;
        /// <summary>
        /// When executed, adds a new goal event to the period.
        /// </summary>
        public ICommand AddGoalCommand
        {
            get
            {
                if (addGoalCommand == null)
                {
                    addGoalCommand = new RelayCommand(param => AddGoal());
                }
                return addGoalCommand;
            }
        }

        /// <summary>
        /// Validates goal insertion to the period and if allowed adds it.
        /// </summary>
        private void AddGoal()
        {
            if (string.IsNullOrWhiteSpace(NewGoal.Side))
            {
                _ = MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.Scorer == null)
            {
                _ = MessageBox.Show("Please select scorer.", "Scorer not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.Scorer == NewGoal.Assist)
            {
                _ = MessageBox.Show("Goal and assist can not be made by the same player.", "Goal and assist error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if ((NewGoal.PenaltyShot && NewGoal.OwnGoal) || (NewGoal.PenaltyShot && NewGoal.DelayedPenalty))
            {
                _ = MessageBox.Show("Own goal or delayed penalty goal can not be scored on penalty shot.", "Own goal or delayed penalty goal on penalty shot", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.TimeInSeconds >= duration * 60)
            {
                _ = MessageBox.Show("Time exceeds period duration.", "Time exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewGoal.PenaltyShot || NewGoal.OwnGoal)
            {
                NewGoal.Assist = new PlayerInRoster { id = SportsData.NOID };
            }

            Goals.Add(NewGoal);
            NewGoal = new Goal();
            GoalSide = null;
            Goals.Sort();
        }
        #endregion

        #region Penalties
        private ObservableCollection<Penalty> penalties;
        /// <summary>
        /// Collection of all penalty events in the period.
        /// </summary>
        public ObservableCollection<Penalty> Penalties
        {
            get => penalties;
            set
            {
                penalties = value;
                OnPropertyChanged();
            }
        }

        private Penalty newPenalty;
        /// <summary>
        /// New penalty event to be added to the period.
        /// </summary>
        public Penalty NewPenalty
        {
            get => newPenalty;
            set
            {
                newPenalty = value;
                OnPropertyChanged();
            }
        }

        private string penaltySide;
        /// <summary>
        /// Team side of the new penalty.
        /// </summary>
        public string PenaltySide
        {
            get => penaltySide;
            set
            {
                if (penaltySide != value)
                {
                    penaltySide = value;
                    NewPenalty.Side = value;
                    NewPenalty.Player = null;
                    if (value == "Home")
                    {
                        PenaltyRoster = HomeRoster;
                    }
                    else
                    {
                        PenaltyRoster = AwayRoster;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PlayerInRoster> penaltyRoster;
        /// <summary>
        /// Team roster for the penalty by side.
        /// </summary>
        public ObservableCollection<PlayerInRoster> PenaltyRoster
        {
            get => penaltyRoster;
            set
            {
                penaltyRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addPenaltyCommand;
        /// <summary>
        /// When executed, adds a new penalty event to the period.
        /// </summary>
        public ICommand AddPenaltyCommand
        {
            get
            {
                if (addPenaltyCommand == null)
                {
                    addPenaltyCommand = new RelayCommand(param => AddPenalty());
                }
                return addPenaltyCommand;
            }
        }

        /// <summary>
        /// Validates penalty insertion to the period and if allowed adds it.
        /// </summary>
        private void AddPenalty()
        {
            if (string.IsNullOrWhiteSpace(NewPenalty.Side))
            {
                _ = MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenalty.Player == null)
            {
                _ = MessageBox.Show("Please select player.", "Player not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenalty.PenaltyReason == null || NewPenalty.PenaltyType == null)
            {
                _ = MessageBox.Show("Please select penalty reason and type.", "Penalty not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenalty.TimeInSeconds >= duration * 60)
            {
                _ = MessageBox.Show("Time exceeds period duration.", "Time exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Penalties.Add(NewPenalty);
            NewPenalty = new Penalty();
            PenaltySide = null;
            Penalties.Sort();
        }
        #endregion

        #region PenaltyShots
        private ObservableCollection<PenaltyShot> penaltyShots;
        /// <summary>
        /// Collection of all penalty shot events in the period.
        /// </summary>
        public ObservableCollection<PenaltyShot> PenaltyShots
        {
            get => penaltyShots;
            set
            {
                penaltyShots = value;
                OnPropertyChanged();
            }
        }

        private PenaltyShot newPenaltyShot;
        /// <summary>
        /// New penalty shot event to be added to the period.
        /// </summary>
        public PenaltyShot NewPenaltyShot
        {
            get => newPenaltyShot;
            set
            {
                newPenaltyShot = value;
                OnPropertyChanged();
            }
        }

        private string penaltyShotSide;
        /// <summary>
        /// Team side of the new penalty shot.
        /// </summary>
        public string PenaltyShotSide
        {
            get => penaltyShotSide;
            set
            {
                if (penaltyShotSide != value)
                {
                    penaltyShotSide = value;
                    NewPenaltyShot.Side = value;
                    NewPenaltyShot.Player = null;
                    if (value == "Home")
                    {
                        PenaltyShotRoster = HomeRoster;
                    }
                    else
                    {
                        PenaltyShotRoster = AwayRoster;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PlayerInRoster> penaltyShotRoster;
        /// <summary>
        /// Team roster for the penalty shot by side.
        /// </summary>
        public ObservableCollection<PlayerInRoster> PenaltyShotRoster
        {
            get => penaltyShotRoster;
            set
            {
                penaltyShotRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addPenaltyShotCommand;
        /// <summary>
        /// When executed, adds a new penalty shot event to the period.
        /// </summary>
        public ICommand AddPenaltyShotCommand
        {
            get
            {
                if (addPenaltyShotCommand == null)
                {
                    addPenaltyShotCommand = new RelayCommand(param => AddPenaltyShot());
                }
                return addPenaltyShotCommand;
            }
        }

        /// <summary>
        /// Validates penalty shot insertion to the period and if allowed adds it.
        /// </summary>
        private void AddPenaltyShot()
        {
            if (string.IsNullOrWhiteSpace(NewPenaltyShot.Side))
            {
                _ = MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenaltyShot.Player == null)
            {
                _ = MessageBox.Show("Please select player.", "Player not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenaltyShot.TimeInSeconds >= duration * 60)
            {
                _ = MessageBox.Show("Time exceeds period duration.", "Time exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PenaltyShots.Add(NewPenaltyShot);
            NewPenaltyShot = new PenaltyShot();
            PenaltyShotSide = null;
            PenaltyShots.Sort();
        }
        #endregion

        #region GoalieShifts
        private ObservableCollection<GoalieShift> goalieShifts;
        /// <summary>
        /// Collection of all goaltender shift events in the period.
        /// </summary>
        public ObservableCollection<GoalieShift> GoalieShifts
        {
            get => goalieShifts;
            set
            {
                goalieShifts = value;
                OnPropertyChanged();
            }
        }

        private GoalieShift newGoalieShift;
        /// <summary>
        /// New goaltender shift event to be added to the period.
        /// </summary>
        public GoalieShift NewGoalieShift
        {
            get => newGoalieShift;
            set
            {
                newGoalieShift = value;
                OnPropertyChanged();
            }
        }

        private string goalieShiftSide;
        /// <summary>
        /// Team side of the new goaltender shift.
        /// </summary>
        public string GoalieShiftSide
        {
            get => goalieShiftSide;
            set
            {
                if (goalieShiftSide != value)
                {
                    goalieShiftSide = value;
                    NewGoalieShift.Side = value;
                    NewGoalieShift.Player = null;
                    if (value == "Home")
                    {
                        GoalieShiftRoster = HomeRoster;
                    }
                    else
                    {
                        GoalieShiftRoster = AwayRoster;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PlayerInRoster> goalieShiftRoster;
        /// <summary>
        /// Team roster for the goaltender shift by side.
        /// </summary>
        public ObservableCollection<PlayerInRoster> GoalieShiftRoster
        {
            get => goalieShiftRoster;
            set
            {
                goalieShiftRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addGoalieShiftCommand;
        /// <summary>
        /// When executed, adds a new goaltender shift event to the period.
        /// </summary>
        public ICommand AddGoalieShiftCommand
        {
            get
            {
                if (addGoalieShiftCommand == null)
                {
                    addGoalieShiftCommand = new RelayCommand(param => AddGoalieShift());
                }
                return addGoalieShiftCommand;
            }
        }

        /// <summary>
        /// Validates goaltender shift insertion to the period and if allowed adds it.
        /// </summary>
        private void AddGoalieShift()
        {
            if (string.IsNullOrWhiteSpace(NewGoalieShift.Side))
            {
                _ = MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoalieShift.Player == null)
            {
                _ = MessageBox.Show("Please select player.", "Player not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoalieShift.EndTimeInSeconds - NewGoalieShift.StartTimeInSeconds < 1)
            {
                _ = MessageBox.Show("Goaltender shift must last at least 1 second.", "Shift is too short", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoalieShift.EndTimeInSeconds > duration * 60 || NewGoalieShift.StartTimeInSeconds > duration * 60)
            {
                _ = MessageBox.Show("Goaltender shift exceeds period duration.", "Shift exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GoalieShifts.Add(NewGoalieShift);
            NewGoalieShift = new GoalieShift();
            GoalieShiftSide = null;
            GoalieShifts.Sort();
        }
        #endregion

        #region TimeOuts
        private ObservableCollection<TimeOut> timeOuts;
        /// <summary>
        /// Collection of all time-out events in the period.
        /// </summary>
        public ObservableCollection<TimeOut> TimeOuts
        {
            get => timeOuts;
            set
            {
                timeOuts = value;
                OnPropertyChanged();
            }
        }

        private TimeOut newTimeOut;
        /// <summary>
        /// New time-out event to be added to the period.
        /// </summary>
        public TimeOut NewTimeOut
        {
            get => newTimeOut;
            set
            {
                newTimeOut = value;
                OnPropertyChanged();
            }
        }

        private string timeOutSide;
        /// <summary>
        /// Team side of the new time-out.
        /// </summary>
        public string TimeOutSide
        {
            get => timeOutSide;
            set
            {
                if (timeOutSide != value)
                {
                    timeOutSide = value;
                    NewTimeOut.Side = value;
                    OnPropertyChanged();
                }
            }
        }

        private ICommand addTimeOutCommand;
        /// <summary>
        /// When executed, adds a new time-out event to the period.
        /// </summary>
        public ICommand AddTimeOutCommand
        {
            get
            {
                if (addTimeOutCommand == null)
                {
                    addTimeOutCommand = new RelayCommand(param => AddTimeOut());
                }
                return addTimeOutCommand;
            }
        }

        /// <summary>
        /// Validates time-out insertion to the period and if allowed adds it.
        /// </summary>
        private void AddTimeOut()
        {
            if (string.IsNullOrWhiteSpace(NewTimeOut.Side))
            {
                _ = MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewTimeOut.TimeInSeconds >= duration * 60)
            {
                _ = MessageBox.Show("Time exceeds period duration.", "Time exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeOuts.Add(NewTimeOut);
            NewTimeOut = new TimeOut();
            TimeOutSide = null;
            TimeOuts.Sort();
        }
        #endregion
    }

    /// <summary>
    /// Class for representing a match event.
    /// </summary>
    public class Event : NotifyPropertyChanged, IComparable
    {
        /// <summary>
        /// Index of the event.
        /// </summary>
        public int index;

        private BasicStat stat;
        /// <summary>
        /// Event itself as a specific statistics.
        /// </summary>
        public BasicStat Stat
        {
            get => stat;
            set
            {
                stat = value;
                OnPropertyChanged();
            }
        }

        private Period period;
        /// <summary>
        /// Period of the event.
        /// </summary>
        public Period Period
        {
            get => period;
            set
            {
                period = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Description of the event.
        /// </summary>
        public string Text
        {
            get
            {
                string p = "Period " + Period.Number + "\t\t";
                if (Period.overtime) { p = "Overtime\t\t"; }
                return p + Stat.Text;
            }
        }

        /// <summary>
        /// Returns description of the event.
        /// </summary>
        /// <returns>Description of the event.</returns>
        public override string ToString()
        {
            return Stat.Text;
        }

        /// <summary>
        /// Compare events by periods and then by time ine period.
        /// </summary>
        /// <param name="obj">Other event to compare with.</param>
        /// <returns>-1 if happend sooner, 0 if happend at the same time, 1 if happend later.</returns>
        public int CompareTo(object obj)
        {
            int otherPeriod = ((Event)obj).Period.Number;
            if (Period.Number < otherPeriod)
            {
                return -1;
            }
            else if (Period.Number == otherPeriod)
            {
                int otherTime = ((Event)obj).Stat.TimeInSeconds;
                if (Stat.TimeInSeconds < otherTime)
                {
                    return -1;
                }
                else if (Stat.TimeInSeconds == otherTime)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }
    }

    /// <summary>
    /// Class representing a container for choosing if the event happend in penalty or not.
    /// </summary>
    public class SwapEvent : NotifyPropertyChanged
    {
        /// <summary>
        /// Instantiates new SwapEvent.
        /// </summary>
        /// <param name="inPenalty">Event to swap.</param>
        public SwapEvent(Event inPenalty)
        {
            InPenaltyEvent = inPenalty;
            OutPenaltyEvent = null;
        }

        private Event inPenaltyEvent;
        /// <summary>
        /// This is the event instance if it happened in the penalty. Otherwise null.
        /// </summary>
        public Event InPenaltyEvent
        {
            get => inPenaltyEvent;
            set
            {
                inPenaltyEvent = value;
                OnPropertyChanged();
            }
        }

        private Event outPenaltyEvent;
        /// <summary>
        /// This is the event instance if it did not happen in the penalty. Otherwise null.
        /// </summary>
        public Event OutPenaltyEvent
        {
            get => outPenaltyEvent;
            set
            {
                outPenaltyEvent = value;
                OnPropertyChanged();
            }
        }

        private ICommand eventInCommand;
        /// <summary>
        /// When executed, it puts event into penalty.
        /// </summary>
        public ICommand EventInCommand
        {
            get
            {
                if (eventInCommand == null)
                {
                    eventInCommand = new RelayCommand(param => EventIn());
                }
                return eventInCommand;
            }
        }

        private ICommand eventOutCommand;
        /// <summary>
        /// When executed, it puts event out of penalty.
        /// </summary>
        public ICommand EventOutCommand
        {
            get
            {
                if (eventOutCommand == null)
                {
                    eventOutCommand = new RelayCommand(param => EventOut());
                }
                return eventOutCommand;
            }
        }

        /// <summary>
        /// It puts event into penalty.
        /// </summary>
        private void EventIn()
        {
            if (InPenaltyEvent == null && OutPenaltyEvent != null)
            {
                InPenaltyEvent = OutPenaltyEvent;
                OutPenaltyEvent = null;
            }
        }

        /// <summary>
        /// It puts event out of penalty.
        /// </summary>
        private void EventOut()
        {
            if (InPenaltyEvent != null && OutPenaltyEvent == null)
            {
                OutPenaltyEvent = InPenaltyEvent;
                InPenaltyEvent = null;
            }
        }
    }

    /// <summary>
    /// Class for representing collision of an event with end of a penalty.
    /// </summary>
    public class PenaltyEndCollision : NotifyPropertyChanged
    {
        /// <summary>
        /// Instantiates a new PenaltyEndCollision.
        /// </summary>
        /// <param name="inPenalty">State of game in the penalty.</param>
        /// <param name="outPenalty">State of game out of the penalty.</param>
        /// <param name="penalty">Penalty instance.</param>
        public PenaltyEndCollision(State inPenalty, State outPenalty, Penalty penalty)
        {
            InPenalty = inPenalty;
            OutPenalty = outPenalty;
            this.penalty = penalty;
            SwapEvents = new();
        }

        /// <summary>
        /// Current penalty instance.
        /// </summary>
        public Penalty penalty;

        private State inPenalty;
        /// <summary>
        /// State of game in the penalty.
        /// </summary>
        public State InPenalty
        {
            get => inPenalty;
            set
            {
                inPenalty = value;
                OnPropertyChanged();
            }
        }

        private State outPenalty;
        /// <summary>
        /// State of game out of the penalty.
        /// </summary>
        public State OutPenalty
        {
            get => outPenalty;
            set
            {
                outPenalty = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SwapEvent> swapEvents;
        /// <summary>
        /// Collection of events colliding with the penalty.
        /// </summary>
        public ObservableCollection<SwapEvent> SwapEvents
        {
            get => swapEvents;
            set
            {
                swapEvents = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns event description.
        /// </summary>
        /// <returns>Event description.</returns>
        public override string ToString()
        {
            string from = (penalty.startTime / 60) + ":" + (penalty.startTime % 60).ToString("00");
            string to = (penalty.endTime / 60) + ":" + (penalty.endTime % 60).ToString("00");
            return "Penalty from " + from + " to " + to;
        }
    }

    /// <summary>
    /// Template class for representing an event as a statistics.
    /// </summary>
    public class Stat : NotifyPropertyChanged { }

    /// <summary>
    /// Class containing shared properties between different types of statistics.
    /// </summary>
    public class BasicStat : Stat, IComparable
    {
        private int minute;
        /// <summary>
        /// Minute when the event happened.
        /// </summary>
        public int Minute
        {
            get => minute;
            set
            {
                minute = value;
                OnPropertyChanged();
            }
        }

        private int second;
        /// <summary>
        /// Second when the event happened.
        /// </summary>
        public int Second
        {
            get => second;
            set
            {
                second = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Time when the event happened in format "mm:ss".
        /// </summary>
        public string Time => Minute + ":" + Second.ToString("00");

        /// <summary>
        /// Time when the event happened in seconds.
        /// </summary>
        public int TimeInSeconds => (Minute * 60) + Second;

        private string side;
        /// <summary>
        /// Side of the team that did the event.
        /// </summary>
        public string Side
        {
            get => side;
            set
            {
                side = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Description of the statistics.
        /// </summary>
        public virtual string Text => Time + "\t\t" + Side;

        /// <summary>
        /// Current score of the home team.
        /// </summary>
        public int homeScore;
        /// <summary>
        /// Current score of the away team.
        /// </summary>
        public int awayScore;
        /// <summary>
        /// Current strength of the teams.
        /// </summary>
        public string strength;

        /// <summary>
        /// Compares two statistics based on time they happend at.
        /// </summary>
        /// <param name="obj">Other statistics to compare with.</param>
        /// <returns>-1 if it happend earlier, 0 if it happened at the same time, 1 if it happened later.</returns>
        public int CompareTo(object obj)
        {
            int otherTime = ((BasicStat)obj).TimeInSeconds;
            if (TimeInSeconds < otherTime)
            {
                return -1;
            }
            else if (TimeInSeconds == otherTime)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private ICommand deleteCommand;
        /// <summary>
        /// When executed, it deletes the statistics (event).
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(param => Delete((Period)param));
                }
                return deleteCommand;
            }
        }

        /// <summary>
        /// deletes the statistics (event) from the period.
        /// </summary>
        /// <param name="period"></param>
        private void Delete(Period period)
        {
            switch (this)
            {
                case Goal:
                    _ = period.Goals.Remove((Goal)this);
                    break;
                case Penalty:
                    _ = period.Penalties.Remove((Penalty)this);
                    break;
                case PenaltyShot:
                    _ = period.PenaltyShots.Remove((PenaltyShot)this);
                    break;
                case TimeOut:
                    _ = period.TimeOuts.Remove((TimeOut)this);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Class representing the end of a period. Used as a stopper dividing events into periods.
    /// </summary>
    public class PeriodEnd : BasicStat { }

    /// <summary>
    /// Class representing a goal event.
    /// </summary>
    public class Goal : BasicStat
    {
        /// <summary>
        /// True if it was a game losing own goal.
        /// </summary>
        public bool gameLosingOwnGoal;
        /// <summary>
        /// True if it was a game winning goal.
        /// </summary>
        public bool gameWinningGoal;
        /// <summary>
        /// Goaltender of the opponent team.
        /// </summary>
        public PlayerInRoster goalie;

        private PlayerInRoster scorer = new();
        /// <summary>
        /// Scorer of the goal.
        /// </summary>
        public PlayerInRoster Scorer
        {
            get => scorer;
            set
            {
                scorer = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster assist = new();
        /// <summary>
        /// Player that assisted on the goal.
        /// </summary>
        public PlayerInRoster Assist
        {
            get => assist;
            set
            {
                assist = value;
                OnPropertyChanged();
            }
        }

        private bool penaltyShot;
        /// <summary>
        /// True, if it was a goal from a penalty shot.
        /// </summary>
        public bool PenaltyShot
        {
            get => penaltyShot;
            set
            {
                penaltyShot = value;
                OnPropertyChanged();
            }
        }

        private bool ownGoal;
        /// <summary>
        /// True, if it was an own goal.
        /// </summary>
        public bool OwnGoal
        {
            get => ownGoal;
            set
            {
                ownGoal = value;
                OnPropertyChanged();
            }
        }

        private bool delayedPenalty;
        /// <summary>
        /// True, if the goal was scored durng delayed penalty.
        /// </summary>
        public bool DelayedPenalty
        {
            get => delayedPenalty;
            set
            {
                delayedPenalty = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True, if the goal was scored into an empty net.
        /// </summary>
        public bool emptyNet;

        /// <summary>
        /// Returns the name of the type of the goal.
        /// </summary>
        public string Type
        {
            get
            {
                string s = "normal";
                if (PenaltyShot) { s = "penalty shot"; }
                if (DelayedPenalty) { s = "delayed penalty goal"; }
                if (OwnGoal) { s = "own goal"; }
                return s;
            }
        }

        /// <summary>
        /// Returns the description of the event.
        /// </summary>
        public override string Text
        {
            get
            {
                string t = "goal";
                if (PenaltyShot) { t = "penalty shot goal"; }
                if (DelayedPenalty) { t = "delayed penalty goal"; }
                if (OwnGoal) { t = "own goal"; }
                string s = Time + "\t\t" + Side + "\t\t" + t + " scored by " + Scorer.Number;
                if (Assist != null && string.IsNullOrWhiteSpace(Assist.Name)) { s += " (" + Assist.Number + ")"; }
                return s;
            }
        }
    }

    /// <summary>
    /// Class representing a penalty event.
    /// </summary>
    public class Penalty : BasicStat
    {
        /// <summary>
        /// Start time of the penalty in period.
        /// </summary>
        public int startTime;
        /// <summary>
        /// End time of the penalty in period.
        /// </summary>
        public int endTime;
        /// <summary>
        /// Duration of the penalty in seconds.
        /// </summary>
        public int duration;
        /// <summary>
        /// If true, the penalty was punished by goal.
        /// </summary>
        public bool punished;

        private PlayerInRoster player = new();
        /// <summary>
        /// Player that recieved the penalty.
        /// </summary>
        public PlayerInRoster Player
        {
            get => player;
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private PenaltyReason penaltyReason;
        /// <summary>
        /// Reason of the penalty.
        /// </summary>
        public PenaltyReason PenaltyReason
        {
            get => penaltyReason;
            set
            {
                penaltyReason = value;
                OnPropertyChanged();
            }
        }

        private PenaltyType penaltyType;
        /// <summary>
        /// Type of the penalty.
        /// </summary>
        public PenaltyType PenaltyType
        {
            get => penaltyType;
            set
            {
                penaltyType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Returns description of the event.
        /// </summary>
        public override string Text => Time + "\t\t" + Side + "\t\t" + PenaltyType.Name + " for " + Player.Number;
    }

    /// <summary>
    /// Class representing a penalty shot event.
    /// </summary>
    public class PenaltyShot : BasicStat
    {
        /// <summary>
        /// Goaltender during the penalty shot.
        /// </summary>
        public PlayerInRoster goalie;

        private PlayerInRoster player = new();
        /// <summary>
        /// Player given the penalty shot.
        /// </summary>
        public PlayerInRoster Player
        {
            get => player;
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private bool wasGoal;
        /// <summary>
        /// True, if a goal was scored from the penalty shot.
        /// </summary>
        public bool WasGoal
        {
            get => wasGoal;
            set
            {
                wasGoal = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Description of the penalty shot event.
        /// </summary>
        public override string Text
        {
            get
            {
                string s = Time + "\t\t" + Side + "\t\tpenalty shot goal";
                if (!WasGoal) { s += " not"; }
                s += " scored by " + Player.Number;
                return s;
            }
        }
    }

    /// <summary>
    /// Class representing a goaltender shift event.
    /// </summary>
    public class GoalieShift : Stat, IComparable
    {
        private int startMinute;
        /// <summary>
        /// Starting minute of the shift in period.
        /// </summary>
        public int StartMinute
        {
            get => startMinute;
            set
            {
                startMinute = value;
                OnPropertyChanged();
            }
        }

        private int startSecond;
        /// <summary>
        /// Starting second of the shift in period.
        /// </summary>
        public int StartSecond
        {
            get => startSecond;
            set
            {
                startSecond = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Starting time of the shift in period in format "mm:ss".
        /// </summary>
        public string StartTime => StartMinute + ":" + StartSecond.ToString("00");

        /// <summary>
        /// Starting time of the shift in period in seconds.
        /// </summary>
        public int StartTimeInSeconds => (StartMinute * 60) + StartSecond;

        private int endMinute;
        /// <summary>
        /// Ending minute of the shift in period.
        /// </summary>
        public int EndMinute
        {
            get => endMinute;
            set
            {
                endMinute = value;
                OnPropertyChanged();
            }
        }

        private int endSecond;
        /// <summary>
        /// Ending second of the shift in period.
        /// </summary>
        public int EndSecond
        {
            get => endSecond;
            set
            {
                endSecond = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Ending time of the shift in period in format "mm:ss".
        /// </summary>
        public string EndTime => EndMinute + ":" + EndSecond.ToString("00");

        /// <summary>
        /// Ending time of the shift in period in seconds.
        /// </summary>
        public int EndTimeInSeconds => (EndMinute * 60) + EndSecond;

        private string side;
        /// <summary>
        /// Team side of the goaltender.
        /// </summary>
        public string Side
        {
            get => side;
            set
            {
                side = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster player = new();
        /// <summary>
        /// The goaltender playing the shift.
        /// </summary>
        public PlayerInRoster Player
        {
            get => player;
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Compares two shifts based on the starting time.
        /// </summary>
        /// <param name="obj">Other shift to compare with.</param>
        /// <returns>-1 if it happened earlier, 0 if happened at the same time, 1 if it happened later.</returns>
        public int CompareTo(object obj)
        {
            int otherTime = ((GoalieShift)obj).StartTimeInSeconds;
            if (StartTimeInSeconds < otherTime)
            {
                return -1;
            }
            else if (StartTimeInSeconds == otherTime)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private ICommand deleteCommand;
        /// <summary>
        /// When executed, it deletes the shift from the period events.
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(param => ((Period)param).GoalieShifts.Remove(this));
                }
                return deleteCommand;
            }
        }
    }

    /// <summary>
    /// Class representing a goaltender change event.
    /// </summary>
    public class GoalieChange : BasicStat
    {
        /// <summary>
        /// Instantiates a new GoalieChange.
        /// </summary>
        /// <param name="gs">Shift of the goaltender.</param>
        /// <param name="entered">True, if the goaltender entered the net, started the shift. Otherwise false.</param>
        public GoalieChange(GoalieShift gs, bool entered)
        {
            if (entered)
            {
                Minute = gs.StartMinute;
                Second = gs.StartSecond;
            }
            else
            {
                Minute = gs.EndMinute;
                Second = gs.EndSecond;
            }
            Side = gs.Side;
            Player = gs.Player;
            Entered = entered;
        }

        private PlayerInRoster player = new();
        /// <summary>
        /// The goaltender making the change.
        /// </summary>
        public PlayerInRoster Player
        {
            get => player;
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True, if the goaltender entered the net, started the shift.
        /// </summary>
        private bool entered;
        public bool Entered
        {
            get => entered;
            set
            {
                entered = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The paired event. Start or end of shift of the other changed goaltender.
        /// </summary>
        public Event pairEvent;

        /// <summary>
        /// Description of the event.
        /// </summary>
        public override string Text
        {
            get
            {
                string s = Time + "\t\t" + Side + "\t\tgoaltender ";
                if (Entered) { s += "in"; } else { s += "out"; }
                return s;
            }
        }
    }

    /// <summary>
    /// Class representing a time-out event.
    /// </summary>
    public class TimeOut : BasicStat
    {
        /// <summary>
        /// Description of the time-out event.
        /// </summary>
        public override string Text => Time + "\t\t" + Side + "\t\ttime-out";
    }

    /// <summary>
    /// Class representing a shootout shot.
    /// </summary>
    public class ShootoutShot : Stat
    {
        /// <summary>
        /// Instantiates a new ShootoutShot.
        /// </summary>
        /// <param name="number">Order of the shot in shootout.</param>
        /// <param name="side">Side of the team making the shot.</param>
        /// <param name="playerRoster">Roster of the team making the shot.</param>
        /// <param name="goalieRoster">Roster of the team defending the shot.</param>
        public ShootoutShot(int number, string side, ObservableCollection<PlayerInRoster> playerRoster, ObservableCollection<PlayerInRoster> goalieRoster)
        {
            Number = number;
            Side = side;
            PlayerRoster = playerRoster;
            GoalieRoster = goalieRoster;
        }

        /// <summary>
        /// Instantiates a new ShootoutShot.
        /// </summary>
        /// <param name="number">Order of the shot in shootout.</param>
        /// <param name="side">Side of the team making the shot.</param>
        /// <param name="playerRoster">Roster of the team making the shot.</param>
        /// <param name="goalieRoster">Roster of the team defending the shot.</param>
        /// <param name="player">Player making the shot.</param>
        /// <param name="goalie">Goaltender defending the shot.</param>
        public ShootoutShot(int number, string side, ObservableCollection<PlayerInRoster> playerRoster, ObservableCollection<PlayerInRoster> goalieRoster, PlayerInRoster player, PlayerInRoster goalie)
        {
            Number = number;
            Side = side;
            PlayerRoster = playerRoster;
            GoalieRoster = goalieRoster;
            Player = player;
            Goalie = goalie;
        }

        private int number;
        /// <summary>
        /// Order of the shot in shootout.
        /// </summary>
        public int Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        private string side;
        /// <summary>
        /// Side of the team making the shot.
        /// </summary>
        public string Side
        {
            get => side;
            set
            {
                side = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster player = new();
        /// <summary>
        /// Player making the shot.
        /// </summary>
        public PlayerInRoster Player
        {
            get => player;
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> playerRoster;
        /// <summary>
        /// Roster of the team making the shot.
        /// </summary>
        public ObservableCollection<PlayerInRoster> PlayerRoster
        {
            get => playerRoster;
            set
            {
                playerRoster = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster goalie = new();
        /// <summary>
        /// Goaltender defending the shot.
        /// </summary>
        public PlayerInRoster Goalie
        {
            get => goalie;
            set
            {
                goalie = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> goalieRoster;
        /// <summary>
        /// Roster of the team defending the shot.
        /// </summary>
        public ObservableCollection<PlayerInRoster> GoalieRoster
        {
            get => goalieRoster;
            set
            {
                goalieRoster = value;
                OnPropertyChanged();
            }
        }

        private bool wasGoal;
        /// <summary>
        /// True, if a goal was scored from the shot.
        /// </summary>
        public bool WasGoal
        {
            get => wasGoal;
            set
            {
                wasGoal = value;
                OnPropertyChanged();
            }
        }

        private ICommand resetCommand;
        /// <summary>
        /// When executed, it resets the shootout shot data.
        /// </summary>
        public ICommand ResetCommand
        {
            get
            {
                if (resetCommand == null)
                {
                    resetCommand = new RelayCommand(param => Reset());
                }
                return resetCommand;
            }
        }

        /// <summary>
        /// Resets the shootout shot data.
        /// </summary>
        private void Reset()
        {
            ObservableCollection<PlayerInRoster> tmpP = PlayerRoster;
            PlayerRoster = null;
            Player = new PlayerInRoster();
            PlayerRoster = tmpP;

            ObservableCollection<PlayerInRoster> tmpG = GoalieRoster;
            GoalieRoster = null;
            Goalie = new PlayerInRoster();
            GoalieRoster = tmpG;

            WasGoal = false;
        }
    }

    /// <summary>
    /// Class representing a shutout of a goalkeeper or team.
    /// </summary>
    public class Shutout
    {
        /// <summary>
        /// Instantiates a new Shutout.
        /// </summary>
        /// <param name="goalie">Goaltender with the shutout. Null if it is a team shutout.</param>
        /// <param name="side">Side of the team with shutout.</param>
        public Shutout(PlayerInRoster goalie, string side)
        {
            this.goalie = goalie;
            this.side = side;
        }

        /// <summary>
        /// Goaltender with the shutout. Null if it is a team shutout.
        /// </summary>
        public PlayerInRoster goalie;

        /// <summary>
        /// Side of the team with shutout.
        /// </summary>
        public string side;
    }

    /// <summary>
    /// Class representing a state of the game.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Instatiates a new State.
        /// </summary>
        /// <param name="p">Period of the state.</param>
        /// <param name="startTime">Time when the game state changed to this state.</param>
        /// <param name="endTime">Time when this game state changed to another state.</param>
        /// <param name="hStrength">Number of players in play for the home team.</param>
        /// <param name="aStrength">Number of players in play for the away team.</param>
        /// <param name="hGoalieIn">True, if there was a goalie in the net of the home team. Otherwise false, means empty net.</param>
        /// <param name="aGoalieIn">True, if there was a goalie in the net of the away team. Otherwise false, means empty net.</param>
        /// <param name="hGoals">Number of scored goals by the home team.</param>
        /// <param name="aGoals">Number of scored goals by the away team.</param>
        public State(Period p, int startTime, int endTime, int hStrength, int aStrength, bool hGoalieIn, bool aGoalieIn, int hGoals, int aGoals)
        {
            period = p;
            this.startTime = startTime;
            this.endTime = endTime;
            homeStrength = hStrength;
            awayStrength = aStrength;
            homeGoalieIn = hGoalieIn;
            awayGoalieIn = aGoalieIn;
            homeGoals = hGoals;
            awayGoals = aGoals;
        }

        /// <summary>
        /// Creates a deepcopy of a state.
        /// </summary>
        /// <param name="s">State to copy.</param>
        public State(State s)
        {
            period = s.period;
            startTime = s.startTime;
            endTime = s.endTime;
            homeStrength = s.homeStrength;
            awayStrength = s.awayStrength;
            homeGoalieIn = s.homeGoalieIn;
            awayGoalieIn = s.awayGoalieIn;
            homeGoals = s.homeGoals;
            awayGoals = s.awayGoals;
        }

        /// <summary>
        /// Period of the state.
        /// </summary>
        public Period period;

        /// <summary>
        /// Time when the game state changed to this state.
        /// </summary>
        public int startTime;

        /// <summary>
        /// Time when this game state changed to another state.
        /// </summary>
        public int endTime;

        /// <summary>
        /// Number of players in play for the home team.
        /// </summary>
        public int homeStrength;

        /// <summary>
        /// Number of players in play for the away team.
        /// </summary>
        public int awayStrength;

        /// <summary>
        /// True, if there was a goalie in the net of the home team. Otherwise false, means empty net.
        /// </summary>
        public bool homeGoalieIn;

        /// <summary>
        /// True, if there was a goalie in the net of the away team. Otherwise false, means empty net.
        /// </summary>
        public bool awayGoalieIn;

        /// <summary>
        /// Number of scored goals by the home team.
        /// </summary>
        public int homeGoals;

        /// <summary>
        /// Number of scored goals by the away team.
        /// </summary>
        public int awayGoals;

        /// <summary>
        /// Returns current teams strength. First is for home team.
        /// </summary>
        /// <returns>Current teams strength. First is for home team.</returns>
        public string StrengthToString()
        {
            string s = homeStrength.ToString();
            if (!homeGoalieIn) { s += "g"; }
            s += " v " + awayStrength;
            if (!awayGoalieIn) { s += "g"; }
            return s;
        }

        /// <summary>
        /// Returns description of the state.
        /// </summary>
        /// <returns>Description of the state.</returns>
        public override string ToString()
        {
            return period.Name + "\t" + startTime + "\t" + endTime + "\t" + StrengthToString() + "\t" + homeGoals + ":" + awayGoals;
        }
    }
    #endregion

    /// <summary>
    /// Viewmodel for adding or editing a match.
    /// </summary>
    public class AddMatchViewModel : NotifyPropertyChanged
    {
        #region Properties

        #region DateTime
        private DateTime matchDate = DateTime.Today;
        /// <summary>
        /// Date of the start of the match.
        /// </summary>
        public DateTime MatchDate
        {
            get => matchDate;
            set
            {
                matchDate = value;
                OnPropertyChanged();
            }
        }

        private int matchTimeHours;
        /// <summary>
        /// Hour the match started.
        /// </summary>
        public int MatchTimeHours
        {
            get => matchTimeHours;
            set
            {
                matchTimeHours = value;
                OnPropertyChanged();
            }
        }

        private int matchTimeMinutes;
        /// <summary>
        /// Minute the match started.
        /// </summary>
        public int MatchTimeMinutes
        {
            get => matchTimeMinutes;
            set
            {
                matchTimeMinutes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Date and time the match started.
        /// </summary>
        public DateTime MatchDateTime => MatchDate + new TimeSpan(MatchTimeHours, MatchTimeMinutes, 0);
        #endregion

        #region Rosters
        private ObservableCollection<Team> availableTeamsHome;
        /// <summary>
        /// Collection of teams available for the home team selection.
        /// </summary>
        public ObservableCollection<Team> AvailableTeamsHome
        {
            get => availableTeamsHome;
            set
            {
                availableTeamsHome = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> availableTeamsAway;
        /// <summary>
        /// Collection of teams available for the away team selection.
        /// </summary>
        public ObservableCollection<Team> AvailableTeamsAway
        {
            get => availableTeamsAway;
            set
            {
                availableTeamsAway = value;
                OnPropertyChanged();
            }
        }

        private Team homeTeam;
        /// <summary>
        /// Instance of the selected home team. If changed, it edits the available away teams collection and resets the home team match events.
        /// </summary>
        public Team HomeTeam
        {
            get => homeTeam;
            set
            {
                if (homeTeam != null)
                {
                    AvailableTeamsAway.Add(homeTeam);
                }
                homeTeam = value;
                _ = AvailableTeamsAway.Remove(homeTeam);
                LoadRoster("Home");

                foreach (Period p in Periods)
                {
                    for (int i = p.Goals.Count - 1; i >= 0; i--)
                    {
                        if (p.Goals[i].Side == "Home")
                        {
                            p.Goals.RemoveAt(i);
                        }
                    }
                    for (int i = p.Penalties.Count - 1; i >= 0; i--)
                    {
                        if (p.Penalties[i].Side == "Home")
                        {
                            p.Penalties.RemoveAt(i);
                        }
                    }
                    for (int i = p.PenaltyShots.Count - 1; i >= 0; i--)
                    {
                        if (p.PenaltyShots[i].Side == "Home")
                        {
                            p.PenaltyShots.RemoveAt(i);
                        }
                    }
                    for (int i = p.GoalieShifts.Count - 1; i >= 0; i--)
                    {
                        if (p.GoalieShifts[i].Side == "Home")
                        {
                            p.GoalieShifts.RemoveAt(i);
                        }
                    }
                }

                ShootoutSeries = 0;
                OnPropertyChanged();
            }
        }

        private Team awayTeam;
        /// <summary>
        /// Instance of the selected away team. If changed, it edits the available home teams collection and resets the away team match events.
        /// </summary>
        public Team AwayTeam
        {
            get => awayTeam;
            set
            {
                if (awayTeam != null)
                {
                    AvailableTeamsHome.Add(awayTeam);
                }
                awayTeam = value;
                _ = AvailableTeamsHome.Remove(awayTeam);
                LoadRoster("Away");

                foreach (Period p in Periods)
                {
                    for (int i = p.Goals.Count - 1; i >= 0; i--)
                    {
                        if (p.Goals[i].Side == "Away")
                        {
                            p.Goals.RemoveAt(i);
                        }
                    }
                    for (int i = p.Penalties.Count - 1; i >= 0; i--)
                    {
                        if (p.Penalties[i].Side == "Away")
                        {
                            p.Penalties.RemoveAt(i);
                        }
                    }
                    for (int i = p.PenaltyShots.Count - 1; i >= 0; i--)
                    {
                        if (p.PenaltyShots[i].Side == "Away")
                        {
                            p.PenaltyShots.RemoveAt(i);
                        }
                    }
                    for (int i = p.GoalieShifts.Count - 1; i >= 0; i--)
                    {
                        if (p.GoalieShifts[i].Side == "Away")
                        {
                            p.GoalieShifts.RemoveAt(i);
                        }
                    }
                }

                ShootoutSeries = 0;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> homePlayers = new();
        /// <summary>
        /// Collection of the players of the home team.
        /// </summary>
        public ObservableCollection<PlayerInRoster> HomePlayers
        {
            get => homePlayers;
            set
            {
                homePlayers = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> awayPlayers = new();
        /// <summary>
        /// Collection of the players of the away team.
        /// </summary>
        public ObservableCollection<PlayerInRoster> AwayPlayers
        {
            get => awayPlayers;
            set
            {
                awayPlayers = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> homeRoster = new();
        /// <summary>
        /// Collection of the players of the home team roster that participated in the match.
        /// </summary>
        public ObservableCollection<PlayerInRoster> HomeRoster
        {
            get => homeRoster;
            set
            {
                homeRoster = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> awayRoster = new();
        /// <summary>
        /// Collection of the players of the away team roster that participated in the match.
        /// </summary>
        public ObservableCollection<PlayerInRoster> AwayRoster
        {
            get => awayRoster;
            set
            {
                awayRoster = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Match info
        /// <summary>
        /// Identification number of the qualification the match is in.
        /// </summary>
        int qualificationID = SportsData.NOID;
        /// <summary>
        /// Index of the bracket the match is in.
        /// </summary>
        int bracketIndex = -1;
        /// <summary>
        /// Index of the round the match is in.
        /// </summary>
        int round = -1;
        /// <summary>
        /// Number of the match in the serie.
        /// </summary>
        int serieMatchNumber = -1;
        /// <summary>
        /// Identification number of the first team of the serie.
        /// </summary>
        int bracketFirstTeamID = SportsData.NOID;
        /// <summary>
        /// Score of the home team.
        /// </summary>
        int HomeScore;
        /// <summary>
        /// Score of the away team.
        /// </summary>
        int AwayScore;

        private bool played;
        /// <summary>
        /// If true, the match was played, otherwise it is scheduled. Sets the visibility of the forms.
        /// </summary>
        public bool Played
        {
            get => played;
            set
            {
                played = value;
                if (value)
                {
                    FormsVisibility = Visibility.Visible;
                    ForfeitVisibility = Visibility.Visible;
                    NotPlayedSaveButtonVisibility = Visibility.Collapsed;
                }
                else
                {
                    FormsVisibility = Visibility.Collapsed;
                    ForfeitVisibility = Visibility.Collapsed;
                    NotPlayedSaveButtonVisibility = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }

        private bool forfeit;
        /// <summary>
        /// If truem the match was forfeited. Sets the visibility of the forfeited side selection.
        /// </summary>
        public bool Forfeit
        {
            get => forfeit;
            set
            {
                forfeit = value;
                if (value)
                {
                    ForfeitSideVisibility = Visibility.Visible;
                }
                else
                {
                    ForfeitSideVisibility = Visibility.Collapsed;
                }
                OnPropertyChanged();
            }
        }

        private string forfeitWinnerSide;
        /// <summary>
        /// Side which won after the forfeit of the match.
        /// </summary>
        public string ForfeitWinnerSide
        {
            get => forfeitWinnerSide;
            set
            {
                forfeitWinnerSide = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Periods
        private int periodCount;
        /// <summary>
        /// Number of periods. Creates or deletes periods when changed to fit the number.
        /// </summary>
        public int PeriodCount
        {
            get => periodCount;
            set
            {
                int actualCount = Periods.Count;
                if (Overtime) { actualCount--; }

                if (value == actualCount)
                {
                    periodCount = value;
                }
                else
                {
                    int dif = value - periodCount;
                    if (dif < 0)
                    {
                        for (int i = 0; i > dif; i--)
                        {
                            Periods.RemoveAt(--actualCount);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dif; i++)
                        {
                            Periods.Insert(actualCount++, new Period(this));
                        }
                    }
                    periodCount = value;
                }
                OnPropertyChanged();
            }
        }

        private int periodDuration;
        /// <summary>
        /// Period duration in minutes. Deletes match events that happened after the max period duration.
        /// </summary>
        public int PeriodDuration
        {
            get => periodDuration;
            set
            {
                periodDuration = value;

                foreach (Period p in Periods)
                {
                    p.duration = value;

                    for (int i = p.Goals.Count - 1; i >= 0; i--)
                    {
                        if (p.Goals[i].Minute >= p.duration)
                        {
                            p.Goals.RemoveAt(i);
                        }
                    }
                    for (int i = p.Penalties.Count - 1; i >= 0; i--)
                    {
                        if (p.Penalties[i].Minute >= p.duration)
                        {
                            p.Penalties.RemoveAt(i);
                        }
                    }
                    for (int i = p.PenaltyShots.Count - 1; i >= 0; i--)
                    {
                        if (p.PenaltyShots[i].Minute >= p.duration)
                        {
                            p.PenaltyShots.RemoveAt(i);
                        }
                    }
                    for (int i = p.GoalieShifts.Count - 1; i >= 0; i--)
                    {
                        if (p.GoalieShifts[i].StartMinute >= p.duration)
                        {
                            p.GoalieShifts.RemoveAt(i);
                        }
                    }
                    for (int i = p.TimeOuts.Count - 1; i >= 0; i--)
                    {
                        if (p.TimeOuts[i].Minute >= p.duration)
                        {
                            p.TimeOuts.RemoveAt(i);
                        }
                    }
                }
                OnPropertyChanged();
            }
        }

        private bool overtime;
        /// <summary>
        /// True, if an overtime was played in the match. Adds or removes the last overtime period.
        /// </summary>
        public bool Overtime
        {
            get => overtime;
            set
            {
                overtime = value;
                if (value)
                {
                    Periods.Add(new Period(this, true));
                }
                else
                {
                    Periods.RemoveAt(Periods.Count - 1);
                }
                OnPropertyChanged();
            }
        }

        private bool isShootout;
        /// <summary>
        /// True, if an shootout was played in the match. Sets shootout visibility.
        /// </summary>
        public bool IsShootout
        {
            get => isShootout;
            set
            {
                isShootout = value;
                if (value)
                {
                    ShootoutVisibility = Visibility.Visible;
                }
                else
                {
                    ShootoutVisibility = Visibility.Collapsed;
                }
                OnPropertyChanged();
            }
        }

        private int shootoutSeries;
        /// <summary>
        /// Number of shootout series. Creates or delete shootout series to fit the number.
        /// </summary>
        public int ShootoutSeries
        {
            get => shootoutSeries;
            set
            {
                if (value == Math.Round((float)Shootout.Count / 2.0))
                {
                    shootoutSeries = value;
                }
                else
                {
                    int dif = value - shootoutSeries;
                    if (dif < 0)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            for (int i = 0; i > dif; i--)
                            {
                                Shootout.RemoveAt(Shootout.Count - 1);
                            }
                        }
                    }
                    else
                    {

                        for (int i = 0; i < dif; i++)
                        {
                            int number = (Shootout.Count / 2) + 1;
                            Shootout.Add(new ShootoutShot(number, "Home", HomeRoster, AwayRoster));
                            Shootout.Add(new ShootoutShot(number, "Away", AwayRoster, HomeRoster));
                        }
                    }
                    shootoutSeries = value;
                }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Period> periods;
        /// <summary>
        /// Collection of periods of the match.
        /// </summary>
        public ObservableCollection<Period> Periods
        {
            get => periods;
            set
            {
                periods = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ShootoutShot> shootout;
        /// <summary>
        /// Collection of shootout shots.
        /// </summary>
        public ObservableCollection<ShootoutShot> Shootout
        {
            get => shootout;
            set
            {
                shootout = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Visibilities
        public Visibility loadingVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the loading screen.
        /// </summary>
        public Visibility LoadingVisibility
        {
            get => loadingVisibility;
            set
            {
                loadingVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility pageVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the page.
        /// </summary>
        public Visibility PageVisibility
        {
            get => pageVisibility;
            set
            {
                pageVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility notPlayedSaveButtonVisibility = Visibility.Visible;
        /// <summary>
        /// Visibility of the save button for scheduled matches.
        /// </summary>
        public Visibility NotPlayedSaveButtonVisibility
        {
            get => notPlayedSaveButtonVisibility;
            set
            {
                notPlayedSaveButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility formsVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the match events forms.
        /// </summary>
        public Visibility FormsVisibility
        {
            get => formsVisibility;
            set
            {
                formsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility forfeitVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the forfeit form.
        /// </summary>
        public Visibility ForfeitVisibility
        {
            get => forfeitVisibility;
            set
            {
                forfeitVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility forfeitSideVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the forfeit side selection.
        /// </summary>
        public Visibility ForfeitSideVisibility
        {
            get => forfeitSideVisibility;
            set
            {
                forfeitSideVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility shootoutVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the shootout section.
        /// </summary>
        public Visibility ShootoutVisibility
        {
            get => shootoutVisibility;
            set
            {
                shootoutVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility checkButtonVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the check button.
        /// </summary>
        public Visibility CheckButtonVisibility
        {
            get => checkButtonVisibility;
            set
            {
                checkButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility saveButtonVisibility = Visibility.Collapsed;
        /// <summary>
        /// Visibility of the save button for played matches.
        /// </summary>
        public Visibility SaveButtonVisibility
        {
            get => saveButtonVisibility;
            set
            {
                saveButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Data
        /// <summary>
        /// Current instance of the NavigationStore.
        /// </summary>
        public NavigationStore ns;
        /// <summary>
        /// Instance of a viewmodel of a schedule to return to, when the match will be saved.
        /// </summary>
        public NotifyPropertyChanged scheduleToReturnVM;
        /// <summary>
        /// Identification number of the season the match is played in.
        /// </summary>
        public int seasonID;
        /// <summary>
        /// Current match instance.
        /// </summary>
        public Match match;
        /// <summary>
        /// True, if the match is being edited. False, if the match is being created and added to the database.
        /// </summary>
        public bool edit;
        /// <summary>
        /// Thread for loading data from the scanned gamesheet with OCR script.
        /// </summary>
        public Thread gamesheetLoadingThread;
        /// <summary>
        /// Process for loading data from the scanned gamesheet with OCR script.
        /// </summary>
        private Process pythonProcess;

        private ObservableCollection<string> sides;
        /// <summary>
        /// Collection of sides. Home and Away.
        /// </summary>
        public ObservableCollection<string> Sides
        {
            get => sides;
            set
            {
                sides = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Collection of all team strength situations.
        /// </summary>
        public ObservableCollection<Strength> Strengths { get; private set; }

        /// <summary>
        /// Collection of all penalty reasons.
        /// </summary>
        public ObservableCollection<PenaltyReason> PenaltyReasons { get; private set; }

        /// <summary>
        /// Collection of all penalty types.
        /// </summary>
        public ObservableCollection<PenaltyType> PenaltyTypes { get; private set; }
        #endregion

        #region Commands
        private ICommand processCommand;
        /// <summary>
        /// When executed, it will process the match events.
        /// </summary>
        public ICommand ProcessCommand
        {
            get
            {
                if (processCommand == null)
                {
                    processCommand = new RelayCommand(param => Process());
                }
                return processCommand;
            }
        }

        private ICommand eventUpCommand;
        /// <summary>
        /// When executed, it will push the event earlier in the event order.
        /// </summary>
        public ICommand EventUpCommand
        {
            get
            {
                if (eventUpCommand == null)
                {
                    eventUpCommand = new RelayCommand(param => EventUp((Event)param));
                }
                return eventUpCommand;
            }
        }

        private ICommand eventDownCommand;
        /// <summary>
        /// When executed, it will push the event later in the event order.
        /// </summary>
        public ICommand EventDownCommand
        {
            get
            {
                if (eventDownCommand == null)
                {
                    eventDownCommand = new RelayCommand(param => EventDown((Event)param));
                }
                return eventDownCommand;
            }
        }

        private ICommand checkCommand;
        /// <summary>
        /// When executed, it will check the match events for collicions.
        /// </summary>
        public ICommand CheckCommand
        {
            get
            {
                if (checkCommand == null)
                {
                    checkCommand = new RelayCommand(param => Check());
                }
                return checkCommand;
            }
        }

        private ICommand saveCommand;
        /// <summary>
        /// When executed, it will save the played match.
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

        private ICommand notPlayedSaveCommand;
        /// <summary>
        /// When executed, it will save the scheduled match.
        /// </summary>
        public ICommand NotPlayedSaveCommand
        {
            get
            {
                if (notPlayedSaveCommand == null)
                {
                    notPlayedSaveCommand = new RelayCommand(param => NotPlayedSave());
                }
                return notPlayedSaveCommand;
            }
        }

        private ICommand exportGamesheetCommand;
        /// <summary>
        /// When executed, it will export preprepared gameshhet with filled rosters to PDF.
        /// </summary>
        public ICommand ExportGamesheetCommand
        {
            get
            {
                if (exportGamesheetCommand == null)
                {
                    exportGamesheetCommand = new RelayCommand(param => ExportGamesheet());
                }
                return exportGamesheetCommand;
            }
        }

        private ICommand exportGamesheetXLSXCommand;
        /// <summary>
        /// When executed, it will export preprepared gameshhet with filled rosters to XLSX.
        /// </summary>
        public ICommand ExportGamesheetXLSXCommand
        {
            get
            {
                if (exportGamesheetXLSXCommand == null)
                {
                    exportGamesheetXLSXCommand = new RelayCommand(param => ExportGamesheet("XLSX"));
                }
                return exportGamesheetXLSXCommand;
            }
        }

        private ICommand loadGamesheetCommand;
        /// <summary>
        /// When executed, it load the data from scanned gamesheet by OCR technique.
        /// </summary>
        public ICommand LoadGamesheetCommand
        {
            get
            {
                if (loadGamesheetCommand == null)
                {
                    loadGamesheetCommand = new RelayCommand(param => StartGameSheetLoading());
                }
                return loadGamesheetCommand;
            }
        }
        
        private ICommand cancelGamesheetLoadingCommand;
        /// <summary>
        /// When executed, it will cancel the gamesheet loading by OCR.
        /// </summary>
        public ICommand CancelGamesheetLoadingCommand
        {
            get
            {
                if (cancelGamesheetLoadingCommand == null)
                {
                    cancelGamesheetLoadingCommand = new RelayCommand(param => CancelGamesheetLoading());
                }
                return cancelGamesheetLoadingCommand;
            }
        }

        #endregion

        #region MatchEvents
        private ObservableCollection<Event> events;
        /// <summary>
        /// Collection of all match events.
        /// </summary>
        public ObservableCollection<Event> Events
        {
            get => events;
            set
            {
                events = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Event> conflictEvents;
        /// <summary>
        /// Collection of all match events that are in conflict.
        /// </summary>
        public ObservableCollection<Event> ConflictEvents
        {
            get => conflictEvents;
            set
            {
                conflictEvents = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PenaltyEndCollision> penaltyEndCollisions;
        /// <summary>
        /// Collection of all match events that are in conflict with penalty end.
        /// </summary>
        public ObservableCollection<PenaltyEndCollision> PenaltyEndCollisions
        {
            get => penaltyEndCollisions;
            set
            {
                penaltyEndCollisions = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Game states of the match in chronological order.
        /// </summary>
        List<State> timeSpans;
        #endregion

        #endregion

        #region Constructors
        private static readonly object _lock = new();

        /// <summary>
        /// Instantiates a new AddMatchViewModel for adding a match into a group.
        /// </summary>
        /// <param name="navigationStore">Current instance of the NavigationStore.</param>
        /// <param name="round">Index of the round the match will be inserted into.</param>
        public AddMatchViewModel(NavigationStore navigationStore, int round)
        {
            ns = navigationStore;
            seasonID = SportsData.SEASON.ID;
            scheduleToReturnVM = new GroupsScheduleViewModel(ns);
            this.round = round;
            Periods = new();
            BindingOperations.EnableCollectionSynchronization(Periods, _lock);
            Shootout = new();
            BindingOperations.EnableCollectionSynchronization(Shootout, _lock);
            LoadTeams();
            LoadSides();
            LoadStrengths();
            PenaltyReasons = SportsData.LoadPenaltyReasons();
            PenaltyTypes = SportsData.LoadPenaltyTypes();
        }

        /// <summary>
        /// Instantiates a new AddMatchViewModel for adding a match into a qualification bracket.
        /// </summary>
        /// <param name="navigationStore">Current instance of the NavigationStore.</param>
        /// <param name="scheduleToReturnVM">Instance of a viewmodel of a schedule to return to, when the match will be saved.</param>
        /// <param name="qualificationID">Identification number of qualification the match will be inserted into.</param>
        /// <param name="bracketIndex">Index of the serie of the round the match will be inserted into.</param>
        /// <param name="round">index of the round of the bracket the match will be inserted into.</param>
        /// <param name="serieMatchNumber">Number of the match in the current serie.</param>
        /// <param name="first">Instance of the first team of the serie.</param>
        /// <param name="second">Instance of the second team of the serie.</param>
        public AddMatchViewModel(NavigationStore navigationStore, NotifyPropertyChanged scheduleToReturnVM, int qualificationID, int bracketIndex, int round, int serieMatchNumber, Team first, Team second)
        {
            ns = navigationStore;
            seasonID = SportsData.SEASON.ID;
            this.scheduleToReturnVM = scheduleToReturnVM;
            this.qualificationID = qualificationID;
            this.bracketIndex = bracketIndex;
            this.round = round;
            this.serieMatchNumber = serieMatchNumber;
            bracketFirstTeamID = first.ID;
            Periods = new();
            BindingOperations.EnableCollectionSynchronization(Periods, _lock);
            Shootout = new();
            BindingOperations.EnableCollectionSynchronization(Shootout, _lock);
            AvailableTeamsHome = new();
            availableTeamsHome.Add(first);
            availableTeamsHome.Add(second);
            AvailableTeamsAway = new();
            AvailableTeamsAway.Add(first);
            AvailableTeamsAway.Add(second);
            LoadSides();
            LoadStrengths();
            PenaltyReasons = SportsData.LoadPenaltyReasons();
            PenaltyTypes = SportsData.LoadPenaltyTypes();
        }

        /// <summary>
        /// Instantiates a new AddMatchViewModel for editing an existing match.
        /// </summary>
        /// <param name="navigationStore">Current instance of the NavigationStore.</param>
        /// <param name="m">Instance of the match to edit.</param>
        /// <param name="scheduleToReturnVM">Instance of a viewmodel of a schedule to return to, when the match will be saved.</param>
        public AddMatchViewModel(NavigationStore navigationStore, Match m, NotifyPropertyChanged scheduleToReturnVM)
        {
            edit = true;
            ns = navigationStore;
            seasonID = m.Season.ID;
            match = m;
            this.scheduleToReturnVM = scheduleToReturnVM;
            LoadMatchInfo(m.ID);

            MatchDate = m.Datetime.Date;
            MatchTimeHours = m.Datetime.Hour;
            MatchTimeMinutes = m.Datetime.Minute;

            Periods = new();
            BindingOperations.EnableCollectionSynchronization(Periods, _lock);
            Shootout = new();
            BindingOperations.EnableCollectionSynchronization(Shootout, _lock);

            if (serieMatchNumber == -1)
            {
                LoadTeams();
            }
            else
            {
                AvailableTeamsHome = new();
                availableTeamsHome.Add(m.HomeTeam);
                availableTeamsHome.Add(m.AwayTeam);
                AvailableTeamsAway = new();
                AvailableTeamsAway.Add(m.HomeTeam);
                AvailableTeamsAway.Add(m.AwayTeam);

                HomeScore = m.HomeScore;
                AwayScore = m.AwayScore;
            }

            LoadSides();
            LoadStrengths();
            PenaltyReasons = SportsData.LoadPenaltyReasons();
            PenaltyTypes = SportsData.LoadPenaltyTypes();

            //set teams
            HomeTeam = AvailableTeamsHome.First(x => x.ID == m.HomeTeam.ID);
            AwayTeam = AvailableTeamsAway.First(x => x.ID == m.AwayTeam.ID);

            if (m.Played)
            {
                Played = m.Played;
                if (m.Overtime) { Overtime = m.Overtime; }
                if (m.Shootout) { IsShootout = m.Shootout; }
                if (m.Forfeit) { Forfeit = m.Forfeit; ForfeitWinnerSide = m.HomeScore > m.AwayScore ? "Home" : "Away"; }

                PeriodCount = m.Periods;
                PeriodDuration = m.PeriodDuration;

                //set rosters
                LoadExistingRosters(m.ID);

                //load all period events
                LoadExistingEvents(m.ID);

                //load shootout
                LoadExistingShootout(m.ID);
            }
        }
        #endregion

        #region Methods

        #region Loading
        /// <summary>
        /// Loads the playing side.
        /// </summary>
        private void LoadSides()
        {
            Sides = new ObservableCollection<string>
            {
                "Home",
                "Away"
            };
        }

        /// <summary>
        /// Loads the strengths from the database.
        /// </summary>
        private void LoadStrengths()
        {
            Strengths = new();

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT id, situation, advantage FROM strength", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    Strength s = new()
                    {
                        ID = int.Parse(row["id"].ToString()),
                        Situation = row["situation"].ToString(),
                        Advantage = row["advantage"].ToString()
                    };

                    Strengths.Add(s);
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
        /// Loads the available teams from the season from the database.
        /// </summary>
        private void LoadTeams()
        {
            AvailableTeamsHome = new();
            AvailableTeamsAway = new();

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT team_id, t.name AS team_name, season_id " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "WHERE season_id = " + seasonID, connection);

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
                        ID = int.Parse(tm["team_id"].ToString()),
                        Name = tm["team_name"].ToString()
                    };

                    AvailableTeamsHome.Add(t);
                    AvailableTeamsAway.Add(t);
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
        /// Loads the roster of the team from the database.
        /// </summary>
        private void LoadRoster(string side)
        {
            ObservableCollection<PlayerInRoster> roster = new();
            int teamID;
            if (side == "Home")
            {
                teamID = HomeTeam.ID;
            }
            else
            {
                teamID = AwayTeam.ID;
            }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT player_id, number, pos.name AS position_name, p.first_name AS player_first_name, p.last_name AS player_last_name " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN player AS p ON p.id = player_id " +
                                                "INNER JOIN position AS pos ON pos.code = position_code " +
                                                "WHERE season_id = " + seasonID + " AND team_id = " + teamID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PlayerInRoster p = new()
                    {
                        id = int.Parse(row["player_id"].ToString()),
                        Name = row["player_first_name"].ToString() + " " + row["player_last_name"].ToString(),
                        Number = int.Parse(row["number"].ToString()),
                        Position = row["position_name"].ToString(),
                        vm = this
                    };

                    roster.Add(p);
                }
                roster = new(roster.OrderBy(x => x.Number));

                if (side == "Home")
                {
                    HomePlayers = roster;
                    foreach (Period p in Periods)
                    {
                        p.HomeRoster = new();
                    }
                }
                else
                {
                    AwayPlayers = roster;
                    foreach (Period p in Periods)
                    {
                        p.AwayRoster = new();
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
        /// Loads the existing roster of the team from the database with filled attendance.
        /// </summary>
        private void LoadExistingRosters(int matchID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT player_id, side " +
                                                "FROM player_matches " +
                                                "WHERE match_id = " + matchID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    if (row["side"].ToString() == "H")
                    {
                        HomePlayers.First(x => x.id == int.Parse(row["player_id"].ToString())).Present = true;
                    }
                    else
                    {
                        AwayPlayers.First(x => x.id == int.Parse(row["player_id"].ToString())).Present = true;
                    }
                }
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databseROSTERS.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// Loads the match information from the database.
        /// </summary>
        private void LoadMatchInfo(int matchID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT qualification_id, bracket_index, round, serie_match_number, bracket_first_team FROM matches WHERE id = " + matchID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                qualificationID = int.Parse(dataTable.Rows[0]["qualification_id"].ToString());
                bracketIndex = int.Parse(dataTable.Rows[0]["bracket_index"].ToString());
                round = int.Parse(dataTable.Rows[0]["round"].ToString());
                serieMatchNumber = int.Parse(dataTable.Rows[0]["serie_match_number"].ToString());
                bracketFirstTeamID = int.Parse(dataTable.Rows[0]["bracket_first_team"].ToString());
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databseMATCH.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// Loads all the existing events of the match from the database.
        /// </summary>
        private void LoadExistingEvents(int matchID)
        {
            //load goals
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT player_id, assist_player_id, period, period_seconds, team_id, own_goal, penalty_shot, delayed_penalty " +
                                                "FROM goals WHERE match_id = " + matchID + " ORDER BY order_in_match", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Goal g = new()
                    { DelayedPenalty = Convert.ToBoolean(int.Parse(row["delayed_penalty"].ToString())),
                        PenaltyShot = Convert.ToBoolean(int.Parse(row["penalty_shot"].ToString())),
                        OwnGoal = Convert.ToBoolean(int.Parse(row["own_goal"].ToString())),
                        Side = HomeTeam.ID == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
                        Minute = int.Parse(row["period_seconds"].ToString()) / 60,
                        Second = int.Parse(row["period_seconds"].ToString()) % 60
                    };

                    int scorerID = int.Parse(row["player_id"].ToString());
                    int assistID = int.Parse(row["assist_player_id"].ToString());
                    if (g.Side == "Home")
                    {
                        g.Scorer = HomeRoster.First(x => x.id == scorerID);
                        if (assistID != SportsData.NOID) { g.Assist = HomeRoster.First(x => x.id == assistID); } else { g.Assist = new PlayerInRoster { id = SportsData.NOID }; }
                    }
                    else
                    {
                        g.Scorer = AwayRoster.First(x => x.id == scorerID);
                        if (assistID != SportsData.NOID) { g.Assist = AwayRoster.First(x => x.id == assistID); } else { g.Assist = new PlayerInRoster { id = SportsData.NOID }; }
                    }

                    int periodNumber = int.Parse(row["period"].ToString());
                    if (Overtime && periodNumber > 100)
                    {
                        Periods.Last().Goals.Add(g);
                    }
                    else
                    {
                        Periods.First(x => x.Number == periodNumber).Goals.Add(g);
                    }
                }

                //load penalties
                cmd = new MySqlCommand("SELECT player_id, period, period_seconds, team_id, penalty_reason_id, penalty_type_id " +
                                       "FROM penalties WHERE match_id = " + matchID + " ORDER BY order_in_match", connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Penalty p = new()
                    {
                        Side = HomeTeam.ID == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
                        Minute = int.Parse(row["period_seconds"].ToString()) / 60,
                        Second = int.Parse(row["period_seconds"].ToString()) % 60,
                        PenaltyReason = PenaltyReasons.First(x => x.Code == row["penalty_reason_id"].ToString()),
                        PenaltyType = PenaltyTypes.First(x => x.Code == row["penalty_type_id"].ToString())
                    };

                    int playerID = int.Parse(row["player_id"].ToString());
                    if (p.Side == "Home")
                    {
                        p.Player = HomeRoster.First(x => x.id == playerID);
                    }
                    else
                    {
                        p.Player = AwayRoster.First(x => x.id == playerID);
                    }

                    int periodNumber = int.Parse(row["period"].ToString());
                    if (Overtime && periodNumber > 100)
                    {
                        Periods.Last().Penalties.Add(p);
                    }
                    else
                    {
                        Periods.First(x => x.Number == periodNumber).Penalties.Add(p);
                    }
                }

                //load penalty shots
                cmd = new MySqlCommand("SELECT player_id, period, period_seconds, team_id, was_goal " +
                                       "FROM penalty_shots WHERE match_id = " + matchID + " ORDER BY order_in_match", connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    PenaltyShot ps = new()
                    {
                        Side = HomeTeam.ID == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
                        Minute = int.Parse(row["period_seconds"].ToString()) / 60,
                        Second = int.Parse(row["period_seconds"].ToString()) % 60,
                        WasGoal = Convert.ToBoolean(int.Parse(row["was_goal"].ToString()))
                    };

                    int playerID = int.Parse(row["player_id"].ToString());
                    if (ps.Side == "Home")
                    {
                        ps.Player = HomeRoster.First(x => x.id == playerID);
                    }
                    else
                    {
                        ps.Player = AwayRoster.First(x => x.id == playerID);
                    }

                    int periodNumber = int.Parse(row["period"].ToString());
                    if (Overtime && periodNumber > 100)
                    {
                        Periods.Last().PenaltyShots.Add(ps);
                    }
                    else
                    {
                        Periods.First(x => x.Number == periodNumber).PenaltyShots.Add(ps);
                    }
                }

                //load time-outs
                cmd = new MySqlCommand("SELECT period, period_seconds, team_id " +
                                       "FROM time_outs WHERE match_id = " + matchID + " ORDER BY order_in_match", connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    TimeOut t = new()
                    {
                        Side = HomeTeam.ID == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
                        Minute = int.Parse(row["period_seconds"].ToString()) / 60,
                        Second = int.Parse(row["period_seconds"].ToString()) % 60
                    };

                    int periodNumber = int.Parse(row["period"].ToString());
                    if (Overtime && periodNumber > 100)
                    {
                        Periods.Last().TimeOuts.Add(t);
                    }
                    else
                    {
                        Periods.First(x => x.Number == periodNumber).TimeOuts.Add(t);
                    }
                }

                //load shifts
                cmd = new MySqlCommand("SELECT player_id, period, period_seconds, end_period_seconds, team_id " +
                                       "FROM shifts WHERE match_id = " + matchID + " ORDER BY order_in_match", connection);

                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    GoalieShift s = new()
                    {
                        Side = HomeTeam.ID == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
                        StartMinute = int.Parse(row["period_seconds"].ToString()) / 60,
                        StartSecond = int.Parse(row["period_seconds"].ToString()) % 60,
                        EndMinute = int.Parse(row["end_period_seconds"].ToString()) / 60,
                        EndSecond = int.Parse(row["end_period_seconds"].ToString()) % 60
                    };

                    int playerID = int.Parse(row["player_id"].ToString());
                    if (s.Side == "Home")
                    {
                        s.Player = HomeRoster.First(x => x.id == playerID);
                    }
                    else
                    {
                        s.Player = AwayRoster.First(x => x.id == playerID);
                    }

                    int periodNumber = int.Parse(row["period"].ToString());
                    if (Overtime && periodNumber > 100)
                    {
                        Periods.Last().GoalieShifts.Add(s);
                    }
                    else
                    {
                        Periods.First(x => x.Number == periodNumber).GoalieShifts.Add(s);
                    }
                }

                connection.Close();
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
        /// Loads the existing shootout of the match from the database.
        /// </summary>
        private void LoadExistingShootout(int matchID)
        {
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT player_id, goalie_id, number, was_goal " +
                                                "FROM shootout_shots WHERE match_id = " + matchID + " ORDER BY number", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                ShootoutSeries = (dataTable.Rows.Count + 1) / 2;

                foreach (DataRow row in dataTable.Rows)
                {
                    ShootoutShot ss = Shootout.First(x => x.Number == int.Parse(row["number"].ToString()));
                    if (ss.Player.Number != null) { ss = Shootout.Where(x => x.Number == int.Parse(row["number"].ToString())).ElementAt(1); }
                    ss.WasGoal = Convert.ToBoolean(int.Parse(row["was_goal"].ToString()));

                    int playerID = int.Parse(row["player_id"].ToString());
                    int goalieID = int.Parse(row["goalie_id"].ToString());
                    if (ss.Side == "Home")
                    {
                        ss.Player = HomeRoster.First(x => x.id == playerID);
                        ss.Goalie = AwayRoster.First(x => x.id == goalieID);
                    }
                    else
                    {
                        ss.Player = AwayRoster.First(x => x.id == playerID);
                        ss.Goalie = HomeRoster.First(x => x.id == goalieID);
                    }
                }

                connection.Close();
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
        /// Selects the scanned gamesheet document file via file dialog and starts GamesheetOCR.exe with the file as parameter and process its output. Fills all the obtained data into the viewmodels formulars.
        /// </summary>
        private void LoadGamesheet()
        {
            try
            {
                if (HomeTeam == null)
                {
                    _ = MessageBox.Show("Please select the home team.", "Home team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (AwayTeam == null)
                {
                    _ = MessageBox.Show("Please select the away team.", "Away team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //warning: all filled data will be overwriten
                if (MessageBox.Show("Warning: All filled data will be overwriten. Do you wish to continue?", "Overwrite data", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }

                //select file (png, jpg)
                string gamesheetPath = "";
                OpenFileDialog openFileDialog = new();
                openFileDialog.Filter = "Pictures (*.jpg;*.png)|*.jpg;*.png";
                openFileDialog.DefaultExt = ".png";

                bool? result = openFileDialog.ShowDialog();
                if (result.ToString() != string.Empty)
                {
                    gamesheetPath = openFileDialog.FileName;
                }
                if (!File.Exists(gamesheetPath))
                {
                    return;
                }

                //show loading screen
                PageVisibility = Visibility.Collapsed;
                LoadingVisibility = Visibility.Visible;

                //run python script on it
                //***_via cmd and .py script_***
                //Process cmd = new();
                //cmd.StartInfo.FileName = "cmd.exe";
                //cmd.StartInfo.RedirectStandardInput = true;
                //cmd.StartInfo.RedirectStandardOutput = true;
                //cmd.StartInfo.CreateNoWindow = true;
                //cmd.StartInfo.UseShellExecute = false;
                //_ = cmd.Start();
                //
                //cmd.StandardInput.WriteLine("py " + SportsData.PythonOCRPath + " " + gamesheetPath + " " + HomePlayers.Count + " " + AwayPlayers.Count);
                //cmd.StandardInput.Flush();
                //cmd.StandardInput.Close();
                //cmd.WaitForExit();
                //string output = cmd.StandardOutput.ReadToEnd();

                //via executable (no need for python and libraries installed)
                pythonProcess = new();
                pythonProcess.StartInfo.FileName = SportsData.PythonOCRPath;
                pythonProcess.StartInfo.Arguments = gamesheetPath + " " + HomePlayers.Count + " " + AwayPlayers.Count;
                pythonProcess.StartInfo.RedirectStandardOutput = true;
                pythonProcess.StartInfo.CreateNoWindow = true;
                pythonProcess.StartInfo.UseShellExecute = false;
                pythonProcess.Start();
                
                pythonProcess.WaitForExit();
                string output = pythonProcess.StandardOutput.ReadToEnd();

                //retrieve results
                output = output.Replace("[", string.Empty);
                output = output.Replace("'", string.Empty);
                output = output.Replace("]", string.Empty);

                string[] data = output.Split("END", StringSplitOptions.RemoveEmptyEntries)./*Skip(1). (for cmd calling .py, first row is Microsoft...)*/ToArray();
                Array.Resize(ref data, data.Length - 1);

                List<string> errorList = new();

                //MATCH INFO
                data[0] = data[0].Replace("\r\n", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
                string[] info = data[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                Played = true;
                //PeriodCount = 0;
                IsShootout = false;

                if (info[0] == "T") { Forfeit = true; } else if (info[0] == "F" || info[0] == "empty") { Forfeit = false; } else { Forfeit = false; errorList.Add("- Match forfeited"); }
                if (int.TryParse(info[3], out int periodCount)) { PeriodCount = periodCount; } else { PeriodCount = 0; errorList.Add("- Period count"); }
                if (int.TryParse(info[4], out int periodDuration)) { PeriodDuration = periodDuration; } else { PeriodDuration = 0; errorList.Add("- Period duration"); }
                if (info[1] == "T") { Overtime = true; } else if (info[1] == "F" || info[1] == "empty") { Overtime = false; } else { Overtime = false; errorList.Add("- Overtime played"); }
                if (info[2] == "T") { IsShootout = true; } else if (info[2] == "F" || info[2] == "empty") { IsShootout = false; } else { IsShootout = false; errorList.Add("- Shootout happened"); }

                //HOME ROSTER
                data[1] = data[1].Replace(" ", string.Empty);
                string[] homeRoster = data[1].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < homeRoster.Length; i++)
                {
                    if (i == HomePlayers.Count) { errorList.Add("- There are more players in home roster than there are in home team"); break; }
                    if (homeRoster[i] == "T") { HomePlayers[i].Present = true; } else if (homeRoster[i] == "F" || homeRoster[i] == "empty") { HomePlayers[i].Present = false; } else { HomePlayers[i].Present = false; errorList.Add("- Home roster: player " + HomePlayers[i].Name + " #" + HomePlayers[i].Number); }
                }

                //AWAY ROSTER
                data[2] = data[2].Replace(" ", string.Empty);
                string[] awayRoster = data[2].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < awayRoster.Length; i++)
                {
                    if (i == AwayPlayers.Count) { errorList.Add("- There are more players in away roster than there are in away team"); break; }
                    if (awayRoster[i] == "T") { AwayPlayers[i].Present = true; } else if (awayRoster[i] == "F" || awayRoster[i] == "empty") { AwayPlayers[i].Present = false; } else { AwayPlayers[i].Present = false; errorList.Add("- Away roster: player " + AwayPlayers[i].Name + " #" + AwayPlayers[i].Number); }
                }

                //SHOOTOUT SHOTS
                if (IsShootout)
                {
                    string[] shootoutShots = data[8].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    string[] last = shootoutShots.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int emptyCellsInRow = 0;

                    if (!ProcessSideCell(last[0], out _)) { emptyCellsInRow++; }
                    if (!int.TryParse(last[1], out _)) { emptyCellsInRow++; }
                    if (!int.TryParse(last[2], out _)) { emptyCellsInRow++; }
                    if (!ProcessBooleanCell(last[3], out _, out _)) { emptyCellsInRow++; }

                    if (emptyCellsInRow > 1) { Array.Resize(ref shootoutShots, shootoutShots.Length - 1); }

                    ShootoutSeries = (shootoutShots.Length + 1) / 2;

                    for (int i = 0; i < shootoutShots.Length; i++)
                    {
                        string[] shootoutShot = shootoutShots[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        string errorMessage = "- Shootout shot in serie " + ((i / 2) + 1);

                        if (!ProcessSideCell(shootoutShot[0], out string side)) { errorList.Add(errorMessage); continue; }
                        if (!ProcessPlayerCell(shootoutShot[1], side, out PlayerInRoster player)) { errorList.Add(errorMessage); continue; }
                        if (!ProcessPlayerCell(shootoutShot[2], SwapSide(side), out PlayerInRoster goalie)) { errorList.Add(errorMessage); continue; }
                        if (!ProcessBooleanCell(shootoutShot[3], out bool wasGoal, out bool emptyWasGoal) && !emptyWasGoal) { errorList.Add(errorMessage); continue; }

                        //first is always home shot, then away
                        int index = side == "Home" ? (i / 2) * 2 : (i / 2) * 2 + 1;

                        Shootout[index].Number = (i / 2) + 1;
                        Shootout[index].Side = side;
                        Shootout[index].Player = Shootout[index].PlayerRoster.First(x => x.Number == player.Number);
                        Shootout[index].Goalie = Shootout[index].GoalieRoster.First(x => x.Number == goalie.Number);
                        Shootout[index].WasGoal = wasGoal;
                    }
                }

                if (PeriodCount == 0 || PeriodDuration == 0)
                {
                    return;
                }

                //GOALS
                string[] goals = data[3].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                string[] lastRow = goals.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int emptyCells = 0;

                if (!ProcessPeriodCell(lastRow[0], out _, out _)) { emptyCells++; }
                if (!ProcessPeriodTimeCell(lastRow[1], out _, out _)) { emptyCells++; }
                if (!ProcessSideCell(lastRow[2], out _)) { emptyCells++; }
                if (!int.TryParse(lastRow[3], out _)) { emptyCells++; }
                if (!int.TryParse(lastRow[4], out _) && lastRow[4] != "X") { emptyCells++; }
                if (!ProcessGoalTypeCell(lastRow[5], out _)) { emptyCells++; }

                if (emptyCells > 1) { Array.Resize(ref goals, goals.Length - 1); }

                for (int i = 0; i < goals.Length; i++)
                {
                    //process
                    string[] goal = goals[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string errorMessage = "- Goal: ";

                    if (!ProcessPeriodCell(goal[0], out int period, out bool overtime)) { errorList.Add(errorMessage + "unrecognized period"); continue; }
                    errorMessage += overtime ? "Overtime" : "period " + period;
                    if (!ProcessPeriodTimeCell(goal[1], out int minute, out int second)) { errorList.Add(errorMessage + ", unrecognized time"); continue; }
                    errorMessage += ", time " + minute + ":" + second;
                    if (!ProcessSideCell(goal[2], out string side)) { errorList.Add(errorMessage + ", unrecognized side"); continue; }
                    errorMessage += ", " + side;
                    if (!ProcessPlayerCell(goal[3], side, out PlayerInRoster player)) { errorList.Add(errorMessage + ", unrecognized player"); continue; }
                    errorMessage += ", " + player.Name;
                    if (!ProcessAssistCell(goal[4], side, out PlayerInRoster assist, out bool wasAssist)) { errorList.Add(errorMessage + ", unrecognized assist"); continue; }
                    if (player.Number == assist.Number) { errorList.Add(errorMessage + ", Goal and assist can not be made by the same player."); continue; }
                    if (!ProcessGoalTypeCell(goal[5], out string goalType)) { errorList.Add(errorMessage + ", unrecognized goal type"); continue; }

                    //insert
                    Goal g = new();
                    g.Minute = minute;
                    g.Second = second;
                    g.Side = side;
                    g.Scorer = player;
                    g.Assist = wasAssist ? assist : new PlayerInRoster { id = SportsData.NOID };
                    switch (goalType)
                    {
                        case "N":
                            break;
                        case "PS":
                            g.PenaltyShot = true;
                            break;
                        case "DP":
                            g.DelayedPenalty = true;
                            break;
                        case "OG":
                            g.OwnGoal = true;
                            break;
                        default:
                            break;
                    }

                    if (g.PenaltyShot || g.OwnGoal)
                    {
                        g.Assist = new PlayerInRoster { id = SportsData.NOID };
                    }

                    Period p = Periods.Last();
                    if (!overtime) { p = Periods[period - 1]; }
                    p.Goals.Add(g);
                }

                //sort
                foreach (Period p in Periods)
                {
                    if (p.Goals.Count > 0) { p.Goals.Sort(); }
                }

                //PENALTIES
                string[] penalties = data[4].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                lastRow = penalties.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                emptyCells = 0;

                if (!ProcessPeriodCell(lastRow[0], out _, out _)) { emptyCells++; }
                if (!ProcessPeriodTimeCell(lastRow[1], out _, out _)) { emptyCells++; }
                if (!ProcessSideCell(lastRow[2], out _)) { emptyCells++; }
                if (!int.TryParse(lastRow[3], out _)) { emptyCells++; }
                if (!ProcessPenaltyReasonCell(lastRow[4], out _)) { emptyCells++; }
                if (!ProcessPenaltyTypeCell(lastRow[5], out _)) { emptyCells++; }

                if (emptyCells > 1) { Array.Resize(ref penalties, penalties.Length - 1); }

                for (int i = 0; i < penalties.Length; i++)
                {
                    //process
                    string[] penalty = penalties[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string errorMessage = "- Penalty: ";

                    if (!ProcessPeriodCell(penalty[0], out int period, out bool overtime)) { errorList.Add(errorMessage + "unrecognized period"); continue; }
                    errorMessage += overtime ? "Overtime" : "period " + period;
                    if (!ProcessPeriodTimeCell(penalty[1], out int minute, out int second)) { errorList.Add(errorMessage + ", unrecognized time"); continue; }
                    errorMessage += ", time " + minute + ":" + second;
                    if (!ProcessSideCell(penalty[2], out string side)) { errorList.Add(errorMessage + ", unrecognized side"); continue; }
                    errorMessage += ", " + side;
                    if (!ProcessPlayerCell(penalty[3], side, out PlayerInRoster player)) { errorList.Add(errorMessage + ", unrecognized player"); continue; }
                    errorMessage += ", " + player.Name;
                    if (!ProcessPenaltyReasonCell(penalty[5], out PenaltyReason penaltyReason)) { errorList.Add(errorMessage + ", unrecognized penalty reason"); continue; }
                    if (!ProcessPenaltyTypeCell(penalty[5], out PenaltyType penaltyType)) { errorList.Add(errorMessage + ", unrecognized penalty type"); continue; }

                    //insert
                    Penalty pn = new();
                    pn.Minute = minute;
                    pn.Second = second;
                    pn.Side = side;
                    pn.Player = player;
                    pn.PenaltyReason = penaltyReason;
                    pn.PenaltyType = penaltyType;

                    Period p = Periods.Last();
                    if (!overtime) { p = Periods[period - 1]; }
                    p.Penalties.Add(pn);
                }

                //sort
                foreach (Period p in Periods)
                {
                    if (p.Penalties.Count > 0) { p.Penalties.Sort(); }
                }

                //PENALTY SHOTS
                string[] penaltyShots = data[5].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                lastRow = penaltyShots.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                emptyCells = 0;

                if (!ProcessPeriodCell(lastRow[0], out _, out _)) { emptyCells++; }
                if (!ProcessPeriodTimeCell(lastRow[1], out _, out _)) { emptyCells++; }
                if (!ProcessSideCell(lastRow[2], out _)) { emptyCells++; }
                if (!int.TryParse(lastRow[3], out _)) { emptyCells++; }
                if (!ProcessBooleanCell(lastRow[4], out _, out _)) { emptyCells++; }

                if (emptyCells > 1) { Array.Resize(ref penaltyShots, penaltyShots.Length - 1); }

                for (int i = 0; i < penaltyShots.Length; i++)
                {
                    //process
                    string[] penaltyShot = penaltyShots[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string errorMessage = "- Penalty shot: ";

                    if (!ProcessPeriodCell(penaltyShot[0], out int period, out bool overtime)) { errorList.Add(errorMessage + "unrecognized period"); continue; }
                    errorMessage += overtime ? "Overtime" : "period " + period;
                    if (!ProcessPeriodTimeCell(penaltyShot[1], out int minute, out int second)) { errorList.Add(errorMessage + ", unrecognized time"); continue; }
                    errorMessage += ", time " + minute + ":" + second;
                    if (!ProcessSideCell(penaltyShot[2], out string side)) { errorList.Add(errorMessage + ", unrecognized side"); continue; }
                    errorMessage += ", " + side;
                    if (!ProcessPlayerCell(penaltyShot[3], side, out PlayerInRoster player)) { errorList.Add(errorMessage + ", unrecognized player"); continue; }
                    errorMessage += ", " + player.Name;
                    if (!ProcessBooleanCell(penaltyShot[4], out bool scored, out bool empty) && !empty) { errorList.Add(errorMessage + ", can not tell if it was scored"); continue; }

                    //insert
                    PenaltyShot ps = new();
                    ps.Minute = minute;
                    ps.Second = second;
                    ps.Side = side;
                    ps.Player = player;
                    ps.WasGoal = scored;

                    Period p = Periods.Last();
                    if (!overtime) { p = Periods[period - 1]; }
                    p.PenaltyShots.Add(ps);
                }

                //sort
                foreach (Period p in Periods)
                {
                    if (p.PenaltyShots.Count > 0) { p.PenaltyShots.Sort(); }
                }

                //GOALTENDER SHIFTS
                string[] goaltenderShifts = data[6].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                lastRow = goaltenderShifts.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                emptyCells = 0;

                if (!ProcessPeriodCell(lastRow[0], out _, out _)) { emptyCells++; }
                if (!int.TryParse(lastRow[1], out _)) { emptyCells++; }
                if (!ProcessSideCell(lastRow[2], out _)) { emptyCells++; }
                if (!ProcessPeriodTimeCell(lastRow[3], out _, out _)) { emptyCells++; }
                if (!ProcessPeriodTimeCell(lastRow[4], out _, out _)) { emptyCells++; }

                if (emptyCells > 1) { Array.Resize(ref goaltenderShifts, goaltenderShifts.Length - 1); }

                for (int i = 0; i < goaltenderShifts.Length; i++)
                {
                    //process
                    string[] goaltenderShift = goaltenderShifts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string errorMessage = "- Goaltender shift: ";

                    if (!ProcessPeriodCell(goaltenderShift[0], out int period, out bool overtime)) { errorList.Add(errorMessage + "unrecognized period"); continue; }
                    errorMessage += overtime ? "Overtime" : "period " + period;
                    if (!ProcessSideCell(goaltenderShift[2], out string side)) { errorList.Add(errorMessage + ", unrecognized side"); continue; }
                    errorMessage += ", " + side;
                    if (!ProcessPlayerCell(goaltenderShift[1], side, out PlayerInRoster player)) { errorList.Add(errorMessage + ", unrecognized player"); continue; }
                    errorMessage += ", " + player.Name;
                    if (!ProcessPeriodTimeCell(goaltenderShift[3], out int startMinute, out int startSecond)) { errorList.Add(errorMessage + ", unrecognized start time"); continue; }
                    if (!ProcessPeriodTimeCell(goaltenderShift[4], out int endMinute, out int endSecond)) { errorList.Add(errorMessage + ", unrecognized end time"); continue; }

                    //insert
                    GoalieShift shift = new();
                    shift.Player = player;
                    shift.Side = side;
                    shift.StartMinute = startMinute;
                    shift.StartSecond = startSecond;
                    shift.EndMinute = endMinute;
                    shift.EndSecond = endSecond;

                    if (shift.EndTimeInSeconds - shift.StartTimeInSeconds < 1) { errorList.Add(errorMessage + ", goaltender shift is too short"); continue; }

                    Period p = Periods.Last();
                    if (!overtime) { p = Periods[period - 1]; }
                    p.GoalieShifts.Add(shift);
                }

                //sort
                foreach (Period p in Periods)
                {
                    if (p.GoalieShifts.Count > 0) { p.GoalieShifts.Sort(); }
                }

                //TIME-OUTS
                string[] timeOuts = data[7].Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                lastRow = timeOuts.Last().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                emptyCells = 0;

                if (!ProcessPeriodCell(lastRow[0], out _, out _)) { emptyCells++; }
                if (!ProcessPeriodTimeCell(lastRow[1], out _, out _)) { emptyCells++; }
                if (!ProcessSideCell(lastRow[2], out _)) { emptyCells++; }

                if (emptyCells > 1) { Array.Resize(ref timeOuts, timeOuts.Length - 1); }

                for (int i = 0; i < timeOuts.Length; i++)
                {
                    //process
                    string[] timeOut = timeOuts[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string errorMessage = "- Time-out: ";

                    if (!ProcessPeriodCell(timeOut[0], out int period, out bool overtime)) { errorList.Add(errorMessage + "unrecognized period"); continue; }
                    errorMessage += overtime ? "Overtime" : "period " + period;
                    if (!ProcessPeriodTimeCell(timeOut[1], out int minute, out int second)) { errorList.Add(errorMessage + ", unrecognized time"); continue; }
                    errorMessage += ", time " + minute + ":" + second;
                    if (!ProcessSideCell(timeOut[2], out string side)) { errorList.Add(errorMessage + ", unrecognized side"); continue; }

                    //insert
                    TimeOut to = new();
                    to.Minute = minute;
                    to.Second = second;
                    to.Side = side;

                    Period p = Periods.Last();
                    if (!overtime) { p = Periods[period - 1]; }
                    p.TimeOuts.Add(to);
                }

                //sort
                foreach (Period p in Periods)
                {
                    if (p.TimeOuts.Count > 0) { p.TimeOuts.Sort(); }
                }

                //hide loading screen
                LoadingVisibility = Visibility.Collapsed;
                PageVisibility = Visibility.Visible;

                //ERROR LIST
                string s = "";
                foreach (string item in errorList)
                {
                    s += item + "\n";
                }
                _ = MessageBox.Show(s, "Error list", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception) { }
            finally
            {
                //hide loading screen
                LoadingVisibility = Visibility.Collapsed;
                PageVisibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Starts loading of the gamesheet by OCR in a new thread.
        /// </summary>
        private void StartGameSheetLoading()
        {
            gamesheetLoadingThread = new Thread(() => { LoadGamesheet(); });
            gamesheetLoadingThread.Start();
        }

        /// <summary>
        /// Cancels loading of the gamesheet by OCR thread.
        /// </summary>
        private void CancelGamesheetLoading()
        {
            pythonProcess.Kill();
            gamesheetLoadingThread.Interrupt();
            gamesheetLoadingThread.Join();
        }

        /// <summary>
        /// Processes the output of OCR period cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="period">Returned number of period.</param>
        /// <param name="overtime">Returned overtime flag.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessPeriodCell(string cellValue, out int period, out bool overtime)
        {
            period = 0;
            overtime = false;

            if (int.TryParse(cellValue, out int periodNumber))
            {
                period = periodNumber;
                if (period > 0 && period <= PeriodCount)
                {
                    return true;
                }
            }
            else if (cellValue == "OT" && Overtime == true)
            {
                overtime = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processes the output of OCR period time cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="minute">Returned number of minutes.</param>
        /// <param name="second">Returned number of seconds.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessPeriodTimeCell(string cellValue, out int minute, out int second)
        {
            minute = -1;
            second = -1;

            string[] time = cellValue.Split(':');
            if (time.Length != 2) { return false; }

            if (!int.TryParse(time[0], out int m)) { return false; }
            if (!int.TryParse(time[1], out int s)) { return false; }

            if (m < 0 || m > PeriodDuration) { return false; }
            if (s < 0 || s > 59) { return false; }

            minute = m;
            second = s;
            return true;
        }

        /// <summary>
        /// Processes the output of OCR side cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="side">Returned Side.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessSideCell(string cellValue, out string side)
        {
            side = "";

            if (cellValue == "H") { side = "Home"; return true; }
            if (cellValue == "A") { side = "Away"; return true; }

            return false;
        }

        /// <summary>
        /// Processes the output of OCR player cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="side">Returned side.</param>
        /// <param name="player">Returned player number.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessPlayerCell(string cellValue, string side, out PlayerInRoster player)
        {
            player = new PlayerInRoster();

            if (!int.TryParse(cellValue, out int p)) { return false; }

            switch (side)
            {
                case "Home":
                    player = HomeRoster.FirstOrDefault(x => x.Number == p);
                    if (player == null) { return false; }
                    return true;
                case "Away":
                    player = AwayRoster.FirstOrDefault(x => x.Number == p);
                    if (player == null) { return false; }
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Processes the output of OCR assist cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="side">Returned side.</param>
        /// <param name="player">Returned player number.</param>
        /// <param name="exist">True, if there was an assist, otherwise false.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessAssistCell(string cellValue, string side, out PlayerInRoster player, out bool exist)
        {
            player = new PlayerInRoster();
            exist = false;

            if (!int.TryParse(cellValue, out int p))
            {
                if (cellValue == "X") { return true; }
            }

            switch (side)
            {
                case "Home":
                    player = HomeRoster.FirstOrDefault(x => x.Number == p);
                    if (player == null) { return false; }
                    exist = true;
                    return true;
                case "Away":
                    player = AwayRoster.FirstOrDefault(x => x.Number == p);
                    if (player == null) { return false; }
                    exist = true;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Processes the output of OCR goal type cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="type">Returned goal type abbreviation.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessGoalTypeCell(string cellValue, out string type)
        {
            type = "";

            switch (cellValue)
            {
                case "N":
                case "OG":
                case "DP":
                case "PS":
                    type = cellValue;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Processes the output of OCR boolean cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="boolean">Returned boolean.</param>
        /// <param name="empty">True, if there was no value.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessBooleanCell(string cellValue, out bool boolean, out bool empty)
        {
            boolean = false;
            empty = false;

            switch (cellValue)
            {
                case "T":
                    boolean = true;
                    return true;
                case "F":
                    boolean = false;
                    return true;
                case "empty":
                    empty = true;
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Processes the output of OCR penalty reason cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="reason">Returned penalty reason.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessPenaltyReasonCell(string cellValue, out PenaltyReason reason)
        {
            reason = PenaltyReasons.FirstOrDefault(x => x.Code == cellValue);

            if (reason == null) { return false; }
            return true;
        }

        /// <summary>
        /// Processes the output of OCR penalty type cell.
        /// </summary>
        /// <param name="cellValue">Value of the cell.</param>
        /// <param name="type">Returned penalty type.</param>
        /// <returns>True if successful.</returns>
        private bool ProcessPenaltyTypeCell(string cellValue, out PenaltyType type)
        {
            type = PenaltyTypes.FirstOrDefault(x => x.Code == cellValue);

            if (type == null) { return false; }
            return true;
        }

        /// <summary>
        /// Swaps Home for Away and the other way.
        /// </summary>
        /// <param name="side">Input side.</param>
        /// <returns>Opposite side.</returns>
        string SwapSide(string side)
        {
            switch (side)
            {
                case "Home":
                    return "Away";
                case "Away":
                    return "Home";
                default:
                    return "";
            }
        }
        #endregion

        #region Validating
        /// <summary>
        /// Group all events together and sorts them by time. Notifies for time collisions.
        /// </summary>
        private void Process()
        {
            if (PeriodCount == 0)
            {
                _ = MessageBox.Show("There has to be at least 1 period played.", "No period played", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PeriodDuration == 0)
            {
                _ = MessageBox.Show("Period duration can not be 0.", "Invalid period duration", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CheckButtonVisibility = Visibility.Visible;
            SaveButtonVisibility = Visibility.Collapsed;
            PenaltyEndCollisions = new();

            Events = new();
            ConflictEvents = new();

            foreach (Period p in Periods)
            {
                foreach (Goal g in p.Goals)
                {
                    Events.Add(new Event { Stat = g, Period = p });
                }
                foreach (Penalty pn in p.Penalties)
                {
                    Events.Add(new Event { Stat = pn, Period = p });
                }
                foreach (PenaltyShot ps in p.PenaltyShots)
                {
                    if (!ps.WasGoal)
                    {
                        Events.Add(new Event { Stat = ps, Period = p });
                    }
                }
                foreach (TimeOut t in p.TimeOuts)
                {
                    Events.Add(new Event { Stat = t, Period = p });
                }
                foreach (GoalieShift gs in p.GoalieShifts)
                {
                    GoalieChange gIn = new(gs, true);
                    GoalieChange gOut = new(gs, false);
                    Event i = new() { Stat = gIn, Period = p };
                    Event o = new() { Stat = gOut, Period = p };
                    gIn.pairEvent = o;
                    gOut.pairEvent = i;
                    Events.Add(i);
                    Events.Add(o);
                }
            }

            Events.Sort();
            for (int i = 0; i < Events.Count; i++)
            {
                Events[i].index = i;
            }

            ObservableCollection<Event> tmp = new();
            for (int i = 0; i < Events.Count; i++)
            {
                bool found = false;

                for (int j = 0; j < i; j++)
                {
                    if (Events[i].CompareTo(Events[j]) == 0)
                    {
                        tmp.Add(Events[i]);
                        found = true;
                        break;
                    }
                }

                if (found) { continue; }

                for (int j = i + 1; j < Events.Count; j++)
                {
                    if (Events[i].CompareTo(Events[j]) == 0)
                    {
                        tmp.Add(Events[i]);
                        break;
                    }
                }
            }

            ConflictEvents = tmp;

            if (ConflictEvents.Count > 0)
            {
                _ = MessageBox.Show("More events happened at the same time. Please check if the order of events is correct.", "Order confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Pushes event into earlier order that it happened in the match.
        /// </summary>
        /// <param name="e"></param>
        private void EventUp(Event e)
        {
            int idx = ConflictEvents.IndexOf(e);

            if (idx == 0 || ConflictEvents[idx - 1].CompareTo(e) != 0) { return; }

            ConflictEvents[idx] = ConflictEvents[idx - 1];
            ConflictEvents[idx].index++;
            ConflictEvents[idx - 1] = e;
            e.index--;
        }

        /// <summary>
        /// Pushes event into later order that it happened in the match.
        /// </summary>
        /// <param name="e"></param>
        private void EventDown(Event e)
        {
            int idx = ConflictEvents.IndexOf(e);

            if (idx == ConflictEvents.Count - 1 || ConflictEvents[idx + 1].CompareTo(e) != 0) { return; }

            ConflictEvents[idx] = ConflictEvents[idx + 1];
            ConflictEvents[idx].index--;
            ConflictEvents[idx + 1] = e;
            e.index++;
        }

        /// <summary>
        /// Returns number of players of a team in play.
        /// </summary>
        /// <param name="penalties">Number of teams penalties.</param>
        /// <returns>Number of players of a team in play.</returns>
        private int TeamStrength(int penalties)
        {
            if (penalties == 0)
            {
                return 5;
            }
            if (penalties == 1)
            {
                return 4;
            }
            return 3;
        }

        /// <summary>
        /// Checks for time collisions between events and end of penalties.
        /// </summary>
        /// <param name="eventStartTime">Start time of the event.</param>
        /// <param name="actualPenalties">List of actual penalties.</param>
        /// <param name="actualState">Actual state of the game.</param>
        /// <param name="fullPenalties">List of penalties that were not ended by a goal.</param>
        /// <param name="fullPenaltyStateIndices">Indeces of the states of the full penalties.</param>
        private void CheckEndOfPenalties(int eventStartTime, List<Penalty> actualPenalties, State actualState, List<Penalty> fullPenalties, List<int> fullPenaltyStateIndices)
        {
            int penCount = actualPenalties.Count;
            for (int j = 0; j < penCount; j++)
            {
                //if ends at the same time don't do anything
                if (actualPenalties[j].endTime < eventStartTime - 1)
                {
                    //record penalty timespan
                    actualState.endTime = actualPenalties[j].endTime;
                    //if it is at least 1 second long (eliminates 0 seconds lasting penalties terminated by goal)
                    if (actualPenalties[j].endTime - actualState.startTime != 0)
                    {
                        timeSpans.Add(new State(actualState));
                    }

                    //set new state
                    actualState.startTime = actualPenalties[j].endTime;

                    int endingPenaltiesAtSameTime = actualPenalties.Count(x => x.endTime == actualPenalties[j].endTime);
                    for (int k = 0; k < endingPenaltiesAtSameTime; k++)
                    {
                        actualPenalties[j].duration = actualPenalties[j].PenaltyType.Minutes * 60;
                        fullPenalties.Add(actualPenalties[j]);
                        fullPenaltyStateIndices.Add(timeSpans.Count - 1);
                        actualPenalties.RemoveAt(j);
                        penCount--;
                    }
                    actualState.homeStrength = TeamStrength(actualPenalties.Count(x => x.Side == "Home"));
                    actualState.awayStrength = TeamStrength(actualPenalties.Count(x => x.Side == "Away"));
                    j--;
                }
            }
        }

        /// <summary>
        /// Sets event properties according to state they happened in, chcecks penalty end collisions with events.
        /// </summary>
        private void Check()
        {
            SaveButtonVisibility = Visibility.Visible;

            foreach (Event e in ConflictEvents)
            {
                Events[e.index] = e;
            }
            ConflictEvents = new();

            //delete PeriodEnds if some exist from previous check (if data was checked but changed after that)
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].Stat.GetType() == typeof(PeriodEnd))
                {
                    _ = Events.Remove(Events[i]);
                }
            }

            int periodIndex = 0;
            int eventsCount = Events.Count;
            for (int i = 0; i < eventsCount; i++)
            {
                if (Events[i].Period.Number != Periods[periodIndex].Number)
                {
                    int index = periodIndex + 1;
                    if (index == Periods.Count) { index--; }
                    Events.Insert(i, new Event { Stat = new PeriodEnd(), Period = Periods[index] });
                    periodIndex++;
                    eventsCount++;
                }
            }
            for (int i = periodIndex; i < Periods.Count; i++)
            {
                int index = i + 1;
                if (index == Periods.Count) { index--; }
                Events.Add(new Event { Stat = new PeriodEnd(), Period = Periods[index] });
            }

            List<Penalty> actualPenalties = new();
            timeSpans = new List<State>();

            List<Penalty> fullPenalties = new();
            List<int> fullPenaltyStateIndices = new();

            State actualState = new(Periods[0], 0, 0, 5, 5, false, false, 0, 0);
            bool end = false;

            for (int i = 0; i < Events.Count; i++)
            {
                if (end) { break; }
                switch (Events[i].Stat)
                {
                    case PenaltyShot ps:
                        CheckEndOfPenalties(ps.TimeInSeconds, actualPenalties, actualState, fullPenalties, fullPenaltyStateIndices);

                        ps.strength = actualState.StrengthToString();
                        ps.homeScore = actualState.homeGoals;
                        ps.awayScore = actualState.awayGoals;
                        break;

                    case TimeOut t:
                        CheckEndOfPenalties(t.TimeInSeconds, actualPenalties, actualState, fullPenalties, fullPenaltyStateIndices);

                        t.strength = actualState.StrengthToString();
                        t.homeScore = actualState.homeGoals;
                        t.awayScore = actualState.awayGoals;
                        break;

                    case PeriodEnd pe:
                        CheckEndOfPenalties((Events[i].Period.duration * 60) + 2, actualPenalties, actualState, fullPenalties, fullPenaltyStateIndices);

                        actualState.endTime = Events[i].Period.duration * 60;
                        if (actualState.period == Periods.Last())
                        {
                            foreach (Penalty p in actualPenalties)
                            {
                                p.endTime = Events[i].Period.duration * 60;
                                p.duration = p.endTime - p.startTime;
                                if (p.duration < 0)
                                {
                                    p.duration = p.endTime + (Events[i].Period.duration * 60) - p.startTime;
                                }
                            }
                        }
                        timeSpans.Add(new State(actualState));

                        actualState.startTime = 0;
                        actualState.period = Events[i].Period;
                        foreach (Penalty p in actualPenalties)
                        {
                            if (p.startTime > Events[i].Period.duration * 60)
                            {
                                p.startTime -= Events[i].Period.duration * 60;
                            }
                            p.endTime -= Events[i].Period.duration * 60;
                        }
                        break;

                    case GoalieChange gch:
                        CheckEndOfPenalties(gch.TimeInSeconds, actualPenalties, actualState, fullPenalties, fullPenaltyStateIndices);

                        gch.strength = actualState.StrengthToString();
                        gch.homeScore = actualState.homeGoals;
                        gch.awayScore = actualState.awayGoals;

                        actualState.endTime = gch.TimeInSeconds;
                        timeSpans.Add(new State(actualState));

                        actualState.startTime = gch.TimeInSeconds;
                        if (gch.Side == "Home")
                        {
                            actualState.homeGoalieIn = gch.Entered;
                        }
                        else
                        {
                            actualState.awayGoalieIn = gch.Entered;
                        }
                        break;

                    case Penalty p:
                        CheckEndOfPenalties(p.TimeInSeconds, actualPenalties, actualState, fullPenalties, fullPenaltyStateIndices);

                        p.strength = actualState.StrengthToString();
                        p.homeScore = actualState.homeGoals;
                        p.awayScore = actualState.awayGoals;

                        if (p.PenaltyType.Minutes is 10 or 20)
                        {
                            p.duration = p.PenaltyType.Minutes * 60;
                        }

                        if (p.PenaltyType.Minutes == 2 || p.PenaltyType.Minutes == 5)
                        {
                            actualState.endTime = p.TimeInSeconds;
                            timeSpans.Add(new State(actualState));

                            if (actualPenalties.Any(x => x.Player == p.Player) || actualPenalties.Count >= 2)
                            {
                                p.startTime = actualPenalties.Last().endTime + 1;
                            }
                            else
                            {
                                p.startTime = p.TimeInSeconds;
                            }
                            p.endTime = p.startTime + (p.PenaltyType.Minutes * 60);
                            actualPenalties.Add(p);

                            actualState.startTime = p.TimeInSeconds;
                            actualState.homeStrength = TeamStrength(actualPenalties.Count(x => x.Side == "Home"));
                            actualState.awayStrength = TeamStrength(actualPenalties.Count(x => x.Side == "Away"));
                        }
                        break;

                    case Goal g:
                        CheckEndOfPenalties(g.TimeInSeconds, actualPenalties, actualState, fullPenalties, fullPenaltyStateIndices);

                        g.strength = actualState.StrengthToString();
                        g.homeScore = actualState.homeGoals;
                        g.awayScore = actualState.awayGoals;

                        actualState.endTime = g.TimeInSeconds;
                        timeSpans.Add(new State(actualState));

                        if (Events[i].Period.overtime)
                        {
                            end = true;
                            Periods.Last().duration = g.TimeInSeconds;
                        }

                        //update score
                        if (g.Side == "Home")
                        {
                            if (g.OwnGoal) { actualState.awayGoals++; }
                            else { actualState.homeGoals++; }
                            if (!actualState.awayGoalieIn) { g.emptyNet = true; }
                        }
                        else
                        {
                            if (g.OwnGoal) { actualState.homeGoals++; }
                            else { actualState.awayGoals++; }
                            if (!actualState.homeGoalieIn) { g.emptyNet = true; }
                        }

                        actualState.startTime = g.TimeInSeconds;

                        Penalty actualPenalty = actualPenalties.Find(x => x.startTime < g.TimeInSeconds && x.PenaltyType.Minutes == 2);
                        if (!g.PenaltyShot && !g.DelayedPenalty && actualPenalty != null)
                        {
                            if (g.Side == "Home" && actualState.homeStrength > actualState.awayStrength)
                            {
                                actualPenalty = actualPenalties.Find(x => x.startTime < g.TimeInSeconds && x.PenaltyType.Minutes == 2 && x.Side == "Away");
                                actualPenalty.endTime = g.TimeInSeconds;
                                actualPenalty.duration = actualPenalty.endTime - actualPenalty.startTime;
                                if (actualPenalty.duration < 0)
                                {
                                    actualPenalty.duration = actualPenalty.endTime + (Events[i].Period.duration * 60) - actualPenalty.startTime;
                                }
                                actualPenalty.punished = true;
                                _ = actualPenalties.Remove(actualPenalty);
                                if (actualPenalty.duration == actualPenalty.PenaltyType.Minutes * 60) { fullPenalties.Add(actualPenalty); fullPenaltyStateIndices.Add(timeSpans.Count - 1); }
                                foreach (Penalty p in actualPenalties)
                                {
                                    if (p.Side == "Away" && p.startTime < g.TimeInSeconds)
                                    {
                                        p.startTime -= (actualPenalty.PenaltyType.Minutes * 60) - (actualPenalty.endTime - actualPenalty.startTime);
                                        p.endTime -= (actualPenalty.PenaltyType.Minutes * 60) - (actualPenalty.endTime - actualPenalty.startTime);
                                    }
                                }
                            }
                            else if (g.Side == "Away" && actualState.homeStrength < actualState.awayStrength)
                            {
                                actualPenalty = actualPenalties.Find(x => x.startTime < g.TimeInSeconds && x.PenaltyType.Minutes == 2 && x.Side == "Home");
                                actualPenalty.endTime = g.TimeInSeconds;
                                actualPenalty.duration = actualPenalty.endTime - actualPenalty.startTime;
                                if (actualPenalty.duration < 0)
                                {
                                    actualPenalty.duration = actualPenalty.endTime + (Events[i].Period.duration * 60) - actualPenalty.startTime;
                                }
                                actualPenalty.punished = true;
                                _ = actualPenalties.Remove(actualPenalty);
                                if (actualPenalty.duration == actualPenalty.PenaltyType.Minutes * 60) { fullPenalties.Add(actualPenalty); fullPenaltyStateIndices.Add(timeSpans.Count - 1); }
                                foreach (Penalty p in actualPenalties)
                                {
                                    if (p.Side == "Home" && p.startTime < g.TimeInSeconds)
                                    {
                                        p.startTime -= (actualPenalty.PenaltyType.Minutes * 60) - (actualPenalty.endTime - actualPenalty.startTime);
                                        p.endTime -= (actualPenalty.PenaltyType.Minutes * 60) - (actualPenalty.endTime - actualPenalty.startTime);
                                    }
                                }
                            }
                        }

                        actualPenalty = actualPenalties.Find(x => x.startTime < g.TimeInSeconds && x.PenaltyType.Minutes == 5);
                        if (!g.PenaltyShot && !g.DelayedPenalty && actualPenalty != null)
                        {
                            if ((g.Side == "Home" && actualState.homeStrength > actualState.awayStrength) || (g.Side == "Away" && actualState.homeStrength < actualState.awayStrength))
                            {
                                actualPenalty.punished = true;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //add check button and check for events happened at the end of not terminated penalties, were these penalties punished?
            PenaltyEndCollisions = new();

            for (int i = 0; i < fullPenalties.Count; i++)
            {
                //when not the last state
                if (fullPenaltyStateIndices[i] < timeSpans.Count - 1)
                {
                    PenaltyEndCollision penaltyEnd = new(timeSpans[fullPenaltyStateIndices[i]], timeSpans[fullPenaltyStateIndices[i] + 1], fullPenalties[i]);

                    foreach (Event collidingE in Events)
                    {
                        //  all events except period end          &&        occured at the same time
                        if (collidingE.Stat.GetType() != typeof(Period) && collidingE.Stat.TimeInSeconds == fullPenalties[i].endTime)
                        {
                            penaltyEnd.SwapEvents.Add(new SwapEvent(collidingE));
                        }
                    }

                    if (penaltyEnd.SwapEvents.Count > 0)
                    {
                        PenaltyEndCollisions.Add(penaltyEnd);
                    }
                }
            }
        }

        /// <summary>
        /// If editing decided serie in bracket, this method checks if we wont add more wins than possible to the winning team.
        /// </summary>
        /// <param name="homeScore"></param>
        /// <param name="awayScore"></param>
        /// <returns>True, if the match result vioalates the serie limitation.</returns>
        private bool ExceedsFirstToWin(int homeScore, int awayScore)
        {
            if (!(HomeScore <= AwayScore && homeScore > awayScore) || !(HomeScore >= AwayScore && homeScore < awayScore))
            {
                return false;
            }

            int homeWins = 0;
            int awayWins = 0;
            int firstToWin;

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlCommand cmd = new("SELECT home_competitor, away_competitor, home_score, away_score " +
                                                "FROM matches " +
                                                "WHERE qualification_id = " + qualificationID + " AND played = 1 AND round = " + round + " AND bracket_index = " + bracketIndex, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow tm in dataTable.Rows)
                {
                    int hTeam = int.Parse(tm["home_competitor"].ToString());
                    int aTeam = int.Parse(tm["away_competitor"].ToString());
                    int hScore = int.Parse(tm["home_score"].ToString());
                    int aScore = int.Parse(tm["away_score"].ToString());

                    if (hTeam == HomeTeam.ID)
                    {
                        if (hScore > aScore)
                        {
                            homeWins++;
                        }
                        else if (hScore < aScore)
                        {
                            awayWins++;
                        }
                    }
                    else
                    {
                        if (hScore > aScore)
                        {
                            awayWins++;
                        }
                        else if (hScore < aScore)
                        {
                            homeWins++;
                        }
                    }
                }

                cmd = new MySqlCommand("SELECT play_off_best_of FROM seasons WHERE id = " + seasonID, connection);
                dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                firstToWin = int.Parse(dataTable.Rows[0]["play_off_best_of"].ToString());

                if (homeWins >= firstToWin)
                {
                    if (homeScore > awayScore)
                    {
                        return true;
                    }
                }
                if (awayWins >= firstToWin)
                {
                    if (homeScore < awayScore)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                _ = MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
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

        #region Saving
        /// <summary>
        /// Exports empty gamesheet with preprepared rosters of teams into PDF or XLSX format. Opens up file dialog window.
        /// </summary>
        /// <param name="format">PDF or XLSX.</param>
        private void ExportGamesheet(string format = "PDF")
        {
            if (HomeTeam == null)
            {
                _ = MessageBox.Show("Please select the home team.", "Home team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (AwayTeam == null)
            {
                _ = MessageBox.Show("Please select the away team.", "Away team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            //load excel file
            string tempPath = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllBytes(tempPath, Properties.Resources.gamesheet);
            Microsoft.Office.Interop.Excel.Application excelApplication = new();
            Microsoft.Office.Interop.Excel._Workbook excelWorkbook;
            excelWorkbook = excelApplication.Workbooks.Open(tempPath);
            
            //fill data, datetime, teams, rosters
            Microsoft.Office.Interop.Excel.Worksheet gamesheet = (Microsoft.Office.Interop.Excel.Worksheet)excelWorkbook.Worksheets[1];
            
            //match info
            gamesheet.Range["G" + 8].Value = SportsData.COMPETITION.Name;
            gamesheet.Range["J" + 9].Value = SportsData.SEASON.Name;
            
            if (serieMatchNumber < 1)
            {
                gamesheet.Range["J" + 10].Value = "Group";
            }
            else if (qualificationID > 0)
            {
                gamesheet.Range["J" + 10].Value = "Qualification";
            }
            else
            {
                gamesheet.Range["J" + 10].Value = "Play-off";
            }
            
            //teams
            gamesheet.Range["A" + 3].Value = HomeTeam.Name;
            gamesheet.Range["G" + 3].Value = AwayTeam.Name;
            
            //datetime
            gamesheet.Range["C" + 1].Value = MatchDateTime.ToString("d");
            gamesheet.Range["I" + 1].Value = MatchDateTime.ToString("HH:mm");
            
            //rosters
            int row;
            for (int i = 0; i < HomePlayers.Count; i++)
            {
                row = i + 18;
                //number
                gamesheet.Range["A" + row].Value = HomePlayers[i].Number;
                //name
                gamesheet.Range["C" + row].Value = HomePlayers[i].Name;
            }
            for (int i = 0; i < AwayPlayers.Count; i++)
            {
                row = i + 18;
                //number
                gamesheet.Range["L" + row].Value = AwayPlayers[i].Number;
                //name
                gamesheet.Range["N" + row].Value = AwayPlayers[i].Name;
            }

            //select path
            string gamesheetPath = "";
            
            SaveFileDialog saveFileDialog = new();
            switch (format)
            {
                case "PDF":
                    saveFileDialog.Filter = "PDF Files | *.pdf";
                    saveFileDialog.DefaultExt = "pdf";
                    break;
                case "XLSX":
                    saveFileDialog.Filter = "XLSX | *.xlsx";
                    saveFileDialog.DefaultExt = "xlsx";
                    break;
                default:
                    break;
            }
            saveFileDialog.FileName = "gamesheet_" + MatchDateTime.ToString("yyyy_MM_dd_HH_mm") + "_" + HomeTeam.Name + "_vs_" + AwayTeam.Name;

            bool? result = saveFileDialog.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                gamesheetPath = saveFileDialog.FileName;

                switch (format)
                {
                    case "PDF":
                        //export to pdf
                        try
                        {
                            excelWorkbook.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, gamesheetPath);
                        }
                        catch (Exception) { }
                        break;
                    case "XLSX":
                        excelWorkbook.SaveCopyAs(gamesheetPath);
                        break;
                    default:
                        break;
                }
            }

            excelWorkbook.Close(false);
            File.Delete(tempPath);
        }

        /// <summary>
        /// Save the match into the database when it was not played but is only scheduled.
        /// </summary>
        private void NotPlayedSave()
        {
            //validation
            if (serieMatchNumber != -1)
            {
                _ = MessageBox.Show("Qualification or play-off match needs to be played.", "Match not played", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (HomeTeam == null)
            {
                _ = MessageBox.Show("Please select the home team.", "Home team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (AwayTeam == null)
            {
                _ = MessageBox.Show("Please select the away team.", "Away team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!edit)
            {
                bracketFirstTeamID = HomeTeam.ID;
            }

            //saving
            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string querry = "INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, periods, " +
                                        "period_duration, home_competitor, away_competitor, home_score, away_score, datetime, overtime, shootout, forfeit, bracket_first_team) " +
                                        "VALUES (" + seasonID + ", " + 0 + ", " + qualificationID + ", " + bracketIndex +
                                        ", " + round + ", " + serieMatchNumber + ", " + 0 + ", " + 0 + ", " + HomeTeam.ID +
                                        ", " + AwayTeam.ID + ", " + 0 + ", " + 0 + ", '" + MatchDateTime.ToString("yyyy-MM-dd H:mm:ss") + "', " + 0 +
                                        ", " + 0 + ", " + 0 + ", " + bracketFirstTeamID + ")";

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                cmd = new(querry, connection);
                cmd.Transaction = transaction;
                _ = cmd.ExecuteNonQuery();
                int matchID = (int)cmd.LastInsertedId;

                if (edit)
                {
                    //delete match from DB
                    querry = "DELETE FROM matches WHERE id = " + match.ID;

                    cmd = new(querry, connection);
                    cmd.Transaction = transaction;
                    _ = cmd.ExecuteNonQuery();

                    //delete all player/goalie match enlistments and all stats
                    List<string> databases = new() { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                    foreach (string db in databases)
                    {
                        querry = "DELETE FROM " + db + " WHERE match_id = " + match.ID;
                        cmd = new(querry, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                connection.Close();

                ScheduleViewModel scheduleViewModel = new(ns);
                if (edit)
                {
                    new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, new Match { ID = matchID }, scheduleToReturnVM))).Execute(null);
                }
                else
                {
                    switch (scheduleToReturnVM)
                    {
                        case GroupsScheduleViewModel:
                            scheduleViewModel.GroupsSet = true;
                            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, scheduleViewModel)).Execute(null);
                            break;
                        default:
                            break;
                    }
                }
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

        /// <summary>
        /// Saves the match into the database if it was played.
        /// </summary>
        private void Save()
        {
            //resolve score and strength for all PenaltyEndCollisions
            foreach (PenaltyEndCollision endCollision in PenaltyEndCollisions)
            {
                foreach (SwapEvent swapEvent in endCollision.SwapEvents)
                {
                    if (swapEvent.InPenaltyEvent != null)
                    {
                        swapEvent.InPenaltyEvent.Stat.strength = endCollision.InPenalty.StrengthToString();
                        swapEvent.InPenaltyEvent.Stat.homeScore = endCollision.InPenalty.homeGoals;
                        swapEvent.InPenaltyEvent.Stat.awayScore = endCollision.InPenalty.awayGoals;
                    }
                    else if (swapEvent.OutPenaltyEvent != null)
                    {
                        swapEvent.OutPenaltyEvent.Stat.strength = endCollision.OutPenalty.StrengthToString();
                        swapEvent.OutPenaltyEvent.Stat.homeScore = endCollision.OutPenalty.homeGoals;
                        swapEvent.OutPenaltyEvent.Stat.awayScore = endCollision.OutPenalty.awayGoals;
                    }
                }
            }

            //add goalies to goals
            foreach (Event e in Events)
            {
                if (e.Stat.GetType() == typeof(Goal) && !((Goal)e.Stat).emptyNet)
                {
                    for (int i = Events.IndexOf(e); i >= 0; i--)
                    {
                        if (Events[i].Stat.GetType() == typeof(GoalieChange) && ((GoalieChange)Events[i].Stat).Entered && ((GoalieChange)Events[i].Stat).Side != ((Goal)e.Stat).Side)
                        {
                            ((Goal)e.Stat).goalie = ((GoalieChange)Events[i].Stat).Player;
                            break;
                        }
                    }
                }
            }
            //add goalies to penalty shots
            foreach (Event e in Events)
            {
                if (e.Stat.GetType() == typeof(PenaltyShot))
                {
                    for (int i = Events.IndexOf(e); i >= 0; i--)
                    {
                        if (Events[i].Stat.GetType() == typeof(GoalieChange) && ((GoalieChange)Events[i].Stat).Entered && ((GoalieChange)Events[i].Stat).Side != ((PenaltyShot)e.Stat).Side)
                        {
                            ((PenaltyShot)e.Stat).goalie = ((GoalieChange)Events[i].Stat).Player;
                            break;
                        }
                    }
                }
            }

            //save successful penalty shots - pair them with penalty shot goals by player_id, goalie_id, time...
            foreach (Period period in Periods)
            {
                List<Goal> goals = new();
                foreach (Event e in Events)
                {
                    if (e.Stat.GetType() == typeof(Goal) && e.Period == period && ((Goal)e.Stat).PenaltyShot)
                    {
                        goals.Add((Goal)e.Stat);
                    }
                }
                foreach (PenaltyShot ps in period.PenaltyShots)
                {
                    if (ps.WasGoal)
                    {
                        Goal g = goals.Find(x => x.TimeInSeconds == ps.TimeInSeconds && x.Scorer == ps.Player);
                        ps.strength = g.strength;
                        ps.homeScore = g.homeScore;
                        ps.awayScore = g.awayScore;
                        ps.goalie = g.goalie;
                        Events.Add(new Event { Period = period, Stat = ps, index = Events.First(x => x.Stat == g).index });
                    }
                }
            }

            //score
            int homeScore = Events.Count(x => x.Stat.Side == "Home" && x.Stat.GetType() == typeof(Goal) && !((Goal)x.Stat).OwnGoal) + Events.Count(x => x.Stat.Side == "Away" && x.Stat.GetType() == typeof(Goal) && ((Goal)x.Stat).OwnGoal);
            int awayScore = Events.Count(x => x.Stat.Side == "Away" && x.Stat.GetType() == typeof(Goal) && !((Goal)x.Stat).OwnGoal) + Events.Count(x => x.Stat.Side == "Home" && x.Stat.GetType() == typeof(Goal) && ((Goal)x.Stat).OwnGoal);

            //game-winning/loosing goal
            if (!IsShootout && !Forfeit && homeScore != awayScore)
            {
                Goal gwg = null;
                if (homeScore > awayScore)
                {
                    gwg = (Goal)Events.First(x => x.Stat.GetType() == typeof(Goal) && x.Stat.homeScore == awayScore).Stat;
                }
                else
                {
                    gwg = (Goal)Events.First(x => x.Stat.GetType() == typeof(Goal) && x.Stat.awayScore == homeScore).Stat;
                }

                if (!gwg.OwnGoal)
                {
                    gwg.gameWinningGoal = true;
                }
                else
                {
                    gwg.gameLosingOwnGoal = true;
                }
            }

            //shutouts
            List<Shutout> shutouts = new();
            if (!Forfeit)
            {
                if (awayScore == 0)
                {
                    shutouts.Add(new Shutout(null, "Home"));
                    if (Events.Where(x => x.Stat.GetType() == typeof(GoalieChange) && x.Stat.Side == "Home").GroupBy(x => ((GoalieChange)x.Stat).Player.Number).Count() == 1)
                    {
                        shutouts.Add(new Shutout(((GoalieChange)Events.First(x => x.Stat.GetType() == typeof(GoalieChange) && x.Stat.Side == "Home").Stat).Player, "Home"));
                    }
                }
                if (homeScore == 0)
                {
                    shutouts.Add(new Shutout(null, "Away"));
                    if (Events.Where(x => x.Stat.GetType() == typeof(GoalieChange) && x.Stat.Side == "Away").GroupBy(x => ((GoalieChange)x.Stat).Player.Number).Count() == 1)
                    {
                        shutouts.Add(new Shutout(((GoalieChange)Events.First(x => x.Stat.GetType() == typeof(GoalieChange) && x.Stat.Side == "Away").Stat).Player, "Away"));
                    }
                }
            }

            //forfeit
            if (Forfeit)
            {
                if (ForfeitWinnerSide == "Home")
                {
                    homeScore = Math.Max(homeScore, 1);
                    awayScore = 0;
                }
                else
                {
                    homeScore = 0;
                    awayScore = Math.Max(awayScore, 1);
                }
            }

            //shootout
            int decidingShootoutGoalIndex = 0;
            string decidingShootoutGoalSide = "";
            if (IsShootout && ShootoutSeries > 0)
            {
                int homeGoals = 0;
                int awayGoals = 0;

                foreach (ShootoutShot shot in Shootout)
                {
                    if (shot.Player.Present && shot.Goalie.Present && shot.WasGoal)
                    {
                        _ = shot.Side == "Home" ? homeGoals++ : awayGoals++;
                    }
                }

                if (homeGoals > awayGoals)
                {
                    homeScore++;
                    decidingShootoutGoalIndex = Shootout.Where(x => x.Side == "Home" && x.WasGoal).ElementAt(awayGoals).Number;
                    decidingShootoutGoalSide = "Home";
                }
                if (homeGoals < awayGoals)
                {
                    awayScore++;
                    decidingShootoutGoalIndex = Shootout.Where(x => x.Side == "Away" && x.WasGoal).ElementAt(homeGoals).Number;
                    decidingShootoutGoalSide = "Away";
                }
            }

            //validation
            if (bracketIndex != -1 && qualificationID != SportsData.NOID && ExceedsFirstToWin(homeScore, awayScore))
            {
                _ = MessageBox.Show("Match can not be added. The winner of this match already has the required number of wins to win the series.", "Series match number of violation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (HomeTeam == null)
            {
                _ = MessageBox.Show("Please select the home team.", "Home team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (AwayTeam == null)
            {
                _ = MessageBox.Show("Please select the away team.", "Away team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (IsShootout && ShootoutSeries == 0)
            {
                _ = MessageBox.Show("Shootout must have at least 1 serie.", "No shootout serie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (IsShootout && ShootoutSeries > 0)
            {
                for (int i = 0; i < Shootout.Count - 2; i++)
                {
                    if (!Shootout[i].Player.Present || !Shootout[i].Goalie.Present)
                    {
                        _ = MessageBox.Show("Shootout atempt missing.", "Incomplete shootout", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if ((!Shootout[^2].Player.Present || !Shootout[^2].Goalie.Present) && (!Shootout[^1].Player.Present || !Shootout[^1].Goalie.Present))
                {
                    _ = MessageBox.Show("Shootout atempt missing.", "Incomplete shootout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            //saving
            int matchID = SportsData.NOID;

            if (!edit)
            {
                bracketFirstTeamID = HomeTeam.ID;
            }

            MySqlConnection connection = new(SportsData.ConnectionStringSport);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string matchInsertionQuerry = "INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, periods, " +
                                        "period_duration, home_competitor, away_competitor, home_score, away_score, datetime, overtime, shootout, forfeit, bracket_first_team) " +
                                        "VALUES (" + seasonID + ", " + 1 + ", " + qualificationID + ", " + bracketIndex +
                                        ", " + round + ", " + serieMatchNumber + ", " + PeriodCount + ", " + PeriodDuration + ", " + HomeTeam.ID +
                                        ", " + AwayTeam.ID + ", " + homeScore + ", " + awayScore + ", '" + MatchDateTime.ToString("yyyy-MM-dd H:mm:ss") + "', " + Convert.ToInt32(Overtime) +
                                        ", " + Convert.ToInt32(IsShootout) + ", " + Convert.ToInt32(Forfeit) + ", " + bracketFirstTeamID + ")";

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                //match insertion
                cmd = new MySqlCommand(matchInsertionQuerry, connection)
                {
                    Transaction = transaction
                };
                _ = cmd.ExecuteNonQuery();
                matchID = (int)cmd.LastInsertedId;

                //time-outs insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(TimeOut)))
                {
                    int teamID = t.Stat.Side == "Home" ? HomeTeam.ID : AwayTeam.ID;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.ID : HomeTeam.ID;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).ID;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;

                    string querry = "INSERT INTO time_outs(match_id, period, period_seconds, order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score) " +
                                              "VALUES (" + matchID + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //period scores insertion
                foreach (Period p in Periods)
                {
                    int homePeriodScore = p.Goals.Count(x => (x.Side == "Home" && !x.OwnGoal) || (x.Side == "Away" && x.OwnGoal));
                    int awayPeriodScore = p.Goals.Count(x => (x.Side == "Away" && !x.OwnGoal) || (x.Side == "Home" && x.OwnGoal));

                    string querry = "INSERT INTO period_score(match_id, period, home_team_id, away_team_id, home_score, away_score) " +
                                              "VALUES (" + matchID + ", " + p.Number + ", " + HomeTeam.ID + ", " + AwayTeam.ID + ", " + homePeriodScore + ", " + awayPeriodScore + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //game states insertion
                foreach (State s in timeSpans)
                {
                    int strengthID = Strengths.First(x => x.Situation == s.StrengthToString()).ID;

                    string querry = "INSERT INTO game_state(match_id, period, home_team_id, away_team_id, start_period_seconds, end_period_seconds, strength_id, home_score, away_score) " +
                                              "VALUES (" + matchID + ", " + s.period.Number + ", " + HomeTeam.ID + ", " + AwayTeam.ID + ", " + s.startTime + ", " + s.endTime + ", " + strengthID + ", " + s.homeGoals + ", " + s.awayGoals + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //shutouts insertion
                foreach (Shutout so in shutouts)
                {
                    int goalieID = so.goalie == null ? SportsData.NOID : so.goalie.id;
                    int teamID = so.side == "Home" ? HomeTeam.ID : AwayTeam.ID;
                    int opponentTeamID = so.side == "Home" ? AwayTeam.ID : HomeTeam.ID;

                    string querry = "INSERT INTO shutouts(match_id, goalie_id, team_id, opponent_team_id) " +
                                              "VALUES (" + matchID + ", " + goalieID + ", " + teamID + ", " + opponentTeamID + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //penalties insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(Penalty)))
                {
                    Penalty p = (Penalty)t.Stat;

                    int teamID = t.Stat.Side == "Home" ? HomeTeam.ID : AwayTeam.ID;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.ID : HomeTeam.ID;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).ID;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;

                    string querry = "INSERT INTO penalties(match_id, player_id, period, period_seconds, order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score, start_period_seconds, end_period_seconds, duration, penalty_reason_id, penalty_type_id, punished) " +
                                              "VALUES (" + matchID + ", " + p.Player.id + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ", " + p.startTime + ", " + p.endTime + ", " + p.duration + ", " + p.PenaltyReason.Code + ", '" + p.PenaltyType.Code + "', " + Convert.ToInt32(p.punished) + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //goals insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(Goal)))
                {
                    Goal g = (Goal)t.Stat;

                    int teamID = t.Stat.Side == "Home" ? HomeTeam.ID : AwayTeam.ID;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.ID : HomeTeam.ID;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).ID;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;
                    int gwg_glog = 0;
                    if (g.gameWinningGoal)
                    {
                        gwg_glog = 1;
                    }
                    else if (g.gameLosingOwnGoal)
                    {
                        gwg_glog = -1;
                    }
                    int goalieID = g.goalie == null ? SportsData.NOID : g.goalie.id;
                    int assistID = g.Assist == null ? SportsData.NOID : g.Assist.id;

                    string querry = "INSERT INTO goals(match_id, player_id, goalie_id, assist_player_id, period, period_seconds, order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score, gwg_glog, own_goal, empty_net, penalty_shot, delayed_penalty) " +
                                              "VALUES (" + matchID + ", " + g.Scorer.id + ", " + goalieID + ", " + assistID + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ", " + gwg_glog + ", " + Convert.ToInt32(g.OwnGoal) + ", " + Convert.ToInt32(g.emptyNet) + ", " + Convert.ToInt32(g.PenaltyShot) + ", " + Convert.ToInt32(g.DelayedPenalty) + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //penalty shots insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(PenaltyShot)))
                {
                    PenaltyShot ps = (PenaltyShot)t.Stat;

                    int teamID = t.Stat.Side == "Home" ? HomeTeam.ID : AwayTeam.ID;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.ID : HomeTeam.ID;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).ID;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;
                    int goalieID = ps.goalie == null ? SportsData.NOID : ps.goalie.id;

                    string querry = "INSERT INTO penalty_shots(match_id, player_id, goalie_id, period, period_seconds, order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score, was_goal) " +
                                              "VALUES (" + matchID + ", " + ps.Player.id + ", " + goalieID + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ", " + Convert.ToInt32(ps.WasGoal) + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //shootout shots insertion
                foreach (ShootoutShot ss in Shootout.Where(x => x.Player.Present && x.Goalie.Present))
                {
                    int teamID = ss.Side == "Home" ? HomeTeam.ID : AwayTeam.ID;
                    int opponentTeamID = ss.Side == "Home" ? AwayTeam.ID : HomeTeam.ID;
                    int decidingGoal = decidingShootoutGoalIndex == ss.Number && ss.Side == decidingShootoutGoalSide ? 1 : 0;

                    string querry = "INSERT INTO shootout_shots(match_id, player_id, goalie_id, number, team_id, opponent_team_id, was_goal, deciding_goal) " +
                                              "VALUES (" + matchID + ", " + ss.Player.id + ", " + ss.Goalie.id + ", " + ss.Number + ", " + teamID + ", " + opponentTeamID + ", " + Convert.ToInt32(ss.WasGoal) + ", " + decidingGoal + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //shifts insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(GoalieChange)))
                {
                    GoalieChange gch = (GoalieChange)t.Stat;
                    if (!gch.Entered) { continue; }

                    int teamID = t.Stat.Side == "Home" ? HomeTeam.ID : AwayTeam.ID;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.ID : HomeTeam.ID;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).ID;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;
                    int duration = gch.pairEvent.Stat.TimeInSeconds - gch.TimeInSeconds;

                    string querry = "INSERT INTO shifts(match_id, player_id, period, period_seconds, order_in_match, end_order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score, end_period_seconds, duration) " +
                                              "VALUES (" + matchID + ", " + gch.Player.id + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + gch.pairEvent.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ", " + gch.pairEvent.Stat.TimeInSeconds + ", " + duration + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //player matches insertion
                foreach (PlayerInRoster p in HomeRoster)
                {
                    int result = 0;
                    if (homeScore > awayScore)
                    {
                        if (Overtime || IsShootout) { result = 10; } else { result = 1; }
                    }
                    else if (homeScore < awayScore)
                    {
                        if (Overtime || IsShootout) { result = 20; } else { result = 2; }
                    }

                    string querry = "INSERT INTO player_matches(player_id, match_id, result, team_id, side) " +
                                              "VALUES (" + p.id + ", " + matchID + ", " + result + ", " + HomeTeam.ID + ", 'H')";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }
                foreach (PlayerInRoster p in AwayRoster)
                {
                    int result = 0;
                    if (homeScore > awayScore)
                    {
                        if (Overtime || IsShootout) { result = 20; } else { result = 2; }
                    }
                    else if (homeScore < awayScore)
                    {
                        if (Overtime || IsShootout) { result = 10; } else { result = 1; }
                    }

                    string querry = "INSERT INTO player_matches(player_id, match_id, result, team_id, side) " +
                                              "VALUES (" + p.id + ", " + matchID + ", " + result + ", " + AwayTeam.ID + ", 'A')";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                //goalie matches insertion
                List<GoalieInMatch> goalies = new();
                for (int i = 0; i < Events.Count; i++)
                {
                    if (Events[i].Stat.GetType() == typeof(GoalieChange) && ((GoalieChange)Events[i].Stat).Entered)
                    {
                        if (!goalies.Any(x => x.id == ((GoalieChange)Events[i].Stat).Player.id))
                        {
                            if (goalies.Count == 0)
                            {
                                goalies.Add(new GoalieInMatch { id = ((GoalieChange)Events[i].Stat).Player.id, side = Events[i].Stat.Side, started = 1 });
                            }
                            else
                            {
                                goalies.Add(new GoalieInMatch { id = ((GoalieChange)Events[i].Stat).Player.id, side = Events[i].Stat.Side, started = 0 });

                                for (int j = 0; j < goalies.Count - 2; j++)
                                {
                                    goalies[j].relieved = 1;
                                }
                            }
                        }
                    }
                }
                if (goalies.Count > 1)
                {
                    for (int j = 0; j < goalies.Count - 2; j++)
                    {
                        goalies[j].relieved = 1;
                    }
                }

                for (int i = 0; i < goalies.Count; i++)
                {
                    string side = goalies[i].side == "Home" ? "H" : "A";
                    int teamID = goalies[i].side == "Home" ? HomeTeam.ID : AwayTeam.ID;

                    int result = 0;
                    if (side == "H")
                    {
                        if (homeScore > awayScore)
                        {
                            if (Overtime || IsShootout) { result = 10; } else { result = 1; }
                        }
                        else if (homeScore < awayScore)
                        {
                            if (Overtime || IsShootout) { result = 20; } else { result = 2; }
                        }
                    }
                    else
                    {
                        if (homeScore > awayScore)
                        {
                            if (Overtime || IsShootout) { result = 20; } else { result = 2; }
                        }
                        else if (homeScore < awayScore)
                        {
                            if (Overtime || IsShootout) { result = 10; } else { result = 1; }
                        }
                    }

                    string querry = "INSERT INTO goalie_matches(player_id, match_id, result, team_id, side, started, relieved) " +
                                              "VALUES (" + goalies[i].id + ", " + matchID + ", " + result + ", " + teamID + ", '" + side + "', " + goalies[i].started + ", " + goalies[i].relieved + ")";
                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();
                }

                if (edit)
                {
                    //delete match from DB
                    string querry = "DELETE FROM matches WHERE id = " + match.ID;

                    cmd = new MySqlCommand(querry, connection)
                    {
                        Transaction = transaction
                    };
                    _ = cmd.ExecuteNonQuery();

                    //delete all player/goalie match enlistments and all stats
                    List<string> databases = new() { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                    foreach (string db in databases)
                    {
                        querry = "DELETE FROM " + db + " WHERE match_id = " + match.ID;
                        cmd = new MySqlCommand(querry, connection)
                        {
                            Transaction = transaction
                        };
                        _ = cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                connection.Close();

                ScheduleViewModel scheduleViewModel = new(ns);
                if (edit)
                {
                    new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, new Match { ID = matchID }, scheduleToReturnVM))).Execute(null);
                }
                else
                {
                    switch (scheduleToReturnVM)
                    {
                        case GroupsScheduleViewModel:
                            scheduleViewModel.GroupsSet = true;
                            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, scheduleViewModel)).Execute(null);
                            break;
                        case QualificationScheduleViewModel:
                            scheduleViewModel.QualificationSet = true;
                            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, scheduleViewModel)).Execute(null);
                            break;
                        case PlayOffScheduleViewModel:
                            scheduleViewModel.PlayOffSet = true;
                            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, scheduleViewModel)).Execute(null);
                            break;
                        default:
                            break;
                    }
                }
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

        //ignore
        #region Testing
        private ICommand generateMatchCommand;
        /// <summary>
        /// When executed, it generates a random match. Only for testing purposes.
        /// </summary>
        public ICommand GenerateMatchCommand
        {
            get
            {
                if (generateMatchCommand == null)
                {
                    generateMatchCommand = new RelayCommand(param => GenerateMatch());
                }
                return generateMatchCommand;
            }
        }

        /// <summary>
        /// Generates a random match. Only for testing purposes.
        /// </summary>
        public void GenerateMatch()
        {
            Random r = new();

            //date and time
            DateTime start = new(2020, 10, 1);
            int range = (new DateTime(2021, 3, 15) - start).Days;
            MatchDate = start.AddDays(r.Next(range));
            MatchTimeHours = r.Next(8, 14);
            int randomMinute = r.Next(0, 3);
            switch (randomMinute)
            {
                case 0:
                    MatchTimeMinutes = 0;
                    break;
                case 1:
                    MatchTimeMinutes = 15;
                    break;
                case 2:
                    MatchTimeMinutes = 30;
                    break;
                case 3:
                    MatchTimeMinutes = 45;
                    break;
                default:
                    break;
            }

            //select goalies
            int homeGoalieID = HomePlayers.Where(x => x.Position == "goaltender").OrderBy(x => r.Next()).First().id;
            int awayGoalieID = AwayPlayers.Where(x => x.Position == "goaltender").OrderBy(x => r.Next()).First().id;
            HomePlayers.First(x => x.id == homeGoalieID).Present = true;
            AwayPlayers.First(x => x.id == awayGoalieID).Present = true;

            //player rosters
            foreach (PlayerInRoster p in HomePlayers.OrderBy(x => r.Next()).Take(r.Next(HomePlayers.Count / 2, HomePlayers.Count)))
            {
                p.Present = true;
            }
            foreach (PlayerInRoster p in AwayPlayers.OrderBy(x => r.Next()).Take(r.Next(AwayPlayers.Count / 2, AwayPlayers.Count)))
            {
                p.Present = true;
            }

            //periods
            PeriodCount = 3;
            PeriodDuration = 10;

            List<Goal> allGoals = new();

            foreach (Period p in Periods)
            {
                bool homeTimeOut = false;
                bool awayTimeOut = false;

                //randomly simulate each second in period
                for (int i = 0; i < PeriodDuration * 60; i++)
                {
                    //average 13 goals, 10.75 assists, 3.5 penalty minutes per match
                    //average 16 events per match
                    //average 5 events per period
                    //on average event occurs each 2 minutes
                    //on average event occurs each 120th second
                    //probability was decreased 2 times
                    if (r.Next(1, 120 * 2) == 100)
                    {
                        //side probability 50-50
                        string side = r.Next(1, 10) % 2 == 0 ? "Home" : "Away";

                        //75% goals, 15% penalties, 5% saved penalty shots, 5% timeout
                        int eventType = r.Next(1, 100);
                        if (eventType <= 75)
                        {
                            //goal
                            //time, side, player, assists, penalty_shot, own_goal, delayed_penalty
                            p.Goals.Add(new Goal
                            {
                                Minute = i / 60,
                                Second = i % 60,
                                Side = side,
                                Scorer = side == "Home" ? HomeRoster.Where(x => x.id != homeGoalieID).OrderBy(x => r.Next()).First() : AwayRoster.Where(x => x.id != awayGoalieID).OrderBy(x => r.Next()).First(),
                                DelayedPenalty = r.Next(1, 50) == 1
                            });
                            //80% with assist
                            if (r.Next(1, 100) <= 80)
                            {
                                int scorerID = p.Goals.Last().Scorer.id;
                                p.Goals.Last().Assist = side == "Home" ? HomeRoster.Where(x => x.id != scorerID).OrderBy(x => r.Next()).First() : AwayRoster.Where(x => x.id != scorerID).OrderBy(x => r.Next()).First();
                            }
                            else
                            {
                                p.Goals.Last().Assist = new PlayerInRoster { id = SportsData.NOID };
                            }
                            //if no assist, penalty shot = 10%, own goal = 3%
                            if (p.Goals.Last().Assist.id == SportsData.NOID)
                            {
                                int probability = r.Next(1, 100);
                                if (probability <= 50)
                                {
                                    p.Goals.Last().PenaltyShot = true;
                                }
                                else if (probability <= 55)
                                {
                                    p.Goals.Last().OwnGoal = true;
                                }
                            }

                            allGoals.Add(p.Goals.Last());
                        }
                        else if (eventType <= 90)
                        {
                            //penalty
                            //time, side, player, reason, type
                            p.Penalties.Add(new Penalty
                            {
                                Minute = i / 60,
                                Second = i % 60,
                                Side = side,
                                Player = side == "Home" ? HomeRoster.OrderBy(x => r.Next()).First() : AwayRoster.OrderBy(x => r.Next()).First(),
                                PenaltyReason = PenaltyReasons.OrderBy(x => r.Next()).First()
                            });
                            int penaltyType = r.Next(1, 100);
                            if (penaltyType <= 80)
                            {
                                p.Penalties.Last().PenaltyType = PenaltyTypes.First(x => x.Minutes == 2);
                            }
                            else if (penaltyType <= 90)
                            {
                                p.Penalties.Last().PenaltyType = PenaltyTypes.First(x => x.Minutes == 5);
                            }
                            else if (penaltyType <= 95)
                            {
                                p.Penalties.Last().PenaltyType = PenaltyTypes.First(x => x.Minutes == 10);
                            }
                            else
                            {
                                p.Penalties.Last().PenaltyType = PenaltyTypes.OrderBy(x => r.Next()).First(x => x.Minutes == 20);
                            }
                        }
                        else if (eventType <= 95)
                        {
                            //saved penalty shots
                            //time, side, player, no_goal
                            p.PenaltyShots.Add(new PenaltyShot
                            {
                                Minute = i / 60,
                                Second = i % 60,
                                Side = side,
                                Player = side == "Home" ? HomeRoster.Where(x => x.id != homeGoalieID).OrderBy(x => r.Next()).First() : AwayRoster.Where(x => x.id != awayGoalieID).OrderBy(x => r.Next()).First(),
                                WasGoal = false
                            });
                        }
                        else
                        {
                            //timeout
                            //time, side
                            if ((side == "Home" && homeTimeOut) || (side == "Away" && awayTimeOut))
                            {
                                continue;
                            }
                            else
                            {
                                p.TimeOuts.Add(new TimeOut
                                {
                                    Minute = i / 60,
                                    Second = i % 60,
                                    Side = side,
                                });

                                if (side == "Home") { homeTimeOut = true; } else { awayTimeOut = true; }
                            }
                        }
                    }
                }

                //scored penalty shots
                foreach (Goal g in p.Goals)
                {
                    if (g.PenaltyShot)
                    {
                        p.PenaltyShots.Add(new PenaltyShot
                        {
                            Minute = g.Minute,
                            Second = g.Second,
                            Side = g.Side,
                            Player = g.Scorer,
                            WasGoal = true
                        });
                    }
                }

                p.Goals.Sort();
                p.Penalties.Sort();
                p.PenaltyShots.Sort();
                p.TimeOuts.Sort();

                p.GoalieShifts.Add(new GoalieShift
                {
                    Side = "Home",
                    Player = HomeRoster.First(x => x.id == homeGoalieID),
                    StartMinute = 0,
                    StartSecond = 0,
                    EndMinute = 10,
                    EndSecond = 0
                });

                p.GoalieShifts.Add(new GoalieShift
                {
                    Side = "Away",
                    Player = AwayRoster.First(x => x.id == awayGoalieID),
                    StartMinute = 0,
                    StartSecond = 0,
                    EndMinute = 10,
                    EndSecond = 0
                });
            }

            int homeScore = allGoals.Count(x => x.Side == "Home" && !x.OwnGoal) + allGoals.Count(x => x.Side == "Away" && x.OwnGoal);
            int awayScore = allGoals.Count(x => x.Side == "Away" && !x.OwnGoal) + allGoals.Count(x => x.Side == "Home" && x.OwnGoal);
            //if it is a tie
            if (homeScore == awayScore)
            {
                IsShootout = true;

                //simulate shootout
                bool end = false;
                string firstSide = "Home";
                string secondSide = "Away";
                if (r.Next() % 2 == 0)
                {
                    firstSide = "Away";
                    secondSide = "Home";
                }
                int firstGoals = 0;
                int secondGoals = 0;
                int number = 1;

                while (!end)
                {
                    ShootoutShot firstShot = new(number, firstSide,
                        playerRoster: firstSide == "Home" ? HomeRoster : AwayRoster,
                        goalieRoster: firstSide == "Home" ? AwayRoster : HomeRoster,
                        player: firstSide == "Home" ? HomeRoster.Where(x => x.id != homeGoalieID).OrderBy(x => r.Next()).First() : AwayRoster.Where(x => x.id != awayGoalieID).OrderBy(x => r.Next()).First(),
                        goalie: firstSide == "Home" ? AwayRoster.First(x => x.id == awayGoalieID) : HomeRoster.First(x => x.id == homeGoalieID));
                    ShootoutShot secondShot = new(number, secondSide,
                        playerRoster: secondSide == "Home" ? HomeRoster : AwayRoster,
                        goalieRoster: secondSide == "Home" ? AwayRoster : HomeRoster,
                        player: secondSide == "Home" ? HomeRoster.Where(x => x.id != homeGoalieID).OrderBy(x => r.Next()).First() : AwayRoster.Where(x => x.id != awayGoalieID).OrderBy(x => r.Next()).First(),
                        goalie: secondSide == "Home" ? AwayRoster.First(x => x.id == awayGoalieID) : HomeRoster.First(x => x.id == homeGoalieID));

                    if (Shootout.Count < 2)
                    {
                        //simulate both shots
                        firstShot.WasGoal = r.Next() % 2 == 0;
                        if (firstShot.WasGoal) { firstGoals++; }
                        Shootout.Add(firstShot);
                        secondShot.WasGoal = r.Next() % 2 == 0;
                        if (secondShot.WasGoal) { secondGoals++; }
                        Shootout.Add(secondShot);
                    }
                    else if (Shootout.Count < 4)
                    {
                        //simulate both shots
                        firstShot.WasGoal = r.Next() % 2 == 0;
                        if (firstShot.WasGoal) { firstGoals++; }
                        Shootout.Add(firstShot);
                        secondShot.WasGoal = r.Next() % 2 == 0;
                        if (secondShot.WasGoal) { secondGoals++; }
                        Shootout.Add(secondShot);

                        //check for win (if goal difference is 2 -> end)
                        if (Math.Abs(firstGoals - secondGoals) == 2) { end = true; }
                    }
                    else if (Shootout.Count < 6)
                    {
                        int firstGoalsBefore = firstGoals;

                        //simulate first shot
                        firstShot.WasGoal = r.Next() % 2 == 0;
                        if (firstShot.WasGoal) { firstGoals++; }
                        Shootout.Add(firstShot);

                        //check for win
                        //if first is losing by 1 goal and missed the shot
                        if (firstGoalsBefore == secondGoals - 1 && !firstShot.WasGoal)
                        { end = true; continue; }
                        //if first is winning by 1 goal and shots a goal
                        if (firstGoalsBefore == secondGoals + 1 && firstShot.WasGoal)
                        { end = true; continue; }

                        //simulate second shot
                        secondShot.WasGoal = r.Next() % 2 == 0;
                        if (secondShot.WasGoal) { secondGoals++; }
                        Shootout.Add(secondShot);

                        //check for win
                        if (firstGoals != secondGoals) { end = true; }
                    }
                    else
                    {
                        //simulate both shots
                        firstShot.WasGoal = r.Next() % 2 == 0;
                        if (firstShot.WasGoal) { firstGoals++; }
                        Shootout.Add(firstShot);
                        secondShot.WasGoal = r.Next() % 2 == 0;
                        if (secondShot.WasGoal) { secondGoals++; }
                        Shootout.Add(secondShot);

                        //check for win
                        if (firstGoals != secondGoals) { end = true; }
                    }
                    number++;
                }
                shootoutSeries = (Shootout.Count + 1) / 2;
            }
        }
        #endregion

        #endregion
    }
}