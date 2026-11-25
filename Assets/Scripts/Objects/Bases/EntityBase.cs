using System.Collections;
using UnityEngine;

//攻击属性（单次/持续性/无）
public enum DamageKind{Instant,Continuous,None}

public abstract class EntityBase : MonoBehaviour{
    
    [SerializeField] protected float speed;
    [SerializeField] protected Transform hpBar;
    protected float hp,MaxHP;
    protected int level;
    protected Difficulty difficulty;
    protected bool canAffected = true, isDead = false,invincible=true;
    public abstract Faction EntityFaction{get;}//阵营
    protected abstract bool TryGetAimPosition(out Vector2 aim);//获取目标钩子
    protected abstract void OnDied(); //死亡钩子
    protected abstract void OnDamaged();//受伤钩子
    protected virtual void OnContinuousScatterFired(){}//连续散射开火钩子，可选重写
    protected virtual void OnContinuousScatterStarted(){}//连续散射状态进行钩子，可选
    protected virtual void OnContinuousScatterFinished(){}//连续散射状态结束钩子，可选
    protected virtual void OnLaserStarted(){}//激光状态进行钩子
    protected virtual void OnLaserFinished(){}//激光状态结束钩子
    protected Color originColor=Color.white; //保留原色,兜底白色  

    //受击相关
    //扣血
    protected void UpdateHp(float damage,bool feedback){
        if(damage<=0||invincible)return;
        //单次伤害显示受击，帧伤不走反馈直接扣血
        if(feedback){//过滤帧伤
            if(!canAffected)return;//处于受击状态直接返回，不扣血
            OnDamaged();
        }
        hp-=damage;
        if(hpBar==null){Debug.LogError("没找到血条预制体");}
        else {HpBarControl.UpdateHpBar(hpBar,hp,MaxHP);}
        if(hp<=0.0001f){Kill();}//float容差 
    }
    //识别攻击及其类型
    protected bool TryGetDamage(Collider2D collider,out float damage,out DamageKind kind) {
        if(collider.CompareTag("Bullet")||collider.CompareTag("Laser")){//是子弹或是激光
            if(collider.TryGetComponent(out Bullet bullet)&&bullet.Faction != EntityFaction) {//子弹
                kind=DamageKind.Instant;
                damage=bullet.Damage;
                return true;
            }else{ //激光
                Laser laser=collider.GetComponentInParent<Laser>();
                if (laser != null && laser.Faction != EntityFaction) {//!
                    kind=DamageKind.Continuous;
                    damage=laser.Damage;
                    return true;
                }
            }
        }
        damage=0;
        kind=DamageKind.None;
        return false;
    }
    
    //受击协程
    protected IEnumerator AffectedProcess(bool lockInvincible,Color flashColor,float flashSecond,int times){//是否有子弹无敌帧，闪烁速度/次数
        if(lockInvincible)canAffected=false;//有子弹无敌帧则开启无敌
        var sr=GetComponent<SpriteRenderer>();
        for(int i=0;i<times;i++){
            sr.color=flashColor;//是boss被打闪红，玩家被打闪烁
            yield return new WaitForSeconds(flashSecond);
            sr.color=originColor;
            yield return new WaitForSeconds(flashSecond);
        }
        if(lockInvincible)canAffected=true;
    }

    //开启受击，设计由Start中Invoke控制
    protected void EndInvincible(){invincible=false;}

    //瞬间死亡
    protected void Kill() {
        if(isDead||invincible)return;
        isDead=true;
        OnDied();
        Destroy(gameObject);
    }

    
    //攻击相关
    //获取攻击方位（四元数）
    protected Quaternion GetAttackQuaternion(float angleOffset=0 ,float baseAngle=0){
        if(!TryGetAimPosition(out Vector2 aim))return Quaternion.identity;
        Vector2 self = new(transform.position.x, transform.position.y);
        float angle=Vector3.SignedAngle(Vector3.up,(aim - self).normalized,Vector3.forward)
                    +angleOffset+baseAngle;
        return Quaternion.Euler(0,0,angle);
    }

    //自机狙,发射单个子弹
    protected void SingleBulletAttack(BulletConfig cfg){
        Bullet bullet=BulletPool.Instance.Get(cfg.shape,transform.position,GetAttackQuaternion());
        if(bullet==null){Debug.LogError($"{gameObject.tag}未获取到子弹");return;};
        bullet.Init(cfg,EntityFaction);
    }

    //单次散射子弹（扇形扩散）
    protected void ScatterBulletAttack(BulletConfig cfg, int bulletCount, float spreadAngle, float baseAngle){
        for (int i = 0; i < bulletCount; i++){
            float angleOffset = (i - bulletCount / 2.0f) * spreadAngle;
            Bullet bullet=BulletPool.Instance.Get(cfg.shape,transform.position,GetAttackQuaternion(angleOffset,baseAngle));
            if(bullet==null){Debug.LogError($"{gameObject.tag}未获取到子弹");return;};
            bullet.Init(cfg,EntityFaction);
        }
    }
    //持续散射子弹
    protected IEnumerator ContinuousScatterBulletAttack(BulletConfig cfg, int bulletCount, float spreadAngle, float interval, int shots){
        OnContinuousScatterStarted();//连续射击状态钩子
        for (int j = 0; j < shots; j++){
            OnContinuousScatterFired();
            float baseAngle = j * 8f % 360; // 每次射击角度变化8度
            ScatterBulletAttack(cfg, bulletCount, spreadAngle, baseAngle);
            yield return new WaitForSeconds(interval);
        }
        OnContinuousScatterFinished();
    }

    //激光攻击
    protected void LaserAttack(LaserConfig cfg,float preLaserDelay,float duration,  int laserCount=1, float spreadAngle=0, float baseAngle=0){
        for (int i = 0; i < laserCount; i++){
            OnLaserStarted();
            float angleOffset = (i - laserCount / 2.0f) * spreadAngle;
            StartCoroutine(LaserAttackProcess(cfg, duration,preLaserDelay,GetAttackQuaternion(angleOffset,baseAngle)));
        }
    }
    //激光攻击协程
    protected IEnumerator LaserAttackProcess(LaserConfig cfg,float duration,float preLaserDelay, Quaternion rotation){
        var pos=transform.position;
        for (int i = 0; i < 2; i++) {
            Laser laser = LaserPool.Instance.Get(LaserType.NormalLaser,pos,rotation,i==0);
            if(laser==null){Debug.LogError($"{gameObject.tag}未获取到激光");break;};
            laser.Init(cfg,EntityFaction,i==0);
            yield return new WaitForSeconds(i==0?preLaserDelay:duration);
            laser.Despawn();
        }
        OnLaserFinished();
    }
    
    protected void RigidBodyNeverSleep() {
        var rb = GetComponent<Rigidbody2D>();
        if(rb != null) rb.sleepMode = RigidbodySleepMode2D.NeverSleep;//唤醒刚体，防止吃不到伤害
    }

}
