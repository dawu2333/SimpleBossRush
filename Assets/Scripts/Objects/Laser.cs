using UnityEngine;
public enum LaserType{NormalLaser,RotateLaser};
public enum LaserVisual{NormalPre,Normal,RotatePre,Rotate}
public class Laser : MonoBehaviour{
    private LaserConfig config;
    private Faction faction;
    private bool isPre;
    private float lifeTime;
    public int Damage=>config.damage;
    public LaserType Type=>config.type;
    public Faction Faction=>faction;
    public bool Ispre=>isPre;
    void Start(){
    }
    void Update(){
        lifeTime+=Time.deltaTime;
        if(Type==LaserType.RotateLaser){Rotate();}
        if(lifeTime>=15f)Despawn();//超时回收

    }
    public void Init(LaserConfig cfg,Faction faction,bool isPre){
        lifeTime=0;
        if(isPre)cfg.damage=0;//先判断类型设置伤害，不然cfg覆盖后会被覆盖
        config=cfg;
        this.faction=faction;
        this.isPre=isPre;
        
        transform.localScale=new Vector3(cfg.width,transform.localScale.y,1);
        transform.localPosition=new Vector3(transform.localPosition.x,transform.localPosition.y,-1);
    }
     public void Despawn() {
        LaserPool.Instance?.ReCycle(this);
        return;
    }
    void Rotate(){gameObject.transform.Rotate(new Vector3(0,0,config.rotateSpeed==0?-20*Time.deltaTime:config.rotateSpeed*Time.deltaTime));}
}
