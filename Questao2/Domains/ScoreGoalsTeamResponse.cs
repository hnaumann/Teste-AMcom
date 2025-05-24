using System.Text.RegularExpressions;

namespace Questao2.Domains;

public class ScoreGoalsTeamResponse
{
    public int page { get; set; }
    public int per_page { get; set; }
    public int total { get; set; }
    public int total_pages { get; set; }
    public List<Match> data { get; set; }
}

