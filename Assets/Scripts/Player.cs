using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour{
    public GameObject bulletPrefabs;
    public float speed;
    public int hp,level;
    public float attack_time;//攻击间隔
    private float current_time;//冷却时间
    private float x=17.5f,y=9.5f;
    private Transform hpBar;
    private int MaxHP;
    private bool canAffected=false;
    void Awake() {
        hpBar=transform.GetChild(0).GetChild(1);//获取血条
        hpBar.SetParent(transform.GetChild(0),false);
        transform.GetChild(0).SetParent(transform,false);
    }
    void Start(){
        Invoke("StartGameCanAffected", 2);
        if(PlayerPrefs.GetInt("PlayerHP")!=0)hp=PlayerPrefs.GetInt("PlayerHP");
        MaxHP=hp;
        if(PlayerPrefs.GetString("Difficulty")=="Hard"||PlayerPrefs.GetString("Difficulty")=="Extreme")transform.localScale=new Vector3(0.8f,0.8f,1);
        level=PlayerPrefs.GetInt("Level")==0?1:PlayerPrefs.GetInt("Level");
        if(level>=3){
            attack_time/=level==3?1.2f:level==4?1.5f:level==5?2:1;
            float scale=level==3?.8f:level==4?.6f:level==5?.4f:1f;
            transform.localScale=new Vector3(scale,scale,1);
        }
        //Debug.Log("playerhp="+hp+"level="+level);
    }

    void Update(){
        Move();
        current_time+=Time.deltaTime;
        if(Input.GetMouseButton(0)&&current_time>=attack_time){
            Shoot();
            current_time=0;
        }
        //if(Input.GetMouseButtonDown(1)){UpdateHp(1);}
    }

    void Move(){
        if(transform.position.x<=x&&transform.position.x>=-x&&transform.position.y<=y&&transform.position.y>=-y){
            if(Input.GetKey(KeyCode.W)){transform.position+=Vector3.up*speed*Time.deltaTime;}
            if(Input.GetKey(KeyCode.S)){transform.position+=Vector3.down*speed*Time.deltaTime;}
            if(Input.GetKey(KeyCode.A)){transform.position+=Vector3.left*speed*Time.deltaTime;}
            if(Input.GetKey(KeyCode.D)){transform.position+=Vector3.right*speed*Time.deltaTime;}
        }
        else if(transform.position.x>x){
            transform.position=new Vector3(17.49f,transform.position.y,transform.position.z);
        }else if(transform.position.x<-x){
            transform.position=new Vector3(-17.49f,transform.position.y,transform.position.z);
        }else if(transform.position.y>y){
            transform.position=new Vector3(transform.position.x,9.49f,transform.position.z);
        }else if(transform.position.y<-y){
            transform.position=new Vector3(transform.position.x,-9.49f,transform.position.z);
        }
    }

    void Shoot(){
        Vector2 mousePosition=Input.mousePosition;//鼠标的屏幕所在位置
        Vector2 mouseWorldPosition=Camera.main.ScreenToWorldPoint(mousePosition);//转换为世界坐标
        Vector2 gameObjectPosition=new Vector2(transform.position.x,transform.position.y);

        Vector2 direction=(mouseWorldPosition-gameObjectPosition).normalized;//鼠标射击方向向量
        float angle=Vector3.SignedAngle(Vector3.up,direction,Vector3.forward);

        GameObject bullet=Instantiate(bulletPrefabs,transform.position,Quaternion.Euler(0,0,angle));

        //Debug.Log($"Mouse Position: {mouseWorldPosition}, Direction: {direction}, Angle: {angle}");
        Destroy(bullet,10f);
    }
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag=="Boss"){//如果碰到Boss，直接趋势
            GameOver();
        }else if(other.gameObject.tag=="BossBullet"&&canAffected){//如果碰到子弹，减少伤害值点生命值
            UpdateHp(other.GetComponent<Bullet>().damage);
            Destroy(other.gameObject);
        }else if((other.gameObject.tag=="Laser"||other.gameObject.tag=="RotateLaser")&&canAffected){//如果碰到激光，减少3点生命值
            UpdateHp(other.GetComponentInParent<Laser>().damage);
        }
    }

    void UpdateHp(int damage){
        StartCoroutine(AffectedProcess());
        PlayerPrefs.SetInt("Affected",PlayerPrefs.GetInt("Affected")+1);
        hp-=damage;
        float percentage=(float)hp/MaxHP;
        float scaleXValue = 1.5f * percentage;
        float posXValue = -0.75f + 0.75f * percentage;
        //Debug.Log("percentage: "+percentage+" posXValue: "+posXValue+" scaleXValue: "+scaleXValue);
        hpBar.transform.localPosition=new Vector3(posXValue,hpBar.transform.localPosition.y,hpBar.transform.localPosition.z);
        hpBar.transform.localScale=new Vector3(scaleXValue,hpBar.transform.localScale.y,hpBar.transform.localScale.z);
        if(hp<=0){GameOver();}
    }

    void GameOver(){
        Destroy(gameObject);
        //Debug.Log("似了");
        PlayerPrefs.SetInt("IsWin",0);
        SceneManager.LoadScene("EndScene");
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

}
