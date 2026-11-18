using System.Collections;
using UnityEngine;

public class Player : EntityBase{
    [SerializeField] private GameObject bulletPrefabs;
    [SerializeField] private float attack_time;//攻击间隔
    private float current_time;//冷却时间
    private readonly float boundX = 17.5f,boundY = 9.5f;
    private Camera mainCamera;
    public override Faction EntityFaction => Faction.Player;
    void Awake() {
        mainCamera=Camera.main;
        var rb = GetComponent<Rigidbody2D>();
        if(rb != null) rb.sleepMode = RigidbodySleepMode2D.NeverSleep;//唤醒刚体，防止吃不到伤害
    }
    void Start(){
        Invoke(nameof(EndInvincible), 2);//开局无敌两秒
        InitiateData();
    }

    void Update(){
        Move();
        Attack();
    }
    
    void InitiateData(){
        //初始化基础数值
        if(speed==0)speed=10;
        if(attack_time==0)attack_time=0.2f;
        
        //关卡/难度/颜色
        level=BaseInfo.Level;
        difficulty=BaseInfo.Difficulty;
        var lv = LevelData.levels[level];
        originColor=GetComponent<SpriteRenderer>().color;

        //血量/攻速/大小
        hp = BaseInfo.IsDebug?BaseInfo.PlayerHP:lv.playerHP;
        MaxHP=hp;
        attack_time/=lv.playerAttackCoef;
        float scale=lv.playerScale;
        if(difficulty=="Hard"||difficulty=="Extreme")scale*=0.8f;
        transform.localScale=new Vector3(scale,scale,1);
    }

    
    //受击相关
    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.tag=="Boss"){Kill();return;}//碰到Boss直接死
        if(TryGetDamage(collision,out float damage,out DamageKind kind)&&kind==DamageKind.Instant){
            UpdateHp(damage,true);
        }
    }
    void OnTriggerStay2D(Collider2D collision){
        if(collision.tag=="Boss"){Kill();return;}
        if(TryGetDamage(collision,out float damage,out DamageKind kind)&&kind==DamageKind.Continuous){
            UpdateHp(damage*Time.deltaTime,false);
        }
    }

    protected override void OnDamaged(){
        StartCoroutine(AffectedProcess(false,0.15f,2));
        BaseInfo.Affected++;//受击数+1
    }

    protected override void OnDied(){
        GameControl.Instance?.PlayerDie();
    }


    //攻击相关
    protected override bool TryGetAimPosition(out Vector2 aim) {
        if(mainCamera==null)mainCamera=Camera.main;
        if(mainCamera==null){aim=default;return false;}//护栏
        aim=mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return true;
    }

    //攻击触发
    void Attack(){
        if(Time.timeScale==0) return;
        current_time+=Time.deltaTime;
        if(Input.GetMouseButton(0)&&current_time>=attack_time){
            Shoot();
            current_time=0;
        }
    }
    //射击
    void Shoot(){
        SingleBulletAttack(InitInstanceData.PlayerBulletConfig());
    }


    //移动相关
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

}
