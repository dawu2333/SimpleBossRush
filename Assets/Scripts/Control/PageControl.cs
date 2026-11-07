using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PageControl : MonoBehaviour{

    //DebugScene、StartMenu、LevelChoice
    
    [SerializeField] private TMP_InputField BossHpInput;
    [SerializeField] private TMP_InputField PlayerHpInput;
    [SerializeField] private TMP_Dropdown DifficultyChoice;
    [SerializeField] private TMP_Dropdown LevelChoice;
    
    private bool isStartMenu,isLevelChoice,isDebugScene,isEndScene;
    private string[] difficulty = {"Easy", "Normal", "Hard","Extreme"};
    private string selectedDifficulty;

    //EndScene
    [SerializeField] private TMP_Text AffectedNumberText;
    [SerializeField] private GameObject AffectedNumberArea;
    [SerializeField] private TMP_Text Title;
    private AudioSource audioSource;

    void Awake(){
        isStartMenu=SceneManager.GetActiveScene().name=="StartMenu";
        if(isStartMenu)BaseInfo.Reset();//回主页重置属性
        isLevelChoice=SceneManager.GetActiveScene().name=="LevelChoice";
        isDebugScene=SceneManager.GetActiveScene().name=="DebugScene";
        isEndScene=SceneManager.GetActiveScene().name=="EndScene";
        if(isDebugScene||isLevelChoice)InitDifficultyChoiceDropDown();
        if(isDebugScene)InitLevelChoiceDropDown();
    }
    void Start(){
        if(!isEndScene)return;
        EnterEndScene();
    }
    public void StartGame(){//开始界面、调试界面独有
        if(!isStartMenu&&!isDebugScene)return;
        if(isStartMenu){SceneManager.LoadScene("LevelChoice");return;}

        //DebugScene
        BaseInfo.IsDebug=true;
        BaseInfo.Level=LevelChoice.value+1;
        BaseInfo.Difficulty=DifficultyChoice.options[DifficultyChoice.value].text;
        BaseInfo.PlayerHP=int.TryParse(PlayerHpInput.text, out int php) && php > 0 ? php : 10;
        BaseInfo.BossHP = int.TryParse(BossHpInput.text, out int bhp) && bhp > 0 ? bhp : 200;
        BaseInfo.Affected=0;
        
        SceneManager.LoadScene("MainScene");
    }
    public void StartGame(int level){
        if(level<1 || level>=LevelData.levels.Length) level=1;
        selectedDifficulty=DifficultyChoice.options[DifficultyChoice.value].text;
        
        BaseInfo.IsDebug = false;  
        BaseInfo.Level=level;
        BaseInfo.Difficulty=selectedDifficulty;
        BaseInfo.Affected=0;

        SceneManager.LoadScene("MainScene");
    }
    public void Retry(){
        BaseInfo.Affected=0;
        SceneManager.LoadScene("MainScene");
    }
    public void Return(){
        if(isDebugScene)SceneManager.LoadScene("DebugScene");
        else SceneManager.LoadScene("StartMenu");
    }
    public void ExitGame(){
        Application.Quit();
    }
    public void Back(){SceneManager.LoadScene("StartMenu");}
    public void EnterDebug(){SceneManager.LoadScene("DebugScene");}
    public void EnterEndScene(){
        bool isWin=BaseInfo.IsWin;
        audioSource=GetComponent<AudioSource>();
        if(!isWin&&audioSource != null){
            AffectedNumberArea.SetActive(false);
            audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Lose"));
        }else{
            AffectedNumberText.text=BaseInfo.Affected.ToString();
            audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Win"));
        }
        Title.text=isWin?"You Win!":"You Lose!";
    }
    private void InitDifficultyChoiceDropDown(){
        DifficultyChoice.ClearOptions();
        foreach (var item in difficulty) {
            DifficultyChoice.options.Add(new TMP_Dropdown.OptionData(item));
        }
    }
    private void InitLevelChoiceDropDown(){
        LevelChoice.ClearOptions();
        for (int i=1;i<=5;i++) {
            LevelChoice.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }
    }
}
