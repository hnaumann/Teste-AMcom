using Questao2.Domains;
using Refit;

namespace Questao2.Services;

public interface IScoredGoals
{
    [Get("/api/football_matches?year={year}&{homeAway}={team}&page={page}")]
    public Task<ScoreGoalsTeamResponse> GetScoreGoalsTeam(int year, string homeAway, string team, int page);
}