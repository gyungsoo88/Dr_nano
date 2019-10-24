using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JicaGames;

public class CancerController : BallController
{
    // 공격을 당하지 않은 누적 시간
    private float peaceTime = 0;

    override public void GrowUp()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(GrowingUp());
        }
    }

    private IEnumerator GrowingUp()
    {
        while (gameObject.activeInHierarchy)
        {
            if (peaceTime < DataManager.Instance.cancerGrowthDelay)
            {
                peaceTime += Time.deltaTime;
                yield return null;
            }
            else
            {
                int newNumber = int.Parse(numberText.text) + 1;
                numberText.text = newNumber.ToString();
                float newScale = DataManager.Instance.cancerMinSize + newNumber * DataManager.Instance.cancerDeltaSize;
                transform.localScale = new Vector3(newScale, newScale, 1);
                yield return new WaitForSeconds(DataManager.Instance.cancerGrowthRate);
            }
        }
    }

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
        scaleDownFactor = DataManager.Instance.cancerDeltaSize;
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
        base.OnTriggerEnter2D(collision);
        peaceTime = 0;
    }
}
