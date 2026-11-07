
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameControl : MonoBehaviour{
    public static GameControl Instance { get; private set; }
    [SerializeField] private GameObject bossPrefabs, playerPrefabs;
    
    private AudioSource audioSource;
    private AudioClip BGM1,BGM2;
    private bool isEscape;
    private int bossNum=0;
    const float Spacing = 14f;//多boss间隔
    public static float boundX=15.4f,boundY=7.4f;//游戏边界
    void Awake() {
        if(Instance==null)Instance=this;
        InitAudio();
    }
    void Start(){
        InitiateGame();
    }

    void Update(){
        PauseGame();
    }
    void OnDestroy(){
        if(Instance==this)Instance=null;//清除单例
    }
    void InitiateGame(){
        isEscape=false;

        //由难度控制音效和boss数量
        if(BaseInfo.Level<1 || BaseInfo.Level>=LevelData.levels.Length) {BaseInfo.Level=1;}
        var lv = LevelData.levels[BaseInfo.Level];
        bossNum=lv.bossNum;
        Time.timeScale=1;
        audioSource.clip=BaseInfo.Level>=3?BGM1:BGM2;
        audioSource.Play();
        audioSource.volume=0.1f;//音量百分比

        float startX = -(lv.bossNum - 1) * Spacing / 2f;
        Instantiate(playerPrefabs,new Vector3(0,-10,-1),Quaternion.identity);
        for(int i=0;i<lv.bossNum;i++){
            Instantiate(bossPrefabs, new Vector3(startX + i*Spacing, 0, -1), Quaternion.identity);
        }
    }
    public void BossDie(){
        bossNum--;
        if(bossNum==0){
            BaseInfo.IsWin=true;
            SceneManager.LoadScene("EndScene");
        }
    }
    public void PlayerDie(){
        BaseInfo.IsWin=false;
        SceneManager.LoadScene("EndScene");
    }
    void PauseGame(){
        if(Input.GetKeyDown(KeyCode.Escape)){
            isEscape=!isEscape;
            Time.timeScale=isEscape?0:1;
        }
    }
    private void InitAudio() {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop=true;
        BGM1 = Resources.Load<AudioClip>("BGM/AttackWithoutMercy");
        BGM2 = Resources.Load<AudioClip>("BGM/Crusade");   
    } 
}
