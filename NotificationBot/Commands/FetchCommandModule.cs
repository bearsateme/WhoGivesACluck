﻿using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using NotificationBot.Models.Enums;

namespace NotificationBot.Commands
{
    public class FetchCommandModule : BaseCommandModule 
    {
        [Command("status")]
        public async Task FetchCommand(CommandContext context)
        {
            var result = Utility.GetResult($"https://statsapi.web.nhl.com/api/v1/schedule?teamId=12&date={DateTime.Today:yyyy-MM-dd}");
            var builder = new StringBuilder();
            
            switch (result.Result.Status)
            {
                case GameStatus.Away:
                    await context.RespondAsync("Hurricanes are playing away.");
                    break;
                case GameStatus.Invalid:
                case GameStatus.None:
                    await context.RespondAsync("Hurricanes are not playing today");
                    break;
                case GameStatus.Pending:
                    builder.AppendLine("The game has not yet finished");
                    builder.AppendLine($"Hurricanes: {result.Result.HomeScore}");
                    builder.AppendLine($"{result.Result.Opponent}: {result.Result.AwayScore}");
                    await context.RespondAsync(builder.ToString());
                    break;
                case GameStatus.Finished:
                    builder.AppendLine("The game has finished");
                    builder.AppendLine($"Hurricanes: {result.Result.HomeScore}");
                    builder.AppendLine($"{result.Result.Opponent}: {result.Result.AwayScore}");
                    if (result.Result.HomeScore > result.Result.AwayScore)
                    {
                        builder.AppendLine("Have you gotten your free sandwich?");
                    }
                    else
                    {
                        builder.AppendLine("No sandwiches unfortunately");
                    }
                    await context.RespondAsync(builder.ToString());
                    break;
            }
        }
    
    }
}