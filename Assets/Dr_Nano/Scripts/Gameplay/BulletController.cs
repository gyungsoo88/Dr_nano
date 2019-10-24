using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JicaGames;

public class BulletController : MonoBehaviour {

    [HideInInspector]
    public Vector2 MovingDirection
    {
        get
        {
            return movingDirection;
        }
        set
        {
            movingDirection = value;
            if (value.Equals(Vector2.zero))
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(movingDirection.y, movingDirection.x)
                    * Mathf.Rad2Deg - 90);
            }
        }
    }
    private Vector2 movingDirection = Vector2.zero;
    [HideInInspector]
    public float movingSpeed = 0f;

    public void MoveBullet()
    {
        StartCoroutine(Moving());
    }

    IEnumerator Moving()
    {
        while (gameObject.activeInHierarchy)
        {
            Vector2 pos = transform.position;
            pos += movingDirection * movingSpeed * Time.deltaTime;
            transform.position = pos;
            yield return null;

            if (transform.position.y >= GameManager.Instance.yPos
                || transform.position.y < PlayerController.Instance.limitBottom)
            {
                gameObject.SetActive(false);
                yield break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            BallController ballControl = collision.GetComponent<BallController>();
            if (ballControl.IsVisible)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.bulletExplore);
                Vector2 contactPos = collision.bounds.ClosestPoint(transform.position);
                Vector3 lookDirection = (collision.transform.position - transform.position).normalized;
                GameManager.Instance.PlayBulletExplore(contactPos, lookDirection);
                gameObject.SetActive(false);
            }        
        }
    }
}
