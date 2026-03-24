using System.Collections.Generic;

public static class GameData
{
    public static List<string> collectedItems = new List<string>();
    public static int audioLogsHeard = 0;
    public static int currentChapter = 1;

    public static void Reset()
    {
        collectedItems.Clear();
        audioLogsHeard = 0;
        currentChapter = 1;
    }
}