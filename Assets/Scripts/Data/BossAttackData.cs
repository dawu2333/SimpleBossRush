using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public readonly struct AttackDef { 
    private readonly int maxShots; 
    private readonly float interval;
    private readonly bool isLaser;
    private readonly AudioClip audioClip; 
    public readonly int MaxShots=>maxShots;
    public readonly float Interval=>interval;
    public readonly bool IsLaser=>isLaser;
    public readonly AudioClip Sound=>audioClip;
    public AttackDef(int maxShots,float interval,bool isLaser=false,AudioClip sound=null) {//条件sound默认为Null,由钩子调用
        this.maxShots=maxShots;
        this.interval=interval;
        this.isLaser=isLaser;
        audioClip=sound;
    }
}

public static class BossAttackData {//Boss各攻击方式的开火次数及间隔的存储，与InitInstanceData共同组成Boss的完整攻击流程
    public static int[] bossAttackList;//攻击池
    public static int[] extraBossAttackList={1,2,3,4,5,6,8,9,10};//level 5 的额外攻击池：故意不含7，7与激光风车(10) 同时在场时弹幕过密，实测几乎无解
    public static int[] extremeAttackList={1,2,3};//极限难度Boss的攻击池

    //攻击方式分类，未来重构用
    public readonly static int[] singleBulletAttackList={1,2,3,6};
    public readonly static int[] scatterBulletAttackList={4,5};
    public readonly static int[] singleLaserAttackList={8,9};
    public readonly static int[] ContinuousScatterBulletAttackList={7};

    public static IReadOnlyDictionary<string,AudioClip> bossBulletAttackAudios=new Dictionary<string, AudioClip>(){
        {"singleBulletAttack1",Resources.Load<AudioClip>("AudioClip/rifleGunShot1")},
         {"singleBulletAttack2",Resources.Load<AudioClip>("AudioClip/283")},
         {"singleBulletAttack3",Resources.Load<AudioClip>("AudioClip/510")},
         {"scatterBulletAttack1",Resources.Load<AudioClip>("AudioClip/406")},
         {"scatterBulletAttack2",Resources.Load<AudioClip>("AudioClip/gunShot2")},
         {"singleBulletAttack6",Resources.Load<AudioClip>("AudioClip/rifleGunShot2")},
         {"ContinuousScatterBulletAttack1",Resources.Load<AudioClip>("AudioClip/gunShot1")}
    };
    public static IReadOnlyDictionary<string,AudioClip> bossLaserAttackAudios=new Dictionary<string, AudioClip>() {
         {"singleLaserAttack1",Resources.Load<AudioClip>("AudioClip/railgunShot2")},
         {"singleLaserAttack2",Resources.Load<AudioClip>("AudioClip/railgunShot1")}
    };

    private static Dictionary<int,AttackDef> bossAttacks =new Dictionary<int, AttackDef>(){
        {1,new AttackDef(10,.1f,bossBulletAttackAudios.GetValueOrDefault("singleBulletAttack1"))},
        {2,new AttackDef(0,.5f,bossBulletAttackAudios.GetValueOrDefault("singleBulletAttack2"))},
        {3,new AttackDef(0,2f,bossBulletAttackAudios.GetValueOrDefault("singleBulletAttack3"))},
        {4,new AttackDef(0,2.5f,bossBulletAttackAudios.GetValueOrDefault("scatterBulletAttack1"))},
        {5,new AttackDef(0,3f,bossBulletAttackAudios.GetValueOrDefault("scatterBulletAttack2"))},
        {6,new AttackDef(50,.1f,bossBulletAttackAudios.GetValueOrDefault("singleBulletAttack6"))},
        {7,new AttackDef(1,6)},//协程均为一轮，不然弹幕密度会失控
        {8,new AttackDef(0,1,true,bossLaserAttackAudios.GetValueOrDefault("singleLaserAttack1"))},
        {9,new AttackDef(20,.15f,true,bossLaserAttackAudios.GetValueOrDefault("singleLaserAttack2"))},
        {10,new AttackDef(1,2f,true)}};//同上

    //预缓存
    private static List<AudioClip> bulletAudioClips=bossBulletAttackAudios.Values.ToList();
    private static List<AudioClip> laserAudioClips=bossLaserAttackAudios.Values.ToList();

    //获取一般攻击
    public static AttackDef GetBossAttacks(int i){
        if(bossAttacks.TryGetValue(i, out var d)) return d;
        Debug.LogError($"攻击 id {i} 不在表中");
        return bossAttacks[1];//返回默认值1
    }
    //随机攻击
    public static AttackDef BossRandomAttacks(int i,int maxShots=1,float interval=1) {
        return i switch {
            1 => new AttackDef(Random.Range(5, 10), Random.Range(.1f, .8f),sound:GetRandomBulletSound()),
            2 => new AttackDef(maxShots, interval),//协程，间隙由上层控制
            3 => new AttackDef(maxShots, interval,true,GetRandomLaserSound()),//激光协程
            _ => new AttackDef(Random.Range(5, 10), Random.Range(.1f, .8f),sound:GetRandomBulletSound())//默认子弹音效
        };
    }

    //返回一个随机音效
    private static AudioClip GetRandomBulletSound() {
        var i=Random.Range(0,bulletAudioClips.Count);
        return bulletAudioClips[i];
    }
    private static AudioClip GetRandomLaserSound() {
        var i=Random.Range(0,laserAudioClips.Count);
        return laserAudioClips[i];
    }

}
