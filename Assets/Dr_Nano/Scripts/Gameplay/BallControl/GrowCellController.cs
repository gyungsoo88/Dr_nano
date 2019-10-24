using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JicaGames;

public class GrowCellController : BallController
{
    int growcellHP;
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
        scaleDownFactor = DataManager.Instance.growCellDeltaSize;
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
                growcellHP = Random.Range(DataManager.Instance.growcellHP - 5, DataManager.Instance.growcellHP + 5);
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
                DataManager.Instance.AddScore(1);
                if (int.Parse(numberText.text) >= growcellHP - 1)
                {
                    GameManager.Instance.CreateBoostUp(transform.position);
                    DisableObject(true);
                }
                else
                {
                    int newNumber = int.Parse(numberText.text) + 1;
                    numberText.text = newNumber.ToString();
                    float newScale = DataManager.Instance.growCellMinSize + newNumber * DataManager.Instance.growCellDeltaSize;
                    transform.localScale = new Vector3(newScale, newScale, 1);
                }
            }
            else if (collision.CompareTag("Destroy"))
            {
                DataManager.Instance.AddScore(growcellHP - int.Parse(numberText.text));
                DisableObject(true);
                GameManager.Instance.CreateBoostUp(transform.position);
            }
        }
    }
}
