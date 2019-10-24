using System.Collections.Generic;
using UnityEngine;

enum RewardedAdType
{
    UNITY,
    ADMOB,
}

enum BannerAdType
{
    NONE,
    ADMOB,
    UNITY,
}

enum InterstitialAdType
{
    UNITY,
    ADMOB,
}

[System.Serializable]
class ShowAdConfig
{
    public GameState GameStateForShowingAd = GameState.GameOver;
    public int GameStateCountForShowingAd = 3;
    public float ShowingAdDelay = 0.2f;
    public List<InterstitialAdType> ListInterstitialAdType = new List<InterstitialAdType>();
}


namespace JicaGames
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; set; }

        [Header("Show Banner Ad config")]
        [SerializeField] private BannerAdType bannerAdType = BannerAdType.NONE;
        [SerializeField] private float showingBannerAdDelay = 0.5f;


        [Header("Show Interstitial Ad Config")]
        [SerializeField] private List<ShowAdConfig> listShowInterstitialAdConfig = new List<ShowAdConfig>();

        [Header("Show Rewarded Video Ad Config")]
        [SerializeField] private float showingRewardedVideoAdDelay = 0.2f;
        [SerializeField] private List<RewardedAdType> listRewardedAdType = new List<RewardedAdType>();

        [Header("Rewarded Coins Config")]
        [SerializeField] private int minRewardedCoins = 40;
        [SerializeField] private int maxRewardedCoins = 80;
        [SerializeField] private float rewardDelay = 0.2f;

        private List<int> listShowAdCount = new List<int>();
        private RewardedAdType readyAdType = RewardedAdType.UNITY;

        private bool isCalledback = false;
        private bool isRewarded = false;
        private void OnEnable()
        {
            GameManager.GameStateChanged += GameManager_GameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.GameStateChanged -= GameManager_GameStateChanged;
        }

        private void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
               // DontDestroyOnLoad(gameObject);
            }
        }


        // Use this for initialization
        void Start()
        {
            foreach (ShowAdConfig o in listShowInterstitialAdConfig)
            {
                listShowAdCount.Add(o.GameStateCountForShowingAd);
            }

            //Show banner ad
            if (bannerAdType == BannerAdType.ADMOB)
            {
                AdmobController.Instance.LoadAndShowBanner(showingBannerAdDelay);
            }
            else if (bannerAdType == BannerAdType.UNITY)
            {
                UnityAdController.Instance.ShowBanner(showingBannerAdDelay);
            }

            //Request interstitial ads (unity ads auto requests interstitial)
            foreach (ShowAdConfig o in listShowInterstitialAdConfig)
            {
                foreach(InterstitialAdType a in o.ListInterstitialAdType)
                {
                    if (a == InterstitialAdType.ADMOB)
                    {
                        AdmobController.Instance.RequestInterstitial();
                    }
                }
            }

            //Request rewarded video (unity ads auto requests rewarded video)
            foreach (RewardedAdType o in listRewardedAdType)
            {
                if (o == RewardedAdType.ADMOB)
                {
                    AdmobController.Instance.RequestRewardedVideo();
                }
            }
        }

        private void Update()
        {
            if (isCalledback)
            {
                isCalledback = false;
                if (isRewarded)
                {
                    if (GameManager.Instance.GameState == GameState.Revive)
                    {
                        GameManager.Instance.SetContinueGame();
                    }
                    else
                    {
                        int coins = Random.Range(minRewardedCoins, maxRewardedCoins) / 5 * 5;
                        UIManager.Instance.StartReward(rewardDelay, coins);
                    }
                }
                else
                {
                    if (GameManager.Instance.GameState == GameState.Revive)
                        GameManager.Instance.GameOver();
                }
            }
        }


        private void GameManager_GameStateChanged(GameState obj)
        {
            for (int i = 0; i < listShowAdCount.Count; i++)
            {
                if (listShowInterstitialAdConfig[i].GameStateForShowingAd == obj)
                {
                    listShowAdCount[i]--;
                    if (listShowAdCount[i] <= 0)
                    {
                        //Reset gameCount 
                        listShowAdCount[i] = listShowInterstitialAdConfig[i].GameStateCountForShowingAd;
                        if (DataManager.Instance.freepassAdView)
                        {
                            return;
                        }
                            for (int a = 0; a < listShowInterstitialAdConfig[i].ListInterstitialAdType.Count; a++)
                            {
                                InterstitialAdType type = listShowInterstitialAdConfig[i].ListInterstitialAdType[a];
                                if (type == InterstitialAdType.ADMOB && AdmobController.Instance.IsInterstitialReady())
                                {
                                    AdmobController.Instance.ShowInterstitial(listShowInterstitialAdConfig[i].ShowingAdDelay);
                                    break;
                                }
                                else if (type == InterstitialAdType.UNITY && UnityAdController.Instance.IsInterstitialReady())
                                {
                                    UnityAdController.Instance.ShowInterstitial(listShowInterstitialAdConfig[i].ShowingAdDelay);
                                    break;
                                }
                            }
                        
                        }
                }
            }
        }


        /// <summary>
        /// Determines whether rewarded video ad is ready.
        /// </summary>
        /// <returns></returns>
        public bool IsRewardedVideoAdReady()
        {
            for(int i = 0; i < listRewardedAdType.Count; i++)
            {
                if (listRewardedAdType[i] == RewardedAdType.UNITY && UnityAdController.Instance.IsRewardedVideoReady())
                {
                    readyAdType = RewardedAdType.UNITY;
                    return true;
                }
                else if(listRewardedAdType[i] == RewardedAdType.ADMOB && AdmobController.Instance.IsRewardedVideoReady())
                {
                    readyAdType = RewardedAdType.ADMOB;
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Show the rewarded video ad with delay time
        /// </summary>
        /// <param name="delay"></param>
        public void ShowRewardedVideoAd()
        {
            if (readyAdType == RewardedAdType.UNITY)
            {
                UnityAdController.Instance.ShowRewardedVideo(showingRewardedVideoAdDelay);
            }
            else if (readyAdType == RewardedAdType.ADMOB)
            {
                AdmobController.Instance.ShowRewardedVideo(showingRewardedVideoAdDelay);
            }
        }

        public void OnRewardedVideoClosed(bool isFinishedVideo)
        {
            isCalledback = true;
            isRewarded = isFinishedVideo;
        }
    }
}
