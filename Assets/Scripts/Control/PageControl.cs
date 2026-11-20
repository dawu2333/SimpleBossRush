using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum Page{StartMenu,LevelChoice,Main,Debug,End,WeaponChoice}

public class PageControl : MonoBehaviour{

    //Debug、StartMenu、LevelChoice
    [SerializeField] private TMP_InputField BossHpInput;
    [SerializeField] private TMP_InputField PlayerHpInput;
    [SerializeField] private TMP_Dropdown DifficultyChoice;
    [SerializeField] private TMP_Dropdown LevelChoice;
    [SerializeField] private TMP_Dropdown WeaponChoice;
    [SerializeField] private RectTransform StartMenufunctionArea;
    private Page currentPage;
    private WeaponKind[] weaponKinds = (WeaponKind[])System.Enum.GetValues(typeof(WeaponKind));
    private Difficulty selectedDifficulty;
    private WeaponKind selectedWeapon;

    //End
    [SerializeField] private TMP_Text AffectedNumberText;
    [SerializeField] private GameObject AffectedNumberArea;
    [SerializeField] private TMP_Text Title;
    private AudioSource audioSource;

    void Awake(){
        string SceneName=SceneManager.GetActiveScene().name;
        //判断当前场景
        currentPage=SceneName switch {
            "StartMenu"=>Page.StartMenu,
            "LevelChoice"=>Page.LevelChoice,
            "Debug"=>Page.Debug,
            "End"=>Page.End,
            "Main"=>Page.Main,
            "WeaponChoice"=>Page.WeaponChoice,
            _=>Page.StartMenu
        };
        //资源缓存
        audioSource=GetComponent<AudioSource>();
        
        if(currentPage==Page.StartMenu){
            BaseInfo.Reset();//回主页时重置属性
            GetFunctionMenuChild(1).gameObject.SetActive(PlayerPrefsControl.IsPassEx());//通关Ex难度解锁Debug页面
        }

        //初始化下拉框
        if(currentPage==Page.Debug||currentPage==Page.LevelChoice)InitDifficultyChoiceDropDown();
        if(currentPage==Page.Debug){
            InitLevelChoiceDropDown();
            InitWeaponChoiceDropDown();
        }
    }
    void Start(){
        if(!(currentPage==Page.End))return;
        EnterEndScene();
    }
    public void StartGame(){//开始、调试和武器选择界面独有
        switch (currentPage) {
            case Page.StartMenu:SceneManager.LoadScene("LevelChoice");break;
            case Page.WeaponChoice:
                //BaseInfo.weaponKind=selectedWeapon;//暂未实现武器选择
                SceneManager.LoadScene("Main");
                break;
            case Page.Debug:
                BaseInfo.isDebug=true;
                BaseInfo.level=LevelChoice.value+1;
                BaseInfo.difficulty=DifficultyData.ConvertToDifficulty(DifficultyChoice.options[DifficultyChoice.value].text);
                BaseInfo.weaponKind=WeaponKindData.ConvertToWeaponKind(WeaponChoice.options[WeaponChoice.value].text);
                BaseInfo.playerHP=int.TryParse(PlayerHpInput.text, out int php) && php > 0 ? php : 10;
                BaseInfo.bossHP = int.TryParse(BossHpInput.text, out int bhp) && bhp > 0 ? bhp : 200;
                BaseInfo.affected=0;
                SceneManager.LoadScene("Main");
                break;
            default:Debug.LogError("错误触发");break;
        }
    }
    
    public void StartGame(int level){//通过关卡按钮触发

        if(level<1 || level>=LevelData.levels.Length) level=1;
        selectedDifficulty=DifficultyData.ConvertToDifficulty(DifficultyChoice.options[DifficultyChoice.value].text);
        
        BaseInfo.isDebug = false;  
        BaseInfo.level=level;
        BaseInfo.difficulty=selectedDifficulty;
        BaseInfo.affected=0;

        SceneManager.LoadScene("WeaponChoice");
    }
    public void Retry(){//End专属
        BaseInfo.affected=0;
        SceneManager.LoadScene("Main");
    }
    public void Return(){//Debug、End
    #if UNITY_EDITOR
        if(BaseInfo.isDebug)SceneManager.LoadScene("Debug");
    #endif
        else SceneManager.LoadScene("StartMenu");
    }

    public void ExitGame(){//退出游戏
        Application.Quit();
    }
    public void Back(){
        if(currentPage==Page.LevelChoice){SceneManager.LoadScene("StartMenu");return;}
        if(currentPage==Page.WeaponChoice){SceneManager.LoadScene("LevelChoice");return;}
    }
    public void EnterDebugScene(){SceneManager.LoadScene("Debug");}
    public void EnterEndScene(){
        bool isWin=BaseInfo.isWin;
        if(!isWin){//输赢
            AffectedNumberArea.SetActive(false);
            if(audioSource != null)audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Lose"));
        }else{
            AffectedNumberText.text=BaseInfo.affected.ToString();
            if(audioSource != null)audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Win"));
        }
        Title.text=isWin?"You Win!":"You Lose!";
    }
    public void EnterWeaponChoiceScene() {SceneManager.LoadScene("WeaponChoice");}
    //初始化难度选择
    private void InitDifficultyChoiceDropDown(){
        DifficultyChoice.ClearOptions();
        foreach (var item in DifficultyData.GetDifficulties()) {
            DifficultyChoice.options.Add(new TMP_Dropdown.OptionData(DifficultyData.ConvertToString(item)));
        }
        //默认选中此前选过的难度
        //DifficultyChoice.options[DifficultyChoice.value].text = DifficultyData.ConvertToString(BaseInfo.difficulty);
        DifficultyChoice.RefreshShownValue();
    }
    //初始化武器选择
    private void InitWeaponChoiceDropDown(){
        WeaponChoice.ClearOptions();
        foreach (var item in weaponKinds) {
            WeaponChoice.options.Add(new TMP_Dropdown.OptionData(WeaponKindData.ConvertToString(item)));
        }
        WeaponChoice.RefreshShownValue();
    }
    //初始化关卡选择
    private void InitLevelChoiceDropDown(){
        LevelChoice.ClearOptions();
        for (int i=1;i<LevelData.levels.Length;i++) {
            LevelChoice.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }
        LevelChoice.RefreshShownValue();
    }
    private RectTransform GetFunctionMenuChild(int i) {
        return (RectTransform)StartMenufunctionArea.GetChild(i);
    }
}
