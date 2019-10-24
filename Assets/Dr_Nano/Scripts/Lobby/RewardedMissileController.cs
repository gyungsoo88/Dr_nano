﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JicaGames;

public class RewardedMissileController : MonoBehaviour
{
    [SerializeField] private GameObject sunbrust = null;
    [SerializeField] private Text missileTxt = null;

    public void StartReward(int missileAmount)
    {
        StartCoroutine(Rewarding(missileAmount));
    }

    IEnumerator Rewarding(int amount)
    {
        sunbrust.SetActive(false);

        //Reward missile
        for (int i = 1; i <= amount; i++)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.item);
            DataManager.Instance.AddMissile(1);
            missileTxt.text = i.ToString();
            yield return new WaitForSeconds(0.2f);
        }

        SoundManager.Instance.PlaySound(SoundManager.Instance.rewarded);

        //Rotate the sunbrust
        sunbrust.SetActive(true);
        RectTransform rect = sunbrust.GetComponent<RectTransform>();
        float t = 0;
        while (t < 3f)
        {
            t += Time.deltaTime;
            rect.eulerAngles += new Vector3(0, 0, 100f * Time.deltaTime);
            yield return null;
        }

        gameObject.SetActive(false);
    }

}
