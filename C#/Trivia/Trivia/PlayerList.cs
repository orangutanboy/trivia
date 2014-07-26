using System.Collections.Generic;
using System.IO;

namespace UglyTrivia
{
    public class PlayerList : List<Player>
    {
        public int CurrentPlayerIndex { get; private set; }

        public void MoveToNextPlayer()
        {
            if (CurrentPlayerIndex == (Count - 1))
            {
                CurrentPlayerIndex = 0;
            }
            else
            {
                CurrentPlayerIndex++;
            }
        }

        public Player CurrentPlayer
        {
            get
            {
                return this[CurrentPlayerIndex];
            }
        }
    }
}
