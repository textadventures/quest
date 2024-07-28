using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest;

namespace QuestConsole
{
    public class Runner
    {
        private string m_filename;
        private ConsolePlayer m_player;
        private PlayerHelper m_helper;
        private bool m_running = true;

        public Runner(string filename)
        {
            m_filename = filename;
        }

        public void Start()
        {
            IASL game = GameLauncher.GetGame(m_filename, null);

            m_player = new ConsolePlayer(game);
            m_player.ClearBuffer += ClearBuffer;
            m_player.Finish += Finish;
            m_helper = new PlayerHelper(game, m_player);

            List<string> errors = new List<string>();
            if (!m_helper.Initialise(m_player, out errors))
            {
                Console.WriteLine("Failed to load game");
                foreach (string error in errors)
                {
                    Console.WriteLine(error);
                }
                Console.ReadKey();
            }
            else
            {
                game.Begin();
                PlayGame();
            }
        }

        private void Finish()
        {
            m_running = false;
            ClearBuffer();
        }

        private void ClearBuffer()
        {
            m_player.OutputText(m_helper.ClearBuffer());
        }

        private void PlayGame()
        {
            do
            {
                ClearBuffer();
                string input = Console.ReadLine();

                m_helper.SendCommand(input, m_player.GetTickCountAndStopTimer(), null);
            } while (m_running);
        }
    }
}
