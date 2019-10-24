using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using JicaGames;

public enum LobbyState
{
    Home,
    Upgrade,
    Shop,
    PopUp
}

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { private set; get; }
    public LobbyState LobbyState { get { return lobbyState; } }
        
    [SerializeField] private GameObject UpgradePanel = null;  
    [SerializeField] private GameObject ShopPanel = null;
    [SerializeField] private GameObject OptionUI = null;
    [SerializeField] private GameObject QuitUI = null;
    [SerializeField] private GameObject DailyRewardUI = null;
    //[SerializeField] private GameObject DailyRewardPanel = null;
    [SerializeField] private GameObject LanguageUI = null;
    [SerializeField] private GameObject TutorialUI = null;

    [SerializeField] private GameObject homeBtn = null;
    [SerializeField] private GameObject shopBtn = null;
    [SerializeField] private GameObject optionBtn = null;
    [SerializeField] private GameObject playBtn = null;
    [SerializeField] private GameObject dailyRewardBtn = null;
       
    [SerializeField] private GameObject shareBtn = null;
    [SerializeField] private GameObject soundOnBtn = null;
    [SerializeField] private GameObject soundOffBtn = null;

    [SerializeField] private RewardedCoinsController rewardCoinsControl = null;
    [SerializeField] private RewardedMissileController rewardMissileControl = null;
    [SerializeField] private RewardedBombController rewardBombControl = null;
    [SerializeField] private RewardedLaserController rewardLaserControl = null;
    [SerializeField] private Text dailyRewardTxt = null;

    [SerializeField] private Button[] rewardBtns;


    private LobbyState lobbyState = LobbyState.Home;
    private LobbyState lastLobbyState;

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

    private void Start()
    {
        CloseAllPopUps();
        ShopPanel.SetActive(false);
        UpgradePanel.SetActive(false);

        rewardCoinsControl.gameObject.SetActive(false);
        rewardMissileControl.gameObject.SetActive(false);
        rewardBombControl.gameObject.SetActive(false);
        rewardLaserControl.gameObject.SetActive(false);
        UpdateMuteButtons();

        if (DataManager.Instance.firstLogin == 0)
        {
            DailyRewardButtonReset();
            DataManager.Instance.firstLogin = 1;
        }
        else
        {
            for (int i = 0; i < rewardBtns.Length; i++)
            {
                rewardBtns[i].interactable = false;
            }
        }
    }


    private void Update()
    {
        Debug.Log(lobbyState);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (lobbyState == LobbyState.PopUp)
            {
                CloseAllPopUps();
                GetLastLobbyState();
                return;
            }
            if (QuitUI.activeInHierarchy)
            {
                QuitUI.SetActive(false);
            }
            else
            {
                QuitUI.SetActive(true);
            }            
        }

        if (DailyRewardManager.Instance.CanRewardNow())
        {
            if (DataManager.Instance.DailyRewardGet == 0)
            {
                DailyRewardButtonReset();
            }

            dailyRewardTxt.text = Lean.Localization.LeanLocalization.GetTranslationText("Reward_txt");

            DataManager.Instance.DailyRewardGet = 1;


        }
        else
        {
            string hours = DailyRewardManager.Instance.TimeUntilNextReward.Hours.ToString();
            string minutes = DailyRewardManager.Instance.TimeUntilNextReward.Minutes.ToString();
            string seconds = DailyRewardManager.Instance.TimeUntilNextReward.Seconds.ToString();
            dailyRewardTxt.text = hours + ":" + minutes + ":" + seconds;
        }

    }
    public void PlayBtn()
    {

        SceneManager.LoadScene("GamePlay");           
    }

    public void PlayButtonSound()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.button);
    }

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
        UpdateMuteButtons();
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

    public void UpgradeBtn()
    {
        UpgradePanel.SetActive(true);
        ShopPanel.SetActive(false);
        lobbyState = LobbyState.Upgrade;
    }
       

    public void ShopBtn()
    {
        ShopPanel.SetActive(true);
        UpgradePanel.SetActive(false);
        lobbyState = LobbyState.Shop;
    }
        
    public void BackToHome()
    {
        ShopPanel.SetActive(false);
        UpgradePanel.SetActive(false);
        lobbyState = LobbyState.Home;
    }

    //public void DailyRewardBtn()
    //{
    //    if (DailyRewardPanel.activeInHierarchy)
    //    {
    //        DailyRewardPanel.SetActive(false);
    //        rewardCoinsControl.gameObject.SetActive(false);
    //        rewardMissileControl.gameObject.SetActive(false);
    //        rewardBombControl.gameObject.SetActive(false);
    //        rewardLaserControl.gameObject.SetActive(false);
    //        CloseAllPopUps();
    //    }
    //    else
    //    {
    //        CloseAllPopUps();
    //        DailyRewardPanel.SetActive(true);
    //        rewardCoinsControl.gameObject.SetActive(false);
    //        rewardMissileControl.gameObject.SetActive(false);
    //        rewardBombControl.gameObject.SetActive(false);
    //        rewardLaserControl.gameObject.SetActive(false);
    //        if (LobbyState != LobbyState.PopUp)
    //        {
    //            lastLobbyState = LobbyState;
    //        }            
    //        lobbyState = LobbyState.PopUp;
    //    }
    //}

    public void RewardBtn()
    {
        DailyRewardManager.Instance.ResetNextRewardTime();
        StartCoroutine(RewardCoroutine(0.5f));
    }

    IEnumerator RewardCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        switch (DataManager.Instance.Days)
        {
            case 0:
                rewardCoinsControl.gameObject.SetActive(true);
                rewardCoinsControl.StartReward(DailyRewardManager.Instance.GetRandomReward());
                break;
            case 1:
                rewardMissileControl.gameObject.SetActive(true);
                rewardMissileControl.StartReward(DailyRewardManager.Instance.GetRewardMissile());
                break;
            case 2:
                rewardBombControl.gameObject.SetActive(true);
                rewardBombControl.StartReward(DailyRewardManager.Instance.GetRewardBomb());
                break;
            case 3:
                rewardLaserControl.gameObject.SetActive(true);
                rewardLaserControl.StartReward(DailyRewardManager.Instance.GetRewardLaser());
                break;
        }

        if (DataManager.Instance.Days == 3)
            DataManager.Instance.Days = 0;
        else
            DataManager.Instance.Days++;

        for (int i = 0; i < rewardBtns.Length; i++)
        {
            rewardBtns[i].interactable = false;
        }

        DataManager.Instance.DailyRewardGet = 0;
    }

    public void OptionBtn()
    {
        OptionUI.SetActive(true);
        SetLastLobbyState();
        lobbyState = LobbyState.PopUp;
    }
    public void CloseOptionBtn()
    {
        OptionUI.SetActive(false);
        GetLastLobbyState();
    }
    public void DailyRewardBtn()
    {
        DailyRewardUI.SetActive(true);
        SetLastLobbyState();
        lobbyState = LobbyState.PopUp;
    }
    public void CloseDailyRewardBtn()
    {
        DailyRewardUI.SetActive(false);
        GetLastLobbyState();
    }
    public void CloseAllPopUps()
    {
        DailyRewardUI.SetActive(false);
        OptionUI.SetActive(false);
        QuitUI.SetActive(false);
        LanguageUI.SetActive(false);
        TutorialUI.SetActive(false);        
    }

    public void GetLastLobbyState()
    {
        lobbyState = lastLobbyState;
    }
    public void SetLastLobbyState()
    {
        lastLobbyState = LobbyState;
    }

    public void QuitButton()
    {
        Application.Quit();
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

    public void DailyRewardButtonReset()
    {
        for (int i = 0; i < rewardBtns.Length; i++)
        {
            rewardBtns[i].interactable = false;
        }

        switch (DataManager.Instance.Days)
        {
            case 0:
                rewardBtns[0].interactable = true;
                break;
            case 1:
                rewardBtns[1].interactable = true;
                break;
            case 2:
                rewardBtns[2].interactable = true;
                break;
            case 3:
                rewardBtns[3].interactable = true;
                break;
        }
    }

}
