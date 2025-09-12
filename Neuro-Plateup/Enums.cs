namespace Neuro_Plateup
{
    [System.Flags]
    public enum GameState
    {
        None = 0,
        Paused = 1 << 0,
        Franchise = 1 << 1,
        Night = 1 << 2,
        Day = 1 << 3,
        FranchiseBuilder = 1 << 4,
        GameOver = 1 << 5,
        Any = Paused | Franchise | Night | Day | FranchiseBuilder | GameOver
    }

    [System.Flags]
    public enum BotRole
    {
        None = 1, // is a valid role that needs evaluation
        Chef = 1 << 1,
        Waiter = 1 << 2,
        Dishwasher = 1 << 3,
        Any = Chef | Waiter | Dishwasher
    }

    [System.Flags]
    public enum GrabType
    {
        Undefined,
        Pickup,
        Drop,
        CombineDrop,
        Fill,
        Dispense
    }
}