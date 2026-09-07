using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PageControl : MonoBehaviour{
    public GameObject BossHpInput;
    public GameObject PlayerHpInput;
    public GameObject DifficultyChoice;
    public GameObject LevelChoice;
    public GameObject AffectedText;
    public GameObject AffectedArea;
    public GameObject Title;
    private int BossHp;
    private bool isStartMenu,isLevelChoice,isDebugScene,isEndScene;
    private int PlayerHp,selectedLevel;
    private string[] difficulty = {"Easy", "Normal", "Hard","Extreme"};
    private string selectedDifficulty;
    private AudioSource audioSource;
    void Awake(){
        isStartMenu=SceneManager.GetActiveScene().name=="StartMenu"?true:false;
        isLevelChoice=SceneManager.GetActiveScene().name=="LevelChoice"?true:false;
        isDebugScene=SceneManager.GetActiveScene().name=="DebugScene"?true:false;
        isEndScene=SceneManager.GetActiveScene().name=="EndScene"?true:false;
        if(isDebugScene||isLevelChoice)IniteDropDown();
        if(isDebugScene)IniteDropDown2();
    }
    void Start(){
        if(isEndScene){
            bool isWin=PlayerPrefs.GetInt("IsWin")==1;
            audioSource=GetComponent<AudioSource>();
            if(!isWin){
                AffectedArea.SetActive(false);
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Lose"));
            }else{
                AffectedText.GetComponent<TMP_Text>().text=PlayerPrefs.GetInt("Affected").ToString();
                audioSource.PlayOneShot(Resources.Load<AudioClip>("AudioClip/Win"));
            }
            Title.GetComponent<TMP_Text>().text=PlayerPrefs.GetInt("IsWin")==1?"You Win!":"You Lose!";
        }
    }

    public void StartGame(){
        if(isDebugScene){
            PlayerPrefs.SetInt("Affected",0);
            if(!string.IsNullOrEmpty(PlayerHpInput.GetComponent<TMP_InputField>().text))PlayerHp = int.Parse(PlayerHpInput.GetComponent<TMP_InputField>().text);
            if(!string.IsNullOrEmpty(BossHpInput.GetComponent<TMP_InputField>().text))BossHp = int.Parse(BossHpInput.GetComponent<TMP_InputField>().text);
            selectedDifficulty = difficulty[DifficultyChoice.GetComponent<TMP_Dropdown>().value];
            selectedLevel=LevelChoice.GetComponent<TMP_Dropdown>().value+1;
            PlayerPrefs.SetInt("PlayerHP",PlayerHp==0?10:PlayerHp);
            PlayerPrefs.SetInt("BossHP",BossHp==0?200:BossHp);
            PlayerPrefs.SetString("Difficulty",selectedDifficulty==""?"Easy":selectedDifficulty);
            PlayerPrefs.SetInt("Level",selectedLevel);
            //Debug.Log("level="+selectedLevel+"difficulty="+selectedDifficulty);
            SceneManager.LoadScene("MainScene");
        }else if(isStartMenu){
            SceneManager.LoadScene("LevelChoice");
        }else if(isLevelChoice){
            PlayerPrefs.SetInt("Affected",0);
            PlayerPrefs.SetInt("PlayerHP",10);
            PlayerPrefs.SetInt("BossHP",200);
            PlayerPrefs.SetInt("Level",3);
            selectedDifficulty = difficulty[DifficultyChoice.GetComponent<TMP_Dropdown>().value];
            PlayerPrefs.SetString("Difficulty",selectedDifficulty==""?"Easy":selectedDifficulty);
            SceneManager.LoadScene("MainScene");
        }
    }
    public void StartGame(int level){
        int PHP=10,BHP=200;
        PlayerPrefs.SetInt("Level",level);
        if(level==1){PHP=10;BHP=200;}
        else if(level==2){PHP=10;BHP=250;}
        else if(level==3){PHP=15;BHP=400;}
        else if(level==4){PHP=25;BHP=150;}
        else if(level==5){PHP=20;BHP=800;}

        PlayerPrefs.SetInt("Affected",0);
        PlayerPrefs.SetInt("PlayerHP",PHP);
        PlayerPrefs.SetInt("BossHP",BHP);
        selectedDifficulty = difficulty[DifficultyChoice.GetComponent<TMP_Dropdown>().value];
        PlayerPrefs.SetString("Difficulty",selectedDifficulty==""?"Easy":selectedDifficulty);
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
    private void IniteDropDown(){
        DifficultyChoice.GetComponent<TMP_Dropdown>().ClearOptions();
        foreach (var item in difficulty) {
            DifficultyChoice.GetComponent<TMP_Dropdown>().options.Add(new TMP_Dropdown.OptionData(item));
        }
    }
    private void IniteDropDown2(){
        LevelChoice.GetComponent<TMP_Dropdown>().ClearOptions();
        for (int i=1;i<=5;i++) {
            LevelChoice.GetComponent<TMP_Dropdown>().options.Add(new TMP_Dropdown.OptionData(i.ToString()));
        }
    }
}
