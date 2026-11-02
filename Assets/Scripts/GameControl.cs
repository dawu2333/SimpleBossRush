using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour{
    public static GameControl Instance { get; private set; }
    [SerializeField] private GameObject bossPrefabs, playerPrefabs;
    private AudioSource audioSource;
    private AudioClip BGM1,BGM2;
    private bool isEscape;
    private int level,bossNum=0;
    void Awake() {
        if(Instance==null)Instance=this;
        isEscape=false;
        level=PlayerPrefs.GetInt("Level")==0?1:PlayerPrefs.GetInt("Level");
        audioSource = GetComponent<AudioSource>();
        audioSource.loop=true;
        BGM1 = Resources.Load<AudioClip>("BGM/AttackWithoutMercy");
        BGM2 = Resources.Load<AudioClip>("BGM/Crusade");   
    }
    void Start(){
        #if UNITY_EDITOR
        if(SceneManager.GetActiveScene().name=="TempScene"){PlayerPrefs.SetInt("t",1);}
        #endif
        Time.timeScale=1;
        audioSource.clip=level>=3?BGM1:BGM2;
        audioSource.Play();
        audioSource.volume=0.1f;//音量百分比

        int t=0;
        if(level==4){t=7;Instantiate(bossPrefabs,new Vector3(t,0,-1),Quaternion.identity);bossNum++;}//第四关加一个boss
        Instantiate(bossPrefabs,new Vector3(-t,0,-1),Quaternion.identity);bossNum++;
        Instantiate(playerPrefabs,new Vector3(0,-10,-1),Quaternion.identity);
        //Debug.Log("GameControl,level="+level);
    }
    public void BossDie(){
        bossNum--;
        if(bossNum==0){
            PlayerPrefs.SetInt("IsWin",1);
            SceneManager.LoadScene("EndScene");
        }
    }

    void Update(){
        PauseGame();
    }
    void OnDestroy(){
        if(Instance==this)Instance=null;
    }
    void PauseGame(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            isEscape=!isEscape;
            Time.timeScale=isEscape?0:1;
        }
    }
    
}
