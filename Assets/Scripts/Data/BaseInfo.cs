public static class BaseInfo{//每局游戏的变量
    public static int level = 1;
    public static Difficulty difficulty = Difficulty.Normal;
    public static int playerHP = 10;
    public static int bossHP = 200;
    public static int affected = 0;
    public static bool isWin = false;
    public static bool isDebug=false;
    public static WeaponKind weaponKind=WeaponKind.Common;
    public static void Reset(){
        level = 1;
        difficulty = Difficulty.Normal;
        playerHP = 10;
        bossHP = 200;
        affected = 0;
        isWin = false;
        isDebug=false;
        weaponKind=WeaponKind.Common;
    }
    
}