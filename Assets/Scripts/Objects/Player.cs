using System;
using UnityEngine;
using System.Linq;
using System.Collections;
public class Player : EntityBase{
    [SerializeField] private float attack_time;//攻击间隔
    private float current_time;//冷却时间
    private readonly float boundX = 17.5f,boundY = 9.5f;
    private Camera mainCamera;
    public override Faction EntityFaction => Faction.Player;
    void Awake() {
        mainCamera=Camera.main;
        RigidBodyNeverSleep();
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
        level=BaseInfo.level;
        difficulty=BaseInfo.difficulty;
        var lv = LevelData.levels[level];
        originColor=GetComponent<SpriteRenderer>().color;

        //血量/攻速/大小
        hp = BaseInfo.isDebug?BaseInfo.playerHP:lv.playerHP;
        MaxHP=hp;
        attack_time/=lv.playerAttackCoef;
        float scale=lv.playerScale;
        if(DifficultyData.highDifficulties.Contains(difficulty))scale*=0.8f;
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
        StartCoroutine(AffectedProcess(true,new Color32(202,49,108,0),0.075f,2));
        BaseInfo.affected++;//受击数+1
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
    
    //玩家激光实现
    protected void LaserAttack(LaserConfig cfg,float duration,int laserCount=1,float spreadAngle=0,float baseAngle=0){
        for (int i = 0; i < laserCount; i++){
            float angleOffset = (i - laserCount / 2.0f) * spreadAngle;
            StartCoroutine(LaserAttackProcess(cfg,duration,GetAttackQuaternion(angleOffset,baseAngle)));
        }
    }
    protected IEnumerator LaserAttackProcess(LaserConfig cfg,float duration,Quaternion rotation){
            Laser laser = LaserPool.Instance.Get(LaserType.NormalLaser,transform.position,rotation);
            if(laser==null){Debug.LogError($"{gameObject.tag}未获取到激光");yield break;};
            laser.Init(cfg,EntityFaction);
            yield return new WaitForSeconds(duration);
            laser.Despawn();
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
        switch (BaseInfo.weaponKind) {
            case WeaponKind.Common:SingleBulletAttack(InitInstanceData.PlayerBulletConfig(1));break;
            case WeaponKind.Heavy:SingleBulletAttack(InitInstanceData.PlayerBulletConfig(2));break;
            case WeaponKind.Scatter:ScatterBulletAttack(InitInstanceData.PlayerBulletConfig(3),8,7.5f,0);break;
            case WeaponKind.Laser:LaserAttack(InitInstanceData.BossLaserConfig(2),.4f,1,10,5f);break;//借用，目前有很大问题
            default:SingleBulletAttack(InitInstanceData.PlayerBulletConfig(1));break;//默认为普通子弹
        }
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
