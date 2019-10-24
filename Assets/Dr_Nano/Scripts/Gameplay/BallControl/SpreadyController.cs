using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JicaGames;

public class SpreadyController : BallController
{
    override public void MoveBall()
    {
        if (transform.position.x < PlayerController.Instance.limitLeft ||
            transform.position.x > PlayerController.Instance.limitRight)
        {
            DisableOutOfObject();
            return;
        }
        if (spRender == null)
            spRender = GetComponent<SpriteRenderer>();
        scaleDownFactor = DataManager.Instance.spreadyDeltaSize;
        numberText.enabled = true;
        numberText.text = number.ToString();
        StartCoroutine(MoveDown());
    }


    override protected IEnumerator MoveDown()
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

            Vector2 checkPos = transform.position;
            if (checkPos.y < PlayerController.Instance.limitBottom)
            {
                GameManager.Instance.HP -= int.Parse(numberText.text);
                PlayHitSound();

                if (GameManager.Instance.HP <= 0)
                {
                    StartCoroutine(SetGameOver());
                }
                else
                {
                    DisableObject(false);
                }
            }
        }
    }

    override protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsVisible)
        {
            if (collision.CompareTag("Bullet"))
            {
                if (number > 1)
                {
                    Vector3 pos = transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);
                    if (pos.y > GameManager.Instance.yPos) pos.y = GameManager.Instance.yPos;
                    GameManager.Instance.CreateSpready(number / 2, pos);
                    pos = transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);
                    if (pos.y > GameManager.Instance.yPos) pos.y = GameManager.Instance.yPos;
                    GameManager.Instance.CreateSpready(number / 2, pos);
                }
                DataManager.Instance.AddScore(1);
                DisableObject(true);
            }
            else if (collision.CompareTag("Destroy"))
            {
                if (number > 1)
                {
                    Vector3 pos = transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);
                    if (pos.y > GameManager.Instance.yPos) pos.y = GameManager.Instance.yPos;
                    GameManager.Instance.CreateSpready(number / 2, pos);
                    pos = transform.position + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0);
                    if (pos.y > GameManager.Instance.yPos) pos.y = GameManager.Instance.yPos;
                    GameManager.Instance.CreateSpready(number / 2, pos);
                }
                DataManager.Instance.AddScore(1);
                DisableObject(true);
            }
        }
    }

    override public void DisableObject(bool KilledByPlayer)
    {
        JicaGames.SoundManager.Instance.PlaySound(JicaGames.SoundManager.Instance.ballExplore);
        GameManager.Instance.PlaySpreadyBallExplode(transform.position);
        if (KilledByPlayer)
        {
            GameManager.Instance.AddKillCellCount();
        }
        gameObject.SetActive(false);
        IsVisible = false;
    }
}
