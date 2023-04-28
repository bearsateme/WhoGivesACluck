﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NotificationBot.Commands;
using NotificationBot.Models.Enums;

namespace NotificationBot.Services
{
    public class BotService : IHostedService
    {  
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly DiscordShardedClient _shards;
        
        public BotService(IConfiguration configuration,  IHostApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime;
            _shards = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = configuration["Bot_Key"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
            });
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var commands = await _shards.UseCommandsNextAsync(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "!" },
            });
            
            commands.RegisterCommands<FetchCommandModule>();

            var generalChannels = new List<ulong>();
            foreach (var shardClient in _shards.ShardClients)
            {
                var discord = shardClient.Value;

                foreach (var guild in discord.Guilds)
                {
                    var channels = guild.Value.Channels;

                    foreach (var channel in channels)
                    {
                        if (channel.Value.Name == "general")
                        {
                            generalChannels.Add(channel.Key);
                        }
                    }
                }

                await discord.ConnectAsync();
            }

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                
                var result = await Utility.GetResult($"https://statsapi.web.nhl.com/api/v1/schedule?teamId=12&date={DateTime.Today:yyyy-MM-dd}");

                if (result.Status is GameStatus.Away or GameStatus.None)
                {
                    await Task.Delay(86400000, cancellationToken);
                    continue;
                }

                if (result.Status != GameStatus.Finished)
                {
                    await Task.Delay(300000, cancellationToken);
                    continue;
                }

                if (result.HomeScore > result.AwayScore)
                {
                    foreach (var channel in generalChannels)
                    {
                        foreach (var shardClient in _shards.ShardClients)
                        {
                            var generalChannel = await shardClient.Value.GetChannelAsync(channel);
                            await generalChannel.SendMessageAsync("get your shit");    
                        }
                    }
                    await Task.Delay(86400000, cancellationToken);
                    continue;
                }

                foreach (var channel in generalChannels)
                {
                    foreach (var shardClient in _shards.ShardClients)
                    {
                        var generalChannel = await shardClient.Value.GetChannelAsync(channel);
                        await generalChannel.SendMessageAsync("get your shit");    
                    }
                }
                await Task.Delay(86400000, cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var shardClient in _shards.ShardClients)
            {
                await shardClient.Value.DisconnectAsync();
            }
        }
    }
}