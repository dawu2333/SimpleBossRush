public static class BaseInfo{
    public static int Level = 1;
    public static string Difficulty = "Normal";
    public static int PlayerHP = 10;
    public static int BossHP = 200;
    public static int Affected = 0;
    public static bool IsWin = false;
    public static bool IsDebug=false;
    public static void Reset(){
        Level = 1;
        Difficulty = "Normal";
        PlayerHP = 10;
        BossHP = 200;
        Affected = 0;
        IsWin = false;
        IsDebug=false;
    }
    
}