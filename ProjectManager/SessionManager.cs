using Microsoft.AspNetCore.Http;
using ProjectManager.Models;

namespace ProjectManager;

public static class SessionManager
{
    public static List<Session> Sessions = [];

    public static void AddNewSession(Guid userId) // TODO add parameter currentProject = null by default
    {
        Sessions.Add(new Session(userId));
    }

    public static Session ReturnNewSession(Guid userId) // TODO add parameter currentProject = null by default
    {
        var newSession = new Session(userId);
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
