using UnityEngine;

//子弹功能枚举：普通子弹/碎片子弹/爆炸子弹
public enum BulletBehavior{NormalBullet,FragmentedBullet,ExplosiveBullet};
//子弹形状枚举
public enum BulletShape{Round,Rect,Head,Arrow}
//实现：普通子弹
//未实现：碎片子弹、爆炸子弹

public enum Faction{ Player, Boss }//子弹阵营
public enum ObstacleMode{None,SingleUse,Persistent}

public class Bullet : MonoBehaviour{
    private BulletConfig config;
    private Faction faction;
    private Vector3 baseScale;
    private bool isDespawned=false;
    public int Damage=>config.damage;
    public BulletShape Shape=>config.shape;
    public Faction Faction=>faction;
    void Awake(){
        baseScale=transform.localScale;
    }

    void Update(){
        transform.Translate(0,config.speed*Time.deltaTime,0);//子弹飞行
        IsOutBound();
    }

    
    void OnTriggerEnter2D(Collider2D other){//只负责撞到玩家/boss/bullet后的判断
        if ((other.TryGetComponent<Boss>(out Boss _)&&faction==Faction.Player)||
            (other.TryGetComponent<Player>(out Player _)&&faction==Faction.Boss)
        ){//碰到非自身阵营的实体，直接回收
            Despawn();
            return;
        }
            
        if(other.TryGetComponent<Bullet>(out Bullet otherBullet) && otherBullet.Faction != faction) {
            ObstacleFunction(otherBullet);
        }

    }
    public void Init(BulletConfig cfg,Faction faction){
        isDespawned=false;
        config=cfg;
        this.faction=faction;
        transform.localScale=baseScale*cfg.size;
        GetComponent<SpriteRenderer>().color=config.color;
    }
    
    private void Despawn() {
        if (isDespawned) return;
        isDespawned=true;
        BulletPool.Instance?.ReCycle(this);
        return;
    }
    private void IsOutBound() {//出界判断
        if(Mathf.Abs(transform.position.x) > 20f || Mathf.Abs(transform.position.y) > 12f){
        Despawn();
    }
    }

        //阻挡类子弹逻辑
    private void ObstacleFunction(Bullet other){
        if(config.mode==ObstacleMode.None)return;
        other.Despawn();
        if(config.mode==ObstacleMode.SingleUse)Despawn();
    }

}


