using System;
using System.Collections.Generic;
using System.Linq;

public static class RoundRobin
{
    static public List<Tuple<int, int>> GetMatches(List<int> listTeam)
    {
        int numTeams = listTeam.Count;
        var resList = new List<Tuple<int, int>>();
        var resList2 = new List<Tuple<int, int>>();
        var numGames = (numTeams / 2) * (numTeams - 1);
        var numRounds = 0;
        var gamesPerRound = 0;
        if (numTeams%2 == 0)
        {
            numRounds = numTeams-1;
            gamesPerRound = numTeams / 2;
        }
        else
        {
            numRounds = numTeams;
            gamesPerRound = (numTeams+1) / 2;
        }

        var circlePosList = new List<int>();
        for (int j = 1; j <= gamesPerRound; j++)
        {
            circlePosList.Add(j);
        }
        for (int j = gamesPerRound*2; j > gamesPerRound; j--)
        {
            circlePosList.Add(j);
        }
        for (int i = 0; i < circlePosList.Count()/2; i++)
        {
            resList.Add(Tuple.Create(circlePosList[i], circlePosList[gamesPerRound+i]));
        }


        for (int i = 2; i <= numRounds; i++)
        {
            var lastNum = circlePosList[0];
            for (int j = 0; j < gamesPerRound-1; j++)
            {
                circlePosList[j] = circlePosList[j+1];
            }
            circlePosList[gamesPerRound - 1] = circlePosList[gamesPerRound * 2 - 1];

            for (int j = gamesPerRound * 2 - 1; j > gamesPerRound; j--)
            {
                circlePosList[j] = circlePosList[j - 1];
            }
            circlePosList[gamesPerRound + 1] = lastNum;
            for (int j = 0; j < circlePosList.Count() / 2; j++)
            {
                resList.Add(Tuple.Create(circlePosList[j], circlePosList[gamesPerRound + j]));
            }
        }
        var halfer = numRounds;
        if (numRounds % 2 == 1)
            halfer = numRounds + 1;
        for (int i = halfer / 2; i < numRounds; i++)
        {
            var index = i * gamesPerRound;
            resList[index] = Tuple.Create(resList[index].Item2, resList[index].Item1);
        }

        for (int i = 0; i < halfer / 2; i++)
        {
            for (int j = 0; j < gamesPerRound; j++)
            {
                var index = i * gamesPerRound + j;
                resList2.Add(resList[index]);
            }
            for (int j = 0; j < gamesPerRound; j++)
            {
                var index = (i + (halfer / 2)) * gamesPerRound + j;
                if(index < resList.Count())
                    resList2.Add(resList[index]);
            }
        }
        if(numTeams%2 == 1)
        {
            var resList3 = new List<Tuple<int, int>>();
            var misingTeamNum = listTeam.Count() + 1;
            foreach (var item in resList2)
            {
                if(item.Item1 != misingTeamNum && item.Item2 != misingTeamNum)
                {
                    resList3.Add(item);
                }
            }
            resList2 = resList3;
        }

        return resList2;
    }
}