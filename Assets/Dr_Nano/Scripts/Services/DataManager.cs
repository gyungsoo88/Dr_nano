using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using JicaGames;
/// <summary>
/// 19.09.24 kim hyung gyun
/// 구글에서 적용될 업적에 대한 문구
/// </summary>
public struct GoogleAchievementText
{
    public const string countRemovalBadCell = "이제 내겐 균 밖에 보이지 않아";
    public const string fullGainPoint = "되는데요";
    public const string maxUpgrade = "완전무장";
    public const string allGatheringSkin = "이런 친구들과 함께라면..";
    public const string maxGainCoin = "알부자";
}
public class DataManager : MonoBehaviour
{
    /// <summary>
    /// 19.09.06 - kim hyung gyun
    /// 데이터매니저 하나로 다른 coin, Item, Score,Charactor,UpGrade 매니저를 통합한다
    /// </summary>
    public static DataManager Instance { private set; get; }

    public static event Action<int> CoinsUpdated = delegate { };
    public static event Action<int> ScoreUpdated = delegate { };
    public static event Action<int> BestScoreUpdated = delegate { };

    public static readonly string Selected_Character_Key = "ONEFALL_CURRENT_CHARACTER";
    public static readonly string Day_Of_Count = "DAYS";
    public static readonly string Kill_Count = "KILL_COUNT";
    public static readonly string Gathering_Score = "GATHERING_SCORE";
    public static readonly string GameStart_Count = "GAMESTART_COUNT"; 
    public static readonly string Is_New_Subject = "Is_New_Subject";

    private const string SS_Level_PPK = "SS_LEVEL_KEY";
    private const string DP_Level_PPK = "DP_LEVEL_KEY";

    public int Max_SS_Level = 50;
    public int Max_DP_Level = 50;

    private const string PPK_BOMB = "BOMB";
    private const string PPK_LASER = "LASER";
    private const string PPK_MISSILE = "MISSILE";

    // key name to store high score in PlayerPrefs
    const string PPK_COINS = "ONEFALL_COINS";

    // key name to store high score in PlayerPrefs
    private const string BESTSCORE = "BESTSCORE";

    private static float originalShootingWaitTime = 0.12f;
    private static float originalBulletSpeed = 40f;
    private static float minShootingWaitTime = 0.03f;
    private static float DPratio = 0.05f;
    private static float originalDP = 0.02f;
    /// <summary>
    /// item 초기수치
    /// </summary>
    [SerializeField]
    private int initialMissiles = 1;
    [SerializeField]
    private int initialBombs = 1;
    [SerializeField]
    private int initialLasers = 1;
    [SerializeField]
    int initialCoins = 0;
    // Show the current coins value in editor for easy testing
    [SerializeField]
    int currentCoins;
    [Header("Upgrade Config")]
    public int SSUpgradePrice = 100;
    public int DPUpgradePrice = 100;
    public int DefaultUpgradePrice = 100;
    public int UpgradeIncrese = 10;

    [Header("Gameplay Config")]
    public float minBallScale = 0.3f;
    public float weaknessObjRotationSpeed = 5.0f;
    public int boostUpDamage = 100;
    public int maxHP = 100;
    public float originalBallFallingSpeed = 1.5f;
    public float maxBallFallingSpeed = 15f;
    public float maxballFrequency_modifier = 20;
    public float maxspeed_change_frequency = 10;
    public float speed_change_frequencyIncreaseFactor = 0.2f;
    public float ballFrequency_modifierIncreaseFactor = 0.4f;
    public float ballFallingSpeedIncreaseFactor = 1f;
    public int scoreToIncreaseBallFallingSpeed = 500;
    public float cancerGrowthDelay = 3.0f;
    public float cancerGrowthRate = 1.0f;
    public float ballExplodeScaleUpFactor = 3;
    public float ballExplodeScaleUpTime = 0.5f;
    public float missileMovingSpeed = 30f;
    public float bombMovingSpeed = 5f;
    public float laserMovingSpeed = 25;
    public float doubleGunsTime = 10f;
    public float tripleGunsTime = 5f;
    public float extraGunsTime = 3f;
    public float reviveCountDownTime = 4f;
    public int BossHavingCoins = 100;
    public int ScoreToAge = 100;
    [Range(0f, 1f)] public float coinFrequency = 0.1f;

    [Header("Ball Frequency")]
    [Range(0f, 1f)] public float normalFrequency = 0.001f;
    [Range(0f, 1f)] public float tumorFrequency = 0.002f;
    [Range(0f, 1f)] public float cancerFrequency = 0.002f;
    [Range(0f, 1f)] public float spreadyFrequency = 0.002f;
    [Range(0f, 1f)] public float wallFrequency = 0.002f;
    [Range(0f, 1f)] public float missileCellFrequency = 0.002f;
    [Range(0f, 1f)] public float growCellFrequency = 0.002f;
    [Range(0f, 1f)] public float bossSpawnCancerFrequency = 0.25f;

