﻿namespace DiscordBot.Models;

public interface IMessageHandler
{
    public string Name { get; }
    public uint Priority { get; }

    public PingHandlerConfig Config { get; }

    public bool CheckCustomCondition(SocketMessage message);

    public Task HandlePing(SocketMessage message);
}
