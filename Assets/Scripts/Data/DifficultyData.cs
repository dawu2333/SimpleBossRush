using System.Collections.Generic;
using System.Linq;

public enum Difficulty{Easy,Normal,Hard,Extreme,BossRush}//难度分类，BossRush暂未实现
public static class DifficultyData{
    //枚举转为列表
    private static List<Difficulty> difficulties = System.Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>().ToList();
    public static readonly Difficulty[] highDifficulties={Difficulty.Hard,Difficulty.Extreme,Difficulty.BossRush};//属于高难的难度列表
    public static string ConvertToString(Difficulty difficulty) {//类型转换，难度枚举转字符串
        string d=difficulty switch {
            Difficulty.Easy=>"Easy",
            Difficulty.Normal=>"Normal",
            Difficulty.Hard=>"Hard",
            Difficulty.Extreme=>"Extreme",
            Difficulty.BossRush=>"BossRush",
            _=>"Normal"
        };
        return d;
    }
    public static Difficulty ConvertToDifficulty(string difficulty) {//类型转换，字符串转难度枚举
        Difficulty d = difficulty switch {
            "Easy"=>Difficulty.Easy,
            "Normal"=>Difficulty.Normal,
            "Hard"=>Difficulty.Hard,
            "Extreme"=>Difficulty.Extreme,
            "BossRush"=>Difficulty.BossRush,
            _=>Difficulty.Normal
        };
        return d;
    }

    public static List<Difficulty> GetDifficulties() {
        List<Difficulty> d=new List<Difficulty>(difficulties);
        if(!PlayerPrefsControl.IsPassEx())d.Remove(Difficulty.BossRush);
        if(!PlayerPrefsControl.IsPass())d.Remove(Difficulty.Extreme);
        return d;
    }
}

    