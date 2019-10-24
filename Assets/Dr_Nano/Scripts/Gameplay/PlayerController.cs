using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PlayerState
{
    Prepare,
    Living,
    Die,
}

public class PlayerController : MonoBehaviour {
    public static PlayerController Instance { private set; get; }
    public static event System.Action<PlayerState> PlayerStateChanged = delegate { };
    public PlayerState PlayerState
    {
        get
        {
            return playerState;
        }

        private set
        {
            if (value != playerState)
            {
                value = playerState;
                PlayerStateChanged(playerState);
            }
        }
    }

    private PlayerState playerState = PlayerState.Die;

    [Header("Player Config")]
    public float limitBottom;
    public float limitTop;

    [Header("Player References")]
    [SerializeField]
    private GameObject centerGun = null;
    [SerializeField]
    private GameObject leftGun = null;
    [SerializeField]
    private GameObject far_leftGun = null;
    [SerializeField]
    private GameObject rightGun = null;
    [SerializeField]
    private GameObject far_rightGun = null;

    private SpriteRenderer spRender = null;
    private CircleCollider2D circleCollider = null;
    private SpriteRenderer centerGunRender = null;
    private Vector2 firstPos = Vector2.zero;
    public float limitLeft = 0;
    public float limitRight = 0;
    private int gunCount = 1;
    private bool allowMove = false;

    private Coroutine crCoutingHiddenGunsTime = null;
    private float hiddenGunsTime = 0;

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
        if (obj == GameState.Playing)
        {
            PlayerLiving();
            StartCoroutine(GettingEscapeButton());
        }
        else if (obj == GameState.Prepare)
        {
            PlayerPrepare();
        }
        else if (obj == GameState.Revive || obj == GameState.GameOver)
        {
            PlayerDie();
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

	void Update()
    {
        if (GameManager.Instance.GameState == GameState.Playing)
        {
            if (DataManager.Instance.tutorialON && Input.GetMouseButtonDown(0))
            {
                UIManager.Instance.TutorialPanel.SetActive(false);
                UIManager.Instance.Tutorial_basic.SetActive(false);
                DataManager.Instance.tutorialON = false;
                Time.timeScale = 1;
                UIManager.Instance.Stop = false;
            }

                if (UIManager.Instance.Stop)
            {
                return;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Time.timeScale = 1;
                allowMove = false;
                if (EventSystem.current.currentSelectedGameObject == null)
                {
                    allowMove = true;
                    firstPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
            }
            else if (Input.GetMouseButton(0) && allowMove)
            {
                Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                float posX = Mathf.Clamp(transform.position.x + currentPos.x - firstPos.x, limitLeft, limitRight);
                float posY = Mathf.Clamp(transform.position.y + currentPos.y - firstPos.y, limitBottom, limitTop);

                transform.position = new Vector2(posX, posY);
                firstPos =  currentPos;
            }
            if (Input.GetMouseButtonUp(0))
            {
                Time.timeScale = 0.3f;
            }
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Space))
            {
                DataManager.Instance.AddScore(10);
            }
#endif
        }
    }

