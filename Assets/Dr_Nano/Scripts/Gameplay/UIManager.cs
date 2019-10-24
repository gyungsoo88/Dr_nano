using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using JicaGames;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { private set; get; }


    //Gameplay UI
    [SerializeField] private GameObject gameplayUI = null;
    [SerializeField] private Text scoreTxt = null;
    [SerializeField] private Text coinsTxt = null;
    [SerializeField] private Text patientTxt = null;
    [SerializeField] private Text missileTxt = null;
    [SerializeField] private Text bombTxt = null;
    [SerializeField] private Text laserTxt = null;
    [SerializeField] private Text hpTxt = null;
    [SerializeField] private Slider hpSlider = null;            
    [SerializeField] private Text TutorialName = null;
    [SerializeField] private Text TutorialDescription = null;
    [SerializeField] private Image[] TutorialCellImage = new Image[7];
    [SerializeField] private RectTransform missileImgTrans = null;
    [SerializeField] private RectTransform bombImgTrans = null;
    [SerializeField] private RectTransform laserImgTrans = null;
    public GameObject TutorialPanel = null;
    public GameObject Tutorial_basic = null;


    public bool Stop = false;
    [SerializeField] private GameObject Stoplobby = null; //19/09/09 스톱시 로비로 갈 수있게 하는 버튼

    //Revive UI
    [SerializeField] private GameObject reviveUI = null;
    [SerializeField] private Image reviveCoverImg = null;

    //Game over UI
    [SerializeField]private GameObject gameOverUI = null;
    [SerializeField] private Text bestScoreTxt = null;
    [SerializeField] private Text EndScoreTxt = null; //19/09/11 게임오버시 점수표시
    [SerializeField] private GameObject gameName = null;
    [SerializeField] private RawImage shareImage = null;
    [SerializeField] private Button dailyRewardBtn = null;
    [SerializeField] private Text dailyRewardTxt = null;
    [SerializeField] private GameObject watchAdForCoinsBtn = null;
    [SerializeField] private GameObject playBtn = null;
    [SerializeField] private GameObject restartBtn = null;
    [SerializeField] private GameObject shareBtn = null;
    [SerializeField] private GameObject soundOnBtn = null;
    [SerializeField] private GameObject soundOffBtn = null;
    [SerializeField] private Animator servicesUIAnim = null;
    [SerializeField] private Animator settingUIAnim = null;
    [SerializeField] private AnimationClip servicesUI_Show = null;
    [SerializeField] private AnimationClip servicesUI_Hide = null;
    [SerializeField] private AnimationClip settingUI_Show = null;
    [SerializeField] private AnimationClip settingUi_Hide = null;
    [SerializeField] private RewardedCoinsController rewardCoinsControl = null;
    [SerializeField] private RectTransform coinImgTrans = null;

    public Image Goodone = null;
    public Image Badone = null;




    private void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }


    private void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    private void GameManager_GameStateChanged(GameState obj)
    {
        if (obj == GameState.Revive)
        {
            reviveUI.SetActive(true);
            StartCoroutine(ReviveCountDown());
        }
        else if (obj == GameState.GameOver)
        {
            Invoke("ShowGameOverUI", 0.5f);
        }
        else if (obj == GameState.Playing)
        {
            gameplayUI.SetActive(true);
            gameOverUI.SetActive(false);
            reviveUI.SetActive(false);
            rewardCoinsControl.gameObject.SetActive(false);
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }




    // Use this for initialization
    void Start () {

    
        if (!GameManager.isRestart) //This is the first load
        {
            gameplayUI.SetActive(false);
            reviveUI.SetActive(false);
            rewardCoinsControl.gameObject.SetActive(false);

            shareImage.gameObject.SetActive(false);
            bestScoreTxt.gameObject.SetActive(false);
            restartBtn.SetActive(false);
            watchAdForCoinsBtn.SetActive(false);
            dailyRewardBtn.gameObject.SetActive(false);
            shareBtn.SetActive(false);
            
        }
        TutorialPanel.SetActive(false);
        Tutorial_basic.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {

        if (Stop && !DataManager.Instance.tutorialON)
        {
            SoundManager.Instance.PauseMusic();
        }
        else if (!DataManager.Instance.tutorialON)
        {
            SoundManager.Instance.ResumeMusic();
        }
        UpdateMuteButtons();
        //2019/09/11 인게임 화면에 SCORE : 문구 추가, 게임오버시 스코어 표시
        EndScoreTxt.text = DataManager.Instance.Subject_txt + DataManager.Instance.GameStartCount+"\n"+ DataManager.Instance.PatientName + "\n" + DataManager.Instance.lived_for + (((DataManager.Instance.Score)/ DataManager.Instance.ScoreToAge + 1).ToString()) +DataManager.Instance.years_txt;
        scoreTxt.text =  DataManager.Instance.Score_txt + (DataManager.Instance.Score);
        patientTxt.text = DataManager.Instance.Subject_txt + DataManager.Instance.GameStartCount + "       " + DataManager.Instance.PatientName +"        "+ DataManager.Instance.lived_for + (((DataManager.Instance.Score) / DataManager.Instance.ScoreToAge + 1).ToString()) + DataManager.Instance.years_txt;
        bestScoreTxt.text = DataManager.Instance.BestScore_txt + (((DataManager.Instance.Score) / DataManager.Instance.ScoreToAge+1).ToString()) + DataManager.Instance.years_txt;
        coinsTxt.text = DataManager.Instance.Coins.ToString();

        missileTxt.text = "X" + DataManager.Instance.Missiles.ToString();
        bombTxt.text = "X" + DataManager.Instance.Bombs.ToString();
        laserTxt.text = "X" + DataManager.Instance.Lasers.ToString();

        if(DailyRewardManager.Instance.CanRewardNow())
        {
            dailyRewardTxt.text = DataManager.Instance.Grab_txt;
            dailyRewardBtn.interactable = true;
        }
        else
        {
            string hours = DailyRewardManager.Instance.TimeUntilNextReward.Hours.ToString();
            string minutes = DailyRewardManager.Instance.TimeUntilNextReward.Minutes.ToString();
            string seconds = DailyRewardManager.Instance.TimeUntilNextReward.Seconds.ToString();
            dailyRewardTxt.text = hours + ":" + minutes + ":" + seconds;
            dailyRewardBtn.interactable = false;
        }

        //게임 정지상태이면 로비로 돌아가능 버튼 팝업을 활성화
        if (Stop == false||DataManager.Instance.tutorialON)
        {
            Stoplobby.SetActive(false);
        }else if (Stop == true && !DataManager.Instance.tutorialON)
        {
            Stoplobby.SetActive(true);
        }
	}

    ////////////////////////////Publish functions

    public void PlayButtonSound()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.button);
    }

    public void MissileBtn()
    {
       
        GameManager.Instance.CreateMissile(PlayerController.Instance.transform.position);
    }

    public void BombBtn()
    {
        
        GameManager.Instance.CreateBomb(PlayerController.Instance.transform.position);
    }
    
    public void LaserBtn()
    {
        
        float y = Camera.main.ViewportToWorldPoint(new Vector2(0, 0)).y;
        GameManager.Instance.CreateLaser(new Vector2(0, y));
    }
  
    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
    }

    public void NativeShareBtn()
    {
        ShareManager.Instance.NativeShare();
    }

    public void RateAppBtn()
    {
        Application.OpenURL(ShareManager.Instance.AppUrl);
    }

    public void FacebookShareBtn()
    {
        ShareManager.Instance.FacebookShare();
    }

    //2019/09/10 게임매니저에서 게임시작할 때 사용하도록 하기위해
    //PlayBtn()->PlayingUI()

    public void PlayingUI()
    {
        playBtn.SetActive(false);
        gameOverUI.SetActive(false);
        gameplayUI.SetActive(true);
    }

    public void PlayBtn()
    {
        playBtn.SetActive(false);
        gameOverUI.SetActive(false);
        gameplayUI.SetActive(true);
        GameManager.Instance.PlayingGame();
    }

    public void RestartBtn(float delay)
    {

    GameManager.Instance.LoadScene(SceneManager.GetActiveScene().name, delay);
    }

    public void PlayRestartBtn(float delay)
    {
        Time.timeScale = 1;
        TutorialPanel.SetActive(false);
        Tutorial_basic.SetActive(false);
        DataManager.Instance.tutorialON = false;
        GameManager.Instance.LoadScene(SceneManager.GetActiveScene().name, delay);
        
    }

    public void SettingBtn()
    {
        settingUIAnim.Play(settingUI_Show.name);
        servicesUIAnim.Play(servicesUI_Hide.name);
    }

    public void BackBtn()
    {
        settingUIAnim.Play(settingUi_Hide.name);
        servicesUIAnim.Play(servicesUI_Show.name);
    }

    public void DailyRewardBtn()
    {
        DailyRewardManager.Instance.ResetNextRewardTime();
        StartReward(0.5f, DailyRewardManager.Instance.GetRandomReward());
    }


    public void WatchAdForCoinsBtn()
    {
        watchAdForCoinsBtn.SetActive(false);
        AdManager.Instance.ShowRewardedVideoAd();
    }

    public void ReviveBtn()
    {
        reviveUI.SetActive(false);
        AdManager.Instance.ShowRewardedVideoAd();
    }

    public void SkipBtn()
    {
        GameManager.Instance.GameOver();
    }

    public void UpgradeBtn()
    {
        GameManager.Instance.LoadScene("Upgrade", 0.5f);
    }

    public void CharacterBtn()
    {
        GameManager.Instance.LoadScene("Character", 0.5f);
    }

   //2019/09/09 게임 끝났을 경우 로비씬으로 이동하기버튼 
   public void EndlobbyBtn()
    {
        GameManager.Instance.LoadScene("Lobby", 0.5f);
    }

    //게임 정지 도중에 로비씬으로 이동하기
    public void StopLobby()
    {
        Time.timeScale = 1;
        TutorialPanel.SetActive(false);
        Tutorial_basic.SetActive(false);
        DataManager.Instance.tutorialON = false;
        GameManager.Instance.LoadScene("Lobby", 0.1f);
       

    }

  


    /////////////////////////////Private functions
    void UpdateMuteButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            soundOnBtn.gameObject.SetActive(false);
            soundOffBtn.gameObject.SetActive(true);
        }
        else
        {
            soundOnBtn.gameObject.SetActive(true);
            soundOffBtn.gameObject.SetActive(false);
        }
    }

    void ShowGameOverUI()
    {
        gameplayUI.SetActive(true);
        reviveUI.SetActive(false);
        rewardCoinsControl.gameObject.SetActive(false);
        gameOverUI.SetActive(true);

        bestScoreTxt.gameObject.SetActive(true);
        //2019/09/25 게임이름 false표시
        gameName.SetActive(false);
        shareImage.gameObject.SetActive(true);
        shareImage.texture = GameManager.Instance.LoadedScrenshot();

        playBtn.SetActive(false);
        restartBtn.SetActive(true);

        dailyRewardBtn.gameObject.SetActive(true);
        watchAdForCoinsBtn.SetActive(AdManager.Instance.IsRewardedVideoAdReady());

        shareBtn.SetActive(true);
    }

    IEnumerator ReviveCountDown()
    {
        float t = 0;
        while (t < DataManager.Instance.reviveCountDownTime)
        {
            if (!reviveUI.activeInHierarchy)
                yield break;
            t += Time.deltaTime;
            float factor = t / DataManager.Instance.reviveCountDownTime;
            reviveCoverImg.fillAmount = Mathf.Lerp(1, 0, factor);
            yield return null;
        }
        reviveUI.SetActive(false);
        GameManager.Instance.GameOver();
    }




    public void StartReward(float delay, int coins)
    {
        StartCoroutine(RewardingCoins(delay, coins));
    }
    IEnumerator RewardingCoins(float delay, int coins)
    {
        yield return new WaitForSeconds(delay);
        rewardCoinsControl.gameObject.SetActive(true);
        rewardCoinsControl.StartReward(coins);
    }


    public Vector2 GetImgWorldPos(ItemType type)
    {
        switch(type)
        {
            case ItemType.COIN:
                return Camera.main.ScreenToWorldPoint(coinImgTrans.position);
            case ItemType.MISSILE:
                return Camera.main.ScreenToWorldPoint(missileImgTrans.position);
            case ItemType.BOMB:
                return Camera.main.ScreenToWorldPoint(bombImgTrans.position);
            case ItemType.LASER:
                return Camera.main.ScreenToWorldPoint(laserImgTrans.position);
            default:
                return new Vector2();
        }
    }

    public void SetHPText(int HP)
    {
        hpTxt.text = HP.ToString();
    }

    public void SetHPBarValue(float val)
    {
        hpSlider.value = val;
    }

    //게임 도중 정지버튼
    public void StopBtn()
    {
        if (Stop == false)
        {
            Stop = true;
            
            Time.timeScale = 0;
        }
        else if (Stop == true)
        {
            Stop = false;
            
            Time.timeScale = 0.3f;
            TutorialPanel.SetActive(false);
            Tutorial_basic.SetActive(false);
            DataManager.Instance.tutorialON = false;
        }
    }

    public string RandomName_Generatior()
    {
        int name1_index = Random.Range(0, 10);
        int name2_index = Random.Range(0, 10);
        string name1 = null;
        string name2 = null;
        Lean.Localization.LeanLocalization.UpdateTranslations();
        switch (name1_index)
        {
            case 0: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_0"); break;
            case 1: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_1"); break;
            case 2: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_2"); break;
            case 3: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_3"); break;
            case 4: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_4"); break;
            case 5: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_5"); break;
            case 6: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_6"); break;
            case 7: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_7"); break;
            case 8: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_8"); break;
            case 9: name1 = Lean.Localization.LeanLocalization.GetTranslationText("Name1_9"); break;
        }
        Lean.Localization.LeanLocalization.UpdateTranslations();
        switch (name2_index)
        {
            case 0: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_0"); break;
            case 1: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_1"); break;
            case 2: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_2"); break;
            case 3: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_3"); break;
            case 4: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_4"); break;
            case 5: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_5"); break;
            case 6: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_6"); break;
            case 7: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_7"); break;
            case 8: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_8"); break;
            case 9: name2 = Lean.Localization.LeanLocalization.GetTranslationText("Name2_9"); break;
        }
        return name1 + " " + name2;
    }



    //튜토리얼 창 뛰움 게임중
    public void TutorialPopup(BallType type)
    {
        if (type != 0)
        {
            Tutorial_basic.SetActive(false);
            TutorialPanel.SetActive(true);
        }

        switch ((int)type)
        {
            case 0: //초기 튜토리얼
                {
                    Tutorial_basic.SetActive(true);
                    break;

                }

            case 1: //일반세포
                {
                    TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("healinggerm_name");
                    TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("healing_txt");
                    for (int i = 0; i < TutorialCellImage.Length; ++i)
                    {
                        if (i == 1&& TutorialCellImage[i]!=null)
                            TutorialCellImage[i].enabled = true;
                        else if (TutorialCellImage[i] != null)
                            TutorialCellImage[i].enabled = false;
                    }
                    break;
                }
            case 2:// 종양세포(자라지 않는 암세포)
                {
                    TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("devilgerm_name");
                    TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("devil_txt");
                    for (int i = 0; i < TutorialCellImage.Length; ++i)
                    {
                        if (i == 2 && TutorialCellImage[i] != null)
                            TutorialCellImage[i].enabled = true;
                        else if (TutorialCellImage[i] != null)
                            TutorialCellImage[i].enabled = false;
                    }
                    break;
                }

            case 3:// 암세포(시간에 따라 크기가 커지는 암세포)
                {
                    TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("expanding_germ_name");
                    TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("expanding_txt");
                    for (int i = 0; i < TutorialCellImage.Length; ++i)
                    {
                        if (i == 3 && TutorialCellImage[i] != null)
                            TutorialCellImage[i].enabled = true;
                        else if (TutorialCellImage[i] != null)
                            TutorialCellImage[i].enabled = false;
                    }
                    break;
                } ;
            case 4:// 분열하는 세포
                {
                    {
                        TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("zombiegerm_name");
                        TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("zombie_txt");
                        for (int i = 0; i < TutorialCellImage.Length; ++i)
                        {
                            if (i == 4 && TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = true;
                            else if (TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = false;
                        }
                        break;
                    };

                } ;
            case 5:
                {
                    {
                        TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("shield_germ");
                        TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("Shield_txt");
                        for (int i = 0; i < TutorialCellImage.Length; ++i)
                        {
                            if (i == 5 && TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = true;
                            else if (TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = false;
                        }
                        break;
                    };
                }; // 체력없는 세포(방패)
            case 6:
                {
                    {
                        TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("Cancer_name");
                        TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("cancer_txt");
                        for (int i = 0; i < TutorialCellImage.Length; ++i)
                        {
                            if (i == 6 && TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = true;
                            else if (TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = false;
                        }
                        break;
                    };
                }; // 보스
            case 7:
                {
                    {
                        TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("weakness");
                        TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("weakness_txt");
                        for (int i = 0; i < TutorialCellImage.Length; ++i)
                        {
                            if (i == 7 && TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = true;
                            else if (TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = false;
                        }
                        break;
                    };
                }; // 보스 약점
            case 8:
                {
                    {
                        TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("vaccine_germ");
                        TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("vaccine_txt");
                        for (int i = 0; i < TutorialCellImage.Length; ++i)
                        {
                            if (i == 8 && TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = true;
                            else if (TutorialCellImage[i] != null)
                                TutorialCellImage[i].enabled = false;
                        }
                        break;
                    };
                }
                    ;// 죽으면 미사일이 되서 날아감
            case 9:
                {
                    {
                        {
                            TutorialName.text = Lean.Localization.LeanLocalization.GetTranslationText("spongegerm_name");
                            TutorialDescription.text = Lean.Localization.LeanLocalization.GetTranslationText("sponge_txt");
                            for (int i = 0; i < TutorialCellImage.Length; ++i)
                            {
                                if (i == 9 && TutorialCellImage[i] != null)
                                    TutorialCellImage[i].enabled = true;
                                else if (TutorialCellImage[i] != null)
                                    TutorialCellImage[i].enabled = false;
                            }
                            break;
                        };
                    }
                };// 일정 수치 맞으면 증가하다가 없어짐
        }

    }

}