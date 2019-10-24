﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JicaGames;


public class TumorController : BallController
{
    private Vector2 dir = Vector2.zero;
    override public void MoveBall()
    {
        if (spRender == null)
            spRender = GetComponent<SpriteRenderer>();
        scaleDownFactor = (transform.localScale.x - DataManager.Instance.minBallScale) / (number - 1);
        numberText.enabled = true;
        numberText.text = number.ToString();
        if (Random.value < 0.5f)
        {
            dir = new Vector2(-1, -1).normalized;
        }
        else
        {
            dir = new Vector2(1, -1).normalized;
        }
        StartCoroutine(MoveDown());
    }
    override protected IEnumerator MoveDown()
    {
        while (gameObject.activeInHierarchy)
        {
            Vector2 pos = transform.position;
            pos += dir * fallingSpeed * Time.deltaTime * GameManager.Instance.speed_modifier;
            if (pos.x >= PlayerController.Instance.limitRight)
            {
                transform.position = new Vector2(PlayerController.Instance.limitRight, pos.y);
                dir.x = -dir.x;
            }
            else if (pos.x <= PlayerController.Instance.limitLeft)
            {
                transform.position = new Vector2(PlayerController.Instance.limitLeft, pos.y);
                dir.x = -dir.x;
            }
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
}
