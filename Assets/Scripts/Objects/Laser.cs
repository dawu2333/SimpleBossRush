using UnityEngine;
public enum LaserType{NormalLaser,RotateLaser};
public class Laser : MonoBehaviour{
    private LaserConfig config;
    private Faction faction;

    public int Damage=>config.damage;
    public LaserType Type=>config.type;
    public Faction Faction=>faction;
    void Start(){
        gameObject.transform.localScale=new Vector3(config.width!=0?config.width:1,transform.localScale.y,transform.localScale.z);
        gameObject.transform.localPosition=new Vector3(transform.localPosition.x,transform.localPosition.y,-1);
    }
    void Update(){
        if(Type==LaserType.RotateLaser){Rotate();}
    }
    public void Init(LaserConfig cfg,Faction faction,bool isPreLaser){
        if(isPreLaser)cfg.damage=0;
        config=cfg;
        this.faction=faction;
        transform.localScale=new Vector3(cfg.width,transform.localScale.y,1);
    }
    void Rotate(){gameObject.transform.Rotate(new Vector3(0,0,config.rotateSpeed==0?-20*Time.deltaTime:config.rotateSpeed*Time.deltaTime));}
}
