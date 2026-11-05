using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour{
    public static GameControl Instance { get; private set; }
    public BulletPool BulletPool{get;private set;}
    [SerializeField] private GameObject bossPrefabs, playerPrefabs;
    private AudioSource audioSource;
    private AudioClip BGM1,BGM2;
    private bool isEscape;
    private int level,bossNum=0;
    const float Spacing = 14f;
    void Awake() {
        if(Instance==null)Instance=this;
        BulletPool=GetComponent<BulletPool>();
        isEscape=false;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop=true;
        BGM1 = Resources.Load<AudioClip>("BGM/AttackWithoutMercy");
        BGM2 = Resources.Load<AudioClip>("BGM/Crusade");   
    }
    void Start(){
        InitiateGame();
    }

    void Update(){
        PauseGame();
    }
    void OnDestroy(){
        if(Instance==this)Instance=null;
    }
    void InitiateGame(){
        //由难度控制音效和boss数量
        level=PlayerPrefs.GetInt("Level")==0?1:PlayerPrefs.GetInt("Level");
        if(level<1 || level>=LevelData.levels.Length) {level=1;PlayerPrefs.SetInt("Level",level);}
        var lv = LevelData.levels[level];
        bossNum=lv.bossNum;
        Time.timeScale=1;
        audioSource.clip=level>=3?BGM1:BGM2;
        audioSource.Play();
        audioSource.volume=0.1f;//音量百分比

        float startX = -(lv.bossNum - 1) * Spacing / 2f;
        Instantiate(playerPrefabs,new Vector3(0,-10,-1),Quaternion.identity);
        for(int i=0;i<lv.bossNum;i++){Instantiate(bossPrefabs, new Vector3(startX + i*Spacing, 0, -1), Quaternion.identity);}
    }
    public void BossDie(){
        bossNum--;
        if(bossNum==0){
            PlayerPrefs.SetInt("IsWin",1);
            SceneManager.LoadScene("EndScene");
        }
    }
    public void PlayerDie(){
        PlayerPrefs.SetInt("IsWin",0);
        SceneManager.LoadScene("EndScene");
    }
    void PauseGame(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            isEscape=!isEscape;
            Time.timeScale=isEscape?0:1;
        }
    }
    
}
