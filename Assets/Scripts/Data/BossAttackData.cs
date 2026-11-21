using System.Collections.Generic;
using UnityEngine;
public struct AttackDef { 
    private int maxShots; 
    private float interval; 

    public readonly int MaxShots=>maxShots;
    public readonly float Interval=>interval;
    public AttackDef(int maxShots,float interval) {
        this.maxShots=maxShots;
        this.interval=interval;
    }
}

public static class BossAttackData {//Boss各攻击方式的开火次数及间隔的存储
    public static int[] bossAttackList;//攻击池
    public static int[] extraBossAttackList={1,2,3,4,5,6,8,9,10};//level 5 的额外攻击池：故意不含7，7与激光风车(10) 同时在场时弹幕过密，实测几乎无解
    public static int[] extremeAttackList={1,2,3};//极限难度Boss的攻击池
    public static Dictionary<int,AttackDef> BossAttacks = new Dictionary<int, AttackDef>(){
        {1,new AttackDef(10,.1f)},
        {2,new AttackDef(0,.5f)},
        {3,new AttackDef(0,2f)},
        {4,new AttackDef(0,2.5f)},
        {5,new AttackDef(0,3f)},
        {6,new AttackDef(50,.1f)},
        {7,new AttackDef(0,6)},
        {8,new AttackDef(0,1)},
        {9,new AttackDef(20,.15f)},
        {10,new AttackDef(1,2f)}
    };

    public static AttackDef BossRandomAttacks(int i) {
        return i switch {
            1 => new AttackDef(Random.Range(5, 10), Random.Range(.1f, .8f)),
            2 => new AttackDef(0, Random.Range(1.1f, 3.2f)),
            3 => new AttackDef(Random.Range(1, 8), Random.Range(.4f, 1f)),
            _ => new AttackDef(Random.Range(5, 10), Random.Range(.1f, .8f)),
        };
    }
        

}
