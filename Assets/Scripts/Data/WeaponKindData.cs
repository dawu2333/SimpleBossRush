public enum WeaponKind{Common,Heavy,Scatter,Laser,Tracking}//武器类型,目前只实现了Common
public static class WeaponKindData{
    public static string ConvertToString(WeaponKind weaponKind) {//类型转换，难度枚举转字符串
        string kind=weaponKind switch {
            WeaponKind.Common=>"Common",
            WeaponKind.Heavy=>"Heavy",
            WeaponKind.Scatter=>"Scatter",
            WeaponKind.Laser=>"Laser",
            WeaponKind.Tracking=>"Tracking",
            _=>"Common"
        };
        return kind;
    }
    public static WeaponKind ConvertToWeaponKind(string weaponKind) {//类型转换，字符串转难度枚举
        WeaponKind kind = weaponKind switch {
            "Common"=>WeaponKind.Common,
            "Heavy"=>WeaponKind.Heavy,
            "Scatter"=>WeaponKind.Scatter,
            "Laser"=>WeaponKind.Laser,
            "Tracking"=>WeaponKind.Tracking,
            _=>WeaponKind.Common
        };
        return kind;
    }
}

    