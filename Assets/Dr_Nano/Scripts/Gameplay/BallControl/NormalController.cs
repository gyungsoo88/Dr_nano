using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalController : BallController
{
    override protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsVisible)
        {
            if (collision.CompareTag("Bullet"))
            {
                if (int.Parse(numberText.text) <= 1)
                {
                    GameManager.Instance.HP += number / 2;
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
                GameManager.Instance.HP += number / 2;
                DataManager.Instance.AddScore(int.Parse(numberText.text));
                DisableObject(true);
                GameManager.Instance.CreateBoostUp(transform.position);
            }
        }
    }
}