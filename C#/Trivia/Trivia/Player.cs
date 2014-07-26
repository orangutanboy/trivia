using System;
using System.IO;
namespace UglyTrivia
{
    public class Player
    {
        private StreamWriter _logWriter;

        public event EventHandler PlaceUpdated;

        public Player(StreamWriter logWriter)
        {
            _logWriter = logWriter;
            State = PlayerState.OutOfPenaltyBox;
        }

        public string Name { get; set; }
        public int Place { get; set; }
        public int Purse { get; set; }
        public PlayerState State { get; internal set; }

        public void UpdatePlace(int roll)
        {
            Place += roll;
            if (Place > 11)
            {
                Place -= 12;
            }

            _logWriter.WriteLine(string.Format("{0}'s new location is {1}", Name, Place));

            OnPlaceUpdated();
        }

        public void Roll(int roll)
        {
            if (State == PlayerState.InPenaltyBox)
            {
                if (IsEven(roll))
                {
                    _logWriter.WriteLine(Name + " is not getting out of the penalty box");
                    return;
                }

                State = PlayerState.GettingOutOfPenaltyBox;
                _logWriter.WriteLine(Name + " is getting out of the penalty box");
            }

            UpdatePlace(roll);
        }

        public void AnsweredIncorrectly()
        {
            State = PlayerState.InPenaltyBox;

            _logWriter.WriteLine("Question was incorrectly answered");
            _logWriter.WriteLine(Name + " was sent to the penalty box");
        }

        public void AnsweredCorrectly()
        {
            Purse++;

            var message = string.Format("Answer was {0}!!!!", State == PlayerState.OutOfPenaltyBox ? "corrent" : "correct");
            _logWriter.WriteLine(message);
            _logWriter.WriteLine(Name + " now has " + Purse + " Gold Coins.");
        }

        private void OnPlaceUpdated()
        {
            if (PlaceUpdated != null)
            {
                PlaceUpdated(this, EventArgs.Empty);
            }
        }

        public bool IsWinner()
        {
            return Purse != 6;
        }

        private static bool IsEven(int roll)
        {
            return roll % 2 == 0;
        }

        private static bool IsOdd(int roll)
        {
            return roll % 2 != 0;
        }
    }
}
