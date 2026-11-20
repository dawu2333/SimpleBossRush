using UnityEngine;

public static class PlayerPrefsControl{
    //是否通关(1:true,2:false)

    //通关
    public static bool IsPass() {return PlayerPrefs.GetInt("IsPass")==1;}//返回是否通过
    public static void IsPass(bool isPass) {PlayerPrefs.SetInt("IsPass",isPass?1:0);}//设置是否通关

    //通关Extreme
    public static bool IsPassEx() {return PlayerPrefs.GetInt("IsPassEx")==1;}
    public static void IsPassEx(bool isPass) {PlayerPrefs.SetInt("IsPassEx",isPass?1:0);}

    //通关BossRush
    public static bool IsPassBS() {return PlayerPrefs.GetInt("IsPassBS")==1;}
    public static void IsPassBS(bool isPass) {PlayerPrefs.SetInt("IsPassBS",isPass?1:0);}
}