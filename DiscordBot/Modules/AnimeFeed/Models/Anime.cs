﻿using DiscordBot.Helpers;
using System.Text;

namespace DiscordBot.Modules.AnimeFeed.Models;

public class Anime
{
    public string? Name { get; set; }
    public string? Episode { get; set; }
    public string? Url { get; set; }
    public int? Id { get; set; }
    public List<ulong>? Subscribers { get; set; }

    // name for Booru search, it sometimes differs.

    private string _booruName = string.Empty;
    public string? BooruName
    {
        get
        {
            if (string.IsNullOrEmpty(_booruName))
                return Name;
            else return _booruName;
        }
        set
        {
            _booruName = value ?? string.Empty;
        }
    }

    public List<string>? Notes { get; set; }

    public Anime()
    {
        Subscribers = [];
        Notes = [];
    }

    public bool Equals(Anime? other)
    {
        if (other == null) return false;

        return Name == other.Name;
    }

    public override string ToString()
    {
        return $"{Name} - {Episode}";
    }

    private string GetUserMentions()
    {
        if (Subscribers == null)
            return string.Empty;

        return String.Join(Environment.NewLine, Subscribers.Select(x => $"<@{x}>"));
    }

    public Embed GetEmbed()
    {
        string animeUrlTemplate = "https://gogocdn.net/cover/{0}.png";
        string gogoanimeUrl = "https://gogoanime3.co/";

        EmbedBuilder builder = new()
        {
            Title = $"Jest Nowy odcinek {Name}",
            ImageUrl = string.Format(animeUrlTemplate, Name!.GenerateSlug()),
            Color = 0x00FFFF,
            Description = GetUserMentions()
        };

        ActionRowBuilder actionRowBuilder = new ActionRowBuilder()
            .WithButton(label: "Nyaa", url: Url, style: ButtonStyle.Link)
            .WithButton(label: "Online", url: gogoanimeUrl, style: ButtonStyle.Link);

        ComponentBuilder componentBuilder = new ComponentBuilder().AddRow(actionRowBuilder);

        EmbedFieldBuilder fieldBuilder = new EmbedFieldBuilder()
            .WithName("Linki:")
            .WithValue(componentBuilder);

        builder.AddField(fieldBuilder);
        var embed = builder.Build();
        return embed;
    }

    public string GetUpdateMessage()
    {
        StringBuilder sb = new();
        if (Notes is not null && Notes.Count > 0)
        {
            foreach (var note in Notes.OrderBy(x => new Guid()).Take(3).ToList())
            {
                sb.AppendLine(note);
            }
        }
        else
        {
            sb.AppendLine($"Jest Nowy odcinek {Name}!");
        }
        sb.AppendLine(GetUserMentions());
        sb.AppendLine("");
        sb.AppendLine(Url);

        return sb.ToString();
    }
}