    [Header("Cancer Settings")]
    [Range(1, 100)] public int cancerMinHP = 1;
    [Range(1, 100)] public int cancerMaxHP = 30;
    [Range(0f, 10f)] public float cancerMinSize = 0.3f;
    [Range(0f, 10f)] public float cancerDeltaSize = 0.01f;
    [Header("GrowCell Settings")]
    [Range(20f, 100f)] public int growcellHP = 25;
    [Range(0f, 10f)] public float growCellMinSize = 0.6f;
    [Range(0f, 10f)] public float growCellDeltaSize = 0.02f;
    [Header("Wall cell Settings")]
    [Range(0f, 1f)] public float wallSize = 0.4f;
    [Header("Spready cell Settings")]
    [Range(0f, 10f)] public float spreadyMinSize = 0.3f;
    [Range(0f, 10f)] public float spreadyDeltaSize = 0.01f;

    public BallNumberConfig[] ballNumberConfig = null;
    public int[] spreadyHPKindConfig = null;
    public GameObject[] characters = null;

    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public bool HasNewBestScore { get; private set; }

    [HideInInspector]
    public bool tutorialON = false;
    [HideInInspector]
    public string PatientName=null;
    [HideInInspector]
    public string Subject_txt = null;
    [HideInInspector]
    public string lived_for = null;
    [HideInInspector]
    public string years_txt = null;
    [HideInInspector]
    public string Score_txt = null;
    [HideInInspector]
    public string BestScore_txt = null;
    [HideInInspector]
    public string Grab_txt = null;


    public bool freepassAdView = false;
    public int Days
    {
        get
        {
            return PlayerPrefs.GetInt(Day_Of_Count, 0);
        }
        set
        {
            PlayerPrefs.SetInt(Day_Of_Count, value);
            PlayerPrefs.Save();
        }
    }

    public int DailyRewardGet
    {
        get
        {
            return PlayerPrefs.GetInt("DRG", 0);
        }
        set
        {
            PlayerPrefs.SetInt("DRG", value);
            PlayerPrefs.Save();
        }
    }

    public int firstLogin
    {
        get
        {
            return PlayerPrefs.GetInt("FirstLogin", 0);
        }
        set
        {
            PlayerPrefs.SetInt("FirstLogin", value);
            PlayerPrefs.Save();
        }
    }

    public int SelectedIndex
    {
        get
        {
            return PlayerPrefs.GetInt(Selected_Character_Key, 0);
        }
        set
        {
            PlayerPrefs.SetInt(Selected_Character_Key, value);
            PlayerPrefs.Save();
        }
    }

    //누적 처치량
    public int KillCount
    {
        get
        {
            return PlayerPrefs.GetInt(Kill_Count, 0);
        }
        set
        {
            PlayerPrefs.SetInt(Kill_Count, value);
            PlayerPrefs.Save();
        }
    }

    //새게임 시작 량
    public int GameStartCount
    {
        get
        {
            return PlayerPrefs.GetInt(GameStart_Count, 1);
        }
        set
        {
            PlayerPrefs.SetInt(GameStart_Count, value);
            PlayerPrefs.Save();
        }
    }

    //누적 점수
    public int GatheringScore
    {
        get
        {
            return PlayerPrefs.GetInt(Gathering_Score, 0);
        }
        set
        {
            PlayerPrefs.SetInt(Gathering_Score, value);
            PlayerPrefs.Save();
        }
    }


    //게임오버가 됬는지 체크해서 실험자 숫자 올려줌
    public bool IsNewSubject
    {
        get
        {
            if (PlayerPrefs.GetInt(Is_New_Subject, 0) == 0)
                return false;
            else
                return true;
        }
        set
        {
            if (value)
            PlayerPrefs.SetInt(Is_New_Subject, 1);
            else
            PlayerPrefs.SetInt(Is_New_Subject, 0);
            PlayerPrefs.Save();
        }
    }

    public int Coins
    {
        get { return currentCoins; }
        private set { currentCoins = value; }
    }

    public bool tutorialcheck(BallType type)
    {
            int tutorialcheck = 0;
            bool tutorialcheck_bool = true;
            tutorialcheck = PlayerPrefs.GetInt("tutorialcell" + ((int)type).ToString());


            if (tutorialcheck == 0)
                tutorialcheck_bool = true;
            else
                tutorialcheck_bool = false;

            return tutorialcheck_bool;
    }
    public void TutorialSave(BallType type)
    {
        PlayerPrefs.SetInt("tutorialcell" + ((int)type).ToString(), 1);
        PlayerPrefs.Save();
    }
    /// <summary>
    /// 인스턴스화 처리를 시도한다
    /// </summary>
    private void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 시작할때 초기값을 아이템 임시변수에 지정한다
    /// </summary>
    private void Start()
    {
        if (!PlayerPrefs.HasKey(PPK_MISSILE))
            PlayerPrefs.SetInt(PPK_MISSILE, initialMissiles);
        if (!PlayerPrefs.HasKey(PPK_BOMB))
            PlayerPrefs.SetInt(PPK_BOMB, initialBombs);
        if (!PlayerPrefs.HasKey(PPK_LASER))
            PlayerPrefs.SetInt(PPK_LASER, initialLasers);

        Application.targetFrameRate = 60; //프레임수치를 60으로 고정
        //코인과 기타 다른 아이템의 기초적인 초기실행 값지정
        Reset();
    }

