﻿using TicTacToe.Board;
using TicTacToe.Led;

namespace TicTacToe;

public class Game
{
    private readonly Statistics _statistics;
    public Players HumanPlayer { get; private set; }

    public Game(Statistics statistics)
    {
        _statistics = statistics;
    }

    public GameBoard Board { get; } = new GameBoard();

    public GameOverType Winner { get; private set; }

    public void Setup()
    {
        Board.DrawBoard();
        LedManager.SetDark();
        HumanPlayer = (Players)Random.Shared.Next(0, 2);
    }

    public void Play()
    {
        int turnCounter = 0;
        if (HumanPlayer is Players.O)
        {
            turnCounter++;
        }

        do
        {
            var status = Board.CheckForWinner();
            if (status.gameOver)
            {
                SetGameOverType(status.winner);
                break;
            }

            if (turnCounter % 2 is 0)
            {
                PlayerTurn(HumanPlayer);
            }
            else
            {
                OpponentTurn(GetOpposingPlayer(HumanPlayer));
            }

            turnCounter++;

        } while (true);
    }

    public void AnnounceWinner()
    {
        LedManager.FlashEnter(Winner);
        switch (Winner)
        {
            case GameOverType.Tie:
                Console.WriteLine("Tie!");
                _statistics.Ties++;
                break;
            case GameOverType.X:
                Console.WriteLine($"You win!");
                _statistics.Wins++;
                break;
            case GameOverType.O:
                Console.WriteLine("You lose!");
                _statistics.Losses++;
                break;
        }
    }

    private void PlayerTurn(Players player)
    {
        bool result;
        do
        {
            var key = Console.ReadKey(true).Key;

            result = key switch
            {
                ConsoleKey.NumPad7 or ConsoleKey.D1 => Board.DrawPlayer(Boxes.B1, player),
                ConsoleKey.NumPad8 or ConsoleKey.D2 => Board.DrawPlayer(Boxes.B2, player),
                ConsoleKey.NumPad9 or ConsoleKey.D3 => Board.DrawPlayer(Boxes.B3, player),
                ConsoleKey.NumPad4 or ConsoleKey.D4 => Board.DrawPlayer(Boxes.B4, player),
                ConsoleKey.NumPad5 or ConsoleKey.D5 => Board.DrawPlayer(Boxes.B5, player),
                ConsoleKey.NumPad6 or ConsoleKey.D6 => Board.DrawPlayer(Boxes.B6, player),
                ConsoleKey.NumPad1 or ConsoleKey.D7 => Board.DrawPlayer(Boxes.B7, player),
                ConsoleKey.NumPad2 or ConsoleKey.D8 => Board.DrawPlayer(Boxes.B8, player),
                ConsoleKey.NumPad3 or ConsoleKey.D9 => Board.DrawPlayer(Boxes.B9, player),
                _ => false,
            };
        } while (result is false);
    }

    private void OpponentTurn(Players player)
    {
        Box? idealBox = null;
        var winningLine = Board.GetWinningLine(player);
        if (winningLine is not null)
        {
            idealBox = winningLine.GetEmptyBoxes().FirstOrDefault();
        }

        if (idealBox is null)
        {
            var dangerousLines = Board.GetDangerousLines(player);

            if (dangerousLines.Count is not 0)
            {
                List<Box> priorityBoxes = new();
                foreach (var line in dangerousLines)
                {
                    priorityBoxes.AddRange(line.GetEmptyBoxes());
                }

                var bestBoxes = priorityBoxes.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

                idealBox = bestBoxes.MaxBy(x => x.Value).Key;
            }
        }

        if (idealBox is null)
        {
            var boxes = Board.GetEmptyBoxes().ToList();
            idealBox = boxes[Random.Shared.Next(0, boxes.Count)];

        }

        Board.DrawPlayer(idealBox, player);
    }

    private void SetGameOverType(Players? winner) =>
        Winner = winner switch
        {
            Players.X => GameOverType.X,
            Players.O => GameOverType.O,
            _ => GameOverType.Tie,
        };

    public static Players GetOpposingPlayer(Players player) =>
        player switch
        {
            Players.X => Players.O,
            Players.O => Players.X,
            _ => throw new NotImplementedException()
        };
}