    IEnumerator GettingEscapeButton()
    {
        while(GameManager.Instance.GameState == GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIManager.Instance.StopBtn();
            }
            yield return null;
        }
    }

    private void PlayerPrepare()
    {
        //Fire event
        PlayerState = PlayerState.Prepare;
        playerState = PlayerState.Prepare;

        Input.multiTouchEnabled = false;

        //Cache components
        spRender = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        centerGunRender = centerGun.GetComponent<SpriteRenderer>();

        //Replce sprite renderer and polygon collider with selected character
        GameObject character = DataManager.Instance.characters[DataManager.Instance.SelectedIndex];
        SpriteRenderer charRender = character.GetComponent<SpriteRenderer>();
        CircleCollider2D CircleCollider = character.GetComponent<CircleCollider2D>();
        spRender.sprite = charRender.sprite;

        spRender.enabled = false;
        centerGun.SetActive(false);
        leftGun.SetActive(false);
        rightGun.SetActive(false);
        far_leftGun.SetActive(false);
        far_rightGun.SetActive(false);

        //Define the limit left and right

        GameObject LeftBorder = GameManager.Instance.LeftBorder;
        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(LeftBorder.GetComponent<RectTransform>(),
            LeftBorder.transform.position, Camera.main, out pos);
        limitLeft = pos.x + spRender.bounds.size.x;
        limitRight = -limitLeft;
    }

    private void PlayerLiving()
    {
        //Fire event
        PlayerState = PlayerState.Living;
        playerState = PlayerState.Living;

        //Add another actions here
        Time.timeScale = 0.3f;

        gunCount = 1;
        spRender.enabled = true;
        circleCollider.enabled = true;
        centerGun.SetActive(true);
        leftGun.SetActive(false);
        rightGun.SetActive(false);
        far_leftGun.SetActive(false);
        far_rightGun.SetActive(false);

        StartCoroutine(FireBullets());
    }

    private void PlayerDie()
    {
        //Fire event
        PlayerState = PlayerState.Die;
        playerState = PlayerState.Die;

        //Add another actions here

        spRender.enabled = false;
        circleCollider.enabled = false;
        centerGun.SetActive(false);
        leftGun.SetActive(false);
        rightGun.SetActive(false);
        far_leftGun.SetActive(false);
        far_rightGun.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PlayerState != PlayerState.Living)
        {
            return;
        }
        if (collision.CompareTag("Item"))
        {
            ItemController itemControl = collision.GetComponent<ItemController>();
            if (itemControl.itemType != ItemType.COIN)
            {
                GameManager.Instance.PlayBallExplode(collision.transform.position,Color.blue);
                if (itemControl.itemType == ItemType.HIDDEN_GUNS)
                {
                    switch(gunCount)
                    {
                        case 1:
                            gunCount = 2;
                            hiddenGunsTime = DataManager.Instance.doubleGunsTime;
                            crCoutingHiddenGunsTime = StartCoroutine(CRCountingHiddenGunsTime());
                            break;
                        case 2:
                            gunCount = 3;
                            hiddenGunsTime += DataManager.Instance.tripleGunsTime;
                            StopCoroutine(crCoutingHiddenGunsTime);
                            crCoutingHiddenGunsTime = StartCoroutine(CRCountingHiddenGunsTime());
                            break;
                        case 3:
                            gunCount = 4;
                            hiddenGunsTime += DataManager.Instance.extraGunsTime;
                            StopCoroutine(crCoutingHiddenGunsTime);
                            crCoutingHiddenGunsTime = StartCoroutine(CRCountingHiddenGunsTime());
                            break;
                        case 4:
                            gunCount = 5;
                            hiddenGunsTime += DataManager.Instance.extraGunsTime;
                            StopCoroutine(crCoutingHiddenGunsTime);
                            crCoutingHiddenGunsTime = StartCoroutine(CRCountingHiddenGunsTime());
                            break;
                        default:
                            hiddenGunsTime += DataManager.Instance.extraGunsTime;
                            StopCoroutine(crCoutingHiddenGunsTime);
                            crCoutingHiddenGunsTime = StartCoroutine(CRCountingHiddenGunsTime());
                            break;
                    }
                }
            }
        }
    }

    private IEnumerator CRCountingHiddenGunsTime()
    {
        while (hiddenGunsTime > 0)
        {
            hiddenGunsTime -= Time.deltaTime;
            yield return null;
        }
        gunCount = 1;
    }


    IEnumerator FireBullets()
    {
        while (playerState == PlayerState.Living)
        {
            yield return new WaitForSeconds(DataManager.Instance.GetShootingWaitTime());
            if (gunCount == 1) //Fire center gun
            {
                //Hide left and right gun
                centerGun.SetActive(true);
                leftGun.SetActive(false);
                rightGun.SetActive(false);
                far_leftGun.SetActive(false);
                far_rightGun.SetActive(false);

                //Create position, moving direction for center bullet
                Vector2 centerDir = centerGun.transform.TransformDirection(Vector2.up);
                Vector2 centerBulletPoint = (Vector2)centerGun.transform.position
                    + centerDir * (centerGunRender.bounds.size.y / 2);

                //Enable center bullet and move
                GameManager.Instance.FireBullet(centerBulletPoint, centerDir);
            }
            else if (gunCount == 2) //Fire left and right guns
            {
                //Hide center gun
                centerGun.SetActive(false);
                leftGun.SetActive(true);
                rightGun.SetActive(true);
                far_leftGun.SetActive(false);
                far_rightGun.SetActive(false);

                //Create position, moving direction for left bullet
                Vector2 leftDir = leftGun.transform.TransformDirection(Vector2.up);
                Vector2 leftBulletPoint = (Vector2)leftGun.transform.position
                    + leftDir * (centerGunRender.bounds.size.y / 2);
                //Enable left bullet and move
                GameManager.Instance.FireBullet(leftBulletPoint, leftDir);

                //Create position, moving direction for right bullet
                Vector2 rightDir = rightGun.transform.TransformDirection(Vector2.up);
                Vector2 rightBulletPoint = (Vector2)rightGun.transform.position
                    + rightDir * (centerGunRender.bounds.size.y / 2);

                //Enable right bullet and move
                GameManager.Instance.FireBullet(rightBulletPoint, rightDir);
            }
            else if (gunCount == 3) //Fire three guns
            {
                //Enable three guns
                centerGun.SetActive(true);
                leftGun.SetActive(true);
                rightGun.SetActive(true);
                far_leftGun.SetActive(false);
                far_rightGun.SetActive(false);

                //Create position, moving direction for center bullet
                Vector2 centerDir = centerGun.transform.TransformDirection(Vector2.up);
                Vector2 centerBulletPoint = (Vector2)centerGun.transform.position
                    + centerDir * (centerGunRender.bounds.size.y / 2);
                //Enable center bullet and move
                GameManager.Instance.FireBullet(centerBulletPoint, centerDir);

                //Create position, moving direction for left bullet
                Vector2 leftDir = leftGun.transform.TransformDirection(Vector2.up);
                Vector2 leftBulletPoint = (Vector2)leftGun.transform.position
                    + leftDir * (centerGunRender.bounds.size.y / 2);
                //Enable left bullet and move
                GameManager.Instance.FireBullet(leftBulletPoint, leftDir);

                //Create position, moving direction for right bullet
                Vector2 rightDir = rightGun.transform.TransformDirection(Vector2.up);
                Vector2 rightBulletPoint = (Vector2)rightGun.transform.position
                    + rightDir * (centerGunRender.bounds.size.y / 2);

                //Enable right bullet and move
                GameManager.Instance.FireBullet(rightBulletPoint, rightDir);
            }
            else if (gunCount == 4) //Fire three guns
            {
                //Enable three guns
                centerGun.SetActive(false);
                leftGun.SetActive(true);
                rightGun.SetActive(true);
                far_leftGun.SetActive(true);
                far_rightGun.SetActive(true);

                //Create position, moving direction for left bullet
                Vector2 leftDir = leftGun.transform.TransformDirection(Vector2.up);
                Vector2 leftBulletPoint = (Vector2)leftGun.transform.position
                    + leftDir * (centerGunRender.bounds.size.y / 2);
                //Enable left bullet and move
                GameManager.Instance.FireBullet(leftBulletPoint, leftDir);

                //Create position, moving direction for left bullet
                Vector2 far_leftDir = far_leftGun.transform.TransformDirection(Vector2.up);
                Vector2 far_leftBulletPoint = (Vector2)far_leftGun.transform.position
                    + far_leftDir * (centerGunRender.bounds.size.y / 2);
                //Enable left bullet and move
                GameManager.Instance.FireBullet(far_leftBulletPoint, far_leftDir);

                //Create position, moving direction for right bullet
                Vector2 rightDir = rightGun.transform.TransformDirection(Vector2.up);
                Vector2 rightBulletPoint = (Vector2)rightGun.transform.position
                    + rightDir * (centerGunRender.bounds.size.y / 2);

                //Enable right bullet and move
                GameManager.Instance.FireBullet(rightBulletPoint, rightDir);

                //Create position, moving direction for right bullet
                Vector2 far_rightDir = far_rightGun.transform.TransformDirection(Vector2.up);
                Vector2 far_rightBulletPoint = (Vector2)far_rightGun.transform.position
                    + far_rightDir * (centerGunRender.bounds.size.y / 2);

                //Enable right bullet and move
                GameManager.Instance.FireBullet(far_rightBulletPoint, far_rightDir);
            }
            else if (gunCount == 5) //Fire three guns
            {
                //Enable three guns
                centerGun.SetActive(true);
                leftGun.SetActive(true);
                rightGun.SetActive(true);
                far_leftGun.SetActive(true);
                far_rightGun.SetActive(true);

                //Create position, moving direction for center bullet
                Vector2 centerDir = centerGun.transform.TransformDirection(Vector2.up);
                Vector2 centerBulletPoint = (Vector2)centerGun.transform.position
                    + centerDir * (centerGunRender.bounds.size.y / 2);
                //Enable center bullet and move
                GameManager.Instance.FireBullet(centerBulletPoint, centerDir);

                //Create position, moving direction for left bullet
                Vector2 leftDir = leftGun.transform.TransformDirection(Vector2.up);
                Vector2 leftBulletPoint = (Vector2)leftGun.transform.position
                    + leftDir * (centerGunRender.bounds.size.y / 2);
                //Enable left bullet and move
                GameManager.Instance.FireBullet(leftBulletPoint, leftDir);

                //Create position, moving direction for left bullet
                Vector2 far_leftDir = far_leftGun.transform.TransformDirection(Vector2.up);
                Vector2 far_leftBulletPoint = (Vector2)far_leftGun.transform.position
                    + far_leftDir * (centerGunRender.bounds.size.y / 2);
                //Enable left bullet and move
                GameManager.Instance.FireBullet(far_leftBulletPoint, far_leftDir);

                //Create position, moving direction for right bullet
                Vector2 rightDir = rightGun.transform.TransformDirection(Vector2.up);
                Vector2 rightBulletPoint = (Vector2)rightGun.transform.position
                    + rightDir * (centerGunRender.bounds.size.y / 2);

                //Enable right bullet and move
                GameManager.Instance.FireBullet(rightBulletPoint, rightDir);

                //Create position, moving direction for right bullet
                Vector2 far_rightDir = far_rightGun.transform.TransformDirection(Vector2.up);
                Vector2 far_rightBulletPoint = (Vector2)far_rightGun.transform.position
                    + far_rightDir * (centerGunRender.bounds.size.y / 2);

                //Enable right bullet and move
                GameManager.Instance.FireBullet(far_rightBulletPoint, far_rightDir);
            }
        }
        centerGun.SetActive(false);
        leftGun.SetActive(false);
        rightGun.SetActive(false);
        far_leftGun.SetActive(false);
        far_rightGun.SetActive(false);
    }
}
