using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour{
    [SerializeField] private GameObject bossPrefabs;
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
        Time.timeScale=1;
        audioSource.clip=level>=3?BGM1:BGM2;
        audioSource.Play();
        audioSource.volume=0.1f;//音量百分比
        if(level==4){Instantiate(bossPrefabs,new Vector3(4,0,-1),Quaternion.identity);}
        //Debug.Log("GameControl,level="+level);
    }

    void Update(){
        PauseGame();
    }
    void PauseGame(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            isEscape=!isEscape;
            Time.timeScale=isEscape?0:1;
        }
    }
    
}
