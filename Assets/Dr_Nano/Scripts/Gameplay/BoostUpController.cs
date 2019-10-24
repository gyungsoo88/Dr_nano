using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostUpController : MonoBehaviour {

    [SerializeField]
    private BoostUpType boostUpType = BoostUpType.BOMB;

    private SpriteRenderer spRender = null;
    private List<Vector2> listDirection = new List<Vector2>();


    public void MoveUp()
    {

        StartCoroutine(Moving());

        if (boostUpType == BoostUpType.BOMB)
        {
            if (spRender == null)
            {
                if (boostUpType == BoostUpType.BOMB)
                    spRender = GetComponent<SpriteRenderer>();
            }

            if (listDirection.Count == 0)
            {
                listDirection.Add(Vector2.up);
                listDirection.Add(Vector2.left);
                listDirection.Add(Vector2.right);
                listDirection.Add(Vector2.up + Vector2.left);
                listDirection.Add(Vector2.up + Vector2.right);
                listDirection.Add(Vector2.down + Vector2.left);
                listDirection.Add(Vector2.down + Vector2.right);
            }

            StartCoroutine(Fire());
            StartCoroutine(Rotate());
        }
    }

	
    IEnumerator Moving()
    {
        float speed = 0;
        if (boostUpType == BoostUpType.BOMB)
            speed = DataManager.Instance.bombMovingSpeed;
        else if (boostUpType == BoostUpType.LASER)
            speed = DataManager.Instance.laserMovingSpeed;
        else if (boostUpType == BoostUpType.MISSILE)
            speed = DataManager.Instance.missileMovingSpeed;
        while (gameObject.activeInHierarchy)
        {
            Vector2 pos = transform.position;
            pos += Vector2.up * speed * Time.deltaTime;
            transform.position = pos;
            yield return null;

            if (GameManager.Instance.GameState == GameState.Revive)
            {
                gameObject.SetActive(false);
                yield break;
            }

            if (transform.position.y >= GameManager.Instance.yPos)
            {
                gameObject.SetActive(false);
                yield break;
            }
        }
    }




    //Use for bomb
    IEnumerator Fire()
    {
        while (gameObject.activeInHierarchy)
        {
            foreach(Vector2 o in listDirection)
            {
                
                GameManager.Instance.FireBullet(transform.position, o);
            }
            yield return new WaitForSeconds(DataManager.Instance.GetShootingWaitTime());
        }
    }

    IEnumerator Rotate()
    {
        while (gameObject.activeInHierarchy)
        {
            transform.eulerAngles += new Vector3(0, 0, 150f * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (boostUpType == BoostUpType.LASER ||
            boostUpType == BoostUpType.MISSILE)
        {
            if (collision.CompareTag("Ball"))
            {
                BallController ballControl = collision.GetComponent<BallController>();
                if (ballControl.IsVisible && ballControl.IsBoss())
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
