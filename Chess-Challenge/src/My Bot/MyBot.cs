using ChessChallenge.API;
using ChessChallenge.Application;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        

        Move[] moves = board.GetLegalMoves();
        return moves[0];
        double[] score = new double[moves.Length];

        // Ok for now
        Move result = moves[0];
        double max_so_far = 0;

        for (int i = 0; i < moves.Length; i++)
        {
            Move m = moves[i];
            
            // the chaos factor allows the bot to tie-break best moves
            // If the moves are evaluated the same, there will always be a small difference between them
            score[i] += evaluate(board, m);
 
            // Check 1 move deep what opp can take
            //var r = opponent_responses(board, m);
            //var responses = r.Item1;
            //Console.WriteLine("[[ GONNA CHECK ({0}) RESPONSES ]]", responses.Length);
            //double opp_best_response = 0;
            //var j = 0;
            //foreach (double s in r.Item2)
            //{
            //    if (s < opp_best_response)
            //    {
            //        opp_best_response = s;
            //        Console.WriteLine("But in response {0} -{1}", responses[j], s);
            //    }
            //    j += 1;
            //}
            //score[i] -= opp_best_response;
            // forkForStyle(board, m) => [liczba zforkowanych figur ^4 + liczba * base]  1->1 + 20, 2->16 + 40, 3->81 + 60, ... 8->4096 + 

            // Pick the best
            if (score[i] > max_so_far)
            {
                max_so_far = score[i];
                result = moves[i];
            }
        }
        Console.WriteLine("Playing {0}", result);
        return result;
   
        // The ultimate goal is not to win, but to fork the most
        // 
        // Nintendo ForkBomb Advance
        // fork - if possible.
        //      a fork is a move, which 
        //          attacks at least 2 pieces(or pawns)
        //          wins a piece on next turn (wins as in does not lose the immediate exchange)
        //  a losing immediate exchange would be offering to trade a queen for a knight right away
        // attack - if possible = not losing a piece immediately
        // advance - develop pieces, using a heatmap? if a piece has not yet moved doit 
        //
        // (central pawns, knights, bishops, 
        //  castle, other pawns (but not in front of king), 
        //  rooks, 
        //
    }

    // jak porównać ze sobą dwie pozycje pod względem podobieństwa s
    //      w której turze zagrano jaki ruch
    // no dwie giery bd podobne jak jest tura t=1 i { e4e5 } -- to ta sama giera (s=1),
    //
    // 1.e4e5 2.d4d5  i  1.e4d5 2.d4e5
    //      na końcu jest ta sama pozycja (fen==?) i jest 3. tura (t=3)
    //      więc dalej s=1
    // czyli zapisz fen i nr tury t. (str fen = Board.getFenString, int t = Board.PlyCount)
    // to powinno zmniejszyć bazę znacząco?? jak znacząco EKSPERYMENT
    // The number of permutations of a game that long, (t=k)
    // czyli dla k=10?
    // k=0 1
    // k=1 1
    // k=2 2!*2!
    // k=3 3!*3!
    // k=10 10!*10!
    // t = (k!) ** 2
    // czyli jak cacheujemy przeszukiwanie to można zmniejszyć coś


    private double evaluate(Board board, Move m)
    {
        Random rand = new Random();
        int[] piece_values = { 0, 1, 3, 3, 5, 9, 300 };

        Console.WriteLine("[ {0} ]", m);
        double v = rand.NextDouble();
        board.MakeMove(m);
        if (board.IsInCheckmate())
        {
            v += 10000;
            Console.WriteLine(" MATE ");
        }
        if (board.IsInCheck())
        {
            v += 200;
            Console.WriteLine(" CHECK ");
        }
        // Play the best capture
        if (m.CapturePieceType != PieceType.None)
        {
            v += piece_values[(int)m.CapturePieceType];
            Console.WriteLine("{0} takes {1}  +{2}", m.MovePieceType, m.CapturePieceType, piece_values[(int)m.CapturePieceType]);
        }
        board.UndoMove(m);
        return v;
    }
    private Tuple<Move[], IEnumerable<double>> opponent_responses(Board board, Move m)
    {
        board.MakeMove(m);
        Move[] responses = board.GetLegalMoves();
        var score = responses.Select(r => evaluate(board, r));
        board.UndoMove(m);
        return Tuple.Create(responses, score);
    }
}