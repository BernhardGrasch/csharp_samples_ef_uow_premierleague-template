using Microsoft.EntityFrameworkCore;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace PremierLeague.Persistence
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<Team> GetAllWithGames()
        {
            return _dbContext.Teams.Include(t => t.HomeGames).Include(t => t.AwayGames).ToList();
        }

        public IEnumerable<Team> GetAll()
        {
            return _dbContext.Teams.OrderBy(t => t.Name).ToList();
        }

        public void AddRange(IEnumerable<Team> teams)
        {
            _dbContext.Teams.AddRange(teams);
        }

        public Team Get(int teamId)
        {
            return _dbContext.Teams.Find(teamId);
        }

        public void Add(Team team)
        {
            _dbContext.Teams.Add(team);
        }

        public (Team Team, int Goals) GetTeamWithMostGoals()
        {
            return _dbContext
                .Teams
                .Select(t => new
                {
                    Team = t,
                    Goals = t.AwayGames.Sum(h => h.GuestGoals) + t.HomeGames.Sum(a => a.HomeGoals)
                })
                .AsEnumerable()
                .Select(t => (t.Team, t.Goals))
                .OrderByDescending(t => t.Goals)
                .First();
        }

        public (Team Team, int Goals) GetTeamWithMostAwayGoals()
        {
            return _dbContext
                .Teams
                .Select(t => new
                {
                    Team = t,
                    Goals = t.AwayGames.Sum(g => g.GuestGoals)
                })
                .AsEnumerable()
                .Select(t => (t.Team, t.Goals))
                .OrderByDescending(t => t.Goals)
                .First();
        }

        public (Team Team, int Goals) GetTeamWithMostHomeGoals()
        {
            return _dbContext
                .Teams
                .Select(t => new
                {
                    Team = t,
                    Goals = t.HomeGames.Sum(g => g.HomeGoals)
                })
                .AsEnumerable()
                .Select(t => (t.Team, t.Goals))
                .OrderByDescending(t => t.Goals)
                .First();
        }

        public (Team Team, int GoalDifference) GetTeamWithBestGoalDifference()
        {
            return _dbContext
                .Teams
                .Select(t => new
                {
                    Team = t,
                    GoalDifference = t.HomeGames.Sum(g => g.HomeGoals) - t.HomeGames.Sum(g => g.GuestGoals)
                    + t.AwayGames.Sum(g => g.HomeGoals) - t.AwayGames.Sum(g => g.GuestGoals)
                })
                .AsEnumerable()
                .Select(t => (t.Team, t.GoalDifference))
                .OrderByDescending(t => t.GoalDifference)
                .First();
        }

        public IEnumerable<TeamStatisticDto> GetTeamStatistics()
        {
            return _dbContext
                .Teams
                .Select(t => new TeamStatisticDto
                {
                    Name = t.Name,
                    AvgGoalsShotAtHome = (double)t.HomeGames.Sum(g => g.HomeGoals) / t.HomeGames.Count(),
                    AvgGoalsShotOutwards = (double)t.AwayGames.Sum(g => g.GuestGoals) / t.AwayGames.Count(),
                    AvgGoalsShotInTotal = (double)(t.HomeGames.Sum(g => g.HomeGoals) + t.AwayGames.Sum(g => g.GuestGoals)) 
                        / (t.AwayGames.Count() + t.HomeGames.Count()),
                    AvgGoalsGotAtHome = (double)t.HomeGames.Sum(g => g.GuestGoals) / t.HomeGames.Count(),
                    AvgGoalsGotOutwards = (double)t.AwayGames.Sum(g => g.HomeGoals) / t.AwayGames.Count(),
                    AvgGoalsGotInTotal = (double)(t.HomeGames.Sum(g => g.GuestGoals) + t.AwayGames.Sum(g => g.HomeGoals)) 
                        / (t.HomeGames.Count() + t.AwayGames.Count())
                })
                .AsEnumerable()
                .OrderByDescending(t => t.AvgGoalsShotInTotal)
                .ToList();
        }

        public IEnumerable<TeamTableRowDto> GetTeamStandings()
        {
            return _dbContext
                .Teams
                .Select(t => new TeamTableRowDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Matches = t.HomeGames.Count + t.AwayGames.Count,
                    Won = t.HomeGames.Where(g => g.HomeGoals > g.GuestGoals).Count() + t.AwayGames.Where(g => g.GuestGoals > g.HomeGoals).Count(),
                    Lost = t.HomeGames.Where(g => g.HomeGoals < g.GuestGoals).Count() + t.AwayGames.Where(g => g.GuestGoals < g.HomeGoals).Count(),
                    GoalsFor = t.HomeGames.Sum(g => g.HomeGoals) + t.AwayGames.Sum(g => g.GuestGoals),
                    GoalsAgainst = t.HomeGames.Sum(g => g.GuestGoals) + t.AwayGames.Sum(g => g.HomeGoals),
                })
                .AsEnumerable()
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalDifference)
                .ToList();
        }
    }
}