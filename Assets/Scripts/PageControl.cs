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
    public void StartGame(){
        if(!isStartMenu&&!isDebugScene)return;
        if(isStartMenu){SceneManager.LoadScene("LevelChoice");return;}

        //DebugScene
        int PlayerHp = int.TryParse(PlayerHpInput.text, out int php) && php > 0 ? php : 10;
        int BossHp = int.TryParse(BossHpInput.text, out int bhp) && bhp > 0 ? bhp : 200;
        int selectedLevel=LevelChoice.value+1;
        selectedDifficulty=DifficultyChoice.options[DifficultyChoice.value].text;

        PlayerPrefs.SetInt("Level",selectedLevel);
        PlayerPrefs.SetString("Difficulty",selectedDifficulty);
        PlayerPrefs.SetInt("Affected",0);
        PlayerPrefs.SetInt("PlayerHP",PlayerHp);
        PlayerPrefs.SetInt("BossHP",BossHp);
        
        SceneManager.LoadScene("MainScene");
    }
    public void StartGame(int level){
        if(level<1 || level>=LevelData.levels.Length) level=1;
        selectedDifficulty=DifficultyChoice.options[DifficultyChoice.value].text;
        
        PlayerPrefs.SetInt("Level",level);
        PlayerPrefs.SetInt("Affected",0);
        PlayerPrefs.SetString("Difficulty",selectedDifficulty);

        SceneManager.LoadScene("MainScene");
    }
    public void Retry(){
        PlayerPrefs.SetInt("Affected",0);
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
        bool isWin=PlayerPrefs.GetInt("IsWin")==1;
        audioSource=GetComponent<AudioSource>();
        if(!isWin){
            AffectedNumberArea.SetActive(false);
            audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Lose"));
        }else{
            AffectedNumberText.text=PlayerPrefs.GetInt("Affected").ToString();
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
