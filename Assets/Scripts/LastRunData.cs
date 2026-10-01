public static class LastRunData
{
    public static bool HasRunData { get; private set; } = false;
    public static int Seed { get; private set; }
    public static int RoomsFled { get; private set; }
    public static int SkullTally { get; private set; }
    public static int GoblinTally { get; private set; }
    public static int HeartsTally { get; private set; }
    public static int ShieldsTally { get; private set; }
    public static int ShieldsBroken { get; private set; }
    public static float TimeSurvived { get; private set; }
    public static string CauseOfDeath { get; private set; }

    // Save stats at the end of a run
    public static void SaveRun(int runSeed, int roomSkip, int skulls, int goblins, int hearts, int shields, int shieldsBrokenCount, float timeSurvived, bool runWon, string causeOfDeath)
    {
        Seed = runSeed;
        RoomsFled = roomSkip;
        SkullTally = skulls;
        GoblinTally = goblins;
        HeartsTally = hearts;
        ShieldsTally = shields;
        ShieldsBroken = shieldsBrokenCount;
        TimeSurvived = timeSurvived;
        
        CauseOfDeath = runWon ? string.Empty : causeOfDeath;
        HasRunData = true;
    }

    // Reset when starting a new run
    public static void ResetData()
    {
        HasRunData = false;
        Seed = 1337;
        RoomsFled = 0;
        SkullTally = 0;
        GoblinTally = 0;
        HeartsTally = 0;
        ShieldsTally = 0;
        ShieldsBroken = 0;
        TimeSurvived = 0f;
        CauseOfDeath = string.Empty;
    }
}