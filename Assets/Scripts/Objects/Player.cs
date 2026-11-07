using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour{
    [SerializeField] private GameObject bulletPrefabs;
    [SerializeField] private float speed;
    [SerializeField] private float attack_time;//攻击间隔
    [SerializeField] private Transform hpBar;
    private int hp;
    private float current_time;//冷却时间
    private readonly float boundX = 17.5f,boundY = 9.5f;
    private int MaxHP;
    private bool canAffected=false,isDead=false;
    private Camera mainCamera;
    void Awake() {
        mainCamera=Camera.main;
    }
    void Start(){
        Invoke(nameof(StartGameCanAffected), 2);
        InitiateData();
    }

    void Update(){
        Move();
        Attack();
    }
    void Attack(){
        if(Time.timeScale==0) return;
        current_time+=Time.deltaTime;
        if(Input.GetMouseButton(0)&&current_time>=attack_time){
            Shoot();
            current_time=0;
        }
    }
    
    void InitiateData(){
        var lv = LevelData.levels[BaseInfo.Level];
        hp = BaseInfo.IsDebug?BaseInfo.PlayerHP:lv.playerHP;
        MaxHP=hp;
        attack_time/=lv.playerAttackCoef;
        float scale=lv.playerScale;
        if(BaseInfo.Difficulty=="Hard"||BaseInfo.Difficulty=="Extreme")scale*=0.8f;
        transform.localScale=new Vector3(scale,scale,1);
    }

    void Move(){
        float dx=0,dy=0;
        if(Input.GetKey(KeyCode.W)){dy+=speed*Time.deltaTime;}
        if(Input.GetKey(KeyCode.S)){dy-=speed*Time.deltaTime;}
        if(Input.GetKey(KeyCode.A)){dx-=speed*Time.deltaTime;}
        if(Input.GetKey(KeyCode.D)){dx+=speed*Time.deltaTime;}
        transform.position=new Vector3(
            Mathf.Clamp(transform.position.x+dx,-boundX,boundX),
            Mathf.Clamp(transform.position.y+dy,-boundY,boundY),
            transform.position.z
        );
    }
    void Shoot(){
        Bullet bullet=BulletPool.Instance.Get(InitInstanceData.PlayerBulletConfig().shape,transform.position,GetAttackQuaternion());
        bullet.Init(InitInstanceData.PlayerBulletConfig(),Faction.Player);
    }
    void OnTriggerEnter2D(Collider2D other) {
        if(!canAffected)return;
        if(other.gameObject.tag=="Boss"){PlayerDie();return;}
        if(other.TryGetComponent(out Bullet otherBullet) && otherBullet.Faction == Faction.Boss){
            UpdateHp(otherBullet.Damage);
        }else if(other.TryGetComponent(out Laser laser) && laser.Faction == Faction.Boss){//如果碰到激光，减少3点生命值
            UpdateHp(laser.Damage);
        }
    }
    void OnTriggerStay2D(Collider2D collision){
        if(collision.gameObject.tag=="Boss"&&canAffected){PlayerDie();}
    }
    void UpdateHp(int damage){
        StartCoroutine(AffectedProcess());
        BaseInfo.Affected++;
        hp-=damage;
        float percentage=Mathf.Clamp01((float)hp/MaxHP);
        float scaleXValue = 1.5f * percentage;
        float posXValue = -0.75f + 0.75f * percentage;
        hpBar.localPosition=new Vector3(posXValue,hpBar.localPosition.y,hpBar.localPosition.z);
        hpBar.localScale=new Vector3(scaleXValue,hpBar.localScale.y,hpBar.localScale.z);
        if(hp<=0){PlayerDie();}
    }

    IEnumerator AffectedProcess(){
        canAffected=false;
        for(int i=0;i<2;i++){
            GetComponent<SpriteRenderer>().color = new Color32(202,49,108,0);
            yield return new WaitForSeconds(0.1f);
            GetComponent<SpriteRenderer>().color = new Color32(202,49,108,255);
            yield return new WaitForSeconds(0.1f);
        }
        canAffected=true;
    }

    void StartGameCanAffected(){canAffected=true;}
    void PlayerDie(){
        if(isDead) return;
        isDead=true;
        GameControl.Instance?.PlayerDie();
        Destroy(gameObject);
    }
    private Quaternion GetAttackQuaternion(){
        Vector3 mousePosition=Input.mousePosition;//鼠标的屏幕所在位置
        Vector2 mouseWorldPosition=mainCamera.ScreenToWorldPoint(mousePosition);//转换为世界坐标
        Vector2 gameObjectPosition=transform.position;//获取游戏物体的世界坐标
        float angle=Vector3.SignedAngle(Vector3.up,(mouseWorldPosition-gameObjectPosition).normalized,Vector3.forward);
        return Quaternion.Euler(0,0,angle);
    }

}
