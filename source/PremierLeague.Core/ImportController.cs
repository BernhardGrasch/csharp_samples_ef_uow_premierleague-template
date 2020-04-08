using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PremierLeague.Core.Entities;
using Utils;

namespace PremierLeague.Core
{
    public static class ImportController
    {
        private const string Filename = "PremierLeague.csv";
        public static IEnumerable<Game> ReadFromCsv()
        {
            string filePath = MyFile.GetFullNameInApplicationTree(Filename);
            string[] lines = File.ReadAllLines(filePath);

            Dictionary<string, Team> teams = new Dictionary<string, Team>();
            List<Game> games = new List<Game>();

            foreach (var item in lines)
            {
                string[] parts = item.Split(";");
                int round = Convert.ToInt32(parts[0]);
                string homeTeam = parts[1];
                string guestTeam = parts[2];
                int homeTeamGoals = Convert.ToInt32(parts[3]);
                int guestTeamGoals = Convert.ToInt32(parts[4]);

                Game game = new Game
                {
                    Round = round,
                    HomeGoals = homeTeamGoals,
                    GuestGoals = guestTeamGoals
                };

                Team newHomeTeam;
                if (!teams.TryGetValue(homeTeam, out newHomeTeam))
                {
                    newHomeTeam = new Team
                    {
                        Name = homeTeam
                    };

                    teams.Add(homeTeam, newHomeTeam);
                    newHomeTeam.HomeGames.Add(game);
                    game.HomeTeam = newHomeTeam;
                }
                else
                {
                    newHomeTeam.HomeGames.Add(game);
                    game.HomeTeam = newHomeTeam;
                }

                Team newGuestTeam;
                if (!teams.TryGetValue(homeTeam, out newGuestTeam))
                {
                    newGuestTeam = new Team
                    {
                        Name = guestTeam
                    };

                    teams.Add(guestTeam, newGuestTeam);
                    newGuestTeam.AwayGames.Add(game);
                    game.GuestTeam = newGuestTeam;
                }
                else
                {
                    newGuestTeam.AwayGames.Add(game);
                    game.GuestTeam = newGuestTeam;
                }

                games.Add(game);

            }
            return games.ToArray();
        }
    }
}
