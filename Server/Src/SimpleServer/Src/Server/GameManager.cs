using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Lockstep.Logging;

namespace Lockstep.FakeServer
{

    public class GameManager
    {
        public List<Game> games = new List<Game>();
        public GameManager()
        {
        }
        public void DoUpdate(float datlaTime)
        {
            if (games.Count > 0)
            {
                foreach (var game in games)
                {
                    game.DoUpdate(datlaTime);
                }
            }
        }
        private int gameId = 0;
        public void CreateGame(Player[] players)
        {
            if (players == null || players.Length == 0) return;
            var game = new Game();
            int i = 0;
            foreach (var player in players)
            {
                player.LocalId = (byte)i;
                player.Game = game;
                i++;
            }
            game.DoStart(gameId++, 0, 0, players, "123");
            games.Add(game);
        }

        public void OnPlayerQuit(Player player)
        {
            player.Game.OnPlayerLeave(player);
        }
    }
}

