public struct ChangePlayerModeEvent
{
    public PlayerMode Mode;

    public ChangePlayerModeEvent(PlayerMode mode)
    {
        Mode = mode;
    }
}

public enum PlayerMode
{
    Garden,
    Maze
}