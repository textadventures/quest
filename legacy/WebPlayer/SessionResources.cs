using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using TextAdventures.Quest;

namespace WebPlayer
{
    public class SessionResources
    {
        private Dictionary<string, IASL> m_games = new Dictionary<string, IASL>();

        public void AddGame(IASL game)
        {
            m_games[game.GameID] = game;
        }
        
        public Stream Get(string id, string filename)
        {
            if (!m_games.ContainsKey(id)) return null;
            return m_games[id].GetResource(filename);
        }
    }
}