    public void Reset()
    {
        // Initialize coins
        Coins = PlayerPrefs.GetInt(PPK_COINS, initialCoins);
        // Initialize score
        Score = 0;
        // Initialize highscore
        BestScore = PlayerPrefs.GetInt(BESTSCORE, 0);
        HasNewBestScore = false;
    }

    public void Update()
    {
        /*
         * 업그레이드 씬에서만 주로 동작하게 되어있다
         */
    }

    /// <summary>
    /// item의 초기수치를 임시파일에서 얻어오기
    /// </summary>
    public int Missiles
    {
        private set { }
        get { return PlayerPrefs.GetInt(PPK_MISSILE); }
    }

    public int Bombs
    {
        private set { }
        get { return PlayerPrefs.GetInt(PPK_BOMB); }
    }

    public int Lasers
    {
        private set { }
        get { return PlayerPrefs.GetInt(PPK_LASER); }
    }

    /// <summary>
    /// amount 값을 받아서 아이템변수용 임시파일에 설정해둡니다
    /// </summary>
    public void AddMissile(int amount)
    {
        int value = Missiles + amount;
        PlayerPrefs.SetInt(PPK_MISSILE, value);
    }

    public void AddBomb(int amount)
    {
        int value = Bombs + amount;
        PlayerPrefs.SetInt(PPK_BOMB, value);
    }

    public void AddLaser(int amount)
    {
        int value = Lasers + amount;
        PlayerPrefs.SetInt(PPK_LASER, value);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        // Store new coin value
        PlayerPrefs.SetInt(PPK_COINS, Coins);
        // Fire event
        CoinsUpdated(Coins);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        // Fire event
        ScoreUpdated(Score);
        if (Score > BestScore)
        {
            UpdateHighScore(Score);
            HasNewBestScore = true;
        }
        else
        {
            HasNewBestScore = false;
        }
    }

    public void UpdateHighScore(int newHighScore)
    {
        // Update highscore if player has made a new one
        if (newHighScore > BestScore)
        {
            BestScore = newHighScore;
            PlayerPrefs.SetInt(BESTSCORE, BestScore);
            BestScoreUpdated(BestScore);
        }
    }

    /// <summary>
    /// amount 값을 받아서 코인변수를 임시파일에 설정해둡니다
    /// </summary>
    public void RemoveCoins(int amount)
    {
        Coins -= amount;
        // Store new coin value
        PlayerPrefs.SetInt(PPK_COINS, Coins);
        // Fire event
        CoinsUpdated(Coins);
    }

    public int Get_SS_Level()
    {
        if (!PlayerPrefs.HasKey(SS_Level_PPK))
        {
            PlayerPrefs.SetInt(SS_Level_PPK, 1);
            return 1;
        }
        else
            return PlayerPrefs.GetInt(SS_Level_PPK);
    }

    public void Set_SS_Level(int increaseAmount)
    {
        PlayerPrefs.SetInt(SS_Level_PPK, increaseAmount);
    }

    public void Set_DP_Level(int increseAmount)
    {
        PlayerPrefs.SetInt(DP_Level_PPK, increseAmount);
    }

    public int Get_DP_Level()
    {
        if (!PlayerPrefs.HasKey(DP_Level_PPK))
        {
            PlayerPrefs.SetInt(DP_Level_PPK, 1);
            return 1;
        }
        else
            return PlayerPrefs.GetInt(DP_Level_PPK);
    }

    public float GetShootingWaitTime()
    {
        int currentLevel = Get_SS_Level();
        float decreaseValue = (originalShootingWaitTime - minShootingWaitTime) / Max_SS_Level;
        float result = originalShootingWaitTime;
        for (int i = 0; i < currentLevel; i++)
        {
            result -= decreaseValue;
        }
        return result;
    }

    public float GetBulletSpeed()
    {
        return originalBulletSpeed;
    }
    
    public float GetDropPercentage()
    {
        float currentLevel = Get_DP_Level();
        float PercentageValue = (DPratio - originalDP)/ Max_DP_Level;
        float result = originalDP;
        for (int i = 0; i < currentLevel; i++)
        {
            result += PercentageValue;
        }
        return result;
    }

    public void TestCoin()
    {
        Coins += 10000;
    }
}