using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JicaGames;
using UnityEngine.SceneManagement;
using System.IO;

public enum GameState
{
    Prepare,
    Playing,
    Revive,
    GameOver,
}

public enum ItemType
{
    COIN,
    HIDDEN_GUNS,
    MISSILE,
    BOMB,
    LASER,
    BOSS_COIN
}

public enum BoostUpType
{
    MISSILE,
    BOMB,
    LASER,
}

[System.Serializable]
public struct BallNumberConfig
{
    public float MinScale;
    public float MaxScale;
    public int MinNumber;
    public int MaxNumber;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { private set; get; }
    public static event System.Action<GameState> GameStateChanged = delegate { };
    public static bool isRestart = false;
    [HideInInspector]
    public bool IsBossAlive = false;
    public GameObject bossObj = null;
    [SerializeField] private GameObject FloorLocation = null;
    public GameObject LeftBorder = null;

    [HideInInspector]
    public int killCellCount;

    private int hp;
    public int HP
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
            if (hp > DataManager.Instance.maxHP)
            {
                hp = DataManager.Instance.maxHP;
            }
            if (hp < 0)
            {
                hp = 0;
            }
            UpdateHPUI();
        }
    }
    public GameState GameState
    {
        get
        {
            return gameState;
        }
        private set
        {
            if (value != gameState)
            {
                gameState = value;
                GameStateChanged(gameState);
            }
        }
    }

    private float hiddenGunsFrequency;
    private float boostUpFrequency;

    private int normal_f_modifier = 0;
    private int tumor_f_modifier = 1;
    private int cancer_f_modifier = 0;
    private int spready_f_modifier = 0;
    private int missilecell_f_modifier = 0;
    private int growcell_f_modifier = 0;
    private int stage = 1;
    private float cell_kind_modifier = 6;
    [HideInInspector]
    public float speed_modifier = 2;
    public float ballFrequency_modifier = 4;
    private float speed_change_frequency = 3;

    [Header("Gameplay References")]
    [SerializeField] private GameObject normalPrefab = null;
    [SerializeField] private GameObject tumorPrefab = null;
    [SerializeField] private GameObject cancerPrefab = null;
    [SerializeField] private GameObject wallPrefab = null;
    [SerializeField] private GameObject spreadyPrefab = null;
    [SerializeField] private GameObject growCellPrefab = null;
    [SerializeField] private GameObject missileCellPrefab = null;
    [SerializeField] private GameObject bulletPrefab = null;
    [SerializeField] private GameObject bulletExplodePrefab = null;
    [SerializeField] private GameObject ballExplodeParticlePrefab = null;
    [SerializeField] private GameObject spreadyballExplodeParticlePrefab = null;
    [SerializeField] private GameObject coinPrefab = null;
    [SerializeField] private GameObject bosscoinPrefab = null;
    [SerializeField] private GameObject missileItemPrefab = null;
    [SerializeField] private GameObject bombItemPrefab = null;
    [SerializeField] private GameObject laserItemPrefab = null;
    [SerializeField] private GameObject hiddenGunsItemPrefab = null;
    [SerializeField] private GameObject missilePrefab = null;
    [SerializeField] private GameObject bombPrefab = null;
    [SerializeField] private GameObject laserPrefab = null;
    [SerializeField] private GameObject[] bulletPrefabs = null;
    [SerializeField] private Color[] bulletColors = null;

    public bool IsRevive { private set; get; }
    private GameState gameState = GameState.GameOver;
    private List<BallController> listBallControl = new List<BallController>();
    private List<NormalController> listNormalControl = new List<NormalController>();
    private List<WallController> listWallControl = new List<WallController>();
    private List<TumorController> listTumorControl = new List<TumorController>();
    private List<CancerController> listCancerControl = new List<CancerController>();
    private List<SpreadyController> listSpreadyControl = new List<SpreadyController>();
    private List<MissileCellController> listMissileCellControl = new List<MissileCellController>();
    private List<GrowCellController> listGrowCellControl = new List<GrowCellController>();
    private List<BulletController> listBulletControl = new List<BulletController>();
    private List<ParticleSystem> listBulletExplode = new List<ParticleSystem>();
    private List<ParticleSystem> listBallExplodeParticle = new List<ParticleSystem>();
    private List<ParticleSystem> listSpreadyBallExplodeParticle = new List<ParticleSystem>();
    private List<ItemController> listCoin = new List<ItemController>();
    private List<ItemController> listBossCoin = new List<ItemController>();
    private List<ItemController> listHiddenGunsItem = new List<ItemController>();
    private List<ItemController> listBombItem = new List<ItemController>();
    private List<ItemController> listLaserItem = new List<ItemController>();
    private List<ItemController> listMissileItem = new List<ItemController>();
    private List<BoostUpController> listMissileControl = new List<BoostUpController>();

    public BoostUpController missileControl = null;
    private BoostUpController bombControl = null;
    private BoostUpController laserControl = null;
    private float leftPoint;
    private float rightPoint;

    [HideInInspector] public float yPos;
    [HideInInspector] public float floor_yPos;
    private float currentBallFallingSpeed = 0;

    /// <summary>
    /// 스피드 증가/감소 스위치
    /// </summary>
    bool speed_switch = true;
    /// <summary>
    /// 500 점버그 수정, 불값
    /// </summary>

    IEnumerator Pulse_Control() // 속도 증가 감소
    {
        while (gameState == GameState.Playing)
        {
            if (speed_switch)
            {
                speed_modifier -= speed_change_frequency * Time.deltaTime;
            }
            else
            {
                speed_modifier += speed_change_frequency * Time.deltaTime;
            }

            if (speed_modifier < 0 || speed_modifier > 2)
            {
                speed_switch = !speed_switch;
                speed_modifier = Mathf.Clamp(speed_modifier, 0, 2);
            }
            yield return null;
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
    void Start()
    {
        Application.targetFrameRate = 60;
        DataManager.Instance.Reset();

        PrepareGame();


        if (DataManager.Instance.IsNewSubject)
        {
            ++DataManager.Instance.GameStartCount;
            DataManager.Instance.IsNewSubject = false;
        }
        //2019/09/10 게임이 시작되면 바로 UI설정값과 게임시작이 될 수 있게 적용
        UIManager.Instance.PlayingUI();
        PlayingGame();
    }

    public void PrepareGame()
    {
        //Fire event
        GameState = GameState.Prepare;
        gameState = GameState.Prepare;

        //Add another actions here
        HP = DataManager.Instance.maxHP;
        boostUpFrequency = DataManager.Instance.GetDropPercentage();
        hiddenGunsFrequency = boostUpFrequency * 2;

        DataManager.Instance.PatientName = UIManager.Instance.RandomName_Generatior();
        DataManager.Instance.Subject_txt = Lean.Localization.LeanLocalization.GetTranslationText("PersonName");
        DataManager.Instance.lived_for = Lean.Localization.LeanLocalization.GetTranslationText("lived_for");
        DataManager.Instance.years_txt = Lean.Localization.LeanLocalization.GetTranslationText("years_txt");
        DataManager.Instance.Score_txt = Lean.Localization.LeanLocalization.GetTranslationText("Score_txt");
        DataManager.Instance.BestScore_txt = Lean.Localization.LeanLocalization.GetTranslationText("Bestscore_txt");
        DataManager.Instance.Grab_txt = Lean.Localization.LeanLocalization.GetTranslationText("Grab_txt");
        killCellCount = 0;

        ParticleSystem.MainModule module = bulletExplodePrefab.GetComponent<ParticleSystem>().main;
        if (DataManager.Instance.SelectedIndex >= bulletColors.Length
            || bulletColors[DataManager.Instance.SelectedIndex] == null)
        {
            module.startColor = Color.white;
        }
        else
        {
            module.startColor = bulletColors[DataManager.Instance.SelectedIndex];
        }

        //Set IsRevived equal to false
        IsRevive = false;

        currentBallFallingSpeed = DataManager.Instance.originalBallFallingSpeed;

        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(LeftBorder.GetComponent<RectTransform>(),
            LeftBorder.transform.position, Camera.main, out pos);
        float borderLeft = pos.x;
        float radius = normalPrefab.GetComponent<CircleCollider2D>().radius;
        leftPoint = borderLeft + radius;
        rightPoint = -borderLeft-radius;

        // Ball 생성 y좌표 설정
        GameObject BallSpawnLocation = GameObject.Find("BallSpawnLocation");
        RectTransformUtility.ScreenPointToWorldPointInRectangle(BallSpawnLocation.GetComponent<RectTransform>(),
            BallSpawnLocation.transform.position, Camera.main, out pos);
        yPos = pos.y;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(FloorLocation.GetComponent<RectTransform>(),
            FloorLocation.transform.position, Camera.main, out pos);
        floor_yPos = pos.y;
        PlayerController.Instance.limitTop = yPos - 8;
        PlayerController.Instance.limitBottom = floor_yPos;

        //Clone a missile
        missileControl = Instantiate(missilePrefab, Vector2.zero, Quaternion.identity).GetComponent<BoostUpController>();
        missileControl.gameObject.SetActive(false);

        //Clone a bomb
        bombControl = Instantiate(bombPrefab, Vector2.zero, Quaternion.identity).GetComponent<BoostUpController>();
        bombControl.gameObject.SetActive(false);

        //Clone a laser
        laserControl = Instantiate(laserPrefab, Vector2.zero, Quaternion.identity).GetComponent<BoostUpController>();
        laserControl.gameObject.SetActive(false);
    }

    /// <summary>
    /// Actual start the game
    /// </summary>
    public void PlayingGame()
    {
        HP = DataManager.Instance.maxHP;
        IsBossAlive = false;
        SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
        //Fire event
        GameState = GameState.Playing;
        gameState = GameState.Playing;


       
        //Add another actions here
        StartCoroutine(Pulse_Control());
        StartCoroutine(CreateBalls());
        StartCoroutine(CreateBoss());
        StartCoroutine(IncreaseBallFallingSpeed());

        tutorialpopup(BallType.None);
    }

    /// <summary>
    /// Call Revive event
    /// </summary>
    public void Revive()
    {
        SoundManager.Instance.StopMusic();
        //Fire event
        GameState = GameState.Revive;
        gameState = GameState.Revive;

        //Add another actions here
        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
    }

    /// <summary>
    /// Call GameOver event
    /// </summary>
    public void GameOver()
    {
        SoundManager.Instance.StopMusic();
        //Fire event
        GameState = GameState.GameOver;
        gameState = GameState.GameOver;

        UpdateRecord();

        DataManager.Instance.IsNewSubject = true;
        IsBossAlive = false;
        //Add another actions here
        isRestart = true;
        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);
    }

    // 게임 오버됬을 때
    // 누적 획득 코인, 세포 누적 처치량 등을 업데이트한다.
    private void UpdateRecord()
    {
        DataManager.Instance.GatheringScore += DataManager.Instance.Score;
        DataManager.Instance.KillCount += killCellCount;
        
        Debug.Log("누적 점수 : " + DataManager.Instance.GatheringScore);
        Debug.Log("누적 처치량 : " + DataManager.Instance.KillCount);
    }

    public void AddKillCellCount()
    {
        ++killCellCount;
    }

    public void LoadScene(string sceneName, float delay)
    {
        SoundManager.Instance.StopMusic();
        StartCoroutine(LoadingScene(sceneName, delay));
    }

    private IEnumerator LoadingScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    //Get the inactive ball
    private NormalController GetNormalControl()
    {
        //Find the inactive ball
        foreach (NormalController o in listNormalControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        //Didn't find one -> create new ball
        NormalController normalControl = Instantiate(normalPrefab, Vector2.zero, Quaternion.identity).GetComponent<NormalController>();
        listNormalControl.Add(normalControl);
        normalControl.gameObject.SetActive(false);
        return normalControl;
    }

    private WallController GetWallControl()
    {
        //Find the inactive ball
        foreach (WallController o in listWallControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        //Didn't find one -> create new ball
        WallController wallControl = Instantiate(wallPrefab, Vector2.zero, Quaternion.identity).GetComponent<WallController>();
        listWallControl.Add(wallControl);
        wallControl.gameObject.SetActive(false);
        return wallControl;
    }

    private TumorController GetTumorControl()
    {
        //Find the inactive ball
        foreach (TumorController o in listTumorControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        //Didn't find one -> create new ball
        TumorController tumorControl = Instantiate(tumorPrefab, Vector2.zero, Quaternion.identity).GetComponent<TumorController>();
        listTumorControl.Add(tumorControl);
        tumorControl.gameObject.SetActive(false);
        return tumorControl;
    }

    public BoostUpController GetMissileControl()
    {
        //Find the inactive ball
        foreach (BoostUpController o in listMissileControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        //Didn't find one -> create new ball
        BoostUpController missileControl = Instantiate(missilePrefab, Vector2.zero,
            Quaternion.identity).GetComponent<BoostUpController>();
        listMissileControl.Add(missileControl);
        missileControl.gameObject.SetActive(false);
        return missileControl;
    }

    private CancerController GetCancerControl()
    {
        //Find the inactive ball
        foreach (CancerController o in listCancerControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        //Didn't find one -> create new ball
        CancerController cancerControl = Instantiate(cancerPrefab, Vector2.zero, Quaternion.identity).GetComponent<CancerController>();
        listCancerControl.Add(cancerControl);
        cancerControl.gameObject.SetActive(false);
        return cancerControl;
    }

    private SpreadyController GetSpreadyControl()
    {
        //Find the inactive ball
        foreach (SpreadyController o in listSpreadyControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        //Didn't find one -> create new ball
        SpreadyController spreadyControl = Instantiate(spreadyPrefab, Vector2.zero, Quaternion.identity).GetComponent<SpreadyController>();
        listSpreadyControl.Add(spreadyControl);
        spreadyControl.gameObject.SetActive(false);
        return spreadyControl;
    }

    private MissileCellController GetMissileCellControl()
    {
        //Find the inactive ball
        foreach (MissileCellController o in listMissileCellControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        //Didn't find one -> create new ball
        MissileCellController missileCellControl = Instantiate(missileCellPrefab, Vector2.zero, Quaternion.identity).GetComponent<MissileCellController>();
        listMissileCellControl.Add(missileCellControl);
        missileCellControl.gameObject.SetActive(false);
        return missileCellControl;
    }

    private GrowCellController GetGrowCellControl()
    {
        //Find the inactive ball
        foreach (GrowCellController o in listGrowCellControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }
        //Didn't find one -> create new ball
        GrowCellController growCellControl = Instantiate(growCellPrefab, Vector2.zero, Quaternion.identity).GetComponent<GrowCellController>();
        listGrowCellControl.Add(growCellControl);
        growCellControl.gameObject.SetActive(false);
        return growCellControl;
    }

    //Get the inactive bullet
    private BulletController GetBulletControl()
    {
        //Find the inactive bullet
        foreach (BulletController o in listBulletControl)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one (run out of bullet) -> create new one
        BulletController bulletControl;
        if (bulletPrefabs.Length <= DataManager.Instance.SelectedIndex
            || bulletPrefabs[DataManager.Instance.SelectedIndex] == null)
        {
            bulletControl = Instantiate(bulletPrefab, Vector2.zero, Quaternion.identity).GetComponent<BulletController>();
        }
        else
        {
            bulletControl = Instantiate(bulletPrefabs[DataManager.Instance.SelectedIndex], Vector2.zero, Quaternion.identity).GetComponent<BulletController>();
        }
        listBulletControl.Add(bulletControl);
        bulletControl.gameObject.SetActive(false);
        return bulletControl;
    }

    //Get the bullet explore particle object
    private ParticleSystem GetBulletExplode()
    {
        //Find the inactive particle
        foreach (ParticleSystem o in listBulletExplode)
        {
            if (!o.gameObject.activeInHierarchy)
            {
                return o;
            }
        }

        //Didn't find one (run out of particle) -> create new one
        ParticleSystem explore = Instantiate(bulletExplodePrefab, Vector2.zero, Quaternion.identity).GetComponent<ParticleSystem>();

        listBulletExplode.Add(explore);
        explore.gameObject.SetActive(false);
        return explore;
    }

    private ParticleSystem GetBallExplodeParticle()
    {
        foreach (ParticleSystem o in listBallExplodeParticle)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        ParticleSystem ballExplode = Instantiate(ballExplodeParticlePrefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
        listBallExplodeParticle.Add(ballExplode);
        ballExplode.gameObject.SetActive(false);
        return ballExplode;
    }

    private ParticleSystem GetSpreadyBallExplodeParticle()
    {
        foreach (ParticleSystem o in listSpreadyBallExplodeParticle)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        ParticleSystem spreadyballExplode = Instantiate(spreadyballExplodeParticlePrefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
        listSpreadyBallExplodeParticle.Add(spreadyballExplode);
        spreadyballExplode.gameObject.SetActive(false);
        return spreadyballExplode;
    }

    private ItemController GetBossCoinItem()
    {
        //Find an inactive missile item
        foreach (ItemController o in listBossCoin)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new coin item
        ItemController coin = Instantiate(bosscoinPrefab, Vector2.zero, Quaternion.identity).GetComponent<ItemController>();
        listBossCoin.Add(coin);
        coin.gameObject.SetActive(false);
        return coin;
    }
    //Get coin item
    private ItemController GetCoinItem()
    {
        //Find an inactive missile item
        foreach (ItemController o in listCoin)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new coin item
        ItemController coin = Instantiate(coinPrefab, Vector2.zero, Quaternion.identity).GetComponent<ItemController>();
        listCoin.Add(coin);
        coin.gameObject.SetActive(false);
        return coin;
    }

    //Get hidden guns item
    private ItemController GetHiddenGunsItem()
    {
        //Find an inactive hidden guns item
        foreach (ItemController o in listHiddenGunsItem)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new hidden guns item
        ItemController hiddenGuns = Instantiate(hiddenGunsItemPrefab, Vector2.zero, Quaternion.identity).GetComponent<ItemController>();
        listHiddenGunsItem.Add(hiddenGuns);
        hiddenGuns.gameObject.SetActive(false);
        return hiddenGuns;
    }

    //Get missile item
    private ItemController GetMissileItem()
    {
        //Find an inactive missile item
        foreach (ItemController o in listMissileItem)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new missile item
        ItemController missile = Instantiate(missileItemPrefab, Vector2.zero, Quaternion.identity).GetComponent<ItemController>();
        listMissileItem.Add(missile);
        missile.gameObject.SetActive(false);
        return missile;
    }

    //Get bomb item
    private ItemController GetBombItem()
    {
        //Find an inactive missile item
        foreach (ItemController o in listBombItem)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new bomb item
        ItemController bomb = Instantiate(bombItemPrefab, Vector2.zero, Quaternion.identity).GetComponent<ItemController>();
        listBombItem.Add(bomb);
        bomb.gameObject.SetActive(false);
        return bomb;
    }

    //Get missile item
    private ItemController GetLaserItem()
    {
        //Find an inactive laser item
        foreach (ItemController o in listLaserItem)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new laser item
        ItemController laser = Instantiate(laserItemPrefab, Vector2.zero, Quaternion.identity).GetComponent<ItemController>();
        listLaserItem.Add(laser);
        laser.gameObject.SetActive(false);
        return laser;
    }

    private void StageController()
    {
        switch(stage)
        {
            case 2: normal_f_modifier = 1; cell_kind_modifier = 3f; break;
            case 3: cancer_f_modifier = 1; cell_kind_modifier = 2f; break;
            case 4: spready_f_modifier = 1; cell_kind_modifier = 1.5f; break;
            case 5: missilecell_f_modifier = 1; cell_kind_modifier = 0.8f; break;
            case 6: growcell_f_modifier = 1; cell_kind_modifier = 1f; break;
        }
    }

    public void CreateBossCoin(Vector3 pos)
    {
        ItemController coinControl = GetBossCoinItem();
        CircleCollider2D itemCollider = coinControl.GetComponent<CircleCollider2D>();

        //Set position and falling speed
        coinControl.transform.position = pos;
        coinControl.fallingSpeed = currentBallFallingSpeed;

        //enable object and disable collider
        coinControl.gameObject.SetActive(true);
        itemCollider.enabled = true;
        coinControl.MoveDown();
    }

    private IEnumerator CreateBoss()
    {
        // DataManager.Instance.AddScore(9800);
        while (gameState == GameState.Playing)
        {
            if (DataManager.Instance.Score % 5000 > 4900)
            {
                DataManager.Instance.AddScore(100);
                StartCoroutine(CreatingBoss());
            }
            yield return null;
        }    
    }

    private IEnumerator CreatingBoss()
    {
        while (IsBossAlive && GameState == GameState.Playing)
        {
            yield return null;
        }
        if (GameState == GameState.Playing)
        {
            tutorialpopup(BallType.Boss);
            IsBossAlive = true;
            Vector2 pos = new Vector2((leftPoint + rightPoint) / 2, yPos);
            //Scale for the ball
            float scale = 3;

            //Cache components
            BossController bossControl = bossObj.GetComponent<BossController>();
            CircleCollider2D bossCollider = bossControl.GetComponent<CircleCollider2D>();

            //Set position and scale for the ball
            bossControl.transform.position = pos;
            bossControl.transform.localScale = new Vector2(scale, scale);

            //Set falling speed, color and number for the ball
            bossControl.fallingSpeed = currentBallFallingSpeed;

            bossControl.number = 999;

            bossControl.gameObject.SetActive(true);
            bossCollider.enabled = true;

            //Move the ball down
            bossControl.MoveBall();
            bossControl.weaknessObj.Rotate();
            bossControl.weaknessObj.MoveBall();
            bossControl.BossMakesCancer();
        }
    }

    private IEnumerator CreateBalls()
    {
        while (gameState == GameState.Playing)
        {
            if (!UIManager.Instance.Stop)
            {
                //Create position 
                Vector2 pos = new Vector2(Random.Range(leftPoint, rightPoint), yPos);
                if (IsBossAlive)
                {
                    if (Random.value <= DataManager.Instance.wallFrequency * ballFrequency_modifier)
                    {
                        
                        yield return StartCoroutine(CreatingBalls(BallType.Wall));
                    }
                }
                else
                {
                    if (Random.value <= DataManager.Instance.coinFrequency) //Create coin
                    {
                        //Cache components
                        ItemController coinControl = GetCoinItem();
                        CircleCollider2D itemCollider = coinControl.GetComponent<CircleCollider2D>();

                        //Set position and falling speed
                        coinControl.transform.position = pos;
                        coinControl.fallingSpeed = currentBallFallingSpeed;

                        //enable object and disable collider
                        coinControl.gameObject.SetActive(true);
                        itemCollider.enabled = false;

                        while (Physics2D.OverlapCircle(coinControl.transform.position, itemCollider.radius, 1 << LayerMask.NameToLayer("Overlap")) != null)
                        {
                            yield return null;

                            //Renew position
                            pos = new Vector2(Random.Range(leftPoint, rightPoint), yPos);
                            //Set position
                            coinControl.transform.position = pos;
                        }

                        //Enable collider
                        itemCollider.enabled = true;

                        //Move the coin
                        coinControl.MoveDown();
                    }
                    else if (Random.value <= DataManager.Instance.normalFrequency * ballFrequency_modifier*normal_f_modifier* cell_kind_modifier) //Create normal
                    {
                        yield return StartCoroutine(CreatingBalls(BallType.Normal));
                    }
                    else if (Random.value <= DataManager.Instance.tumorFrequency * ballFrequency_modifier*tumor_f_modifier* cell_kind_modifier) //Create tumor
                    {
                        yield return StartCoroutine(CreatingBalls(BallType.Tumor));
                    }
                    else if (Random.value <= DataManager.Instance.cancerFrequency * ballFrequency_modifier*cancer_f_modifier* cell_kind_modifier) //Create cancer
                    {
                        yield return StartCoroutine(CreatingBalls(BallType.Cancer));
                    }
                    else if (Random.value <= DataManager.Instance.spreadyFrequency * ballFrequency_modifier*spready_f_modifier* cell_kind_modifier)
                    {
                        yield return StartCoroutine(CreatingBalls(BallType.Spready));
                    }
                    else if (Random.value <= DataManager.Instance.missileCellFrequency * ballFrequency_modifier*missilecell_f_modifier* cell_kind_modifier)
                    {
                        yield return StartCoroutine(CreatingBalls(BallType.MissileCell));
                    }
                    else if (Random.value <= DataManager.Instance.growCellFrequency * ballFrequency_modifier * growcell_f_modifier* cell_kind_modifier)
                    {
                        yield return StartCoroutine(CreatingBalls(BallType.GrowCell));
                    }
                }
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void tutorialpopup(BallType type)
    {
        if (DataManager.Instance.tutorialcheck(type) == true)
        {
        
                if ((int)type == 1 || (int)type == 8)
            {
                UIManager.Instance.Goodone.enabled = true;
                UIManager.Instance.Badone.enabled = false;
            }
               else
            {
                UIManager.Instance.Goodone.enabled = false;
                UIManager.Instance.Badone.enabled = true;

            }

            
            UIManager.Instance.TutorialPopup(type);
            DataManager.Instance.TutorialSave(type);
            DataManager.Instance.tutorialON = true;
            Time.timeScale = 0;
            UIManager.Instance.Stop = true;
            
            
        }
    }

    private IEnumerator CreatingBalls(BallType type)
    {
        if ((IsBossAlive && type == BallType.Wall) || type != BallType.Wall)
        {
            Vector2 pos = new Vector2(Random.Range(leftPoint, rightPoint), yPos);

            //Scale for the ball
            float scale = Random.Range(DataManager.Instance.minBallScale, 1f);

            //Cache components
            BallController ballControl = null;

            switch (type)
            {
                case BallType.Normal:
                    tutorialpopup(type);


                    ballControl = GetNormalControl();
                    foreach (BallNumberConfig o in DataManager.Instance.ballNumberConfig)
                    {
                        if (scale >= o.MinScale && scale < o.MaxScale)
                        {
                            ballControl.number = Random.Range(o.MinNumber, o.MaxNumber);
                        }
                    }
                    break;
                case BallType.Tumor:
                    tutorialpopup(type);
                    ballControl = GetTumorControl();
                    foreach (BallNumberConfig o in DataManager.Instance.ballNumberConfig)
                    {
                        if (scale >= o.MinScale && scale < o.MaxScale)
                        {
                            ballControl.number = Random.Range(o.MinNumber, o.MaxNumber);
                        }
                    }
                    break;
                case BallType.MissileCell:
                    tutorialpopup(type);
                    ballControl = GetMissileCellControl();
                    foreach (BallNumberConfig o in DataManager.Instance.ballNumberConfig)
                    {
                        if (scale >= o.MinScale && scale < o.MaxScale)
                        {
                            ballControl.number = Random.Range(o.MinNumber, o.MaxNumber);
                        }
                    }
                    break;
                case BallType.Cancer:
                    tutorialpopup(type);
                    ballControl = GetCancerControl();
                    ballControl.number = Random.Range(DataManager.Instance.cancerMinHP, DataManager.Instance.cancerMaxHP);
                    scale = DataManager.Instance.cancerMinSize + DataManager.Instance.cancerDeltaSize * ballControl.number;
                    break;
                case BallType.Spready:
                    tutorialpopup(type);
                    ballControl = GetSpreadyControl();
                    ballControl.number = DataManager.Instance.spreadyHPKindConfig[Random.Range(0, DataManager.Instance.spreadyHPKindConfig.Length)];
                    scale = DataManager.Instance.spreadyMinSize + ballControl.number * DataManager.Instance.spreadyDeltaSize;
                    break;
                case BallType.Wall:
                    tutorialpopup(type);
                    ballControl = GetWallControl();
                    ballControl.number = 1;
                    scale = DataManager.Instance.wallSize;
                    break;
                case BallType.GrowCell:
                    tutorialpopup(type);
                    ballControl = GetGrowCellControl();
                    ballControl.number = 1;
                    scale = DataManager.Instance.growCellMinSize;
                    break;
            }

            if (ballControl == null)
            {
                yield break;
            }
            Collider2D ballCollider = ballControl.GetComponent<Collider2D>();

            //Set position and scale for the ball
            ballControl.transform.position = pos;
            ballControl.transform.localScale = new Vector2(scale, scale);

            //Set falling speed, color and number for the ball
            ballControl.fallingSpeed = currentBallFallingSpeed;

            ballControl.gameObject.SetActive(true);
            ballCollider.enabled = false;
            ballControl.transform.localScale = new Vector3(scale, scale, 1);

            float radius = 4;
            if (type != BallType.MissileCell)
            {
                radius = ballControl.GetComponent<CircleCollider2D>().radius;
            }
            //The collider is overlapping another     
            while (Physics2D.OverlapCircle(ballControl.transform.position, radius,
                1 << LayerMask.NameToLayer("Overlap")) != null)
            {
                yield return null;

                switch (type)
                {
                    case BallType.Normal:
                        ballControl = GetNormalControl();
                        foreach (BallNumberConfig o in DataManager.Instance.ballNumberConfig)
                        {
                            if (scale >= o.MinScale && scale < o.MaxScale)
                            {
                                ballControl.number = Random.Range(o.MinNumber, o.MaxNumber);
                            }
                        }
                        break;
                    case BallType.Tumor:
                        ballControl = GetTumorControl();
                        foreach (BallNumberConfig o in DataManager.Instance.ballNumberConfig)
                        {
                            if (scale >= o.MinScale && scale < o.MaxScale)
                            {
                                ballControl.number = Random.Range(o.MinNumber, o.MaxNumber);
                            }
                        }
                        break;
                    case BallType.MissileCell:
                        ballControl = GetMissileCellControl();
                        foreach (BallNumberConfig o in DataManager.Instance.ballNumberConfig)
                        {
                            if (scale >= o.MinScale && scale < o.MaxScale)
                            {
                                ballControl.number = Random.Range(o.MinNumber, o.MaxNumber);
                            }
                        }
                        break;
                    case BallType.Cancer:
                        ballControl = GetCancerControl();
                        ballControl.number = Random.Range(DataManager.Instance.cancerMinHP, DataManager.Instance.cancerMaxHP);
                        scale = DataManager.Instance.cancerMinSize + DataManager.Instance.cancerDeltaSize * ballControl.number;
                        break;
                    case BallType.Spready:
                        ballControl = GetSpreadyControl();
                        ballControl.number = DataManager.Instance.spreadyHPKindConfig[
                            Random.Range(0, DataManager.Instance.spreadyHPKindConfig.Length)];
                        scale = DataManager.Instance.spreadyMinSize + ballControl.number * DataManager.Instance.spreadyDeltaSize;
                        break;
                    case BallType.Wall:
                        ballControl = GetWallControl();
                        ballControl.number = 1;
                        scale = DataManager.Instance.wallSize;
                        break;
                    case BallType.GrowCell:
                        ballControl = GetGrowCellControl();
                        ballControl.number = 1;
                        scale = DataManager.Instance.growCellMinSize;
                        break;
                }

                //Renew position, scale
                pos = new Vector2(Random.Range(leftPoint, rightPoint), yPos);
                scale = Random.Range(DataManager.Instance.minBallScale, 1f);

                //Reset position, scale of the ball
                ballControl.transform.position = pos;
                ballControl.transform.localScale = new Vector2(scale, scale);
            }

            ballControl.transform.localScale = new Vector3(scale, scale, 1);

            //No overlapping aymore -> enable collider
            ballCollider.enabled = true;

            //Move the ball down
            ballControl.MoveBall();

            if (type == BallType.Cancer)
            {
                ballControl.GrowUp();
            }
        }
    }

    public void CreateCancer()
    {
        Vector2 pos = new Vector2(Random.Range(leftPoint, rightPoint), yPos);

        //Scale for the ball
        float scale = Random.Range(DataManager.Instance.minBallScale, 1f);

        //Cache components
        BallController ballControl = GetCancerControl();
        CircleCollider2D ballCollider = ballControl.GetComponent<CircleCollider2D>();

        //Set position and scale for the ball
        ballControl.transform.position = pos;
        ballControl.transform.localScale = new Vector2(scale, scale);

        //Set falling speed, color and number for the ball
        ballControl.fallingSpeed = currentBallFallingSpeed;

        ballControl.number = 1;
        scale = DataManager.Instance.cancerMinSize + DataManager.Instance.cancerDeltaSize * ballControl.number;

        ballControl.gameObject.SetActive(true);
        ballCollider.enabled = false;
        ballControl.transform.localScale = new Vector3(scale, scale, 1);
        //No overlapping aymore -> enable collider
        ballCollider.enabled = true;

        //Move the ball down
        ballControl.MoveBall();
        ballControl.GrowUp();
    }

    public void CreateSpready(int number, Vector3 pos)
    {
        //Scale for the ball
        float scale = DataManager.Instance.spreadyMinSize + number * DataManager.Instance.spreadyDeltaSize;

        //Cache components
        BallController ballControl = GetSpreadyControl();
        CircleCollider2D ballCollider = ballControl.GetComponent<CircleCollider2D>();
        ballControl.number = number;

        //Set position and scale for the ball
        ballControl.transform.position = pos;
        ballControl.transform.localScale = new Vector2(scale, scale);

        //Set falling speed, color and number for the ball
        ballControl.fallingSpeed = currentBallFallingSpeed;

        ballControl.gameObject.SetActive(true);
        ballCollider.enabled = false;
        ballControl.transform.localScale = new Vector3(scale, scale, 1);
        
        //No overlapping aymore -> enable collider
        ballCollider.enabled = true;

        //Move the ball down
        ballControl.MoveBall();
    }

    private IEnumerator IncreaseBallFallingSpeed()
    {
        while (gameState == GameState.Playing)
        {
            if (DataManager.Instance.Score != 0
                && DataManager.Instance.Score % DataManager.Instance.scoreToIncreaseBallFallingSpeed >
                DataManager.Instance.scoreToIncreaseBallFallingSpeed - 100)
            {
                ++stage;
                DataManager.Instance.AddScore(100);
                StageController();
               
                ballFrequency_modifier += DataManager.Instance.ballFrequency_modifierIncreaseFactor;
                speed_change_frequency += DataManager.Instance.speed_change_frequencyIncreaseFactor;
                currentBallFallingSpeed += DataManager.Instance.ballFallingSpeedIncreaseFactor;
              
                if (ballFrequency_modifier > DataManager.Instance.maxballFrequency_modifier)
                {
                    ballFrequency_modifier = DataManager.Instance.maxballFrequency_modifier;
                }
                if (speed_change_frequency > DataManager.Instance.maxspeed_change_frequency)
                {
                    speed_change_frequency = DataManager.Instance.maxspeed_change_frequency;
                }
                if (currentBallFallingSpeed > DataManager.Instance.maxBallFallingSpeed)
                {
                    currentBallFallingSpeed = DataManager.Instance.maxBallFallingSpeed;
                }

                foreach (BallController o in listBallControl)
                {
                    if (o.gameObject.activeInHierarchy)
                        o.fallingSpeed = currentBallFallingSpeed;
                }

                ItemController[] items = FindObjectsOfType<ItemController>();
                foreach (ItemController o in items)
                {
                    o.fallingSpeed = currentBallFallingSpeed;
                }

                if (ballFrequency_modifier == DataManager.Instance.maxballFrequency_modifier
                    && speed_change_frequency == DataManager.Instance.maxspeed_change_frequency
                    && currentBallFallingSpeed == DataManager.Instance.maxBallFallingSpeed)
                {
                    yield break;
                }
            }
            yield return null;
        }
    }

    private IEnumerator PlayBulletExploreParticle(Vector2 position, Vector2 lookDir)
    {
        ParticleSystem par = GetBulletExplode();

        par.gameObject.transform.position = position;
        par.transform.up = -lookDir;
        par.gameObject.SetActive(true);

        par.Play();
        var main = par.main;
        yield return new WaitForSeconds(main.startLifetimeMultiplier);
        par.gameObject.SetActive(false);
    }

    private IEnumerator PlayBallExplodeParticle(ParticleSystem par)
    {
        par.Play();
        yield return new WaitForSeconds(par.main.startLifetimeMultiplier);
        par.gameObject.SetActive(false);
    }

    //////////////////////////////////////Publish functions

    /// <summary>
    /// Fire the bullet with given position and direction
    /// </summary>
    /// <param name="position"></param>
    /// <param name="movingDirection"></param>
    public void FireBullet(Vector2 position, Vector2 movingDirection)
    {
        BulletController bulletControl = GetBulletControl();
        bulletControl.transform.position = position;
        bulletControl.MovingDirection = movingDirection;
        bulletControl.movingSpeed = DataManager.Instance.GetBulletSpeed();

        bulletControl.gameObject.SetActive(true);
        bulletControl.MoveBullet();
    }

    /// <summary>
    /// Create boost up (added bullet, bomb, laser)
    /// </summary>
    /// <param name="pos"></param>
    public void CreateBoostUp(Vector2 pos)
    {
        ItemController itemControl = null;
        if (Random.value <= hiddenGunsFrequency) //Create hidden guns
        {
            itemControl = GetHiddenGunsItem();
            itemControl.transform.position = pos;
            itemControl.fallingSpeed = currentBallFallingSpeed;
            itemControl.gameObject.SetActive(true);
            itemControl.MoveDown();
        }
        else if (Random.value <= boostUpFrequency) //Create boost up
        {
            float value = Random.value;
            if (value >= 0 && value < 0.5f) //Create missile item
            {
                itemControl = GetMissileItem();
            }
            else if (value >= 0.5f && value < 0.8f) //Create bomb item
            {
                itemControl = GetBombItem();
            }
            else //Create laser item
            {
                itemControl = GetLaserItem();
            }
            itemControl.transform.position = pos;
            itemControl.fallingSpeed = currentBallFallingSpeed;
            itemControl.gameObject.SetActive(true);
            itemControl.MoveDown();
        }
    }

    public void DropCoins(Vector2 pos)
    {
        ItemController coinControl = GetCoinItem();
        CircleCollider2D itemCollider = coinControl.GetComponent<CircleCollider2D>();

        //Set position and falling speed
        coinControl.transform.position = pos;
        coinControl.fallingSpeed = currentBallFallingSpeed;

        //enable object and disable collider
        coinControl.gameObject.SetActive(true);

        //Enable collider
        itemCollider.enabled = true;

        //Move the coin
        coinControl.MoveDown();
    }

    /// <summary>
    /// Create missile with given position
    /// </summary>
    /// <param name="pos"></param>
    public void CreateMissile(Vector2 pos)
    {
        if (UIManager.Instance.Stop)
        {
            return;
        }
        if (!missileControl.gameObject.activeInHierarchy)
        {
            if (DataManager.Instance.Missiles > 0)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.missileBtn);
                DataManager.Instance.AddMissile(-1);
                missileControl.transform.position = pos;
                missileControl.gameObject.SetActive(true);
                missileControl.MoveUp();
            }
        }
    }

    /// <summary>
    /// Create bomb with given position
    /// </summary>
    /// <param name="pos"></param>
    public void CreateBomb(Vector2 pos)
    {
        if (UIManager.Instance.Stop)
        {
            return;
        }
        if (!bombControl.gameObject.activeInHierarchy)
        {
            if (DataManager.Instance.Bombs > 0)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.bombBtn);
                DataManager.Instance.AddBomb(-1);
                bombControl.transform.position = pos;
                bombControl.gameObject.SetActive(true);
                bombControl.MoveUp();
            }
        }
    }

    /// <summary>
    /// Create laser with given position
    /// </summary>
    /// <param name="pos"></param>
    public void CreateLaser(Vector2 pos)
    {
        if (UIManager.Instance.Stop)
        {
            return;
        }
        if (!laserControl.gameObject.activeInHierarchy)
        {
            if (DataManager.Instance.Lasers > 0)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.laserBtn);
                DataManager.Instance.AddLaser(-1);
                laserControl.transform.position = pos;
                laserControl.gameObject.SetActive(true);
                laserControl.MoveUp();
            }
        }
    }

    /// <summary>
    /// Play bullet explore particle with given position and  z angle
    /// </summary>
    /// <param name="position"></param>
    /// <param name="zAngle"></param>
    public void PlayBulletExplore(Vector2 position, Vector2 lookdir)
    {
        StartCoroutine(PlayBulletExploreParticle(position, lookdir));
    }

    /// <summary>
    /// Play ball explore with given position
    /// </summary>
    /// <param name="position"></param>
    public void PlayBallExplode(Vector2 position, Color color)
    {
        ParticleSystem ballExplode = GetBallExplodeParticle();
        ballExplode.gameObject.SetActive(true);
        ballExplode.transform.position = position;

        ParticleSystem.MainModule main = ballExplode.main;
        main.startColor = color;
        StartCoroutine(PlayBallExplodeParticle(ballExplode));
    }

    public void PlaySpreadyBallExplode(Vector2 position)
    {
        ParticleSystem spreadyballExplode = GetSpreadyBallExplodeParticle();
        spreadyballExplode.gameObject.SetActive(true);
        spreadyballExplode.transform.position = position;
        StartCoroutine(PlayBallExplodeParticle(spreadyballExplode));
    }

    /// <summary>
    /// Load the saved screenshot
    /// </summary>
    /// <returns></returns>
    public Texture LoadedScrenshot()
    {
        byte[] bytes = File.ReadAllBytes(ShareManager.Instance.ScreenshotPath);
        Texture2D tx = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        tx.LoadImage(bytes);
        return tx;
    }

    /// <summary>
    /// Continue the game
    /// </summary>
    public void SetContinueGame()
    {
        IsRevive = true;
        GameObject.Find("Player").transform.position = new Vector3(0, -20, 0);
        PlayingGame();
    }

    private void UpdateHPUI()
    {
        UIManager.Instance.SetHPText(HP);
        UIManager.Instance.SetHPBarValue((float)HP / (float)DataManager.Instance.maxHP);
    }
}