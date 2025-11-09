using Microsoft.AspNetCore.Http;
using ProjectManager.Models;

namespace ProjectManager;

public static class SessionManager
{
    public static List<Session> Sessions = [];

    public static void AddNewSession(User user) 
    {
        Sessions.Add(new Session(user));
    }

    public static Session ReturnNewSession(User user) 
    {
        var newSession = new Session(user);
        Sessions.Add(newSession);

        return newSession;
    }

    public static Session? GetSession(string sessionId)
    {
        return Sessions.FirstOrDefault(s => s.Id == sessionId);
    }

    public static Session? GetSession(Guid userId)
    {
        return Sessions.FirstOrDefault(u => u.UserID == userId);
    }
}
