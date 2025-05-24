using Questao2.Services;
using Refit;

public class Program
{
    const string home = "team1";
    const string away = "team2";

    public static async Task Main()
    {
        string teamName = "Paris Saint-Germain";
        int year = 2013;
        int totalGoals = await getTotalScoredGoals(teamName, year);

        Console.WriteLine("Team "+ teamName +" scored "+ totalGoals.ToString() + " goals in "+ year);

        teamName = "Chelsea";
        year = 2014;
        totalGoals = await getTotalScoredGoals(teamName, year);

        Console.WriteLine("Team " + teamName + " scored " + totalGoals.ToString() + " goals in " + year);

        // Output expected:
        // Team Paris Saint - Germain scored 109 goals in 2013
        // Team Chelsea scored 92 goals in 2014
    }

    public static async Task<int> getTotalScoredGoals(string team, int year)
    {
        var client = RestService.For<IScoredGoals>("https://jsonmock.hackerrank.com");
        var page = 1;
        int gols = 0;

        gols += await GetScoreTeam(client, year, home, team, page);
        gols += await GetScoreTeam(client, year, away, team, page);

        return gols;
    }

    private static async Task<int> GetScoreTeam(IScoredGoals client, int year, string homeAway, string team, int page)
    {
        var responseApi = await client.GetScoreGoalsTeam(year, homeAway, team, page);
        var gols = 0;

        if (responseApi != null)
        {
            for (int i = page; i <= responseApi.total_pages; i++)
            {
                var respostaApiPaginaAtual = await client.GetScoreGoalsTeam(year, homeAway, team, i);

                if (homeAway == home)
                {
                    gols += respostaApiPaginaAtual.data.Sum(x => int.Parse(x.team1goals));
                }
                else
                {
                    gols += respostaApiPaginaAtual.data.Sum(x => int.Parse(x.team2goals));
                }
            }

            return gols;
        }

        return 0;
    }
}