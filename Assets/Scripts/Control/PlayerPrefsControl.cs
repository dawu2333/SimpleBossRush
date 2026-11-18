using UnityEngine;

public static class PlayerPrefsControl{
    //是否通关(1:true,2:false)
    public static bool IsPass() {return PlayerPrefs.GetInt("IsPass")==1;}
    public static void IsPass(bool isPass) {PlayerPrefs.SetInt("IsPass",isPass?1:0);}
}