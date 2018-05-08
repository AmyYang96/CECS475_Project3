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
        public long Weight { get; set; }
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
            if (depthLeft==0 || b.IsFinished)
            {
                return new MinimaxBestMove()
                {
                    Move = null,
                    Weight = b.BoardWeight
                };
            }
            bool isMaximizing = (b.CurrentPlayer == 1) ? true : false;
            long bestWeight = (isMaximizing) ? long.MinValue : long.MaxValue;
            IGameMove bestMove = null;

            foreach(var move in b.GetPossibleMoves())
            {
                b.ApplyMove(move);
                var w = FindBestMove(b, alpha, beta, depthLeft - 1);
                b.UndoLastMove();
                if(isMaximizing && w.Weight > alpha)
                {
                    alpha = (int)w.Weight;
                    bestMove = move;
                } else if(!isMaximizing && w.Weight < beta )
                {
                    beta = (int)w.Weight;
                    bestMove = move;
                }
                if(alpha >= beta) {
                    return new MinimaxBestMove() 
                    {
                        Move = move;
                        Weight = b.BoardWeight;
                    }
                }

            }


            return new MinimaxBestMove()
            {
                Weight = alpha,
                Move = bestMove
            };
        }

    }
}