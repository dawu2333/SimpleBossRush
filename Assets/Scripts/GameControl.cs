using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour{
    public GameObject bossPrefabs;
    private AudioSource audioSource;
    private AudioClip BGM1,BGM2;
    private bool isEscape;
    private int level;
    void Awake() {
        isEscape=false;
        level=PlayerPrefs.GetInt("Level")==0?1:PlayerPrefs.GetInt("Level");
        audioSource = GetComponent<AudioSource>();
        audioSource.loop=true;
        BGM1 = Resources.Load<AudioClip>("BGM/AttackWithoutMercy");
        BGM2 = Resources.Load<AudioClip>("BGM/Crusade");    
    }
    void Start(){
        audioSource.clip=level>=3?BGM1:BGM2;
        audioSource.Play();
        audioSource.volume=0.1f;//音量百分比
        if(level==4){Instantiate(bossPrefabs,new Vector3(4,0,-1),Quaternion.identity);}
        //Debug.Log("GameControl,level="+level);
    }

    // Update is called once per frame
    void Update(){
        PauseGame();
    }
    void PauseGame(){
        if(Input.GetKeyDown(KeyCode.Escape)&&!isEscape){
            Time.timeScale=0;
            isEscape=true;
        }else if(Input.GetKeyDown(KeyCode.Escape)&&isEscape){
            Time.timeScale=1;
            isEscape=false;
        }
    }
    
}
