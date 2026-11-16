using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PageControl : MonoBehaviour{

    //Debug、StartMenu、LevelChoice
    [SerializeField] private TMP_InputField BossHpInput;
    [SerializeField] private TMP_InputField PlayerHpInput;
    [SerializeField] private TMP_Dropdown DifficultyChoice;
    [SerializeField] private TMP_Dropdown LevelChoice;
    [SerializeField] private RectTransform StartMenufunctionArea;
    
    private bool isStartMenu,isLevelChoice,isDebug,isEnd;
    private string[] difficulty = {"Easy", "Normal", "Hard"};
    private string selectedDifficulty;

    //End
    [SerializeField] private TMP_Text AffectedNumberText;
    [SerializeField] private GameObject AffectedNumberArea;
    [SerializeField] private TMP_Text Title;
    private AudioSource audioSource;

    void Awake(){

        isStartMenu=SceneManager.GetActiveScene().name=="StartMenu";
        isLevelChoice=SceneManager.GetActiveScene().name=="LevelChoice";
        isDebug=SceneManager.GetActiveScene().name=="Debug";
        isEnd=SceneManager.GetActiveScene().name=="End";

        //资源缓存
        audioSource=GetComponent<AudioSource>();
        
        if(isStartMenu){
            BaseInfo.Reset();//回主页重置属性
            GetFunctionMenuChild(1).gameObject.SetActive(PlayerPrefsControl.IsPass());
        }
        if(isDebug||isLevelChoice)InitDifficultyChoiceDropDown();
        if(isDebug)InitLevelChoiceDropDown();
    }
    void Start(){
        if(!isEnd)return;
        EnterEndScene();
    }
    public void StartGame(){//开始界面、调试界面独有
        if(!isStartMenu&&!isDebug)return;
        if(isStartMenu){SceneManager.LoadScene("LevelChoice");return;}

        //Debug
        BaseInfo.IsDebug=true;
        BaseInfo.Level=LevelChoice.value+1;
        BaseInfo.Difficulty=DifficultyChoice.options[DifficultyChoice.value].text;
        BaseInfo.PlayerHP=int.TryParse(PlayerHpInput.text, out int php) && php > 0 ? php : 10;
        BaseInfo.BossHP = int.TryParse(BossHpInput.text, out int bhp) && bhp > 0 ? bhp : 200;
        BaseInfo.Affected=0;
        
        SceneManager.LoadScene("Main");
    }
    public void StartGame(int level){
        if(level<1 || level>=LevelData.levels.Length) level=1;
        selectedDifficulty=DifficultyChoice.options[DifficultyChoice.value].text;
        
        BaseInfo.IsDebug = false;  
        BaseInfo.Level=level;
        BaseInfo.Difficulty=selectedDifficulty;
        BaseInfo.Affected=0;

        SceneManager.LoadScene("Main");
    }
    public void Retry(){
        BaseInfo.Affected=0;
        SceneManager.LoadScene("Main");
    }
    public void Return(){
        if(isDebug)SceneManager.LoadScene("Debug");
        else SceneManager.LoadScene("StartMenu");
    }
    public void ExitGame(){
        Application.Quit();
    }
    public void Back(){SceneManager.LoadScene("StartMenu");}
    public void EnterDebugScene(){SceneManager.LoadScene("Debug");}
    public void EnterEndScene(){
        bool isWin=BaseInfo.IsWin;
        if(!isWin){
            AffectedNumberArea.SetActive(false);
            if(audioSource != null)audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Lose"));
        }else{
            AffectedNumberText.text=BaseInfo.Affected.ToString();
            if(audioSource != null)audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Win"));
        }
        Title.text=isWin?"You Win!":"You Lose!";
    }
    public void EnterWeaponChoiceScene() {SceneManager.LoadScene("WeaponChoice");}
    private void InitDifficultyChoiceDropDown(){
        DifficultyChoice.ClearOptions();
        foreach (var item in difficulty) {
            DifficultyChoice.options.Add(new TMP_Dropdown.OptionData(item));
        }
        if(PlayerPrefsControl.IsPass())DifficultyChoice.options.Add(new TMP_Dropdown.OptionData("Extreme"));//若通关，追加极限模式
    }
    private void InitLevelChoiceDropDown(){
        LevelChoice.ClearOptions();
        for (int i=1;i<LevelData.levels.Length;i++) {
            LevelChoice.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }
        
    }
    private RectTransform GetFunctionMenuChild(int i) {
        return (RectTransform)StartMenufunctionArea.GetChild(i);
    }
}
