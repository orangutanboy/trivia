using System;
using System.IO;

namespace UglyTrivia
{
    public class Game
    {
        private StreamWriter _logWriter;

        internal PlayerList Players;
        internal Questions Questions;

        internal Game(StreamWriter logWriter)
        {
            _logWriter = logWriter;

            Players = new PlayerList();
            Questions = new Questions();
        }

        public Game()
            : this(new StreamWriter(Console.OpenStandardOutput()))
        {
        }

        public bool IsPlayable()
        {
            return (Players.Count >= 2);
        }

        public void AddPlayer(string playerName)
        {
            var newPlayer = new Player(_logWriter)
            {
                Name = playerName
            };

            Players.Add(newPlayer);
            newPlayer.PlaceUpdated += player_PlaceUpdated;

            _logWriter.WriteLine(playerName + " was added");
            _logWriter.WriteLine("They are player number " + Players.Count);
        }

        public void Roll(int value)
        {
            _logWriter.WriteLine(CurrentPlayer.Name + " is the current player");
            _logWriter.WriteLine("They have rolled a " + value);

            CurrentPlayer.Roll(value);
            
            if (CurrentPlayer.State != PlayerState.InPenaltyBox)
            {
                _logWriter.WriteLine(Questions.GetQuestion(CurrentCategory));
                Questions.RemoveQuestion(CurrentCategory);
            }
        }

        //TODO: name this something useful
        public bool WasCorrectlyAnswered()
        {
            CurrentPlayer.AnsweredCorrectly();
            var winner = CurrentPlayer.IsWinner();
            Players.MoveToNextPlayer();
            return winner;
        }

        public void WrongAnswer()
        {
            CurrentPlayer.AnsweredIncorrectly();
            Players.MoveToNextPlayer();
        }

        private Category CurrentCategory
        {
            get
            {
                return (Category)(CurrentPlayer.Place % 4);
            }
        }

        private Player CurrentPlayer
        {
            get
            {
                return Players.CurrentPlayer;
            }
        }

        private void player_PlaceUpdated(object sender, EventArgs e)
        {
            _logWriter.WriteLine("The category is " + CurrentCategory);
        }

        private bool IsCurrentPlayerWinner()
        {
            return !(CurrentPlayer.Purse == 6);
        }
    }
}