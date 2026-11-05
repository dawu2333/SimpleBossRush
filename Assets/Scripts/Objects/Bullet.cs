using UnityEngine;

//子弹类型：圆形子弹/矩形子弹/碎片子弹/爆炸子弹
public enum BulletType{CircleBullet,SquareBullet,FragmentedBullet,ExplosiveBullet};
//实现：普通子弹
//未实现：碎片子弹、爆炸子弹

public enum Faction{ Player, Boss }//子弹阵营
public enum ObstacleMode{None,SingleUse,Persistent}

public class Bullet : MonoBehaviour{
    private BulletConfig config;
    private Faction faction;
    private Vector3 baseScale;

    public int Damage=>config.damage;
    public BulletType Type=>config.type;
    public Faction Faction=>faction;
    void Awake(){
        baseScale=transform.localScale;
    }

    void Update(){transform.Translate(0,config.speed*Time.deltaTime,0);}//子弹飞行

    
    void OnTriggerEnter2D(Collider2D other){//只负责撞到玩家/boss/bullet后的判断
        if(!other.TryGetComponent<Bullet>(out Bullet otherBullet))return;
        if(otherBullet.Faction==faction)return;
        ObstacleFunction(otherBullet);
    }
    public void Init(BulletConfig cfg,Faction faction){
        config=cfg;
        this.faction=faction;
        transform.localScale=baseScale*cfg.size;
        GetComponent<SpriteRenderer>().color=config.color;
    }

        //阻挡类子弹逻辑
    void ObstacleFunction(Bullet other){
        if(config.mode==ObstacleMode.None)return;
        Destroy(other.gameObject);
        if(config.mode==ObstacleMode.SingleUse){Destroy(gameObject);}
    }

}


