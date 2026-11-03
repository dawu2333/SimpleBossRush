using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour{
    static readonly Color[] colors={Color.red,Color.blue,Color.cyan,Color.black,Color.green,Color.yellow};
    [SerializeField] private float speed;
    [SerializeField] private GameObject bulletPrefabs;
    [SerializeField] private GameObject laserPrefabs;
    [SerializeField] private GameObject preLaserPrefabs;
    [SerializeField] private GameObject rotateLaserPrefabs;
    [SerializeField] private GameObject rotatePreLaserPrefabs;
    [SerializeField] private Transform hpbar;
    private string difficulty;
    private int hp,MaxHP,level;
    private float attack_time;//攻击间隔
    private float current_time;//冷却时间
    private float boundX=15.4f,boundY=7.4f,tempSpeed;
    private int MaxMoveStep=10;
    private int burstCount=0,currentAttack,burstMax=3;
    private float frequency=0;
    private bool isLaser=false,isDead=false;
    private int moveTimer,moveDir,lastAttack;
    private Transform playerTransform;
    private AudioSource audioSource;
    private int[] BossAttackList;
    private int[] BossAttackList2={1,2,3,4,5,6,8,9,10};//level 5 的额外攻击池：故意不含7，7与激光风车(10) 同时在场时弹幕过密，实测几乎无解
    private int[] ExtremeAttackList={1,2,3};//极限难度Boss的攻击池，

    private Color color;
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

    void Awake(){
        audioSource=GetComponent<AudioSource>();
    }
    void Start(){
        InitiateData();
        StartCoroutine(CountPlus());
    }

    void Update(){
        if (playerTransform == null) return;
        BossMoveAI();
        BossAttackAI();
        #if UNITY_EDITOR
        HandBossAttack();
        #endif
    }

    void OnTriggerEnter2D(Collider2D other){
        //Debug.Log("hit");
        if(other.gameObject.tag == "Bullet"){
            UpdateHp(1);
            StartCoroutine(Affected());
            Destroy(other.gameObject);
        }
    }

    void InitiateData(){
        //初始化难度/关卡
        difficulty=!string.IsNullOrEmpty(PlayerPrefs.GetString("Difficulty"))?PlayerPrefs.GetString("Difficulty"):"Normal";
        level=PlayerPrefs.GetInt("Level")==0?1:PlayerPrefs.GetInt("Level");
        var lv = LevelData.levels[level];

        //初始获取玩家位置
        playerTransform=GameObject.FindWithTag("Player")?.transform;
        if(playerTransform==null){Debug.LogError("Player not found!");GameControl.Instance?.BossDie();Destroy(gameObject);return;}
        
        //难度控制Boss血量/体型/频率/速度/攻击模式
        hp = PlayerPrefs.GetInt("BossHP") != 0 ? PlayerPrefs.GetInt("BossHP") : lv.bossHP;
        MaxHP=hp;
        float scale=lv.bossScale;
        if(difficulty=="Hard"||difficulty=="Extreme")scale*=0.8f;
        transform.localScale=new Vector3(scale,scale,1);
        frequency=difficulty=="Easy"?1:difficulty=="Normal"?0.7f:difficulty=="Hard"?0.5f:difficulty=="Extreme"?0.3f:1f;
        BossAttackList=lv.bossAttackPattern;
        speed=lv.bossSpeed;

        //初始化速度/攻击间隔/移动增量/音量/boss颜色/移动方向/攻击方式
        tempSpeed=speed;
        attack_time=1f;
        moveTimer=0;
        audioSource.volume=0.2f;//音量20%
        color=GetComponent<SpriteRenderer>().color;
        moveDir = Random.Range(1, 9);
        currentAttack=GetNewAttack();
    }

    void UpdateHp(int damage){
        //1.5f是血条的长度，0.75f是血条的偏移量
        hp-=damage;
        float percentage=Mathf.Clamp01((float)hp/MaxHP);
        float scaleXValue = 1.5f * percentage;
        float posXValue = -0.75f + 0.75f * percentage;
        hpbar.localPosition=new Vector3(posXValue,hpbar.localPosition.y,hpbar.localPosition.z);
        hpbar.localScale=new Vector3(scaleXValue,hpbar.localScale.y,hpbar.localScale.z);
        if(hp<=0&&!isDead){
            isDead=true;
            GameControl.Instance?.BossDie();
            Destroy(gameObject);
        }
    }
    void BossAttackAI() {
    current_time += Time.deltaTime;
    if (current_time >= attack_time && burstCount < burstMax) {
        attack_time=(difficulty!="Extreme"?BossAttackChoice(currentAttack):RandomAttack(currentAttack))*frequency;
        if(level==5&&burstCount%2==0){//第五关每两次攻击后随机选择一次攻击方式
            BossAttackChoice(BossAttackList2[Random.Range(0, BossAttackList2.Length)]);
            burstCount++;
        }
        burstCount++; current_time = 0;
        if(isLaser)speed=0;else speed=tempSpeed;
    } else if (current_time >= attack_time && burstCount >= burstMax) {
        burstMax = Random.Range(2, 4);
        currentAttack = GetNewAttack();
        burstCount = 0; current_time = level!=5?attack_time / 2:0;
    }

}
    int GetNewAttack() {
    int newAttack;
    do {
        newAttack = difficulty!="Extreme"?BossAttackList[Random.Range(0, BossAttackList.Length)]:ExtremeAttackList[Random.Range(0, ExtremeAttackList.Length)];
    } while (newAttack == lastAttack); // 确保新选择的攻击方式与上次不同
    lastAttack = newAttack; // 更新上一次的攻击方式
    return newAttack;
}

    float BossAttackChoice(int i){
        //Debug.Log(i);
        switch(i){
            case 1: isLaser = false; SingleAttack(1,3,Color.white,1f,false);burstMax=10;audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/rifleGunShot1")); return .1f;//伤害、弹速均一般的短连发攻击
            case 2: isLaser = false; SingleAttack(1,6.5f,Color.yellow,1.4f,false);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/283")); return .5f;//射速较快且弹速快，伤害一般的攻击
            case 3: isLaser = false; SingleAttack(2,1f,Color.red,10f,true);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/510")); return 2f;//射速和弹速较慢，伤害较大的攻击
            case 4: isLaser = false; ScatterAttack(1,3f,Color.blue,1.4f,8,7.5f,0);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/406")); return 2.5f;//射速较慢，伤害一般，弹速些微慢的八发子弹扇形攻击
            case 5: isLaser = false; ScatterAttack(1,1f,Color.green,1.3f,50,7.2f,0);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/GunShot2")); return 3f;//射速很慢，伤害一般，弹速较慢的单词全屏广域攻击
            case 6: isLaser = false; SingleAttack(1,3.5f,Color.cyan,1.5f,false);burstMax=50;audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/rifleGunShot2")); return .1f;//射速极快、弹速较快，伤害与弹速一般的自机连续攻击
            case 7: isLaser = false; StartCoroutine(ContinuousScatterAttack(1, .8f, new Color(1.0f, 0.753f, 0.796f), 1.3f, level!=5?40:30, level!=5?9:12, 0.5f, 10)); return 6f; // 射速很快，伤害一般，弹速较慢的持续全屏广域攻击(波粒)
            case 8: isLaser = true; LaserAttack(3,.8f,.5f,1,8,15,1);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/railGunShot2"));return 1f;//散射激光
            case 9: isLaser = true; LaserAttack(3,.4f,.4f,.8f,1,10,5);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/railGunShot1"));burstMax=20;return .15f;//快速的激光自机狙
            case 10: isLaser = true; StartCoroutine(RotationLaserAttack(3,-25f,10,1,level!=5?5:4,level!=5?36:45,0));burstMax=1;return 2f;//激光大风车
        }
        return 1f;
    }

    float RandomAttack(int i){
        switch(i){
            case 1: isLaser = false; SingleAttack(Random.Range(1,3),Random.Range(2.5f,9f),colors[Random.Range(0,colors.Length)],Random.Range(2.5f,6),Random.Range(0,2)>0?true:false);burstMax=Random.Range(5,10); return Random.Range(0.1f,.8f);
            case 2: isLaser = false; StartCoroutine(ContinuousScatterAttack(Random.Range(1,3),Random.Range(2.5f,5.5f), colors[Random.Range(0,colors.Length)], Random.Range(0.8f,2), Random.Range(20,40), Random.Range(5,20), Random.Range(.5f,1f), Random.Range(3,7))); return Random.Range(1.1f,3.2f);
            case 3: isLaser = true; LaserAttack(Random.Range(1,3),Random.Range(.3f,.9f),Random.Range(.4f,1f),Random.Range(.4f,.8f),Random.Range(1,8),Random.Range(8,14),5);burstMax=Random.Range(1,8);return Random.Range(.4f,1f);
        }
        return 1f;
    }

    void SingleAttack(int damage,float bulletSpeed,Color color,float size,bool isObstacle){//自机狙,发射单个子弹,伤害和速度由参数决定
        if(playerTransform==null)return;
        float angle=Vector3.SignedAngle(Vector3.up,GetAttackDirection(),Vector3.forward);
        GameObject bullet=Instantiate(bulletPrefabs,transform.position,Quaternion.Euler(0,0,angle));
        bullet.GetComponent<Bullet>().bulletSpeed=bulletSpeed;
        bullet.GetComponent<Bullet>().damage=damage;
        bullet.GetComponent<Bullet>().isObstacle=isObstacle;
        bullet.GetComponent<SpriteRenderer>().color=color;
        bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*size,bullet.transform.localScale.y*size,bullet.transform.localScale.z);
        Destroy(bullet,3f);
    }
    void ScatterAttack(int damage, float bulletSpeed, Color color, float size, int bulletCount, float spreadAngle, float baseAngle){
        if (playerTransform == null) return;
        //case 非4/5的通用枪声
        if(bulletCount<=40&&bulletCount>=20)audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/GunShot1"));

        for (int i = 0; i < bulletCount; i++){
            float angleOffset = (i - bulletCount / 2.0f) * spreadAngle;
            float angle = Vector3.SignedAngle(Vector3.up, GetAttackDirection(), Vector3.forward) + angleOffset + baseAngle;
            GameObject bullet = Instantiate(bulletPrefabs, transform.position, Quaternion.Euler(0, 0, angle));
            bullet.GetComponent<Bullet>().bulletSpeed = bulletSpeed;
            bullet.GetComponent<Bullet>().damage = damage;
            bullet.GetComponent<SpriteRenderer>().color = color;
            bullet.transform.localScale = new Vector3(bullet.transform.localScale.x * size, bullet.transform.localScale.y * size, bullet.transform.localScale.z);
            Destroy(bullet, 3f);
        }
    }
    IEnumerator ContinuousScatterAttack(int damage, float bulletSpeed, Color color, float size, int bulletCount, float spreadAngle, float interval, int shots){
        for (int j = 0; j < shots; j++){
            float baseAngle = (j * 8f) % 360; // 每次射击角度变化8度
            speed=.8f;//射击时减速
            ScatterAttack(damage, bulletSpeed, color, size, bulletCount, spreadAngle, baseAngle);
            yield return new WaitForSeconds(interval);
        }
        speed=tempSpeed;
    }
    void LaserAttack(int damage,float preLaserDelay,float duration, float width, int laserCount, float spreadAngle, float baseAngle){
        if (playerTransform == null) return;

        for (int i = 0; i < laserCount; i++){
            float angleOffset = (i - laserCount / 2.0f) * spreadAngle;
            float angle = Vector3.SignedAngle(Vector3.up, GetAttackDirection(), Vector3.forward) + angleOffset + baseAngle;
            StartCoroutine(LaserAttackProcess(damage, duration,preLaserDelay, width,angle));
        }
    }
    
    IEnumerator LaserAttackProcess(int damage,float duration,float preLaserDelay, float width,float angle){
        GameObject preLaser = Instantiate(preLaserPrefabs, transform.position, Quaternion.Euler(0, 0, angle));
        yield return new WaitForSeconds(preLaserDelay);
        Destroy(preLaser);
        GameObject laser = Instantiate(laserPrefabs, transform.position, Quaternion.Euler(0, 0, angle));
        laser.GetComponent<Laser>().damage = damage;
        laser.GetComponent<Laser>().width = width;
        yield return new WaitForSeconds(duration);
        Destroy(laser);
    }
    //占位，暂未实现
    IEnumerator LaserChangeWidthProcess(GameObject laser,float from,float to,float duration,bool isBegin){
        if(isBegin){
        }
        yield break;
    }

    IEnumerator RotationLaserAttack(int damage,float rotateSpeed,float duration, float width,int laserCount, float spreadAngle, float baseAngle){
        if (playerTransform == null) yield break; 
        gameObject.transform.position=new Vector3(0,0,-2);//将Boss移动到屏幕中心，避免激光旋转时偏离屏幕
        Vector2 direction = GetAttackDirection();
        GameObject[] Lasers=new GameObject[laserCount];
        Coroutine stayCoroutine=StartCoroutine(Stay());

        moveDir=0;

        for (int i = 0; i < laserCount; i++){
            float angleOffset = (i - laserCount / 2.0f) * spreadAngle;
            float angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward) + angleOffset + baseAngle;
            GameObject laser = Instantiate(rotatePreLaserPrefabs, transform.position, Quaternion.Euler(0, 0, angle));
            Lasers[i]=laser;
        }
        yield return new WaitForSeconds(1);
        foreach(GameObject laser in Lasers){
            Destroy(laser);
        }

        for (int i = 0; i < laserCount; i++){
            float angleOffset = (i - laserCount / 2.0f) * spreadAngle;
            float angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward) + angleOffset + baseAngle;
            GameObject laser = Instantiate(rotateLaserPrefabs, transform.position, Quaternion.Euler(0, 0, angle));
            laser.GetComponent<Laser>().damage = damage;
            laser.GetComponent<Laser>().width = width;
            laser.GetComponent<Laser>().rotateSpeed = rotateSpeed;
            Lasers[i]=laser;
        }
        yield return new WaitForSeconds(duration);
        foreach(GameObject laser in Lasers){
            Destroy(laser);
        }
        StopCoroutine(stayCoroutine);
    }
    IEnumerator Stay(){
        while(true){
            moveDir=0;
            //yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.01f);
        }
    }

    void BossMoveAI(){
        if(moveTimer != MaxMoveStep){
            Move(moveDir);
        }
        if(moveTimer > MaxMoveStep){
            moveDir = Random.Range(1, 9); // 方向范围从1到8
            moveTimer = 0;
        }
    }

    void Move(int direction){
        while(direction > 8) direction %= 8; // 确保方向在1到8之间
        if(transform.position.x <= boundX && transform.position.x >= -boundX && transform.position.y <= boundY && transform.position.y >= -boundY){
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
            if(transform.position.x > boundX){
                transform.position = new Vector3(boundX - 0.01f, transform.position.y, transform.position.z);
                this.moveDir = 5; // 左下
            } else if(transform.position.x < -boundX){
                transform.position = new Vector3(-boundX + 0.01f, transform.position.y, transform.position.z);
                this.moveDir = 7; // 右下
            } else if(transform.position.y > boundY){
                transform.position = new Vector3(transform.position.x, boundY - 0.01f, transform.position.z);
                this.moveDir = 6; // 下
            } else if(transform.position.y < -boundY){
                transform.position = new Vector3(transform.position.x, -boundY + 0.01f, transform.position.z);
                this.moveDir = 2; // 上
            }
        }
    }
    IEnumerator CountPlus(){
        while(true){
            moveTimer++;
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }

    IEnumerator Affected(){
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.05f);
        GetComponent<SpriteRenderer>().color = color;
    }

    Vector2 GetAttackDirection(){
        if (playerTransform == null) return Vector2.zero;
        Vector2 playerPosition = playerTransform.position;
        Vector2 bossPosition = new Vector2(transform.position.x, transform.position.y);
        return (playerPosition - bossPosition).normalized;
    }


}
