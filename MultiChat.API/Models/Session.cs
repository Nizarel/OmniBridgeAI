﻿//using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MultiChat.API.Models;


public record Session
{

    public string Id { get; set; }
    public string Type { get; set; }
    public string SessionId { get; set; }
    public int? Tokens { get; set; }
    public string Name { get; set; }

    [JsonIgnore]
    public List<Message> Messages { get; set; }

    public Session()
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Session);
        SessionId = this.Id;
        Tokens = 0;
        Name = "New Chat";
        Messages = new List<Message>();
    }

    public void AddMessage(Message message)
    {
        Messages.Add(message);
    }

    public void UpdateMessage(Message message)
    {
        var match = Messages.Single(m => m.Id == message.Id);
        var index = Messages.IndexOf(match);
        Messages[index] = message;
    }
}