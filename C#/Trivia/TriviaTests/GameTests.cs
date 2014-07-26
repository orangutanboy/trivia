using NUnit.Framework;
using System;
using System.IO;
using UglyTrivia;
using System.Linq;

namespace TriviaTests
{
    [TestFixture]
    public class GameTests
    {
        private StreamWriter _logStreamWriter;
        private Stream _logStream;
        private Game _target;

        [SetUp]
        public void CreateStream()
        {
            _logStream = new MemoryStream();
            _logStreamWriter = new StreamWriter(_logStream);
            _logStreamWriter.AutoFlush = true;
            _target = new Game(_logStreamWriter);
        }

        [TearDown]
        public void DestroyStream()
        {
            _logStreamWriter.Dispose();
        }

        [Test]
        public void QuestionListsAreInitialisedTo50()
        {
            Assert.That(_target.Questions.Select(kvp => kvp.Value).Sum(l => l.Count), Is.EqualTo(200));
        }

        [Test]
        public void QuestionListsAreInitialisedWithCorrectValues()
        {
            Assert.That(_target.Questions[Category.Pop].First(), Is.EqualTo("Pop Question 0"));
            Assert.That(_target.Questions[Category.Pop].Last(), Is.EqualTo("Pop Question 49"));

            Assert.That(_target.Questions[Category.Science].First(), Is.EqualTo("Science Question 0"));
            Assert.That(_target.Questions[Category.Science].Last(), Is.EqualTo("Science Question 49"));
            
            Assert.That(_target.Questions[Category.Sports].First(), Is.EqualTo("Sports Question 0"));
            Assert.That(_target.Questions[Category.Sports].Last(), Is.EqualTo("Sports Question 49"));
            
            Assert.That(_target.Questions[Category.Rock].First(), Is.EqualTo("Rock Question 0"));
            Assert.That(_target.Questions[Category.Rock].Last(), Is.EqualTo("Rock Question 49"));
        }

        [Test]
        public void NewPlayerIsAddedToList()
        {
            _target.AddPlayer("Mark");
            Assert.That(_target.Players.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddingNewPlayerLogsConfirmations()
        {
            _target.AddPlayer("Mark");
            var logStream = GetLogStream();
            Assert.That(logStream, Is.StringContaining("Mark was added"));
            Assert.That(logStream, Is.StringContaining("They are player number 1"));
        }

        [Test]
        public void GameNeedsMoreThan1Player()
        {
            _target.AddPlayer("Mark");
            Assert.That(_target.IsPlayable(), Is.False);
            _target.AddPlayer("Derek");
            Assert.That(_target.IsPlayable(), Is.True);
        }

        [Test]
        public void RollLogsInfo()
        {
            _target.AddPlayer("Mark");
            _target.AddPlayer("Derek");
            _target.Roll(1);
            var logStream = GetLogStream();
            Assert.That(logStream, Is.StringContaining("Mark is the current player"));
            Assert.That(logStream, Is.StringContaining("They have rolled a 1"));
        }

        [Test]
        public void RollWhereCurrentPlayerIsNotInPenaltyBox()
        {
            _target.AddPlayer("Mark");
            _target.AddPlayer("Derek");
            _target.Roll(1);
            Assert.That(_target.Players[0].Place, Is.EqualTo(1));
        }

        [Test]
        public void RollWhereCurrentPlayerIsNotInPenaltyBoxLogsInfo()
        {
            _target.AddPlayer("Mark");
            _target.AddPlayer("Derek");
            _target.Roll(1);
            var logged = GetLogStream();
            Assert.That(logged, Is.StringContaining("Mark's new location is 1"));
            Assert.That(logged, Is.StringContaining("The category is Science"));
        }

        [Test]
        public void BoardHas12SpacesAndIsCircular()
        {
            _target.AddPlayer("Mark");
            _target.Roll(12);
            _target.Roll(4);
            Assert.That(_target.Players[0].Place, Is.EqualTo(4));
        }

        [TestCase(1, "Science")]
        [TestCase(2, "Sports")]
        [TestCase(3, "Rock")]
        [TestCase(4, "Pop")]
        public void QuestionsAreLoggedWhenAsked(int roll, string category)
        {
            _target.AddPlayer("Mark");
            _target.Roll(roll);
            var logged = GetLogStream();
            Assert.That(logged, Is.StringContaining(category + " Question 0"));
        }

        [Test]
        public void QuestionIsAskedAndRemovedWhenAPlayerMoves()
        {
            _target.AddPlayer("Mark");
            _target.Roll(1);
            _target.Roll(1);
            Assert.That(NumberOfQuestionsAsked(_target), Is.EqualTo(2));
        }

        [Test]
        public void ThrowingAnOddWhenInPenaltyBoxGetsPlayerOutOfPenaltyBox()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.InPenaltyBox;
            _target.Roll(1);
            Assert.That(_target.Players[0].State, Is.EqualTo(PlayerState.GettingOutOfPenaltyBox));
        }

        [Test]
        public void ThrowingAnOddWhenInPenaltyBoxMovesPlayerRoundBoad()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.InPenaltyBox;
            _target.Roll(3);
            Assert.That(_target.Players[0].Place, Is.EqualTo(3));
        }

        [Test]
        public void ThrowingAnOddWhenInPenaltyBoxAsksPlayerAQuestion()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.InPenaltyBox;
            _target.Roll(3);
            Assert.That(NumberOfQuestionsAsked(_target), Is.EqualTo(1));
        }

