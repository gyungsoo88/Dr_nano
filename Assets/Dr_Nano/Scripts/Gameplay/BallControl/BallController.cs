using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JicaGames;

public enum BallType
{
    None,
    Normal,     // 일반세포
    Tumor,      // 종양세포(자라지 않는 암세포)
    Cancer,      // 암세포(시간에 따라 크기가 커지는 암세포)
    Spready,     // 분열하는 세포
    Wall,        // 체력없는 세포(방패)
    Boss,
    Weakness,     // 보스 약점
    MissileCell,   // 죽으면 미사일이 되서 날아감
    GrowCell      // 일정 수치 맞으면 증가하다가 없어짐
}

public class BallController : MonoBehaviour {

    [HideInInspector]
    public int number = 1;
    [HideInInspector]
    public float fallingSpeed = 0;
    [HideInInspector]
    public float scaleDownFactor;
    public bool IsVisible { protected set; get; }
    public Text numberText = null;

    protected SpriteRenderer spRender = null;
    [SerializeField]
    protected Color color;

    virtual public void MoveBall()
    {
        if (transform.position.x < PlayerController.Instance.limitLeft ||
            transform.position.x > PlayerController.Instance.limitRight)
        {
            DisableOutOfObject();
            return;
        }
        if (spRender == null)
            spRender = GetComponent<SpriteRenderer>();
        scaleDownFactor = (transform.localScale.x - DataManager.Instance.minBallScale) / (number - 1);
        numberText.enabled = true;
        numberText.text = number.ToString();
        StartCoroutine(MoveDown());
    }

    protected virtual IEnumerator MoveDown()
    {
        while (gameObject.activeInHierarchy)
        {
            Vector2 pos = transform.position;
            pos += Vector2.down * fallingSpeed * Time.deltaTime * GameManager.Instance.speed_modifier;
            transform.position = pos;
            yield return null;

            if (GameManager.Instance.GameState != GameState.Playing)
            {
                DisableObject(false);
                yield break;
            }
            if (!IsVisible)
            {
                Vector2 currentPos = (Vector2)transform.position + Vector2.down * (spRender.bounds.size.y / 2);
                float y = Camera.main.WorldToViewportPoint(currentPos).y;
                if (y < 1f)
                    IsVisible = true;
            }
            Vector2 checkPos = (Vector2)transform.position;
            if (checkPos.y < PlayerController.Instance.limitBottom)
            {
                DisableObject(false);
                yield break;
            }
        }
    }

    protected IEnumerator SetGameOver()
    {
        ShareManager.Instance.CreateScreenshot();
        yield return null;
        DisableObject(false);
        if (GameManager.Instance.IsRevive)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            if (AdManager.Instance.IsRewardedVideoAdReady())
            {
                GameManager.Instance.Revive();
            }
            else
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    virtual protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsVisible)
        {
            if (collision.CompareTag("Bullet"))
            {
                if (int.Parse(numberText.text) <= 1)
                {
                    DataManager.Instance.AddScore(int.Parse(numberText.text));
                    DisableObject(true);
                    GameManager.Instance.CreateBoostUp(transform.position);
                }
                else
                {
                    int newNumber = int.Parse(numberText.text) - 1;
                    numberText.text = newNumber.ToString();
                    float newScale = transform.localScale.x - scaleDownFactor;
                    newScale = (newScale >= DataManager.Instance.minBallScale) ? newScale : DataManager.Instance.minBallScale;
                    transform.localScale = new Vector3(newScale, newScale, 1);
                    DataManager.Instance.AddScore(1);
                }
            }
            else if (collision.CompareTag("Destroy"))
            {
                DataManager.Instance.AddScore(int.Parse(numberText.text));
                DisableObject(true);
                GameManager.Instance.CreateBoostUp(transform.position);
            }
        }    
    }

    virtual public void DisableObject(bool KilledByPlayer)
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.ballExplore);
        GameManager.Instance.PlayBallExplode(transform.position, color);
        if (KilledByPlayer)
        {
            GameManager.Instance.AddKillCellCount();
        }
        gameObject.SetActive(false);
        IsVisible = false;
    }

    virtual public void GrowUp() { }
    virtual public bool IsBoss()
    {
        return false;
    }

    protected void PlayHitSound()
    {
        if (GameManager.Instance.HP >= 50)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.heartL);
        }
        else if (GameManager.Instance.HP <= 49)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.heartS);
        }
    }

    protected void DisableOutOfObject()
    {
        gameObject.SetActive(false);
        IsVisible = false;
    }
}
