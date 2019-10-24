using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JicaGames;

public class BossController : BallController
{
    public BossWeaknessController weaknessObj = null;
    override protected IEnumerator MoveDown()
    {
        while (gameObject.activeInHierarchy)
        {
            if (transform.position.y > GameManager.Instance.yPos - 8)
            {
                Vector2 pos = transform.position;
                pos += Vector2.down * fallingSpeed * Time.deltaTime * GameManager.Instance.speed_modifier;
                transform.position = pos;
            }
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
        }
    }

    public void BossMakesCancer()
    {
        StartCoroutine(BossMakingCancer());
    }

    private IEnumerator BossMakingCancer()
    {
        while (gameObject.activeInHierarchy)
        {
            GameManager.Instance.CreateCancer();
            yield return new WaitForSeconds(DataManager.Instance.bossSpawnCancerFrequency);
        }
    }

    override protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsVisible)
        {
            if (collision.CompareTag("Bullet"))
            {
                if (int.Parse(numberText.text) <= 1)
                {
                    GameManager.Instance.IsBossAlive = false;
                    DataManager.Instance.AddScore(1);
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
                if (int.Parse(numberText.text) <= DataManager.Instance.boostUpDamage)
                {
                    GameManager.Instance.IsBossAlive = false;
                    DataManager.Instance.AddScore(int.Parse(numberText.text));
                    DisableObject(true);
                    GameManager.Instance.CreateBoostUp(transform.position);
                }
                else
                {
                    int newNumber = int.Parse(numberText.text) - DataManager.Instance.boostUpDamage;
                    numberText.text = newNumber.ToString();
                    float newScale = transform.localScale.x - scaleDownFactor;
                    newScale = (newScale >= DataManager.Instance.minBallScale) ? newScale : DataManager.Instance.minBallScale;
                    transform.localScale = new Vector3(newScale, newScale, 1);
                    DataManager.Instance.AddScore(DataManager.Instance.boostUpDamage);
                }
            }
        }
    }

    override public bool IsBoss()
    {
        return true;
    }

    override public void DisableObject(bool KilledByPlayer)
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.ballExplore);
        GameManager.Instance.PlayBallExplode(transform.position, color);
        if (KilledByPlayer)
        {
            GameManager.Instance.AddKillCellCount();
        }
        gameObject.SetActive(false);
        IsVisible = false;
        DropCoins();
    }

    private void DropCoins()
    {
        GameManager.Instance.CreateBossCoin(transform.position);
    }
}