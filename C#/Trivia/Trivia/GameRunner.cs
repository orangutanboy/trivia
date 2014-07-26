using System;
using UglyTrivia;

namespace Trivia
{
    public class GameRunner
    {
        private static bool notAWinner;

        public static void Main(String[] args)
        {
            Game aGame = new Game();

            aGame.AddPlayer("Chet");
            aGame.AddPlayer("Pat");
            aGame.AddPlayer("Sue");

            Random rand = new Random();

            do
            {
                aGame.Roll(rand.Next(5) + 1);

                if (rand.Next(9) == 7)
                {
                    aGame.WrongAnswer();
                    notAWinner = true;
                }
                else
                {
                    notAWinner = aGame.WasCorrectlyAnswered();
                }

            } while (notAWinner);

            Console.ReadLine();
        }
    }
}