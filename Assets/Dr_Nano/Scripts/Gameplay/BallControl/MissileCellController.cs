using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileCellController : BallController
{
    override protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsVisible)
        {
            if (collision.CompareTag("Bullet"))
            {
                if (int.Parse(numberText.text) <= 1)
                {
                    BoostUpController missileControl = GameManager.Instance.GetMissileControl();
                    missileControl.transform.position = transform.position;
                    missileControl.gameObject.SetActive(true);
                    missileControl.MoveUp();
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
                BoostUpController missileControl = GameManager.Instance.GetMissileControl();
                missileControl.transform.position = transform.position;
                missileControl.gameObject.SetActive(true);
                missileControl.MoveUp();
                GameManager.Instance.CreateBoostUp(transform.position);
            }
        }
    }
}
