using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour{
    public float speed;
    public int hp;
    public GameObject bulletPrefabs;
    public GameObject laserPrefabs;
    public GameObject preLaserPrefabs;
    public GameObject rotateLaserPrefabs;
    public GameObject rotatePreLaserPrefabs;
    private string difficulty;
    private int MaxHP,level;
    private Transform hpbar;
    private float attack_time;//攻击间隔
    private float current_time;//冷却时间
    private float x=15.4f,y=7.4f,tempSpeed;
    private int MaxCount=10;
    private int i=0,a,t=3;
    private int count,direction,lastAttack;
    private Transform playerTransform;
    private AudioSource audioSource;
    private int[] BossAttackList={1,2,3,4,5,6,7,8,9,10};
    private int[] BossAttackList2={1,2,3,4,5,6,8,9,10};
    private int[] ExtremeAttackList={1,2,3};
    private Color[] colors={Color.red,Color.blue,Color.cyan,Color.black,Color.green,Color.yellow};
    private bool isLaser;
    private Color color;
    void Awake(){
        audioSource=GetComponent<AudioSource>();
        hpbar=transform.GetChild(0).GetChild(1);//获取血条
        hpbar.SetParent(transform.GetChild(0),false);
        transform.GetChild(0).SetParent(transform,false);
    }
    void Start(){
        audioSource.volume=0.2f;//音量20%
        if(PlayerPrefs.GetInt("BossHP")!=0)hp=PlayerPrefs.GetInt("BossHP");
        
        MaxHP=hp;
        if(!string.IsNullOrEmpty(PlayerPrefs.GetString("Difficulty")))difficulty=PlayerPrefs.GetString("Difficulty");
        level=PlayerPrefs.GetInt("Level")==0?1:PlayerPrefs.GetInt("Level");
        if(level==1){BossAttackList=new int[]{1,2,3,4,5};}
        else if(level==2){BossAttackList=new int[]{1,2,3,4,5,8};}
        else if(level==3){BossAttackList=new int[]{1,2,3,4,5,6,7,8,9,10};}
        else if(level==4){
            speed=7;
            BossAttackList=new int[]{1,2,3,4,5,6,7,8,9};
            transform.localScale=new Vector3(3,3,1);}
        else if(level==5){
            BossAttackList=new int[]{1,2,3,4,5,6,7,8,9,10};
            speed=3;
            transform.localScale=new Vector3(9,9,1);
        }
        color=GetComponent<SpriteRenderer>().color;
        playerTransform=GameObject.Find("player").transform;
        count=MaxCount-1;
        StartCoroutine(CountPlus());
        direction = UnityEngine.Random.Range(1, 9);
        tempSpeed=this.speed;
        a=UnityEngine.Random.Range(1,11);
        //Debug.Log("Bosshp="+hp+"level="+level);
        
    }

    void Update(){
        BossMoveAI();
        BossAttackAI();
        //HandBossAttack();
    }

    void OnTriggerEnter2D(Collider2D other){
        //Debug.Log("hit");
        if(other.gameObject.tag == "Bullet"){
            UpdateHp(1);
            StartCoroutine(Affeted());
            Destroy(other.gameObject);
        }
    }

    void UpdateHp(int damage){
        hp-=damage;
        float percentage=(float)hp/MaxHP;
        float scaleXValue = 1.5f * percentage;
        float posXValue = -0.75f + 0.75f * percentage;
        //Debug.Log("percentage: "+percentage+" posXValue: "+posXValue+" scaleXValue: "+scaleXValue);
        hpbar.transform.localPosition=new Vector3(posXValue,hpbar.transform.localPosition.y,hpbar.transform.localPosition.z);
        hpbar.transform.localScale=new Vector3(scaleXValue,hpbar.transform.localScale.y,hpbar.transform.localScale.z);
        if(hp<=0){
            Destroy(gameObject);
            PlayerPrefs.SetInt("IsWin",1);
            SceneManager.LoadScene("EndScene");
        }
    }
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
    void BossAttackAI() {
    // i为当前攻击方式攻击次数，a为攻击方式，t为攻击次数上限
    current_time += Time.deltaTime;
    if (current_time >= attack_time && i < t) {
        attack_time = difficulty=="Easy"?BossAttackChoice(a):
                        difficulty=="Normal"?BossAttackChoice(a)*0.7f:
                        difficulty=="Hard"?BossAttackChoice(a)*0.5f:
                        difficulty=="Extreme"?RandomAttack(a)*0.6f:1f;
        if(level==5&&i%2==0){
            BossAttackChoice(BossAttackList2[UnityEngine.Random.Range(0, BossAttackList2.Length)]);
            i++;
        }
        i++; current_time = 0;
        if(isLaser)speed=0;else speed=tempSpeed;
    } else if (current_time >= attack_time && i >= t) {
        t = UnityEngine.Random.Range(2, 4);
        a = GetNewAttack();
        i = 0; current_time = level!=5?attack_time / 2:0;
    }

}
    int GetNewAttack() {
    int newAttack;
    do {
        newAttack = difficulty!="Extreme"?BossAttackList[UnityEngine.Random.Range(0, BossAttackList.Length)]:ExtremeAttackList[UnityEngine.Random.Range(0, ExtremeAttackList.Length)];
    } while (newAttack == lastAttack); // 确保新选择的攻击方式与上次不同
    lastAttack = newAttack; // 更新上一次的攻击方式
    return newAttack;
}

    float BossAttackChoice(int i){
        //Debug.Log(i);
        switch(i){
            case 1: SingleAttack(1,3,Color.white,1f,false);t=10;audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/rifleGunShot1")); return .1f;//伤害、弹速均一般的短连发攻击
            case 2: SingleAttack(1,6.5f,Color.yellow,1.4f,false);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/283")); return .5f;//射速较快且弹速快，伤害一般的攻击
            case 3: SingleAttack(2,1f,Color.red,10f,true);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/510")); return 2f;//射速和弹速较慢，伤害较大的攻击
            case 4: ScatterAttack(1,3f,Color.blue,1.4f,8,7.5f,0);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/406")); return 2.5f;//射速较慢，伤害一般，弹速些微慢的八发子弹扇形攻击
            case 5: ScatterAttack(1,1f,Color.green,1.3f,50,7.2f,0);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/GunShot2")); return 3f;//射速很慢，伤害一般，弹速较慢的单词全屏广域攻击
            case 6:SingleAttack(1,3.5f,Color.cyan,1.5f,false);t=50;audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/rifleGunShot2")); return .1f;//射速极快、弹速较快，伤害与弹速一般的自机连续攻击
            case 7:StartCoroutine(ContinuousScatterAttack(1, .8f, new Color(1.0f, 0.753f, 0.796f), 1.3f, level!=5?40:30, level!=5?9:12, 0.5f, 10)); return 6f; // 射速很慢，伤害一般，弹速较慢的全屏广域攻击
            case 8:LaserAttack(3,.8f,.5f,1,8,15,1);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/railGunShot2"));return 1f;//散射激光
            case 9:LaserAttack(3,.4f,.4f,.8f,1,10,5);audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/railGunShot1"));t=20;return .15f;//快速的激光自机狙
            case 10:StartCoroutine(RotationLaserAttack(3,-25f,10,1,level!=5?5:4,level!=5?36:45,0));t=1;return 2f;//激光大风车
        }
        return 1f;
    }

    float RandomAttack(int i){
        switch(i){
            case 1: SingleAttack(UnityEngine.Random.Range(1,3),UnityEngine.Random.Range(2.5f,9f),colors[UnityEngine.Random.Range(0,colors.Length)],UnityEngine.Random.Range(2.5f,6),UnityEngine.Random.Range(0,2)>0?true:false);t=UnityEngine.Random.Range(5,10); return UnityEngine.Random.Range(0.1f,.8f);
            case 2:StartCoroutine(ContinuousScatterAttack(UnityEngine.Random.Range(1,3),UnityEngine.Random.Range(2.5f,5.5f), colors[UnityEngine.Random.Range(0,colors.Length)], UnityEngine.Random.Range(0.8f,2), UnityEngine.Random.Range(10,40), UnityEngine.Random.Range(5,20), UnityEngine.Random.Range(.5f,1f), UnityEngine.Random.Range(3,7))); return UnityEngine.Random.Range(1.1f,3.2f);
            case 3:LaserAttack(UnityEngine.Random.Range(1,3),UnityEngine.Random.Range(.3f,.9f),UnityEngine.Random.Range(.4f,1f),UnityEngine.Random.Range(.4f,.8f),UnityEngine.Random.Range(1,8),UnityEngine.Random.Range(8,14),5);t=UnityEngine.Random.Range(1,8);return UnityEngine.Random.Range(.4f,1f);
        }
        return 1f;
    }

    void SingleAttack(int damage,float bulletSpeed,Color color,float size,bool isObstacle){//自机狙,发射单个子弹,伤害和速度由参数决定
        isLaser=false;
        if(playerTransform==null)return;
        Vector2 playerPosition=playerTransform.position;
        Vector2 BossPosition=new Vector2(transform.position.x,transform.position.y);
        Vector2 direction=(playerPosition-BossPosition).normalized;
        float angle=Vector3.SignedAngle(Vector3.up,direction,Vector3.forward);
        GameObject bullet=Instantiate(bulletPrefabs,transform.position,Quaternion.Euler(0,0,angle));
        bullet.GetComponent<Bullet>().bulletSpeed=bulletSpeed;
        bullet.GetComponent<Bullet>().damage=damage;
        bullet.GetComponent<Bullet>().isObstacle=isObstacle;
        bullet.GetComponent<SpriteRenderer>().color=color;
        bullet.transform.localScale=new Vector3(bullet.transform.localScale.x*size,bullet.transform.localScale.y*size,bullet.transform.localScale.z);
        Destroy(bullet,10f);
    }
    void ScatterAttack(int damage, float bulletSpeed, Color color, float size, int bulletCount, float spreadAngle, float baseAngle){
        isLaser=false;
        if (playerTransform == null) return;
        Vector2 playerPosition = playerTransform.position;
        Vector2 bossPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = (playerPosition - bossPosition).normalized;
        if(bulletCount==40)audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/GunShot1"));

        for (int i = 0; i < bulletCount; i++){
            float angleOffset = (i - bulletCount / 2.0f) * spreadAngle;
            float angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward) + angleOffset + baseAngle;
            GameObject bullet = Instantiate(bulletPrefabs, transform.position, Quaternion.Euler(0, 0, angle));
            bullet.GetComponent<Bullet>().bulletSpeed = bulletSpeed;
            bullet.GetComponent<Bullet>().damage = damage;
            bullet.GetComponent<SpriteRenderer>().color = color;
            bullet.transform.localScale = new Vector3(bullet.transform.localScale.x * size, bullet.transform.localScale.y * size, bullet.transform.localScale.z);
            Destroy(bullet, 10f);
        }
    }
    IEnumerator ContinuousScatterAttack(int damage, float bulletSpeed, Color color, float size, int bulletCount, float spreadAngle, float interval, int shots){
            isLaser=false;
        for (int j = 0; j < shots; j++){
            float baseAngle = (j * 8f) % 360; // 每次射击角度变化8度
            //direction=0;//固定位置
            this.speed=.8f;//射击时减速
            //Debug.Log(baseAngle);
            ScatterAttack(damage, bulletSpeed, color, size, bulletCount, spreadAngle, baseAngle);
            yield return new WaitForSeconds(interval);
        }
        this.speed=tempSpeed;
    }
    void LaserAttack(int damage,float preLaserDelay,float duration, float width, int laserCount, float spreadAngle, float baseAngle){
        isLaser=true;
        if (playerTransform == null) return;
        Vector2 playerPosition = playerTransform.position;
        Vector2 bossPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = (playerPosition - bossPosition).normalized;

        for (int i = 0; i < laserCount; i++){
            float angleOffset = (i - laserCount / 2.0f) * spreadAngle;
            float angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward) + angleOffset + baseAngle;
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
    IEnumerator LaserChangeWidthProcess(GameObject laser,float from,float to,float duration,bool isBegin){
        if(isBegin){
        }
        return null;
    }

    IEnumerator RotationLaserAttack(int damage,float rotateSpeed,float duration, float width,int laserCount, float spreadAngle, float baseAngle){
        isLaser=true;
        gameObject.transform.position=new Vector3(0,0,-2);
        Vector2 playerPosition = playerTransform.position;
        Vector2 bossPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 direction = (playerPosition - bossPosition).normalized;
        GameObject[] Lasers=new GameObject[laserCount];
        Coroutine stayCoroutine=StartCoroutine(Stay());

        this.direction=0;

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
            direction=0;
            //yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.01f);
        }
    }

    void BossMoveAI(){
        if(count != MaxCount){
            Move(direction);
        }
        if(count > MaxCount){
            direction = UnityEngine.Random.Range(1, 9); // 方向范围从1到8
            count = 0;
        }
    }

    void Move(int direction){
        while(direction > 8) direction %= 8; // 确保方向在1到8之间
        if(transform.position.x <= x && transform.position.x >= -x && transform.position.y <= y && transform.position.y >= -y){
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
            if(transform.position.x > x){
                transform.position = new Vector3(x - 0.01f, transform.position.y, transform.position.z);
                this.direction = 5; // 左下
            } else if(transform.position.x < -x){
                transform.position = new Vector3(-x + 0.01f, transform.position.y, transform.position.z);
                this.direction = 7; // 右下
            } else if(transform.position.y > y){
                transform.position = new Vector3(transform.position.x, y - 0.01f, transform.position.z);
                this.direction = 6; // 下
            } else if(transform.position.y < -y){
                transform.position = new Vector3(transform.position.x, -y + 0.01f, transform.position.z);
                this.direction = 2; // 上
            }
        }
    }
    IEnumerator CountPlus(){
        while(true){
            count++;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));
        }
    }

    IEnumerator Affeted(){
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.05f);
        GetComponent<SpriteRenderer>().color = color;
    }


}