        [Test]
        public void ThrowingAnOddWhenInPenaltyBoxLogsInfo()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.InPenaltyBox;
            _target.Roll(3);
            var logged = GetLogStream();
            Assert.That(logged, Is.StringContaining("Mark is getting out of the penalty box"));
            Assert.That(logged, Is.StringContaining("Mark's new location is 3"));
            Assert.That(logged, Is.StringContaining("The category is Rock"));
        }

        [Test]
        public void ThrowingAnEvenWhenInPenaltyBoxLeavesPlayerInPenaltyBox()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.InPenaltyBox;
            _target.Roll(2);
            Assert.That(_target.Players[0].State, Is.EqualTo(PlayerState.InPenaltyBox));
        }

        [Test]
        public void ThrowingAnEvenWhenInPenaltyBoxLogsInfo()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.InPenaltyBox;
            _target.Roll(2);
            var logged = GetLogStream();
            Assert.That(logged, Is.StringContaining("Mark is not getting out of the penalty box"));
        }

        [Test]
        public void PlayerGivingWrongAnswerMovesPlayerToPenaltyBox()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.OutOfPenaltyBox;
            _target.WrongAnswer();
            Assert.That(_target.Players[0].State, Is.EqualTo(PlayerState.InPenaltyBox));
        }

        [Test]
        public void PlayerGivingWrongAnswerMovesPlayToNextPlayer()
        {
            _target.AddPlayer("Mark");
            _target.AddPlayer("Derek");
            _target.Players[0].State = PlayerState.OutOfPenaltyBox;
            _target.WrongAnswer();
            Assert.That(_target.Players.CurrentPlayerIndex, Is.EqualTo(1));
        }

        [Test]
        public void PlayerGivingWrongAnswerLogsInfo()
        {
            _target.AddPlayer("Mark");
            _target.AddPlayer("Derek");
            _target.Players[0].State = PlayerState.OutOfPenaltyBox;
            _target.WrongAnswer();
            var logged = GetLogStream();
            Assert.That(logged, Is.StringContaining("Question was incorrectly answered"));
            Assert.That(logged, Is.StringContaining("Mark was sent to the penalty box"));
        }

        [Test]
        public void PlayerNotInPenaltyBoxGivingCorrectAnswerMovesPlayToNextPlayer()
        {
            _target.AddPlayer("Mark");
            _target.AddPlayer("Derek");
            _target.Players[0].State = PlayerState.OutOfPenaltyBox;
            _target.WasCorrectlyAnswered();
            Assert.That(_target.Players.CurrentPlayerIndex, Is.EqualTo(1));
        }

        [Test]
        public void PlayerNotInPenaltyBoxGivingCorrectAnswerIncreasesPlayerScore()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.OutOfPenaltyBox;
            _target.WasCorrectlyAnswered();
            Assert.That(_target.Players[0].Purse, Is.EqualTo(1));
        }

        [Test]
        public void PlayerNotInPenaltyBoxGivingCorrectAnswerWithScore4DoesNotWinGame()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.OutOfPenaltyBox;
            _target.Players[0].Purse = 4;
            Assert.That(_target.WasCorrectlyAnswered(), Is.True);
        }

        [Test]
        public void PlayerNotInPenaltyBoxGivingCorrectAnswerWithScore5WinsGame()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.OutOfPenaltyBox;
            _target.Players[0].Purse = 5;
            Assert.That(_target.WasCorrectlyAnswered(), Is.False);
        }

        [Test]
        public void PlayerNotInPenaltyBoxGivingCorrectAnswerLogsInfo()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.OutOfPenaltyBox;
            _target.Players[0].Purse = 4;
            _target.WasCorrectlyAnswered();
            var logged = GetLogStream();
            Assert.That(logged, Is.StringContaining("Answer was corrent!!!!"));
            Assert.That(logged, Is.StringContaining("Mark now has 5 Gold Coins."));
        }

        [Test]
        public void PlayerInPenaltyBoxAndGivingCorrectAnswerIsNotWinner()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.GettingOutOfPenaltyBox;
            _target.Players[0].Purse = 4;
            Assert.That(_target.WasCorrectlyAnswered(), Is.True);
        }

        [Test]
        public void PlayerInPenaltyBoxAndGivingCorrectAnswerLogsInfo()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.GettingOutOfPenaltyBox;
            _target.Players[0].Purse = 4;
            _target.WasCorrectlyAnswered();
            var logged = GetLogStream();
            Assert.That(logged, Is.StringContaining("Answer was correct!!!!"));
            Assert.That(logged, Is.StringContaining("Mark now has 5 Gold Coins."));
        }

        [Test]
        public void PlayerInPenaltyBoxAndGivingCorrectAnswerWithScore5WinsGame()
        {
            _target.AddPlayer("Mark");
            _target.Players[0].State = PlayerState.GettingOutOfPenaltyBox;
            _target.Players[0].Purse = 5;
            Assert.That(_target.WasCorrectlyAnswered(), Is.False);
        }

        [Test]
        public void PlayerInPenaltyBoxAndNotGettingOutOfPenaltyBoxAndGivingCorrectAnswerMovesToNextPlayer()
        {
            _target.AddPlayer("Mark");
            _target.AddPlayer("Derek");
            _target.Players[0].State = PlayerState.GettingOutOfPenaltyBox;
            _target.Players[0].Purse = 0;
            _target.WasCorrectlyAnswered();
            Assert.That(_target.Players.CurrentPlayerIndex, Is.EqualTo(1));
        }

        private int NumberOfQuestionsAsked(Game game)
        {
            return 200 - (game.Questions[Category.Pop].Count + game.Questions[Category.Science].Count + game.Questions[Category.Sports].Count + game.Questions[Category.Rock].Count);
        }

        private string GetLogStream()
        {
            _logStream.Seek(0, SeekOrigin.Begin);
            string logged;
            using (var streamReader = new StreamReader(_logStream))
            {
                logged = streamReader.ReadToEnd();
            }
            return logged;
        }
    }
}
