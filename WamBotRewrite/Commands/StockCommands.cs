﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WamBotRewrite.Api;
using WamWooWam.Core;

namespace WamBotRewrite.Commands
{
    sealed class StockCommands : CommandCategory
    {
        private static Random _random = new Random();
        static HttpClient _client = new HttpClient();
        private static DateTime? _startupTime = null;

        internal StockCommands()
        {
            if (_startupTime == null)
            {
                _startupTime = DateTime.Now;
            }
        }

        public override string Name => "Stock";

        public override string Description => "The usual stuff most bots seem to have, including me.";

        [Command("Echo", "Echos the text you give it.", new[] { "echo", "say" })]
        public async Task Echo(CommandContext ctx, params string[] args)
        {
            await ctx.ReplyAsync(string.Join(" ", args));
        }

        [Command("Uptime", "How long have I been running for? This'll let you know.", new[] { "up", "uptime" })]
        public async Task Uptime(CommandContext ctx)
        {
            await ctx.ReplyAsync($"I've been running for {(DateTime.Now - _startupTime.Value).ToNaturalString()}!");
        }

        [Command("Ping", "What's my ping? This'll tell you.", new[] { "ping" })]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.ReplyAsync($"Hola! My ping's currently {Program.Client.Latency}ms!");
        }

        [Command("Dice", "Take a risk, roll the dice...", new[] { "roll" })]
        public async Task Dice(CommandContext ctx, string str)
        {
            string d = str;
            string[] splitd = d.Split('d');

            if (splitd.Length == 2 && int.TryParse(splitd[0], out int count) && int.TryParse(splitd[1], out int max))
            {
                if (count > 0 && max > 0)
                {
                    if (count <= 4096)
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append($"{ctx.Author.Username} rolled {d} and got: ");

                        for (int i = 0; i < count - 1; i++)
                        {
                            builder.Append($"{_random.Next(1, max + 1)}, ");
                        }

                        builder.Append(_random.Next(max));
                        builder.Append("!");
                        await ctx.ReplyAsync(builder.ToString());
                        return;
                    }
                    else
                    {
                        await ctx.ReplyAsync("Yeah more than 4096 dice is probably not a great idea let's be honest.");
                        return;
                    }
                }
                else
                {
                    await ctx.ReplyAsync("I can't generate a negative number of dice you twat.");
                    return;
                }
            }

