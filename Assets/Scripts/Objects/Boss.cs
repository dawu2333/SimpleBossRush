using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss : EntityBase{
    private readonly int MaxMoveStep = 10;//单次最大步移次数
    private AudioSource audioSource;//播放元
    private int attackCount = 0;//攻击次数
    private int activeLasers=0,activeScatters=0;
    private int currentAttack;//当前攻击
    private int maxAttackCount = 3;//最大攻击次数
    private float interval,current_time,tempSpeed,frequency=0;//攻击间隔、冷却时间、临时速度、攻击频率权重
    private bool isPinwheel=false;//是否处于旋转激光状态中
    private int moveTimer,moveDir,lastAttack;//移动增量计数器、移动方向标识、上一次攻击序号
    private Transform playerTransform;//玩家位置
    public override Faction EntityFaction => Faction.Boss;//阵营为Boss

    void Awake(){
        audioSource=GetComponent<AudioSource>();
        RigidBodyNeverSleep();
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
        level=BaseInfo.level;
        difficulty=BaseInfo.difficulty;
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
        hp = BaseInfo.isDebug?BaseInfo.bossHP:lv.bossHP;
        MaxHP=hp;
        float scale=lv.bossScale;
        
        if(DifficultyData.highDifficulties.Contains(difficulty))scale*=0.8f;//难度为高难度时，大小*0.8
        transform.localScale=new Vector3(scale,scale,1);
        frequency= difficulty switch {//攻击频率由难度控制
            Difficulty.Easy=>1f,
            Difficulty.Normal=>0.7f,
            Difficulty.Hard=>0.5f,
            Difficulty.Extreme=>0.3f,
            Difficulty.BossRush=>0.5f,//BossRush为连战，当前值无意义
            _=>1f
        };
        BossAttackData.bossAttackList=lv.bossAttackPattern;
        speed=lv.bossSpeed;

        //初始化速度/攻击间隔/移动增量/音量/boss颜色/移动方向/攻击方式等基础数值
        tempSpeed=speed;
        interval=1f;
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
    protected override void OnContinuousScatterFired() {
        var clip=BossAttackData.bossBulletAttackAudios.GetValueOrDefault("ContinuousScatterBulletAttack1");
        if (clip!=null) audioSource.PlayOneShot(clip);
        else Debug.LogError($"{gameObject}的持续散射攻击无音效");
    }
    //连续散射状态钩子实现
    protected override void OnContinuousScatterStarted() {//散射协程时条件播放
        if(activeScatters>=0)activeScatters++;
        else {Debug.Log("开始散射时活动散射协程小于0");return;}
        RefreshSpeed();
    }
    //连续散射状态结束钩子实现
    protected override void OnContinuousScatterFinished() {
        if(activeScatters>0)activeScatters--;
        else {Debug.Log("结束散射时活动散射协程小于0");return;}
        RefreshSpeed();
    }

    //激光状态钩子实现
    protected override void OnLaserStarted() {
        activeLasers++;
        RefreshSpeed();
    }

    //激光状态结束钩子实现
    protected override void OnLaserFinished() {
        if(activeLasers>0)activeLasers--;
        RefreshSpeed();
    }

    //攻击逻辑
    void BossAttackAI() {
        if(playerTransform==null)return;//找不到玩家不攻击
        AttackDef def;
        current_time += Time.deltaTime;
        if (current_time >= interval && attackCount < maxAttackCount) {//累加时间>间隔且攻击次数<最大攻击次数时，攻击
            def=difficulty!=Difficulty.Extreme?
                BossAttackChoice(currentAttack):
                RandomAttack(currentAttack);
            if(def.Sound != null) audioSource.PlayOneShot(def.Sound);
            interval=def.IsLaser?def.Interval:def.Interval*frequency;//是激光就不*频率加快攻击
            if(def.MaxShots > 0) maxAttackCount = def.MaxShots;

            if(level==5&&attackCount%2==0){//额外攻击：第五关每两次攻击后随机选择一次攻击方式
                var tDef=BossAttackChoice(BossAttackData.extraBossAttackList[Random.Range(0, BossAttackData.extraBossAttackList.Length)]);
                if(tDef.Sound != null) audioSource.PlayOneShot(tDef.Sound);
                attackCount++;//额外攻击算作攻击次数，加压用
            }
            attackCount++; //攻击次数+1
            current_time = 0;//攻击累加时间清零
        } else if (current_time >= interval && attackCount >= maxAttackCount) {//累加时间>间隔且攻击次数>最大攻击次数时，换攻击   
            currentAttack = GetNewAttack();
            maxAttackCount = Random.Range(2, 4);
            attackCount = 0; current_time = level!=5?interval / 2:0;
        }
    }
    //取得下一个攻击
    int GetNewAttack() {
        if(BossAttackData.bossAttackList.Length==1)return BossAttackData.bossAttackList[0];
        int newAttack;
        do {
            newAttack = difficulty!=Difficulty.Extreme?
            BossAttackData.bossAttackList[Random.Range(0, BossAttackData.bossAttackList.Length)]:
            BossAttackData.extremeAttackList[Random.Range(0, BossAttackData.extremeAttackList.Length)];
        } while (newAttack == lastAttack); // 确保新选择的攻击方式与上次不同
        lastAttack = newAttack; // 更新上一次的攻击方式
        return newAttack;
    }

    //攻击选择，返回值为包含开火间隔和攻击次数上限的AttackDef
    AttackDef BossAttackChoice(int i){
        switch(i){
            case 1: 
                SingleBulletAttack(InitInstanceData.BossBulletConfig(1));
                return BossAttackData.GetBossAttacks(1);//所有属性均一般的普通短连射
            case 2: 
                SingleBulletAttack(InitInstanceData.BossBulletConfig(2));
                return BossAttackData.GetBossAttacks(2);//射速较快且弹速快，伤害一般的攻击
            case 3: 
                SingleBulletAttack(InitInstanceData.BossBulletConfig(3));
                return BossAttackData.GetBossAttacks(3);//射速和弹速较慢，伤害较大的攻击
            case 4: 
                ScatterBulletAttack(InitInstanceData.BossBulletConfig(4),8,7.5f,0);
                return BossAttackData.GetBossAttacks(4);//射速较慢，伤害一般，弹速些微慢的八发子弹扇形攻击
            case 5: 
                ScatterBulletAttack(InitInstanceData.BossBulletConfig(5),50,7.2f,0);
                return BossAttackData.GetBossAttacks(5);//射速很慢，伤害一般，弹速较慢的单词全屏广域攻击
            case 6: 
                SingleBulletAttack(InitInstanceData.BossBulletConfig(6));
                return BossAttackData.GetBossAttacks(6);//射速极快、弹速较快，伤害与弹速一般的自机连续攻击
            case 7: 
                StartCoroutine(ContinuousScatterBulletAttack(InitInstanceData.BossBulletConfig(7), level!=5?40:30, level!=5?9:12, 0.5f*frequency, 10)); 
                return BossAttackData.GetBossAttacks(7); // 射速很快，伤害一般，弹速较慢的持续全屏广域攻击(波粒)
            case 8: 
                LaserAttack(InitInstanceData.BossLaserConfig(1),.8f,.5f,8,15,1);
                return BossAttackData.GetBossAttacks(8);//散射激光
            case 9: 
                LaserAttack(InitInstanceData.BossLaserConfig(2),.4f,.4f,1,10,5f);
                return BossAttackData.GetBossAttacks(9);//快速的激光自机狙
            case 10: 
                StartCoroutine(RotationLaserAttack(InitInstanceData.BossLaserConfig(3),10,level!=5?5:4,level!=5?36:45,0));
                return BossAttackData.GetBossAttacks(10);//激光大风车
        }
        return BossAttackData.GetBossAttacks(1);//默认返回1
    }
    //随机攻击
    AttackDef RandomAttack(int i){
        if (i < 1 || i > 3) {return BossAttackData.BossRandomAttacks(1);}//返回默认攻击
        AttackDef attackDef;
        float gap;//内层间隔
        int innerShots;//内层波次
        switch(i){
            case 1: 
                SingleBulletAttack((BulletConfig)InitInstanceData.BossRandomAttackConfig(1));
                attackDef= BossAttackData.BossRandomAttacks(1);break;
            case 2: 
                gap=Random.Range(.5f,1f);
                innerShots=Random.Range(3,7);
                StartCoroutine(ContinuousScatterBulletAttack(
                    (BulletConfig)InitInstanceData.BossRandomAttackConfig(2), 
                    Random.Range(20,40),
                    Random.Range(5,20), 
                    gap*frequency, innerShots)); 
                attackDef= BossAttackData.BossRandomAttacks(2,1,innerShots * gap + 1f);break;//间隙=间隔*波次+缓冲
            case 3: 
                innerShots=Random.Range(2,8);
                float preLaserDelay=Random.Range(.3f,.9f);
                float duration=Random.Range(.4f,1f);
                LaserAttack(
                    (LaserConfig)InitInstanceData.BossRandomAttackConfig(3),
                    preLaserDelay,
                    duration,
                    Random.Range(1,8),
                    Random.Range(8,14),5);
                attackDef= BossAttackData.BossRandomAttacks(3,innerShots,preLaserDelay+duration+.2f);break;//.2f为缓冲
            default:
                Debug.LogError($"错误的随机攻击方式输入({i})");
                attackDef=BossAttackData.BossRandomAttacks(1);break;
        }
        return attackDef;
    }

    //激光大风车
    IEnumerator RotationLaserAttack(LaserConfig cfg,float duration, int activeLaserCount, float spreadAngle, float baseAngle){
        isPinwheel=true;
        gameObject.transform.position=new Vector3(0,0,-1);//将Boss移动到屏幕中心，避免激光旋转时偏离屏幕
        Laser[] Lasers=new Laser[activeLaserCount];

        moveDir=0;//方向归零
        for (int j = 0; j < 2; j++){
            for (int i = 0; i < activeLaserCount; i++){
            float angleOffset = (i - activeLaserCount / 2.0f) * spreadAngle;
            Laser laser=LaserPool.Instance.Get(LaserType.RotateLaser,transform.position,GetAttackQuaternion(angleOffset,baseAngle),j==0);
            if(laser==null){Debug.LogError($"{gameObject.tag}未获取到旋转激光");break;};
            laser.Init(cfg,EntityFaction,j==0);
            Lasers[i]=laser;
            }
            yield return new WaitForSeconds(j==0?1f:duration);
            foreach(Laser laser in Lasers){
                if(laser!=null)laser.Despawn();
            }
        }
        isPinwheel=false;
    }


    //移动相关
    //回调Boss速度
    void RefreshSpeed(){speed = activeLasers > 0 ? 0 : activeScatters > 0 ? 0.8f : tempSpeed;}

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
    void HandBossAttack(){
        if(Input.GetKeyDown(KeyCode.Alpha1)){BossAttackChoice(1);}
        if(Input.GetKeyDown(KeyCode.Alpha2)){BossAttackChoice(2);}
        if(Input.GetKeyDown(KeyCode.Alpha3)){BossAttackChoice(3);}
        if(Input.GetKeyDown(KeyCode.Alpha4)){BossAttackChoice(4);}
        if(Input.GetKeyDown(KeyCode.Alpha5)){BossAttackChoice(5);}
        if(Input.GetKeyDown(KeyCode.Alpha6)){BossAttackChoice(6);}
        if(Input.GetKeyDown(KeyCode.Alpha7)){BossAttackChoice(7);}
        if(Input.GetKeyDown(KeyCode.Alpha8)){BossAttackChoice(8);}
        if(Input.GetKeyDown(KeyCode.Alpha9)){BossAttackChoice(9);}
        if(Input.GetKeyDown(KeyCode.Alpha0)){BossAttackChoice(10);}
    }
#endif

}
