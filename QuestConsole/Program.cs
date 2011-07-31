using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;

namespace QuestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: QUESTCONSOLE.EXE <filename>");
                return;
            }

            string filename = args[0];

            IASL game = GameLauncher.GetGame(filename, null);

            IASLTimer gameTimer = game as IASLTimer;
            if (gameTimer != null)
            {
                gameTimer.RequestNextTimerTick += game_RequestNextTimerTick;
            }

            ConsolePlayer player = new ConsolePlayer();
            PlayerHelper helper = new PlayerHelper(game, player);

            player.Output += player_Output;
            List<string> errors = new List<string>();
            if (!helper.Initialise(player, out errors))
            {
                Console.WriteLine("Failed to load game");
                foreach (string error in errors)
                {
                    Console.WriteLine(error);
                }
            }
            else
            {
                game.Begin();
            }
            Console.ReadKey();
        }

        static void game_RequestNextTimerTick(int obj)
        {
            Console.WriteLine("<timers not implemented>");
        }

        static void player_Output(string text)
        {
            Console.Write(text);
        }
    }
}
