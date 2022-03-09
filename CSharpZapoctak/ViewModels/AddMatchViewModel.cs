using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    #region Classes
    class PlayerInRoster : ViewModelBase
    {
        public int id;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string position;
        public string Position
        {
            get { return position; }
            set
            {
                position = value;
                OnPropertyChanged();
            }
        }

        private int? number = null;
        public int? Number
        {
            get { return number; }
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        private bool present;
        public bool Present
        {
            get { return present; }
            set
            {
                present = value;

                ObservableCollection<PlayerInRoster> n = new ObservableCollection<PlayerInRoster>();
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

                n = new ObservableCollection<PlayerInRoster>();
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

        public AddMatchViewModel vm;
    }

    class GoalieInMatch
    {
        public int id;
        public string side;
        public int started;
        public int relieved;
    }

    class Period : ViewModelBase
    {
        private static readonly object _lock = new object();

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
            Goals = new ObservableCollection<Goal>();
            BindingOperations.EnableCollectionSynchronization(Goals, _lock);
            NewGoal = new Goal();
            GoalsRoster = new ObservableCollection<PlayerInRoster>();
            BindingOperations.EnableCollectionSynchronization(GoalsRoster, _lock);
            Penalties = new ObservableCollection<Penalty>();
            BindingOperations.EnableCollectionSynchronization(Penalties, _lock);
            NewPenalty = new Penalty();
            PenaltyRoster = new ObservableCollection<PlayerInRoster>();
            BindingOperations.EnableCollectionSynchronization(PenaltyRoster, _lock);
            PenaltyShots = new ObservableCollection<PenaltyShot>();
            BindingOperations.EnableCollectionSynchronization(PenaltyShots, _lock);
            NewPenaltyShot = new PenaltyShot();
            PenaltyShotRoster = new ObservableCollection<PlayerInRoster>();
            BindingOperations.EnableCollectionSynchronization(PenaltyShotRoster, _lock);
            GoalieShifts = new ObservableCollection<GoalieShift>();
            BindingOperations.EnableCollectionSynchronization(GoalieShifts, _lock);
            NewGoalieShift = new GoalieShift();
            GoalieShiftRoster = new ObservableCollection<PlayerInRoster>();
            BindingOperations.EnableCollectionSynchronization(GoalieShiftRoster, _lock);
            TimeOuts = new ObservableCollection<TimeOut>();
            BindingOperations.EnableCollectionSynchronization(TimeOuts, _lock);
            NewTimeOut = new TimeOut();
        }

        public AddMatchViewModel vm;

        public readonly bool overtime;

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
        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        public int duration;

        private ObservableCollection<PlayerInRoster> homeRoster;
        public ObservableCollection<PlayerInRoster> HomeRoster
        {
            get { return homeRoster; }
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
        public ObservableCollection<PlayerInRoster> AwayRoster
        {
            get { return awayRoster; }
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
        public ObservableCollection<Goal> Goals
        {
            get { return goals; }
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private Goal newGoal;
        public Goal NewGoal
        {
            get { return newGoal; }
            set
            {
                newGoal = value;
                OnPropertyChanged();
            }
        }

        private string goalSide;
        public string GoalSide
        {
            get { return goalSide; }
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
        public ObservableCollection<PlayerInRoster> GoalsRoster
        {
            get { return goalsRoster; }
            set
            {
                goalsRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addGoalCommand;
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

        private void AddGoal()
        {
            if (string.IsNullOrWhiteSpace(NewGoal.Side))
            {
                MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.Scorer == null)
            {
                MessageBox.Show("Please select scorer.", "Scorer not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.Scorer == NewGoal.Assist)
            {
                MessageBox.Show("Goal and assist can not be made by the same player.", "Goal and assist error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if ((NewGoal.PenaltyShot && NewGoal.OwnGoal) || (NewGoal.PenaltyShot && NewGoal.DelayedPenalty))
            {
                MessageBox.Show("Own goal or delayed penalty goal can not be scored on penalty shot.", "Own goal or delayed penalty goal on penalty shot", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoal.TimeInSeconds >= duration * 60)
            {
                MessageBoxResult msgResult = MessageBox.Show("Time exceeds period duration.", "Time exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewGoal.PenaltyShot || NewGoal.OwnGoal)
            {
                NewGoal.Assist = new PlayerInRoster { id = -1 };
            }

            Goals.Add(NewGoal);
            NewGoal = new Goal();
            GoalSide = null;
            Goals.Sort();
        }
        #endregion

        #region Penalties
        private ObservableCollection<Penalty> penalties;
        public ObservableCollection<Penalty> Penalties
        {
            get { return penalties; }
            set
            {
                penalties = value;
                OnPropertyChanged();
            }
        }

        private Penalty newPenalty;
        public Penalty NewPenalty
        {
            get { return newPenalty; }
            set
            {
                newPenalty = value;
                OnPropertyChanged();
            }
        }

        private string penaltySide;
        public string PenaltySide
        {
            get { return penaltySide; }
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
        public ObservableCollection<PlayerInRoster> PenaltyRoster
        {
            get { return penaltyRoster; }
            set
            {
                penaltyRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addPenaltyCommand;
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

        private void AddPenalty()
        {
            if (string.IsNullOrWhiteSpace(NewPenalty.Side))
            {
                MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenalty.Player == null)
            {
                MessageBox.Show("Please select player.", "Player not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenalty.PenaltyReason == null || NewPenalty.PenaltyType == null)
            {
                MessageBox.Show("Please select penalty reason and type.", "Penalty not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenalty.TimeInSeconds >= duration * 60)
            {
                MessageBoxResult msgResult = MessageBox.Show("Time exceeds period duration.", "Time exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        public ObservableCollection<PenaltyShot> PenaltyShots
        {
            get { return penaltyShots; }
            set
            {
                penaltyShots = value;
                OnPropertyChanged();
            }
        }

        private PenaltyShot newPenaltyShot;
        public PenaltyShot NewPenaltyShot
        {
            get { return newPenaltyShot; }
            set
            {
                newPenaltyShot = value;
                OnPropertyChanged();
            }
        }

        private string penaltyShotSide;
        public string PenaltyShotSide
        {
            get { return penaltyShotSide; }
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
        public ObservableCollection<PlayerInRoster> PenaltyShotRoster
        {
            get { return penaltyShotRoster; }
            set
            {
                penaltyShotRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addPenaltyShotCommand;
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

        private void AddPenaltyShot()
        {
            if (string.IsNullOrWhiteSpace(NewPenaltyShot.Side))
            {
                MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenaltyShot.Player == null)
            {
                MessageBox.Show("Please select player.", "Player not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewPenaltyShot.TimeInSeconds >= duration * 60)
            {
                MessageBoxResult msgResult = MessageBox.Show("Time exceeds period duration.", "Time exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        public ObservableCollection<GoalieShift> GoalieShifts
        {
            get { return goalieShifts; }
            set
            {
                goalieShifts = value;
                OnPropertyChanged();
            }
        }

        private GoalieShift newGoalieShift;
        public GoalieShift NewGoalieShift
        {
            get { return newGoalieShift; }
            set
            {
                newGoalieShift = value;
                OnPropertyChanged();
            }
        }

        private string goalieShiftSide;
        public string GoalieShiftSide
        {
            get { return goalieShiftSide; }
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
        public ObservableCollection<PlayerInRoster> GoalieShiftRoster
        {
            get { return goalieShiftRoster; }
            set
            {
                goalieShiftRoster = value;
                OnPropertyChanged();
            }
        }

        private ICommand addGoalieShiftCommand;
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

        private void AddGoalieShift()
        {
            if (string.IsNullOrWhiteSpace(NewGoalieShift.Side))
            {
                MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoalieShift.Player == null)
            {
                MessageBox.Show("Please select player.", "Player not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoalieShift.EndTimeInSeconds - NewGoalieShift.StartTimeInSeconds < 1)
            {
                MessageBox.Show("Goaltender shift must last at least 1 second.", "Shift is too short", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewGoalieShift.EndTimeInSeconds > duration * 60 || NewGoalieShift.StartTimeInSeconds > duration * 60)
            {
                MessageBoxResult msgResult = MessageBox.Show("Goaltender shift exceeds period duration.", "Shift exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        public ObservableCollection<TimeOut> TimeOuts
        {
            get { return timeOuts; }
            set
            {
                timeOuts = value;
                OnPropertyChanged();
            }
        }

        private TimeOut newTimeOut;
        public TimeOut NewTimeOut
        {
            get { return newTimeOut; }
            set
            {
                newTimeOut = value;
                OnPropertyChanged();
            }
        }

        private string timeOutSide;
        public string TimeOutSide
        {
            get { return timeOutSide; }
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

        private void AddTimeOut()
        {
            if (string.IsNullOrWhiteSpace(NewTimeOut.Side))
            {
                MessageBox.Show("Please select side.", "Side not selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NewTimeOut.TimeInSeconds >= duration * 60)
            {
                MessageBoxResult msgResult = MessageBox.Show("Time exceeds period duration.", "Time exceeds period duration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeOuts.Add(NewTimeOut);
            NewTimeOut = new TimeOut();
            TimeOutSide = null;
            TimeOuts.Sort();
        }
        #endregion
    }

    class Event : ViewModelBase, IComparable
    {
        public int index;

        private BasicStat stat;
        public BasicStat Stat
        {
            get { return stat; }
            set
            {
                stat = value;
                OnPropertyChanged();
            }
        }

        private Period period;
        public Period Period
        {
            get { return period; }
            set
            {
                period = value;
                OnPropertyChanged();
            }
        }

        public string Text
        {
            get
            {
                string p = "Period " + Period.Number + "\t\t";
                if (Period.overtime) { p = "Overtime\t\t"; }
                return p + Stat.Text;
            }
        }

        public override string ToString()
        {
            return Stat.Text;
        }

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

    class SwapEvent : ViewModelBase
    {
        public SwapEvent(Event inPenalty)
        {
            InPenaltyEvent = inPenalty;
            OutPenaltyEvent = null;
        }

        private Event inPenaltyEvent;
        public Event InPenaltyEvent
        {
            get { return inPenaltyEvent; }
            set
            {
                inPenaltyEvent = value;
                OnPropertyChanged();
            }
        }

        private Event outPenaltyEvent;
        public Event OutPenaltyEvent
        {
            get { return outPenaltyEvent; }
            set
            {
                outPenaltyEvent = value;
                OnPropertyChanged();
            }
        }

        private ICommand eventInCommand;
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

        private void EventIn()
        {
            if (InPenaltyEvent == null && OutPenaltyEvent != null)
            {
                InPenaltyEvent = OutPenaltyEvent;
                OutPenaltyEvent = null;
            }
        }

        private void EventOut()
        {
            if (InPenaltyEvent != null && OutPenaltyEvent == null)
            {
                OutPenaltyEvent = InPenaltyEvent;
                InPenaltyEvent = null;
            }
        }
    }

    class PenaltyEndCollision : ViewModelBase
    {
        public PenaltyEndCollision(State inPenalty, State outPenalty, Penalty penalty)
        {
            InPenalty = inPenalty;
            OutPenalty = outPenalty;
            this.penalty = penalty;
            SwapEvents = new ObservableCollection<SwapEvent>();
        }

        public Penalty penalty;

        private State inPenalty;
        public State InPenalty
        {
            get { return inPenalty; }
            set
            {
                inPenalty = value;
                OnPropertyChanged();
            }
        }

        private State outPenalty;
        public State OutPenalty
        {
            get { return outPenalty; }
            set
            {
                outPenalty = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SwapEvent> swapEvents;
        public ObservableCollection<SwapEvent> SwapEvents
        {
            get { return swapEvents; }
            set
            {
                swapEvents = value;
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            string from = (penalty.startTime / 60) + ":" + (penalty.startTime % 60).ToString("00");
            string to = (penalty.endTime / 60) + ":" + (penalty.endTime % 60).ToString("00");
            return "Penalty from " + from + " to " + to;
        }
    }

    class Stat : ViewModelBase { }

    class BasicStat : Stat, IComparable
    {
        private int minute;
        public int Minute
        {
            get { return minute; }
            set
            {
                minute = value;
                OnPropertyChanged();
            }
        }

        private int second;
        public int Second
        {
            get { return second; }
            set
            {
                second = value;
                OnPropertyChanged();
            }
        }

        public string Time
        {
            get { return Minute + ":" + Second.ToString("00"); }
        }

        public int TimeInSeconds
        {
            get { return (Minute * 60) + Second; }
        }

        private string side;
        public string Side
        {
            get { return side; }
            set
            {
                side = value;
                OnPropertyChanged();
            }
        }

        public virtual string Text
        {
            get
            {
                return Time + "\t\t" + Side;
            }
        }

        public int homeScore;
        public int awayScore;
        public string strength;

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

        private void Delete(Period period)
        {
            switch (this)
            {
                case Goal:
                    period.Goals.Remove((Goal)this);
                    break;
                case Penalty:
                    period.Penalties.Remove((Penalty)this);
                    break;
                case PenaltyShot:
                    period.PenaltyShots.Remove((PenaltyShot)this);
                    break;
                case TimeOut:
                    period.TimeOuts.Remove((TimeOut)this);
                    break;
                default:
                    break;
            }
        }
    }

    class PeriodEnd : BasicStat { }

    class Goal : BasicStat
    {
        public bool gameLosingOwnGoal = false;
        public bool gameWinningGoal = false;
        public PlayerInRoster goalie;

        private PlayerInRoster scorer = new PlayerInRoster();
        public PlayerInRoster Scorer
        {
            get { return scorer; }
            set
            {
                scorer = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster assist = new PlayerInRoster();
        public PlayerInRoster Assist
        {
            get { return assist; }
            set
            {
                assist = value;
                OnPropertyChanged();
            }
        }

        private bool penaltyShot;
        public bool PenaltyShot
        {
            get { return penaltyShot; }
            set
            {
                penaltyShot = value;
                OnPropertyChanged();
            }
        }

        private bool ownGoal;
        public bool OwnGoal
        {
            get { return ownGoal; }
            set
            {
                ownGoal = value;
                OnPropertyChanged();
            }
        }

        private bool delayedPenalty;
        public bool DelayedPenalty
        {
            get { return delayedPenalty; }
            set
            {
                delayedPenalty = value;
                OnPropertyChanged();
            }
        }

        public bool emptyNet = false;

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

    class Penalty : BasicStat
    {
        public int startTime;
        public int endTime;
        public int duration = 0;
        public bool punished = false;

        private PlayerInRoster player = new PlayerInRoster();
        public PlayerInRoster Player
        {
            get { return player; }
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private PenaltyReason penaltyReason;
        public PenaltyReason PenaltyReason
        {
            get { return penaltyReason; }
            set
            {
                penaltyReason = value;
                OnPropertyChanged();
            }
        }

        private PenaltyType penaltyType;
        public PenaltyType PenaltyType
        {
            get { return penaltyType; }
            set
            {
                penaltyType = value;
                OnPropertyChanged();
            }
        }

        public override string Text
        {
            get
            {
                return Time + "\t\t" + Side + "\t\t" + PenaltyType.Name + " for " + Player.Number;
            }
        }
    }

    class PenaltyShot : BasicStat
    {
        public PlayerInRoster goalie;

        private PlayerInRoster player = new PlayerInRoster();
        public PlayerInRoster Player
        {
            get { return player; }
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private bool wasGoal;
        public bool WasGoal
        {
            get { return wasGoal; }
            set
            {
                wasGoal = value;
                OnPropertyChanged();
            }
        }

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

    class GoalieShift : Stat, IComparable
    {
        private int startMinute;
        public int StartMinute
        {
            get { return startMinute; }
            set
            {
                startMinute = value;
                OnPropertyChanged();
            }
        }

        private int startSecond;
        public int StartSecond
        {
            get { return startSecond; }
            set
            {
                startSecond = value;
                OnPropertyChanged();
            }
        }

        public string StartTime
        {
            get { return StartMinute + ":" + StartSecond.ToString("00"); }
        }

        public int StartTimeInSeconds
        {
            get { return (StartMinute * 60) + StartSecond; }
        }

        private int endMinute;
        public int EndMinute
        {
            get { return endMinute; }
            set
            {
                endMinute = value;
                OnPropertyChanged();
            }
        }

        private int endSecond;
        public int EndSecond
        {
            get { return endSecond; }
            set
            {
                endSecond = value;
                OnPropertyChanged();
            }
        }

        public string EndTime
        {
            get { return EndMinute + ":" + EndSecond.ToString("00"); }
        }

        public int EndTimeInSeconds
        {
            get { return (EndMinute * 60) + EndSecond; }
        }

        private string side;
        public string Side
        {
            get { return side; }
            set
            {
                side = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster player = new PlayerInRoster();
        public PlayerInRoster Player
        {
            get { return player; }
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

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

    class GoalieChange : BasicStat
    {
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

        private PlayerInRoster player = new PlayerInRoster();
        public PlayerInRoster Player
        {
            get { return player; }
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private bool entered;
        public bool Entered
        {
            get { return entered; }
            set
            {
                entered = value;
                OnPropertyChanged();
            }
        }

        public Event pairEvent;

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

    class TimeOut : BasicStat
    {
        public override string Text
        {
            get
            {
                return Time + "\t\t" + Side + "\t\ttime-out";
            }
        }
    }

    class ShootoutShot : Stat
    {
        public ShootoutShot(int number, string side, ObservableCollection<PlayerInRoster> playerRoster, ObservableCollection<PlayerInRoster> goalieRoster)
        {
            Number = number;
            Side = side;
            PlayerRoster = playerRoster;
            GoalieRoster = goalieRoster;
        }

        public ShootoutShot(int number, string side, PlayerInRoster player, PlayerInRoster goalie)
        {
            Number = number;
            Side = side;
            Player = player;
            Goalie = goalie;
        }

        private int number;
        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                OnPropertyChanged();
            }
        }

        private string side;
        public string Side
        {
            get { return side; }
            set
            {
                side = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster player = new PlayerInRoster();
        public PlayerInRoster Player
        {
            get { return player; }
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> playerRoster;
        public ObservableCollection<PlayerInRoster> PlayerRoster
        {
            get { return playerRoster; }
            set
            {
                playerRoster = value;
                OnPropertyChanged();
            }
        }

        private PlayerInRoster goalie = new PlayerInRoster();
        public PlayerInRoster Goalie
        {
            get { return goalie; }
            set
            {
                goalie = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> goalieRoster;
        public ObservableCollection<PlayerInRoster> GoalieRoster
        {
            get { return goalieRoster; }
            set
            {
                goalieRoster = value;
                OnPropertyChanged();
            }
        }

        private bool wasGoal;
        public bool WasGoal
        {
            get { return wasGoal; }
            set
            {
                wasGoal = value;
                OnPropertyChanged();
            }
        }

        private ICommand resetCommand;
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

    class Shutout
    {
        public Shutout(PlayerInRoster goalie, string side)
        {
            this.goalie = goalie;
            this.side = side;
        }

        public PlayerInRoster goalie;

        public string side;
    }

    class State
    {
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

        public Period period;

        public int startTime;

        public int endTime;

        public int homeStrength;

        public int awayStrength;

        public bool homeGoalieIn;

        public bool awayGoalieIn;

        public int homeGoals;

        public int awayGoals;

        public string StrengthToString()
        {
            string s = homeStrength.ToString();
            if (!homeGoalieIn) { s += "g"; }
            s += " v " + awayStrength;
            if (!awayGoalieIn) { s += "g"; }
            return s;
        }

        public override string ToString()
        {
            return period.Name + "\t" + startTime + "\t" + endTime + "\t" + StrengthToString() + "\t" + homeGoals + ":" + awayGoals;
        }
    }
    #endregion

    class AddMatchViewModel : ViewModelBase
    {
        #region Properties

        #region DateTime
        private DateTime matchDate = DateTime.Today;
        public DateTime MatchDate
        {
            get { return matchDate; }
            set
            {
                matchDate = value;
                OnPropertyChanged();
            }
        }

        private int matchTimeHours;
        public int MatchTimeHours
        {
            get { return matchTimeHours; }
            set
            {
                matchTimeHours = value;
                OnPropertyChanged();
            }
        }

        private int matchTimeMinutes;
        public int MatchTimeMinutes
        {
            get { return matchTimeMinutes; }
            set
            {
                matchTimeMinutes = value;
                OnPropertyChanged();
            }
        }

        public DateTime MatchDateTime
        {
            get { return MatchDate + new TimeSpan(MatchTimeHours, MatchTimeMinutes, 0); }
        }
        #endregion

        #region Rosters
        private ObservableCollection<Team> availableTeamsHome;
        public ObservableCollection<Team> AvailableTeamsHome
        {
            get { return availableTeamsHome; }
            set
            {
                availableTeamsHome = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Team> availableTeamsAway;
        public ObservableCollection<Team> AvailableTeamsAway
        {
            get { return availableTeamsAway; }
            set
            {
                availableTeamsAway = value;
                OnPropertyChanged();
            }
        }

        private Team homeTeam;
        public Team HomeTeam
        {
            get { return homeTeam; }
            set
            {
                if (homeTeam != null)
                {
                    AvailableTeamsAway.Add(homeTeam);
                }
                homeTeam = value;
                AvailableTeamsAway.Remove(homeTeam);
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
        public Team AwayTeam
        {
            get { return awayTeam; }
            set
            {
                if (awayTeam != null)
                {
                    AvailableTeamsHome.Add(awayTeam);
                }
                awayTeam = value;
                AvailableTeamsHome.Remove(awayTeam);
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

        private ObservableCollection<PlayerInRoster> homePlayers = new ObservableCollection<PlayerInRoster>();
        public ObservableCollection<PlayerInRoster> HomePlayers
        {
            get { return homePlayers; }
            set
            {
                homePlayers = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> awayPlayers = new ObservableCollection<PlayerInRoster>();
        public ObservableCollection<PlayerInRoster> AwayPlayers
        {
            get { return awayPlayers; }
            set
            {
                awayPlayers = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> homeRoster = new ObservableCollection<PlayerInRoster>();
        public ObservableCollection<PlayerInRoster> HomeRoster
        {
            get { return homeRoster; }
            set
            {
                homeRoster = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInRoster> awayRoster = new ObservableCollection<PlayerInRoster>();
        public ObservableCollection<PlayerInRoster> AwayRoster
        {
            get { return awayRoster; }
            set
            {
                awayRoster = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Match info
        int qualificationID = -1;
        int bracketIndex = -1;
        int round = -1;
        int serieMatchNumber = -1;
        int bracketFirstTeam = -1;
        int HomeScore;
        int AwayScore;

        private bool played;
        public bool Played
        {
            get { return played; }
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
        public bool Forfeit
        {
            get { return forfeit; }
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
        public string ForfeitWinnerSide
        {
            get { return forfeitWinnerSide; }
            set
            {
                forfeitWinnerSide = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Periods
        private int periodCount;
        public int PeriodCount
        {
            get { return periodCount; }
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
        public int PeriodDuration
        {
            get { return periodDuration; }
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
        public bool Overtime
        {
            get { return overtime; }
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
        public bool IsShootout
        {
            get { return isShootout; }
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
        public int ShootoutSeries
        {
            get { return shootoutSeries; }
            set
            {
                if (value == (Shootout.Count / 2) + 1)
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
        public ObservableCollection<Period> Periods
        {
            get { return periods; }
            set
            {
                periods = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ShootoutShot> shootout;
        public ObservableCollection<ShootoutShot> Shootout
        {
            get { return shootout; }
            set
            {
                shootout = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Visibilities
        public Visibility loadingVisibility = Visibility.Collapsed;
        public Visibility LoadingVisibility
        {
            get { return loadingVisibility; }
            set
            {
                loadingVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility pageVisibility = Visibility.Visible;
        public Visibility PageVisibility
        {
            get { return pageVisibility; }
            set
            {
                pageVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility notPlayedSaveButtonVisibility = Visibility.Visible;
        public Visibility NotPlayedSaveButtonVisibility
        {
            get { return notPlayedSaveButtonVisibility; }
            set
            {
                notPlayedSaveButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility formsVisibility = Visibility.Collapsed;
        public Visibility FormsVisibility
        {
            get { return formsVisibility; }
            set
            {
                formsVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility forfeitVisibility = Visibility.Collapsed;
        public Visibility ForfeitVisibility
        {
            get { return forfeitVisibility; }
            set
            {
                forfeitVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility forfeitSideVisibility = Visibility.Collapsed;
        public Visibility ForfeitSideVisibility
        {
            get { return forfeitSideVisibility; }
            set
            {
                forfeitSideVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility shootoutVisibility = Visibility.Collapsed;
        public Visibility ShootoutVisibility
        {
            get { return shootoutVisibility; }
            set
            {
                shootoutVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility checkButtonVisibility = Visibility.Collapsed;
        public Visibility CheckButtonVisibility
        {
            get { return checkButtonVisibility; }
            set
            {
                checkButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility saveButtonVisibility = Visibility.Collapsed;
        public Visibility SaveButtonVisibility
        {
            get { return saveButtonVisibility; }
            set
            {
                saveButtonVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Data
        public NavigationStore ns;
        public ViewModelBase scheduleToReturnVM;
        public int seasonID;
        public Match match;
        public bool edit;

        private ObservableCollection<string> sides;
        public ObservableCollection<string> Sides
        {
            get { return sides; }
            set
            {
                sides = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Strength> Strengths { get; private set; }

        public ObservableCollection<PenaltyReason> PenaltyReasons { get; private set; }

        public ObservableCollection<PenaltyType> PenaltyTypes { get; private set; }
        #endregion

        #region Commands
        private ICommand processCommand;
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

        private ICommand loadGamesheetCommand;
        public ICommand LoadGamesheetCommand
        {
            get
            {
                if (loadGamesheetCommand == null)
                {
                    loadGamesheetCommand = new RelayCommand(param => Task.Run(() => LoadGamesheet()));
                }
                return loadGamesheetCommand;
            }
        }

        #endregion

        #region MatchEvents
        private ObservableCollection<Event> events;
        public ObservableCollection<Event> Events
        {
            get { return events; }
            set
            {
                events = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Event> conflictEvents;
        public ObservableCollection<Event> ConflictEvents
        {
            get { return conflictEvents; }
            set
            {
                conflictEvents = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<PenaltyEndCollision> penaltyEndCollisions;
        public ObservableCollection<PenaltyEndCollision> PenaltyEndCollisions
        {
            get { return penaltyEndCollisions; }
            set
            {
                penaltyEndCollisions = value;
                OnPropertyChanged();
            }
        }

        List<State> timeSpans;
        #endregion

        #endregion

        #region Constructors
        private static readonly object _lock = new object();

        //ADD for group
        public AddMatchViewModel(NavigationStore navigationStore, int round)
        {
            ns = navigationStore;
            seasonID = SportsData.season.id;
            scheduleToReturnVM = new GroupsScheduleViewModel(ns);
            this.round = round;
            Periods = new ObservableCollection<Period>();
            BindingOperations.EnableCollectionSynchronization(Periods, _lock);
            Shootout = new ObservableCollection<ShootoutShot>();
            BindingOperations.EnableCollectionSynchronization(Shootout, _lock);
            LoadTeams();
            LoadSides();
            LoadStrengths();
            PenaltyReasons = SportsData.LoadPenaltyReasons();
            PenaltyTypes = SportsData.LoadPenaltyTypes();
        }

        //ADD for bracket
        public AddMatchViewModel(NavigationStore navigationStore, ViewModelBase scheduleToReturnVM, int qualificationID, int bracketIndex, int round, int serieMatchNumber, Team first, Team second)
        {
            ns = navigationStore;
            seasonID = SportsData.season.id;
            this.scheduleToReturnVM = scheduleToReturnVM;
            this.qualificationID = qualificationID;
            this.bracketIndex = bracketIndex;
            this.round = round;
            this.serieMatchNumber = serieMatchNumber;
            bracketFirstTeam = first.id;
            Periods = new ObservableCollection<Period>();
            BindingOperations.EnableCollectionSynchronization(Periods, _lock);
            Shootout = new ObservableCollection<ShootoutShot>();
            BindingOperations.EnableCollectionSynchronization(Shootout, _lock);
            AvailableTeamsHome = new ObservableCollection<Team>();
            availableTeamsHome.Add(first);
            availableTeamsHome.Add(second);
            AvailableTeamsAway = new ObservableCollection<Team>();
            AvailableTeamsAway.Add(first);
            AvailableTeamsAway.Add(second);
            LoadSides();
            LoadStrengths();
            PenaltyReasons = SportsData.LoadPenaltyReasons();
            PenaltyTypes = SportsData.LoadPenaltyTypes();
        }

        //EDIT
        public AddMatchViewModel(NavigationStore navigationStore, Match m, ViewModelBase scheduleToReturnVM)
        {
            edit = true;
            ns = navigationStore;
            seasonID = m.Season.id;
            match = m;
            this.scheduleToReturnVM = scheduleToReturnVM;
            LoadMatchInfo(m.id);

            MatchDate = m.Datetime.Date;
            MatchTimeHours = m.Datetime.Hour;
            MatchTimeMinutes = m.Datetime.Minute;

            Periods = new ObservableCollection<Period>();
            BindingOperations.EnableCollectionSynchronization(Periods, _lock);
            Shootout = new ObservableCollection<ShootoutShot>();
            BindingOperations.EnableCollectionSynchronization(Shootout, _lock);

            if (serieMatchNumber == -1)
            {
                LoadTeams();
            }
            else
            {
                AvailableTeamsHome = new ObservableCollection<Team>();
                availableTeamsHome.Add(m.HomeTeam);
                availableTeamsHome.Add(m.AwayTeam);
                AvailableTeamsAway = new ObservableCollection<Team>();
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
            HomeTeam = AvailableTeamsHome.First(x => x.id == m.HomeTeam.id);
            AwayTeam = AvailableTeamsAway.First(x => x.id == m.AwayTeam.id);

            if (m.Played)
            {
                Played = m.Played;
                if (m.Overtime) { Overtime = m.Overtime; }
                if (m.Shootout) { IsShootout = m.Shootout; }
                if (m.Forfeit) { Forfeit = m.Forfeit; ForfeitWinnerSide = m.HomeScore > m.AwayScore ? "Home" : "Away"; }

                PeriodCount = m.Periods;
                PeriodDuration = m.PeriodDuration;

                //set rosters
                LoadExistingRosters(m.id);

                //load all period events
                LoadExistingEvents(m.id);

                //load shootout
                LoadExistingShootout(m.id);
            }
        }
        #endregion

        #region Methods

        #region Loading
        private void LoadSides()
        {
            Sides = new ObservableCollection<string>();
            Sides.Add("Home");
            Sides.Add("Away");
        }

        private void LoadStrengths()
        {
            Strengths = new ObservableCollection<Strength>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT id, situation, advantage FROM strength", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    Strength s = new Strength
                    {
                        id = int.Parse(row["id"].ToString()),
                        Situation = row["situation"].ToString(),
                        Advantage = row["advantage"].ToString()
                    };

                    Strengths.Add(s);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadTeams()
        {
            AvailableTeamsHome = new ObservableCollection<Team>();
            AvailableTeamsAway = new ObservableCollection<Team>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT team_id, t.name AS team_name, season_id " +
                                                "FROM team_enlistment " +
                                                "INNER JOIN team AS t ON t.id = team_id " +
                                                "WHERE season_id = " + seasonID, connection);

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
                        id = int.Parse(tm["team_id"].ToString()),
                        Name = tm["team_name"].ToString()
                    };

                    AvailableTeamsHome.Add(t);
                    AvailableTeamsAway.Add(t);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadRoster(string side)
        {
            ObservableCollection<PlayerInRoster> roster = new ObservableCollection<PlayerInRoster>();
            int teamID;
            if (side == "Home")
            {
                teamID = HomeTeam.id;
            }
            else
            {
                teamID = AwayTeam.id;
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT player_id, number, pos.name AS position_name, p.first_name AS player_first_name, p.last_name AS player_last_name " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN player AS p ON p.id = player_id " +
                                                "INNER JOIN position AS pos ON pos.code = position_code " +
                                                "WHERE season_id = " + seasonID + " AND team_id = " + teamID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());
                connection.Close();

                foreach (DataRow row in dataTable.Rows)
                {
                    PlayerInRoster p = new PlayerInRoster
                    {
                        id = int.Parse(row["player_id"].ToString()),
                        Name = row["player_first_name"].ToString() + " " + row["player_last_name"].ToString(),
                        Number = int.Parse(row["number"].ToString()),
                        Position = row["position_name"].ToString(),
                        vm = this
                    };

                    roster.Add(p);
                }
                roster = new ObservableCollection<PlayerInRoster>(roster.OrderBy(x => x.Number));

                if (side == "Home")
                {
                    HomePlayers = roster;
                    foreach (Period p in Periods)
                    {
                        p.HomeRoster = new ObservableCollection<PlayerInRoster>();
                    }
                }
                else
                {
                    AwayPlayers = roster;
                    foreach (Period p in Periods)
                    {
                        p.AwayRoster = new ObservableCollection<PlayerInRoster>();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadExistingRosters(int matchID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT player_id, side " +
                                                "FROM player_matches " +
                                                "WHERE match_id = " + matchID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
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
                MessageBox.Show("Unable to connect to databseROSTERS.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadMatchInfo(int matchID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT qualification_id, bracket_index, round, serie_match_number, bracket_first_team FROM matches WHERE id = " + matchID, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                qualificationID = int.Parse(dataTable.Rows[0]["qualification_id"].ToString());
                bracketIndex = int.Parse(dataTable.Rows[0]["bracket_index"].ToString());
                round = int.Parse(dataTable.Rows[0]["round"].ToString());
                serieMatchNumber = int.Parse(dataTable.Rows[0]["serie_match_number"].ToString());
                bracketFirstTeam = int.Parse(dataTable.Rows[0]["bracket_first_team"].ToString());
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databseMATCH.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadExistingEvents(int matchID)
        {
            //load goals
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT player_id, assist_player_id, period, period_seconds, team_id, own_goal, penalty_shot, delayed_penalty " +
                                                "FROM goals WHERE match_id = " + matchID + " ORDER BY order_in_match", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Goal g = new Goal { DelayedPenalty = Convert.ToBoolean(int.Parse(row["delayed_penalty"].ToString())),
                        PenaltyShot = Convert.ToBoolean(int.Parse(row["penalty_shot"].ToString())),
                        OwnGoal = Convert.ToBoolean(int.Parse(row["own_goal"].ToString())),
                        Side = HomeTeam.id == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
                        Minute = int.Parse(row["period_seconds"].ToString()) / 60,
                        Second = int.Parse(row["period_seconds"].ToString()) % 60
                    };

                    int scorerID = int.Parse(row["player_id"].ToString());
                    int assistID = int.Parse(row["assist_player_id"].ToString());
                    if (g.Side == "Home")
                    {
                        g.Scorer = HomeRoster.First(x => x.id == scorerID);
                        if (assistID != -1) { g.Assist = HomeRoster.First(x => x.id == assistID); } else { g.Assist = new PlayerInRoster { id = -1 }; }
                    }
                    else
                    {
                        g.Scorer = AwayRoster.First(x => x.id == scorerID);
                        if (assistID != -1) { g.Assist = AwayRoster.First(x => x.id == assistID); } else { g.Assist = new PlayerInRoster { id = -1 }; }
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
                    Penalty p = new Penalty
                    {
                        Side = HomeTeam.id == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
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
                    PenaltyShot ps = new PenaltyShot
                    {
                        Side = HomeTeam.id == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
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
                    TimeOut t = new TimeOut
                    {
                        Side = HomeTeam.id == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
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
                    GoalieShift s = new GoalieShift
                    {
                        Side = HomeTeam.id == int.Parse(row["team_id"].ToString()) ? "Home" : "Away",
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
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadExistingShootout(int matchID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT player_id, goalie_id, number, was_goal " +
                                                "FROM shootout_shots WHERE match_id = " + matchID + " ORDER BY number", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                ShootoutSeries = (dataTable.Rows.Count + 1) / 2;

                foreach (DataRow row in dataTable.Rows)
                {
                    ShootoutShot ss = Shootout.First(x => x.Number == int.Parse(row["number"].ToString()));
                    ss.WasGoal = Convert.ToBoolean(int.Parse(row["was_goal"].ToString()));

                    int playerID = int.Parse(row["player_id"].ToString());
                    int goalieID = int.Parse(row["goalie_id"].ToString());
                    if (ss.Side == "Home")
                    {
                        ss.Player = HomeRoster.First(x => x.id == playerID);
                        ss.Goalie = HomeRoster.First(x => x.id == goalieID);
                    }
                    else
                    {
                        ss.Player = HomeRoster.First(x => x.id == playerID);
                        ss.Goalie = HomeRoster.First(x => x.id == goalieID);
                    }
                }

                connection.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databseSHOOTOUT.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadGamesheet()
        {
            if (HomeTeam == null)
            {
                MessageBox.Show("Please select the home team.", "Home team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (AwayTeam == null)
            {
                MessageBox.Show("Please select the away team.", "Away team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //warning: all filled data will be overwriten
            if (MessageBox.Show("Warning: All filled data will be overwriten. Do you wish to continue?", "Overwrite data", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            //select file (png, jpg)
            string gamesheetPath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pictures (*.jpg;*.png)|*.jpg;*.png";
            openFileDialog.DefaultExt = ".png";

            bool? result = openFileDialog.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                gamesheetPath = openFileDialog.FileName;
            }

            //show loading screen
            PageVisibility = Visibility.Collapsed;
            LoadingVisibility = Visibility.Visible;

            //run python script on it
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine("py " + SportsData.PythonOCRPath + " " + gamesheetPath + " " + HomePlayers.Count + " " + AwayPlayers.Count);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            string output = cmd.StandardOutput.ReadToEnd();

            //retrieve results
            output = output.Replace("[", string.Empty);
            output = output.Replace("'", string.Empty);
            output = output.Replace("]", string.Empty);

            string[] data = output.Split("END", StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
            Array.Resize(ref data, data.Length - 1);

            List<string> errorList = new List<string>();

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
                if (!ProcessGoalTypeCell(goal[5], out string goalType)) { errorList.Add(errorMessage +", unrecognized goal type"); continue; }

                //insert
                Goal g = new Goal();
                g.Minute = minute;
                g.Second = second;
                g.Side = side;
                g.Scorer = player;
                g.Assist = wasAssist ? assist : new PlayerInRoster { id = -1 };
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
                    g.Assist = new PlayerInRoster { id = -1 };
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
                Penalty pn = new Penalty();
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
                PenaltyShot ps = new PenaltyShot();
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
                GoalieShift shift = new GoalieShift();
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
                TimeOut to = new TimeOut();
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
            foreach (var item in errorList)
            {
                s += item + "\n";
            }
            MessageBox.Show(s, "Error list", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        bool ProcessPeriodCell(string cellValue, out int period, out bool overtime)
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

        bool ProcessPeriodTimeCell(string cellValue, out int minute, out int second)
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

        bool ProcessSideCell(string cellValue, out string side)
        {
            side = "";

            if (cellValue == "H") { side = "Home"; return true; }
            if (cellValue == "A") { side = "Away"; return true; }

            return false;
        }

        bool ProcessPlayerCell(string cellValue, string side, out PlayerInRoster player)
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

        bool ProcessAssistCell(string cellValue, string side, out PlayerInRoster player, out bool exist)
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

        bool ProcessGoalTypeCell(string cellValue, out string type)
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

        bool ProcessBooleanCell(string cellValue, out bool boolean, out bool empty)
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

        bool ProcessPenaltyReasonCell(string cellValue, out PenaltyReason reason)
        {
            reason = PenaltyReasons.FirstOrDefault(x => x.Code == cellValue);

            if (reason == null) { return false; }
            return true;
        }

        bool ProcessPenaltyTypeCell(string cellValue, out PenaltyType type)
        {
            type = PenaltyTypes.FirstOrDefault(x => x.Code == cellValue);

            if (type == null) { return false; }
            return true;
        }

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
        //group all events together and sorts them by time
        private void Process()
        {
            if (PeriodCount == 0)
            {
                MessageBox.Show("There has to be at least 1 period played.", "No period played", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PeriodDuration == 0)
            {
                MessageBox.Show("Period duration can not be 0.", "Invalid period duration", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CheckButtonVisibility = Visibility.Visible;
            SaveButtonVisibility = Visibility.Collapsed;
            PenaltyEndCollisions = new ObservableCollection<PenaltyEndCollision>();

            Events = new ObservableCollection<Event>();
            ConflictEvents = new ObservableCollection<Event>();

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
                    GoalieChange gIn = new GoalieChange(gs, true);
                    GoalieChange gOut = new GoalieChange(gs, false);
                    Event i = new Event { Stat = gIn, Period = p };
                    Event o = new Event { Stat = gOut, Period = p };
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

            ObservableCollection<Event> tmp = new ObservableCollection<Event>();
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
                MessageBox.Show("More events happened at the same time. Please check if the order of events is correct.", "Order confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EventUp(Event e)
        {
            int idx = ConflictEvents.IndexOf(e);

            if (idx == 0 || ConflictEvents[idx - 1].CompareTo(e) != 0) { return; }

            ConflictEvents[idx] = ConflictEvents[idx - 1];
            ConflictEvents[idx].index++;
            ConflictEvents[idx - 1] = e;
            e.index--;
        }

        private void EventDown(Event e)
        {
            int idx = ConflictEvents.IndexOf(e);

            if (idx == ConflictEvents.Count - 1 || ConflictEvents[idx + 1].CompareTo(e) != 0) { return; }

            ConflictEvents[idx] = ConflictEvents[idx + 1];
            ConflictEvents[idx].index--;
            ConflictEvents[idx + 1] = e;
            e.index++;
        }

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

        //sets event properties according to state they happened in, chcecks penalty end collisions with events
        private void Check()
        {
            SaveButtonVisibility = Visibility.Visible;

            foreach (Event e in ConflictEvents)
            {
                Events[e.index] = e;
            }
            ConflictEvents = new ObservableCollection<Event>();

            //delete PeriodEnds if some exist
            for (int i = 0; i < Events.Count; i++)
            {
                if (Events[i].Stat.GetType() == typeof(PeriodEnd))
                {
                    Events.Remove(Events[i]);
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

            List<Penalty> actualPenalties = new List<Penalty>();
            timeSpans = new List<State>();

            List<Penalty> fullPenalties = new List<Penalty>();
            List<int> fullPenaltyStateIndices = new List<int>();

            State actualState = new State(Periods[0], 0, 0, 5, 5, false, false, 0, 0);
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
                                actualPenalties.Remove(actualPenalty);
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
                                actualPenalties.Remove(actualPenalty);
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
            PenaltyEndCollisions = new ObservableCollection<PenaltyEndCollision>();

            for (int i = 0; i < fullPenalties.Count; i++)
            {
                //when not the last state
                if (fullPenaltyStateIndices[i] < timeSpans.Count - 1)
                {
                    PenaltyEndCollision penaltyEnd = new PenaltyEndCollision(timeSpans[fullPenaltyStateIndices[i]], timeSpans[fullPenaltyStateIndices[i] + 1], fullPenalties[i]);

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

        private bool ExceedsFirstToWin(int homeScore, int awayScore)
        {
            if (!(HomeScore <= AwayScore && homeScore > awayScore) || !(HomeScore >= AwayScore && homeScore < awayScore))
            {
                return false;
            }

            int homeWins = 0;
            int awayWins = 0;
            int firstToWin;

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT home_competitor, away_competitor, home_score, away_score " +
                                                "FROM matches " +
                                                "WHERE qualification_id = " + qualificationID + " AND played = 1 AND round = " + round + " AND bracket_index = " + bracketIndex, connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow tm in dataTable.Rows)
                {
                    int hTeam = int.Parse(tm["home_competitor"].ToString());
                    int aTeam = int.Parse(tm["away_competitor"].ToString());
                    int hScore = int.Parse(tm["home_score"].ToString());
                    int aScore = int.Parse(tm["away_score"].ToString());

                    if (hTeam == HomeTeam.id)
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
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private void ExportGamesheet()
        {
            if (HomeTeam == null)
            {
                MessageBox.Show("Please select the home team.", "Home team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (AwayTeam == null)
            {
                MessageBox.Show("Please select the away team.", "Away team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //load excel file
            string tempPath = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllBytes(tempPath, Properties.Resources.gamesheet);
            Microsoft.Office.Interop.Excel.Application excelApplication = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook excelWorkbook;
            excelWorkbook = excelApplication.Workbooks.Open(tempPath);

            //fill data, datetime, teams, rosters
            Microsoft.Office.Interop.Excel.Worksheet gamesheet = (Microsoft.Office.Interop.Excel.Worksheet)excelWorkbook.Worksheets[1];

            //match info
            gamesheet.Range["G" + 8].Value = SportsData.competition.Name;
            gamesheet.Range["J" + 9].Value = SportsData.season.Name;

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

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files | *.pdf";
            saveFileDialog.DefaultExt = "pdf";
            saveFileDialog.FileName = MatchDateTime.ToString("yyyy_MM_dd_HH_mm") + "_" + HomeTeam.Name + "_vs_" + AwayTeam.Name;

            bool? result = saveFileDialog.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                gamesheetPath = saveFileDialog.FileName;

                //export to pdf
                try
                {
                    excelWorkbook.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, gamesheetPath);
                }
                catch (Exception) { }
            }

            excelWorkbook.Close(false);
        }

        private void NotPlayedSave()
        {
            //validation
            if (serieMatchNumber != -1)
            {
                MessageBox.Show("Qualification or play-off match needs to be played.", "Match not played", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (HomeTeam == null)
            {
                MessageBox.Show("Please select the home team.", "Home team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (AwayTeam == null)
            {
                MessageBox.Show("Please select the away team.", "Away team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!edit)
            {
                bracketFirstTeam = HomeTeam.id;
            }

            //saving
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string querry = "INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, periods, " +
                                        "period_duration, home_competitor, away_competitor, home_score, away_score, datetime, overtime, shootout, forfeit, bracket_first_team) " +
                                        "VALUES (" + seasonID + ", " + 0 + ", " + qualificationID + ", " + bracketIndex +
                                        ", " + round + ", " + serieMatchNumber + ", " + 0 + ", " + 0 + ", " + HomeTeam.id +
                                        ", " + AwayTeam.id + ", " + 0 + ", " + 0 + ", '" + MatchDateTime.ToString("yyyy-MM-dd H:mm:ss") + "', " + 0 +
                                        ", " + 0 + ", " + 0 + ", " + bracketFirstTeam + ")";

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();
                cmd = new MySqlCommand(querry, connection);
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
                int matchID = (int)cmd.LastInsertedId;

                if (edit)
                {
                    //delete match from DB
                    querry = "DELETE FROM matches WHERE id = " + match.id;

                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                    //delete all player/goalie match enlistments and all stats
                    List<string> databases = new List<string> { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                    foreach (string db in databases)
                    {
                        querry = "DELETE FROM " + db + " WHERE match_id = " + match.id;
                        cmd = new MySqlCommand(querry, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                connection.Close();

                ScheduleViewModel scheduleViewModel = new ScheduleViewModel(ns);
                if (edit)
                {
                    new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, new Match { id = matchID }, scheduleToReturnVM))).Execute(null);
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
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

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
                List<Goal> goals = new List<Goal>();
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
                        Events.Add(new Event { Period = period, Stat = ps });
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
            List<Shutout> shutouts = new List<Shutout>();
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
            if (bracketIndex != -1 && qualificationID != -1 && ExceedsFirstToWin(homeScore, awayScore))
            {
                MessageBox.Show("Match can not be added. The winner of this match already has the required number of wins to win the series.", "Series match number of violation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (HomeTeam == null)
            {
                MessageBox.Show("Please select the home team.", "Home team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (AwayTeam == null)
            {
                MessageBox.Show("Please select the away team.", "Away team missing", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (IsShootout && ShootoutSeries == 0)
            {
                MessageBox.Show("Shootout must have at least 1 serie.", "No shootout serie", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (IsShootout && ShootoutSeries > 0)
            {
                for (int i = 0; i < Shootout.Count - 2; i++)
                {
                    if (!Shootout[i].Player.Present || !Shootout[i].Goalie.Present)
                    {
                        MessageBox.Show("Shootout atempt missing.", "Incomplete shootout", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if ((!Shootout[Shootout.Count - 2].Player.Present || !Shootout[Shootout.Count - 2].Goalie.Present) && (!Shootout[Shootout.Count - 1].Player.Present || !Shootout[Shootout.Count - 1].Goalie.Present))
                {
                    MessageBox.Show("Shootout atempt missing.", "Incomplete shootout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            //saving
            int matchID = -1;

            if (!edit)
            {
                bracketFirstTeam = HomeTeam.id;
            }

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlTransaction transaction = null;
            MySqlCommand cmd = null;
            string matchInsertionQuerry = "INSERT INTO matches(season_id, played, qualification_id, bracket_index, round, serie_match_number, periods, " +
                                        "period_duration, home_competitor, away_competitor, home_score, away_score, datetime, overtime, shootout, forfeit, bracket_first_team) " +
                                        "VALUES (" + seasonID + ", " + 1 + ", " + qualificationID + ", " + bracketIndex +
                                        ", " + round + ", " + serieMatchNumber + ", " + PeriodCount + ", " + PeriodDuration + ", " + HomeTeam.id +
                                        ", " + AwayTeam.id + ", " + homeScore + ", " + awayScore + ", '" + MatchDateTime.ToString("yyyy-MM-dd H:mm:ss") + "', " + Convert.ToInt32(Overtime) +
                                        ", " + Convert.ToInt32(IsShootout) + ", " + Convert.ToInt32(Forfeit) + ", " + bracketFirstTeam + ")";

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                //match insertion
                cmd = new MySqlCommand(matchInsertionQuerry, connection);
                cmd.Transaction = transaction;
                cmd.ExecuteNonQuery();
                matchID = (int)cmd.LastInsertedId;

                //time-outs insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(TimeOut)))
                {
                    int teamID = t.Stat.Side == "Home" ? HomeTeam.id : AwayTeam.id;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.id : HomeTeam.id;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).id;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;

                    string querry = "INSERT INTO time_outs(match_id, period, period_seconds, order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score) " +
                                              "VALUES (" + matchID + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //period scores insertion
                foreach (Period p in Periods)
                {
                    int homePeriodScore = p.Goals.Where(x => (x.Side == "Home" && !x.OwnGoal) || (x.Side == "Away" && x.OwnGoal)).Count();
                    int awayPeriodScore = p.Goals.Where(x => (x.Side == "Away" && !x.OwnGoal) || (x.Side == "Home" && x.OwnGoal)).Count();

                    string querry = "INSERT INTO period_score(match_id, period, home_team_id, away_team_id, home_score, away_score) " +
                                              "VALUES (" + matchID + ", " + p.Number + ", " + HomeTeam.id + ", " + AwayTeam.id + ", " + homePeriodScore + ", " + awayPeriodScore + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //game states insertion
                foreach (State s in timeSpans)
                {
                    int strengthID = Strengths.First(x => x.Situation == s.StrengthToString()).id;

                    string querry = "INSERT INTO game_state(match_id, period, home_team_id, away_team_id, start_period_seconds, end_period_seconds, strength_id, home_score, away_score) " +
                                              "VALUES (" + matchID + ", " + s.period.Number + ", " + HomeTeam.id + ", " + AwayTeam.id + ", " + s.startTime + ", " + s.endTime + ", " + strengthID + ", " + s.homeGoals + ", " + s.awayGoals + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //shutouts insertion
                foreach (Shutout so in shutouts)
                {
                    int goalieID = so.goalie == null ? -1 : so.goalie.id;
                    int teamID = so.side == "Home" ? HomeTeam.id : AwayTeam.id;
                    int opponentTeamID = so.side == "Home" ? AwayTeam.id : HomeTeam.id;

                    string querry = "INSERT INTO shutouts(match_id, goalie_id, team_id, opponent_team_id) " +
                                              "VALUES (" + matchID + ", " + goalieID + ", " + teamID + ", " + opponentTeamID + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //penalties insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(Penalty)))
                {
                    Penalty p = (Penalty)t.Stat;

                    int teamID = t.Stat.Side == "Home" ? HomeTeam.id : AwayTeam.id;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.id : HomeTeam.id;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).id;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;

                    string querry = "INSERT INTO penalties(match_id, player_id, period, period_seconds, order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score, start_period_seconds, end_period_seconds, duration, penalty_reason_id, penalty_type_id, punished) " +
                                              "VALUES (" + matchID + ", " + p.Player.id + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ", " + p.startTime + ", " + p.endTime + ", " + p.duration + ", " + p.PenaltyReason.Code + ", '" + p.PenaltyType.Code + "', " + Convert.ToInt32(p.punished) + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //goals insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(Goal)))
                {
                    Goal g = (Goal)t.Stat;

                    int teamID = t.Stat.Side == "Home" ? HomeTeam.id : AwayTeam.id;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.id : HomeTeam.id;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).id;
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
                    int goalieID = g.goalie == null ? -1 : g.goalie.id;
                    int assistID = g.Assist == null ? -1 : g.Assist.id;

                    string querry = "INSERT INTO goals(match_id, player_id, goalie_id, assist_player_id, period, period_seconds, order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score, gwg_glog, own_goal, empty_net, penalty_shot, delayed_penalty) " +
                                              "VALUES (" + matchID + ", " + g.Scorer.id + ", " + goalieID + ", " + assistID + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ", " + gwg_glog + ", " + Convert.ToInt32(g.OwnGoal) + ", " + Convert.ToInt32(g.emptyNet) + ", " + Convert.ToInt32(g.PenaltyShot) + ", " + Convert.ToInt32(g.DelayedPenalty) + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //penalty shots insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(PenaltyShot)))
                {
                    PenaltyShot ps = (PenaltyShot)t.Stat;

                    int teamID = t.Stat.Side == "Home" ? HomeTeam.id : AwayTeam.id;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.id : HomeTeam.id;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).id;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;
                    int goalieID = ps.goalie == null ? -1 : ps.goalie.id;

                    string querry = "INSERT INTO penalty_shots(match_id, player_id, goalie_id, period, period_seconds, order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score, was_goal) " +
                                              "VALUES (" + matchID + ", " + ps.Player.id + ", " + goalieID + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ", " + Convert.ToInt32(ps.WasGoal) + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //shootout shots insertion
                foreach (ShootoutShot ss in Shootout.Where(x => x.Player.Present && x.Goalie.Present))
                {
                    int teamID = ss.Side == "Home" ? HomeTeam.id : AwayTeam.id;
                    int opponentTeamID = ss.Side == "Home" ? AwayTeam.id : HomeTeam.id;
                    int decidingGoal = decidingShootoutGoalIndex == ss.Number && ss.Side == decidingShootoutGoalSide ? 1 : 0;

                    string querry = "INSERT INTO shootout_shots(match_id, player_id, goalie_id, number, team_id, opponent_team_id, was_goal, deciding_goal) " +
                                              "VALUES (" + matchID + ", " + ss.Player.id + ", " + ss.Goalie.id + ", " + ss.Number + ", " + teamID + ", " + opponentTeamID + ", " + Convert.ToInt32(ss.WasGoal) + ", " + decidingGoal + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //shifts insertion
                foreach (Event t in Events.Where(x => x.Stat.GetType() == typeof(GoalieChange)))
                {
                    GoalieChange gch = (GoalieChange)t.Stat;
                    if (!gch.Entered) { continue; }

                    int teamID = t.Stat.Side == "Home" ? HomeTeam.id : AwayTeam.id;
                    int opponentTeamID = t.Stat.Side == "Home" ? AwayTeam.id : HomeTeam.id;
                    int strengthID = Strengths.First(x => x.Situation == t.Stat.strength).id;
                    int teamScore = t.Stat.Side == "Home" ? t.Stat.homeScore : t.Stat.awayScore;
                    int opponentScore = t.Stat.Side == "Home" ? t.Stat.awayScore : t.Stat.homeScore;
                    int duration = gch.pairEvent.Stat.TimeInSeconds - gch.TimeInSeconds;

                    string querry = "INSERT INTO shifts(match_id, player_id, period, period_seconds, order_in_match, end_order_in_match, team_id, opponent_team_id, strength_id, team_score, opponent_score, end_period_seconds, duration) " +
                                              "VALUES (" + matchID + ", " + gch.Player.id + ", " + t.Period.Number + ", " + t.Stat.TimeInSeconds + ", " + t.index + ", " + gch.pairEvent.index + ", " + teamID + ", " + opponentTeamID + ", " + strengthID + ", " + teamScore + ", " + opponentScore + ", " + gch.pairEvent.Stat.TimeInSeconds + ", " + duration + ")";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
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
                                              "VALUES (" + p.id + ", " + matchID + ", " + result + ", " + HomeTeam.id + ", 'H')";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
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
                                              "VALUES (" + p.id + ", " + matchID + ", " + result + ", " + AwayTeam.id + ", 'A')";
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                //goalie matches insertion
                List<GoalieInMatch> goalies = new List<GoalieInMatch>();
                for (int i = 0; i < Events.Count; i++)
                {
                    if (Events[i].Stat.GetType() == typeof(GoalieChange) && ((GoalieChange)Events[i].Stat).Entered)
                    {
                        if (goalies.Count(x => x.id == ((GoalieChange)Events[i].Stat).Player.id) == 0)
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
                    int teamID = goalies[i].side == "Home" ? HomeTeam.id : AwayTeam.id;

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
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();
                }

                if (edit)
                {
                    //delete match from DB
                    string querry = "DELETE FROM matches WHERE id = " + match.id;

                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                    //delete all player/goalie match enlistments and all stats
                    List<string> databases = new List<string> { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                    foreach (string db in databases)
                    {
                        querry = "DELETE FROM " + db + " WHERE match_id = " + match.id;
                        cmd = new MySqlCommand(querry, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                connection.Close();

                ScheduleViewModel scheduleViewModel = new ScheduleViewModel(ns);
                if (edit)
                {
                    new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, new Match { id = matchID }, scheduleToReturnVM))).Execute(null);
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
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region Testing
        private ICommand generateMatchCommand;
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

        public void GenerateMatch()
        {
            Random r = new Random();

            //date and time
            DateTime start = new DateTime(2019, 10, 1);
            int range = (new DateTime(2020, 3, 15) - start).Days;
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
            HomePlayers.Where(x => x.id == homeGoalieID).First().Present = true;
            AwayPlayers.Where(x => x.id == awayGoalieID).First().Present = true;

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

            List<Goal> allGoals = new List<Goal>();

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
                                DelayedPenalty = r.Next(1, 50) == 1 ? true : false
                            });
                            //80% with assist
                            if (r.Next(1, 100) <= 80)
                            {
                                int scorerID = p.Goals.Last().Scorer.id;
                                p.Goals.Last().Assist = side == "Home" ? HomeRoster.Where(x => x.id != scorerID).OrderBy(x => r.Next()).First() : AwayRoster.Where(x => x.id != scorerID).OrderBy(x => r.Next()).First();
                            }
                            else
                            {
                                p.Goals.Last().Assist = new PlayerInRoster { id = -1 };
                            }
                            //if no assist, penalty shot = 10%, own goal = 3%
                            if (p.Goals.Last().Assist.id == -1)
                            {
                                int probability = r.Next(1, 100);
                                if (probability <= 10)
                                {
                                    p.Goals.Last().PenaltyShot = true;
                                }
                                else if (probability <= 13)
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
                            //saved penalty shot
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

                p.Goals.Sort();
                p.Penalties.Sort();
                p.PenaltyShots.Sort();
                p.TimeOuts.Sort();

                p.GoalieShifts.Add(new GoalieShift
                {
                    Side = "Home",
                    Player = HomeRoster.Where(x => x.id == homeGoalieID).First(),
                    StartMinute = 0,
                    StartSecond = 0,
                    EndMinute = 9,
                    EndSecond = 59
                });

                p.GoalieShifts.Add(new GoalieShift
                {
                    Side = "Away",
                    Player = AwayRoster.Where(x => x.id == awayGoalieID).First(),
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
                    ShootoutShot firstShot = new ShootoutShot(number, firstSide,
                        firstSide == "Home" ? HomeRoster.Where(x => x.id != homeGoalieID).OrderBy(x => r.Next()).First() : AwayRoster.Where(x => x.id != awayGoalieID).OrderBy(x => r.Next()).First(),
                        firstSide == "Home" ? HomeRoster.Where(x => x.id == homeGoalieID).First() : AwayRoster.Where(x => x.id == awayGoalieID).First());
                    ShootoutShot secondShot = new ShootoutShot(number, secondSide,
                        secondSide == "Home" ? HomeRoster.Where(x => x.id != homeGoalieID).OrderBy(x => r.Next()).First() : AwayRoster.Where(x => x.id != awayGoalieID).OrderBy(x => r.Next()).First(),
                        secondSide == "Home" ? HomeRoster.Where(x => x.id == homeGoalieID).First() : AwayRoster.Where(x => x.id == awayGoalieID).First());

                    if (Shootout.Count < 2)
                    {
                        //simulate both shots
                        firstShot.WasGoal = r.Next() % 2 == 0 ? true : false;
                        if (firstShot.WasGoal) { firstGoals++; }
                        Shootout.Add(firstShot);
                        secondShot.WasGoal = r.Next() % 2 == 0 ? true : false;
                        if (secondShot.WasGoal) { secondGoals++; }
                        Shootout.Add(secondShot);
                    }
                    else if (Shootout.Count < 4)
                    {
                        //simulate both shots
                        firstShot.WasGoal = r.Next() % 2 == 0 ? true : false;
                        if (firstShot.WasGoal) { firstGoals++; }
                        Shootout.Add(firstShot);
                        secondShot.WasGoal = r.Next() % 2 == 0 ? true : false;
                        if (secondShot.WasGoal) { secondGoals++; }
                        Shootout.Add(secondShot);

                        //check for win (if goal difference is 2 -> end)
                        if (Math.Abs(firstGoals - secondGoals) == 2) { end = true; }
                    }
                    else if (Shootout.Count < 6)
                    {
                        int firstGoalsBefore = firstGoals;

                        //simulate first shot
                        firstShot.WasGoal = r.Next() % 2 == 0 ? true : false;
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
                        secondShot.WasGoal = r.Next() % 2 == 0 ? true : false;
                        if (secondShot.WasGoal) { secondGoals++; }
                        Shootout.Add(secondShot);

                        //check for win
                        if (firstGoals != secondGoals) { end = true; }
                    }
                    else
                    {
                        //simulate both shots
                        firstShot.WasGoal = r.Next() % 2 == 0 ? true : false;
                        if (firstShot.WasGoal) { firstGoals++; }
                        Shootout.Add(firstShot);
                        secondShot.WasGoal = r.Next() % 2 == 0 ? true : false;
                        if (secondShot.WasGoal) { secondGoals++; }
                        Shootout.Add(secondShot);

                        //check for win
                        if (firstGoals != secondGoals) { end = true; }
                    }
                    number++;
                }
                ShootoutSeries = (Shootout.Count / 2) + 1;
            }
        }
        #endregion

        #endregion
    }
}