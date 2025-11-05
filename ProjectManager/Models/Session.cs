namespace ProjectManager.Models;

public class Session
{
    public Guid Id { get; init; }

    public Guid UserID { get; init; }

    public Session(Guid userID)
    {
        this.UserID = userID;

        Id = Guid.NewGuid();
    }
}
