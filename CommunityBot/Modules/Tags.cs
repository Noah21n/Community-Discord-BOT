﻿using Discord.Commands;
using System.Threading.Tasks;
using CommunityBot.Features.GlobalAccounts;
using Discord;
using CommunityBot.Entities;
using CommunityBot.Extensions;

namespace CommunityBot.Modules
{
    [Group("Tag"), Alias("ServerTag", "Tags", "T", "ServerTags")]
    [Summary("Permanently assing a message to a keyword (for this server) which " +
             "the bot will repeat if someone uses this command with that keyword.")]
    [RequireContext(ContextType.Guild)]
    public class ServerTags : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalGuildAccounts _globalGuildAccounts;

        public ServerTags(GlobalGuildAccounts globalGuildAccounts)
        {
            _globalGuildAccounts = globalGuildAccounts;
        }

        [Command(""), Priority(-1), Remarks("Let the bot send a message with the content of the named tag on the server")]
        public async Task ShowTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                await ReplyAsync("You need to use this with some more input...\n" +
                                 "Try the `help tag` command to get more information on how to use this command.");
                return;
            }
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var response = TagFunctions.GetTag(tagName, guildAcc);
            await ReplyAsync(response);
        }

        [Command("new"), Alias("add"), Remarks("Adds a new (not yet existing) tag to the server")]
        public async Task AddTag(string tagName, [Remainder] string tagContent)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var response = TagFunctions.AddTag(tagName, tagContent, _globalGuildAccounts, guildAcc);
            await ReplyAsync(response);
        }

        [Command("update"), Remarks("Updates the content of an existing tag of the server")]
        public async Task UpdateTag(string tagName, [Remainder] string tagContent)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var response = TagFunctions.UpdateTag(tagName, tagContent, _globalGuildAccounts, guildAcc);
            await ReplyAsync(response);
        }

        [Command("remove"), Remarks("Removes a tag off the server")]
        public async Task RemoveTag(string tagName)
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var response = TagFunctions.RemoveTag(tagName, _globalGuildAccounts, guildAcc);
            await ReplyAsync(response);
        }

        [Command("list"), Remarks("Show all tag on this server")]
        public async Task ListTags()
        {
            var guildAcc = _globalGuildAccounts.GetById(Context.Guild.Id);
            var emb = TagFunctions.BuildTagListEmbed(guildAcc);
            await ReplyAsync("", false, emb);
        }
    }

    [Group("PersonalTags"), Alias("PersonalTag", "PTags", "PTag", "PT")]
    [Summary("Permanently assing a message to a keyword (global for you) which " +
             "the bot will repeat if you use this command with that keyword.")]
    [RequireContext(ContextType.Guild)]
    public class PersonalTags : ModuleBase<MiunieCommandContext>
    {
        private readonly GlobalUserAccounts _globalUserAccounts;

        public PersonalTags(GlobalUserAccounts globalUserAccounts)
        {
            _globalUserAccounts = globalUserAccounts;
        }

        [Command(""), Priority(-1), Remarks("Lets the bot send a message with the content of your named tag")]
        public async Task ShowTag(string tagName = "")
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                await ReplyAsync("You need to use this with some more input...\n" +
                                 "Try the `help ptag` command to get more information on how to use this command.");
                return;
            }
            var userAcc = _globalUserAccounts.GetById(Context.User.Id);
            var response = TagFunctions.GetTag(tagName, userAcc);
            await ReplyAsync(response);
        }

        [Command("new"), Alias("add"), Remarks("Adds a new (not yet existing) tag to your collection")]
        public async Task AddTag(string tagName, [Remainder] string tagContent)
        {
            var userAcc = _globalUserAccounts.GetById(Context.User.Id);
            var response = TagFunctions.AddTag(tagName, tagContent, _globalUserAccounts, userAcc);
            await ReplyAsync(response);
        }

        [Command("update"), Remarks("Updates an existing tag of yours")]
        public async Task UpdateTag(string tagName, [Remainder] string tagContent)
        {
            var userAcc = _globalUserAccounts.GetById(Context.User.Id);
            var response = TagFunctions.UpdateTag(tagName, tagContent, _globalUserAccounts, userAcc);
            await ReplyAsync(response);
        }

        [Command("remove"), Remarks("Removes an existing tag of yours")]
        public async Task RemoveTag(string tagName)
        {
            var userAcc = _globalUserAccounts.GetById(Context.User.Id);
            var response = TagFunctions.RemoveTag(tagName, _globalUserAccounts, userAcc);
            await ReplyAsync(response);
        }

        [Command("list"), Remarks("Show all your tags")]
        public async Task ListTags()
        {
            var userAcc = _globalUserAccounts.GetById(Context.User.Id);
            var emb = TagFunctions.BuildTagListEmbed(userAcc);
            await ReplyAsync("", false, emb);
        }
    }


    internal static class TagFunctions
    {
        internal static string AddTag(string tagName, string tagContent, IGlobalAccounts accounts, IGlobalAccount account)
        {
            var response = "A tag with that name already exists!\n" +
                           "If you want to override it use `update <tagName> <tagContent>`";
            if (account.Tags.ContainsKey(tagName)) return response;
            account.Tags.Add(tagName, tagContent);
            accounts.SaveAccounts(account.Id);
            response = $"Successfully added tag `{tagName}`.";

            return response;
        }

        internal static Embed BuildTagListEmbed(IGlobalAccount account)
        {
            var tags = account.Tags;
            var embB = new EmbedBuilder().WithTitle("No tags set up yet... add some! =)");
            if (tags.Count > 0) embB.WithTitle("Here are all available tags:");

            foreach (var tag in tags)
            {
                embB.AddField(tag.Key, tag.Value, true);
            }

            return embB.Build();
        }

        internal static string GetTag(string tagName, IGlobalAccount account)
        {
            if (account.Tags.ContainsKey(tagName))
                return account.Tags[tagName];
            return "A tag with that name doesn't exists!";
        }

        internal static string RemoveTag(string tagName, IGlobalAccounts accounts, IGlobalAccount account)
        {
            if (!account.Tags.ContainsKey(tagName))
                return "You can't remove a tag that doesn't exist...";

            account.Tags.Remove(tagName);
            accounts.SaveAccounts(account.Id);

            return $"Successfully removed the tag {tagName}!";
        }

        internal static string UpdateTag(string tagName, string tagContent, IGlobalAccounts accounts, IGlobalAccount account)
        {
            if (!account.Tags.ContainsKey(tagName))
                return "You can't update a tag that doesn't exist...";

            account.Tags[tagName] = tagContent;
            accounts.SaveAccounts(account.Id);

            return $"Successfully updated the tag {tagName}!";
        }
    }
}
