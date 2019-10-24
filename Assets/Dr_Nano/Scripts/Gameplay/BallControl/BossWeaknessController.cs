using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWeaknessController : BallController
{
    public void Rotate()
    {
        StartCoroutine(Rotating());
    }

    private IEnumerator Rotating()
    {
        while (gameObject.activeInHierarchy)
        {
            transform.parent.Rotate(new Vector3(0, 0,
                DataManager.Instance.weaknessObjRotationSpeed * Time.deltaTime));
            yield return null;
        }
    }

    override protected IEnumerator MoveDown()
    {
        while (gameObject.activeInHierarchy)
        {
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

    override protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsVisible)
        {
            BallController boss = GameManager.Instance.bossObj.GetComponent<BallController>();
            if (collision.CompareTag("Bullet"))
            {
                if (int.Parse(boss.numberText.text) <= 2)
                {
                    GameManager.Instance.IsBossAlive = false;
                    DataManager.Instance.AddScore(int.Parse(boss.numberText.text));
                    boss.DisableObject(true);
                    GameManager.Instance.CreateBoostUp(boss.transform.position);
                }
                else
                {
                    int newNumber = int.Parse(boss.numberText.text) - 2;
                    boss.numberText.text = newNumber.ToString();
                    float newScale = boss.transform.localScale.x - boss.scaleDownFactor;
                    newScale = (newScale >= DataManager.Instance.minBallScale) ? newScale : DataManager.Instance.minBallScale;
                    boss.transform.localScale = new Vector3(newScale, newScale, 1);
                    DataManager.Instance.AddScore(2);
                }
            }
            else if (collision.CompareTag("Destroy"))
            {
                if (int.Parse(boss.numberText.text) <= DataManager.Instance.boostUpDamage * 2)
                {
                    GameManager.Instance.IsBossAlive = false;
                    DataManager.Instance.AddScore(int.Parse(boss.numberText.text));
                    boss.DisableObject(true);
                    GameManager.Instance.CreateBoostUp(boss.transform.position);
                }
                else
                {
                    int newNumber = int.Parse(boss.numberText.text) - DataManager.Instance.boostUpDamage * 2;
                    boss.numberText.text = newNumber.ToString();
                    float newScale = boss.transform.localScale.x - boss.scaleDownFactor;
                    newScale = (newScale >= DataManager.Instance.minBallScale) ? newScale : DataManager.Instance.minBallScale;
                    boss.transform.localScale = new Vector3(newScale, newScale, 1);
                    DataManager.Instance.AddScore(DataManager.Instance.boostUpDamage * 2);
                }
            }
        }
    }
}