            await ctx.ReplyAsync("Hey! Something's very wrong with your input! Try again.");
        }

        [Command("Pickup", "Grabs a random pickup line!", new[] { "pickup" }, ExtendedDescription =
            "WARNING, side effects may include being: punched in the face, slapped, killed, rejected and/or reported to the police.\n" +
            "Always read the label. If in doubt, please contact your GP.\n" +
            "Wan Kerr Co. Ltd. cannot be held responsible for any loss or damage to you and/or property as a result of using this command.")]
        public async Task Pickup(CommandContext ctx)
        {
            try
            {
                string str = await _client.GetStringAsync("http://pebble-pickup.herokuapp.com/tweets/random");
                JObject obj = JObject.Parse(str);
                JToken tweet = obj["tweet"];
                await ctx.ReplyAsync($"\"{tweet.ToObject<string>()}\"");
            }
            catch
            {
                await ctx.ReplyAsync("Oops! That didn't work! Sorry!");
            }
        }

        [Command("User Info", "Gives information about a user.", new[] { "user", "userinfo", "whodis" })]
        public async Task UserInfo(CommandContext ctx, IUser user = null)
        {
            if (user == null)
            {
                user = ctx.Message.Author;
            }

            IGuildUser memb = user as IGuildUser;
            EmbedBuilder builder = ctx.GetEmbedBuilder(user.Username)
                .WithThumbnailUrl(user.GetAvatarUrl());

            builder.AddField("Username", $"{user.Username}#{user.Discriminator}", memb != null);
            if (memb != null)
            {
                builder.AddField("Display Name", memb.Nickname ?? memb.Username, true);
            }

            builder.AddField("Id", $"{user.Id}", true);
            builder.AddField("Mention", $"\\{user.Mention}", true);

            builder.AddField("Joined Discord", user.CreatedAt.UtcDateTime.ToString(), memb != null);
            if (memb != null)
            {
                builder.AddField("Joined Server", memb.JoinedAt?.UtcDateTime.ToString(), true);
            }

            int guilds = ctx.Client.Guilds.AsParallel().Where(g => g.Users.Any(m => m.Id == user.Id)).Count();
            builder.AddField("Guilds with me", guilds.ToString());

            builder.AddField("Bot?", user.IsBot.ToString(), true);
            builder.AddField("Current?", user.IsCurrent().ToString(), true);

            if (memb != null)
            {
                builder.AddField("Muted?", memb.IsMuted.ToString(), true);
                builder.AddField("Deafened?", memb.IsDeafened.ToString(), true);
                builder.AddField("In voice?", (memb.VoiceChannel != null).ToString(), true);

                if (memb.VoiceChannel != null)
                    builder.AddField("Voice Session Id", memb.VoiceSessionId, true);

                builder.AddField("Roles", memb.RoleIds.Any() ? string.Join(", ", memb.RoleIds.Select(r => ctx.Guild.GetRole(r).Mention)) : "None");
            }

            await ctx.Channel.SendMessageAsync("", false, builder.Build());
        }

        [RequiresGuild]
        [Command("Server Info", "Gives information about a server.", new[] { "server", "serverinfo", "guild" })]
        public async Task GuildInfo(CommandContext ctx, IGuild guild = null)
        {
            if (guild == null)
            {
                guild = ctx.Guild;
            }

            EmbedBuilder builder = ctx.GetEmbedBuilder(guild.Name)
                .WithThumbnailUrl(guild.IconUrl);

            builder.AddField("Id", guild.Id.ToString(), true);
            builder.AddField("Name", guild.Name, true);
            builder.AddField("Created At", guild.CreatedAt.ToString());
            builder.AddField("Available?", guild.Available.ToString(), true);

            if (guild.Available)
            {
                builder.AddField("AFK Channel", $"<#{guild.AFKChannelId}>", true);
                builder.AddField("AFK Timeout", guild.AFKTimeout != 0 ? TimeSpan.FromSeconds(guild.AFKTimeout).ToString() : "Not Set", true);
                builder.AddField("Default Channel", $"<#{guild.DefaultChannelId}>", true);
                builder.AddField("Default Message Notifications", guild.DefaultMessageNotifications == DefaultMessageNotifications.AllMessages ? "NO. NONONONONO. BAD. REEEEEE." : ":ok_hand:", true);
                builder.AddField("Owner", $"<@{guild.OwnerId}>", true);

                if (guild.Roles.Any())
                {
                    bool hasNico = (await guild.GetUserAsync(288349459296026624)) != null;

                    IEnumerable<string> roles =
                        guild.Roles
                        .OrderByDescending(r => r.Position)
                        .Where(r => !r.Name.StartsWith("nico_") || !hasNico) // thx flash bb
                        .Select(r => r.Mention);

                    builder.AddField("Roles", string.Join(" ", roles).Truncate(1024));
                }

                if (guild.Emotes.Any())
                    builder.AddField("Emotes", string.Join(" ", guild.Emotes.Select(g => g.ToString())));

                if (guild.SplashUrl != null)
                    builder.WithImageUrl(guild.SplashUrl);
            }

            await ctx.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("Stats", "Mildly uninteresting info and data about my current state.", new[] { "stats", "info", "about" })]
        public async Task BotStats(CommandContext ctx)
        {
            Process process = Process.GetCurrentProcess();
            AssemblyName mainAssembly = Assembly.GetExecutingAssembly().GetName();

            EmbedBuilder builder = ctx.GetEmbedBuilder("Statistics");
            builder.AddField("Operating System", RuntimeInformation.OSDescription, true);
            builder.AddField("RAM Usage (Current)", Files.SizeSuffix(process.PrivateMemorySize64), true);
            builder.AddField("RAM Usage (Peak)", Files.SizeSuffix(process.PeakWorkingSet64), true);

            builder.AddField("Ping", $"{ctx.Client.Latency}ms", true);
            builder.AddField("Version", $"{mainAssembly.Version}", true);
            builder.AddField("Compiled at", new DateTime(2000, 1, 1).AddDays(mainAssembly.Version.Build).AddSeconds(mainAssembly.Version.MinorRevision * 2).ToString(CultureInfo.CurrentCulture), true);

            builder.AddField("Guilds", ctx.Client.Guilds.Count.ToString(), true);
            builder.AddField("Total Channels", ctx.Client.PrivateChannels
                .Cast<IChannel>()
                .Union(ctx.Client.Guilds.SelectMany(g => g.Channels))
                .Count().ToString(), true);

            builder.AddField("Total Roles", ctx.Client.Guilds.SelectMany(g => g.Roles).Count().ToString(), true);
            builder.AddField("Total Emotes", ctx.Client.Guilds.SelectMany(g => g.Emotes).Count().ToString(), true);
            builder.AddField("Total Members", ctx.Client.Guilds.Sum(g => g.MemberCount).ToString(), true);
            builder.AddField("Available Commands", Program.Commands.Count.ToString(), true);
            //builder.AddField("Available Parse Extensions", Program.ParseExtensions.Count.ToString(), true);

            await ctx.Channel.SendMessageAsync("", false, builder.Build());
        }

        [OwnerOnly]
        [Command("Exit", "Tells me to go away. :(", new[] { "exit", "kys" })]
        public async Task Exit(CommandContext ctx)
        {
            try
            {
                var message = await ctx.ReplyAndAwaitResponseAsync("Are you sure you want me to exit? Y/N");
                if (message.Content.ToLowerInvariant() == "y")
                {
                    await ctx.ReplyAsync("Turrah!");
                    await ctx.Client.StopAsync();
                    Environment.Exit(0);
                }
            }
            catch { }

            await ctx.ReplyAsync("Exit aborted.");
        }

        [Command("Help", "Well, it's help innit.", new[] { "help", "?" })]
        public async Task Help(CommandContext ctx, string name = null)
        {
            EmbedBuilder builder = null;
            if (name != null)
            {
                var commands = Program.Commands.Where(c => c.Aliases.Contains(name.ToLowerInvariant()));
                foreach (var command in commands)
                {
                    if (await Tools.CheckPermissions(Program.Client, ctx.Author, (ISocketMessageChannel)ctx.Channel, command))
                    {
                        builder = ctx.GetEmbedBuilder(command.Name);
                        builder.AddField("Description", command.Description, true);

                        if (!string.IsNullOrWhiteSpace(command.ExtendedDescription))
                        {
                            builder.AddField("Extended Description", command.ExtendedDescription);
                        }

                        builder.AddField("Aliases", string.Join(", ", command.Aliases), true);
                        if (command.Usage != null)
                        {
                            builder.AddField("Usage", $"```cs\r\n{Program.Config.Prefix}{command.Aliases.First()} {command.Usage}\r\n```");
                        }
                    }
                }

                if (builder == null)
                {
                    var category = Program.CommandCategories.Where(g => g.Key.Name.ToLowerInvariant() == name.ToLowerInvariant()).FirstOrDefault();
                    if (category != null)
                    {
                        builder = ctx.GetEmbedBuilder(category.Key.Name)
                            .WithDescription(category.Key.Description);
                        foreach (var command in category)
                        {
                            if (await Tools.CheckPermissions(Program.Client, ctx.Author, (ISocketMessageChannel)ctx.Channel, command))
                            {
                                builder.AddField(command.Name, command.Description, true);
                            }
                        }
                    }
                }
            }
            else
            {
                builder = ctx.GetEmbedBuilder("Help")
                        .WithDescription("Showing all categories and commands, specify a command or category name for more details!");

                StringBuilder str = new StringBuilder();
                foreach (var cat in Program.CommandCategories)
                {
                    str.Clear();
                    str.AppendLine(cat.Key.Description);
                    str.Append("`");

                    bool first = true;
                    foreach (var command in cat)
                    {
                        if (!first)
                        {
                            str.Append(", ");
                        }
                        else
                        {
                            first = !first;
                        }

                        if (await Tools.CheckPermissions(Program.Client, ctx.Author, (ISocketMessageChannel)ctx.Channel, command))
                        {
                            str.Append(command.Aliases.First());
                        }
                    }

                    str.Append("`");

                    builder.AddField($"{cat.Key.Name} Commands", str.ToString());
                }
            }

            if (builder == null)
            {
                builder = ctx.GetEmbedBuilder("Help");
                builder.AddField("Command not found.", $"That command doesn't seem to exist, or you don't have permission to run it! " +
                    $"Run `{Program.Config.Prefix}help` for a list of all commands!");
            }

            builder
                .WithFooter($"Another quality product of Wan Kerr Co. Ltd. " +
                    $"My current prefix is {Program.Config.Prefix} (duh)", Program.Application.Owner.GetAvatarUrl())
                .WithCurrentTimestamp();
            await ctx.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}