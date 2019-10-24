using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JicaGames;

public class ItemController : MonoBehaviour {

    public ItemType itemType = ItemType.COIN;
    [HideInInspector]
    public float fallingSpeed = 0;

    private SpriteRenderer spRender = null;
    private bool stopFalling = false;
    public void MoveDown()
    {
        stopFalling = false;
        if (spRender == null)
            spRender = GetComponent<SpriteRenderer>();
        StartCoroutine(Falling());
    }

    IEnumerator Falling()
    {
        while (gameObject.activeInHierarchy)
        {
            if (stopFalling)
                yield break;
            Vector2 pos = transform.position;
            pos += Vector2.down * fallingSpeed * Time.deltaTime * GameManager.Instance.speed_modifier; 
            transform.position = pos;
            yield return null;

            if (GameManager.Instance.GameState == GameState.Revive)
            {
                gameObject.SetActive(false);
                yield break;
            }

            Vector2 checkPos = (Vector2)transform.position + Vector2.up * spRender.bounds.size.y;
            if (checkPos.y < PlayerController.Instance.limitBottom)
            {
                gameObject.SetActive(false);
                yield break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (stopFalling)
        {
            return;
        }
        if (collision.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.item);
            switch(itemType)
            {
                case ItemType.COIN:
                    stopFalling = true;
                    DataManager.Instance.AddCoins(1);
                    StartCoroutine(MoveToImg(ItemType.COIN));
                    break;
                case ItemType.BOSS_COIN:
                    stopFalling = true;
                    DataManager.Instance.AddCoins(DataManager.Instance.BossHavingCoins);
                    StartCoroutine(MoveToImg(ItemType.COIN));
                    break;
                case ItemType.BOMB:
                    stopFalling = true;
                    DataManager.Instance.AddBomb(1);
                    StartCoroutine(MoveToImg(ItemType.BOMB));
                    break;
                case ItemType.LASER:
                    stopFalling = true;
                    DataManager.Instance.AddLaser(1);
                    StartCoroutine(MoveToImg(ItemType.LASER));
                    break;
                case ItemType.MISSILE:
                    stopFalling = true;
                    DataManager.Instance.AddMissile(1);
                    StartCoroutine(MoveToImg(ItemType.MISSILE));
                    break;
                case ItemType.HIDDEN_GUNS:
                    stopFalling = true;
                    gameObject.SetActive(false);
                    break;
            }
            return;
        }
    }

    IEnumerator MoveToImg(ItemType type)
    {
        Vector2 endPos = UIManager.Instance.GetImgWorldPos(type);
        Vector2 startPos = transform.position;
        float t = 0;
        float lerpTime = 1f;
        while (t < lerpTime)
        {
            t += Time.deltaTime;
            float factor = t / lerpTime;
            transform.position = Vector2.Lerp(startPos, endPos, factor);
            transform.eulerAngles += new Vector3(0, 350 * Time.deltaTime, 0);
            yield return null;
        }
        transform.eulerAngles = Vector3.zero;
        gameObject.SetActive(false);
    }

}
