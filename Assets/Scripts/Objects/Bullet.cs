using UnityEngine;

public class Bullet : MonoBehaviour{
    private BulletConfig config;
    private Faction faction;
    private Vector3 baseScale;
    private bool isDespawned=false;
    public float Damage=>config.damage;
    public BulletShape Shape=>config.shape;
    public Faction Faction=>faction;
    public ObstacleMode ObstacleMode=>config.mode;
    void Awake(){
        baseScale=transform.localScale;
    }

    void Update(){
        transform.Translate(0,config.speed*Time.deltaTime,0);//子弹飞行
        IsOutBound();
    }

    
    void OnTriggerEnter2D(Collider2D other){//只负责撞到玩家/boss/bullet后的判断
        if (other.TryGetComponent(out EntityBase entity)&&entity.EntityFaction!=faction){//碰到非自身阵营的实体，直接回收
            Despawn();
            return;
        }
            
        if(other.TryGetComponent(out Bullet otherBullet) && otherBullet.Faction != faction) {
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


