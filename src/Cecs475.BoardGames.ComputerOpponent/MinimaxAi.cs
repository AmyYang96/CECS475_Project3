using Cecs475.BoardGames.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cecs475.BoardGames.ComputerOpponent
{
    internal struct MinimaxBestMove
    {
        public int Weight { get; set; }
        public IGameMove Move { get; set; }
    }

    public class MinimaxAi : IGameAi
    {
        private int mMaxDepth;
        public MinimaxAi(int maxDepth)
        {
            mMaxDepth = maxDepth;
        }

        public IGameMove FindBestMove(IGameBoard b)
        {
            return FindBestMove(b,
                true ? int.MinValue : int.MaxValue,
                true ? int.MaxValue : int.MinValue,
                mMaxDepth).Move;
        }

        private static MinimaxBestMove FindBestMove(IGameBoard b, int alpha, int beta, int depthLeft)
        {
            return new MinimaxBestMove()
            {
                Move = null
            };
        }

    }
}