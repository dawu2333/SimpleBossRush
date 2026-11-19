using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss : EntityBase{
    private readonly Color[] colors={Color.red,Color.blue,Color.cyan,Color.black,Color.green,Color.yellow};
    private readonly int MaxMoveStep = 10;//单次最大步移次数
    private AudioSource audioSource;//播放元
    private int attackCount = 0;//攻击次数
    private int currentAttack;//当前攻击
    private int maxAttackCount = 3;//最大攻击次数
    private float attack_time,current_time,tempSpeed,frequency=0;//攻击间隔、冷却时间、临时速度、攻击频率权重
    private bool isLaser=false,isPinwheel=false;//是否处于激光发射/旋转激光状态中
    private int moveTimer,moveDir,lastAttack;//移动增量计数器、移动方向标识、上一次攻击序号
    private Transform playerTransform;//玩家位置
    private int[] BossAttackList;//攻击池
    private int[] BossAttackList2={1,2,3,4,5,6,8,9,10};//level 5 的额外攻击池：故意不含7，7与激光风车(10) 同时在场时弹幕过密，实测几乎无解
    private int[] ExtremeAttackList={1,2,3};//极限难度Boss的攻击池，
    public override Faction EntityFaction => Faction.Boss;//阵营为Boss

    void Awake(){
        audioSource=GetComponent<AudioSource>();
    }
    void Start(){
        InitiateData();
        EndInvincible();
        StartCoroutine(CountPlus());
    }

    void Update(){
        if (playerTransform == null) return;
        BossMoveAI();
        BossAttackAI();
        
        #if UNITY_EDITOR
        HandBossAttack();//测试手操攻击
        #endif
    }
    //数值初始化
    void InitiateData(){
        //初始化基础数值
        level=BaseInfo.Level;
        difficulty=BaseInfo.Difficulty;
        originColor=GetComponent<SpriteRenderer>().color;
        var lv = LevelData.levels[level];

        //初始获取玩家位置
        playerTransform=GameObject.FindWithTag("Player")?.transform;
        if(playerTransform==null){
            Debug.LogError("Player not found!");
            GameControl.Instance?.BossDie(level);
            Destroy(gameObject);return;
        }
        
        //难度控制Boss血量/体型/频率/速度/攻击模式等
        hp = BaseInfo.IsDebug?BaseInfo.BossHP:lv.bossHP;
        MaxHP=hp;
        float scale=lv.bossScale;
        if(difficulty=="Hard"||difficulty=="Extreme")scale*=0.8f;
        transform.localScale=new Vector3(scale,scale,1);
        frequency= difficulty switch {
            "Easy"=>1f,
            "Normal"=>0.7f,
            "Hard"=>0.5f,
            "Extreme"=>0.3f,
            _=>1f
        };
        BossAttackList=lv.bossAttackPattern;
        speed=lv.bossSpeed;

        //初始化速度/攻击间隔/移动增量/音量/boss颜色/移动方向/攻击方式等基础数值
        tempSpeed=speed;
        attack_time=1f;
        moveTimer=0;
        audioSource.volume=0.2f;//音量20%
        moveDir = Random.Range(1, 9);
        currentAttack=GetNewAttack();
    }


    //受击相关
    //死亡钩子实现
    protected override void OnDied() {
        GameControl.Instance?.BossDie(level);
    }
    //受伤钩子实现
    protected override void OnDamaged() {
        StartCoroutine(AffectedProcess(false,Color.red,0.025f,1));
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(TryGetDamage(collision,out float damage,out DamageKind kind)&&kind==DamageKind.Instant){
            UpdateHp(damage,true);
        }
    }
    void OnTriggerStay2D(Collider2D collision){
        if(TryGetDamage(collision,out float damage,out DamageKind kind)&&kind==DamageKind.Continuous){
            UpdateHp(damage*Time.deltaTime,false);//每帧伤害
        }
    }


    //攻击相关
    //获取攻击目标
    protected override bool TryGetAimPosition(out Vector2 aim) {
        if(playerTransform==null){aim=default;return false;}
        aim=playerTransform.position;
        return true;
    }

    //连续散射状态钩子实现
    protected override void OnContinuousScatterFired(int bulletCount) {//散射协程时条件播放
        speed=.8f;
        //条件：散射弹幕数为[20-40]
        if(bulletCount<=40&&bulletCount>=20){audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/gunShot1"));} 
    }

    //连续散射状态结束钩子实现
    protected override void OnContinuousScatterFinished() {ApplyAttackSpeed();}

    //攻击逻辑
    void BossAttackAI() {
        if(playerTransform==null)return;//找不到玩家不攻击
        current_time += Time.deltaTime;
        if (current_time >= attack_time && attackCount < maxAttackCount) {
            attack_time=(difficulty!="Extreme"?BossAttackChoice(currentAttack):RandomAttack(currentAttack))*frequency;
            if(level==5&&attackCount%2==0){//第五关每两次攻击后随机选择一次攻击方式
                BossAttackChoice(BossAttackList2[Random.Range(0, BossAttackList2.Length)]);
                attackCount++;
            }
            attackCount++; current_time = 0;
            ApplyAttackSpeed();
        } else if (current_time >= attack_time && attackCount >= maxAttackCount) {
            maxAttackCount = Random.Range(2, 4);
            currentAttack = GetNewAttack();
            attackCount = 0; current_time = level!=5?attack_time / 2:0;
        }
    }
    //取得下一个攻击
    int GetNewAttack() {
        if(BossAttackList.Length==1)return 0;
        int newAttack;
        do {
            newAttack = difficulty!="Extreme"?BossAttackList[Random.Range(0, BossAttackList.Length)]:ExtremeAttackList[Random.Range(0, ExtremeAttackList.Length)];
        } while (newAttack == lastAttack); // 确保新选择的攻击方式与上次不同
        lastAttack = newAttack; // 更新上一次的攻击方式
        return newAttack;
    }

    //攻击选择
    float BossAttackChoice(int i){
        switch(i){
            case 1: 
                isLaser = false; 
                SingleBulletAttack(InitInstanceData.BossBulletConfig(1));
                maxAttackCount=10;
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/rifleGunShot1")); 
                return .1f;//伤害、弹速均一般的短连发攻击
            case 2: 
                isLaser = false; 
                SingleBulletAttack(InitInstanceData.BossBulletConfig(2));
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/283")); 
                return .5f;//射速较快且弹速快，伤害一般的攻击
            case 3: 
                isLaser = false; 
                SingleBulletAttack(InitInstanceData.BossBulletConfig(3));
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/510")); 
                return 2f;//射速和弹速较慢，伤害较大的攻击
            case 4: 
                isLaser = false; 
                ScatterBulletAttack(InitInstanceData.BossBulletConfig(4),8,7.5f,0);
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/406")); 
                return 2.5f;//射速较慢，伤害一般，弹速些微慢的八发子弹扇形攻击
            case 5: 
                isLaser = false; 
                ScatterBulletAttack(InitInstanceData.BossBulletConfig(5),50,7.2f,0);
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/gunShot2")); 
                return 3f;//射速很慢，伤害一般，弹速较慢的单词全屏广域攻击
            case 6: 
                isLaser = false; SingleBulletAttack(InitInstanceData.BossBulletConfig(6));
                maxAttackCount=50;
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/rifleGunShot2")); 
                return .1f;//射速极快、弹速较快，伤害与弹速一般的自机连续攻击
            case 7: 
                isLaser = false; 
                StartCoroutine(ContinuousScatterBulletAttack(InitInstanceData.BossBulletConfig(7), level!=5?40:30, level!=5?9:12, 0.5f, 10)); 
                return 6f; // 射速很快，伤害一般，弹速较慢的持续全屏广域攻击(波粒)
            case 8: 
                isLaser = true; 
                LaserAttack(InitInstanceData.BossLaserConfig(1),.8f,.5f,8,15,1);
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/railgunShot2"));
                return 1f;//散射激光
            case 9: 
                isLaser = true; 
                LaserAttack(InitInstanceData.BossLaserConfig(2),.4f,.4f,1,10,5f);
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/railgunShot1"));
                maxAttackCount=20;
                return .15f;//快速的激光自机狙
            case 10: 
                isLaser = true; 
                StartCoroutine(RotationLaserAttack(InitInstanceData.BossLaserConfig(3),10,level!=5?5:4,level!=5?36:45,0));
                maxAttackCount=1;
                return 2f;//激光大风车
        }
        return 1f;
    }
    //随机攻击
    float RandomAttack(int i){
        switch(i){
            case 1: 
                isLaser = false; 
                SingleBulletAttack(new BulletConfig(
                    BulletBehavior.NormalBullet,
                    Random.Range(1,3),
                    Random.Range(25f,90f),
                    Random.Range(0,2)>0?ObstacleMode.None:
                    Random.Range(0,2)>0?ObstacleMode.SingleUse:ObstacleMode.Persistent,
                    Random.Range(2.5f,6),
                    colors[Random.Range(0,colors.Length)],
                    BulletShape.Round));
                maxAttackCount=Random.Range(5,10); 
                return Random.Range(0.1f,.8f);
            case 2: 
                isLaser = false; 
                StartCoroutine(ContinuousScatterBulletAttack(new BulletConfig(
                    BulletBehavior.NormalBullet,
                    Random.Range(1,3),
                    Random.Range(25f,55f),
                    Random.Range(0,2)>0?ObstacleMode.None:Random.Range(0,2)>0?
                    ObstacleMode.SingleUse:ObstacleMode.Persistent,
                    Random.Range(0.8f,2),colors[Random.Range(0,colors.Length)],
                    BulletShape.Round), 
                    Random.Range(20,40),
                    Random.Range(5,20), 
                    Random.Range(.5f,1f), 
                    Random.Range(3,7))); 
                return Random.Range(1.1f,3.2f);
            case 3: 
                isLaser = true; 
                LaserAttack(new LaserConfig(
                    LaserType.NormalLaser,
                    Random.Range(1,3),
                    Random.Range(.4f,.8f),
                    0),
                    Random.Range(.3f,.9f),
                    Random.Range(.4f,1f),
                    Random.Range(1,8),
                    Random.Range(8,14),5);
                maxAttackCount=Random.Range(1,8);
                return Random.Range(.4f,1f);
        }
        return 1f;
    }

    //激光大风车
    IEnumerator RotationLaserAttack(LaserConfig cfg,float duration, int laserCount, float spreadAngle, float baseAngle){
        isPinwheel=true;
        gameObject.transform.position=new Vector3(0,0,-1);//将Boss移动到屏幕中心，避免激光旋转时偏离屏幕
        Laser[] Lasers=new Laser[laserCount];

        moveDir=0;//方向归零
        for (int j = 0; j < 2; j++){
            for (int i = 0; i < laserCount; i++){
            float angleOffset = (i - laserCount / 2.0f) * spreadAngle;
            Laser laser=LaserPool.Instance.Get(LaserType.RotateLaser,j==0,transform.position,GetAttackQuaternion(angleOffset,baseAngle));
            laser.Init(cfg,EntityFaction,j==0);
            Lasers[i]=laser;
            }
            yield return new WaitForSeconds(j==0?1f:duration);
            foreach(Laser laser in Lasers){
                laser.Despawn();
            }
        }
        isPinwheel=false;
    }


    //移动相关
    //回调Boss速度
    void ApplyAttackSpeed(){
        if(isLaser) speed=0;
        else speed=tempSpeed;
    }

    //移动逻辑
    void BossMoveAI(){
        if(isPinwheel)return;//正在激光大风车携程，不移动
        if(moveTimer < MaxMoveStep){
            Move(moveDir);
        }else{
            moveDir = Random.Range(1, 9); // 方向范围从1到8
            moveTimer = 0;
        }
    }

    //移动执行,direction为方向(1-8)
    void Move(int direction){
        while(direction > 8) direction %= 8; // 确保方向在1到8之间
        if(transform.position.x <= GameControl.boundX && transform.position.x >= -GameControl.boundX && transform.position.y <= GameControl.boundY && transform.position.y >= -GameControl.boundY){
            switch(direction){
                case 1: transform.position += (Vector3.up + Vector3.right) * speed * Time.deltaTime / Mathf.Sqrt(2); break; // 右上
                case 2: transform.position += Vector3.up * speed * Time.deltaTime; break; // 上
                case 3: transform.position += (Vector3.up + Vector3.left) * speed * Time.deltaTime / Mathf.Sqrt(2); break; // 左上
                case 4: transform.position += Vector3.left * speed * Time.deltaTime; break; // 左
                case 5: transform.position += (Vector3.down + Vector3.left) * speed * Time.deltaTime / Mathf.Sqrt(2); break; // 左下
                case 6: transform.position += Vector3.down * speed * Time.deltaTime; break; // 下
                case 7: transform.position += (Vector3.down + Vector3.right) * speed * Time.deltaTime / Mathf.Sqrt(2); break; // 右下
                case 8: transform.position += Vector3.right * speed * Time.deltaTime; break; // 右
                case 0: break;//不动
            }
        } else {
            // 处理边界情况
            if(transform.position.x > GameControl.boundX){
                transform.position = new Vector3(GameControl.boundX - 0.01f, transform.position.y, transform.position.z);
                moveDir = 5; // 左下
            } else if(transform.position.x < -GameControl.boundX){
                transform.position = new Vector3(-GameControl.boundX + 0.01f, transform.position.y, transform.position.z);
                moveDir = 7; // 右下
            } else if(transform.position.y > GameControl.boundY){
                transform.position = new Vector3(transform.position.x, GameControl.boundY - 0.01f, transform.position.z);
                moveDir = 6; // 下
            } else if(transform.position.y < -GameControl.boundY){
                transform.position = new Vector3(transform.position.x, -GameControl.boundY + 0.01f, transform.position.z);
                moveDir = 2; // 上
            }
        }
    }

    //有延迟的无限循环：用作步数自增
    IEnumerator CountPlus(){
        while(true){
            moveTimer++;
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }


    //测试相关
    #if UNITY_EDITOR
        void DebugAttack(int i){
            BossAttackChoice(i);
            ApplyAttackSpeed();      
        }
    void HandBossAttack(){
        if(Input.GetKeyDown(KeyCode.Alpha1)){DebugAttack(1);}
        if(Input.GetKeyDown(KeyCode.Alpha2)){DebugAttack(2);}
        if(Input.GetKeyDown(KeyCode.Alpha3)){DebugAttack(3);}
        if(Input.GetKeyDown(KeyCode.Alpha4)){DebugAttack(4);}
        if(Input.GetKeyDown(KeyCode.Alpha5)){DebugAttack(5);}
        if(Input.GetKeyDown(KeyCode.Alpha6)){DebugAttack(6);}
        if(Input.GetKeyDown(KeyCode.Alpha7)){DebugAttack(7);}
        if(Input.GetKeyDown(KeyCode.Alpha8)){DebugAttack(8);}
        if(Input.GetKeyDown(KeyCode.Alpha9)){DebugAttack(9);}
        if(Input.GetKeyDown(KeyCode.Alpha0)){DebugAttack(10);}
    }
#endif

}
