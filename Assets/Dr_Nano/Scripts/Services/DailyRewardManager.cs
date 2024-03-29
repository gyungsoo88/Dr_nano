﻿using UnityEngine;
using System.Collections;
using System;

namespace JicaGames
{

    public class DailyRewardManager : MonoBehaviour
    {
        public static DailyRewardManager Instance { get; private set; }

        private DateTime NextRewardTime
        {
            get
            {
                return GetNextRewardTime();
            }
        }

        public TimeSpan TimeUntilNextReward
        { 
            get
            {
                return NextRewardTime.Subtract(DateTime.Now);
            }
        }

        [Header("Daily Reward Config")]
        private int rewardHours = 24;
        private int rewardMinutes = 0;
        private int rewardSeconds = 0;
        [SerializeField] private int minRewardValue = 50;
        [SerializeField] private int maxRewardValue = 100;

        private const string NextRewardTimePPK = "ONEFALLGAMES_DAILY_REWARD_TIME";

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
               // DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Is the waiting time has passed and can reward now.
        /// </summary>
        /// <returns><c>true</c> if this instance can reward now; otherwise, <c>false</c>.</returns>
        public bool CanRewardNow()
        {
            return TimeUntilNextReward <= TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the random reward.
        /// </summary>
        /// <returns>The random reward.</returns>
        public int GetRandomReward()
        {
            return 50;
        }

        public int GetRewardMissile()
        {
            return 3;
        }

        public int GetRewardBomb()
        {
            return 2;
        }

        public int GetRewardLaser()
        {
            return 1;
        }

        /// <summary>
        /// Reset the reward time
        /// </summary>
        public void ResetNextRewardTime()
        {
            DateTime next = DateTime.Now.Add(new TimeSpan(rewardHours, rewardMinutes, rewardSeconds));
            SaveNextRewardTime(next);
        }

        void SaveNextRewardTime(DateTime time)
        {
            PlayerPrefs.SetString(NextRewardTimePPK, time.ToBinary().ToString());
            PlayerPrefs.Save();
        }


        /// <summary>
        /// Get next reward time with Datetime format
        /// </summary>
        /// <returns></returns>
        DateTime GetNextRewardTime()
        {
            string storedTime = PlayerPrefs.GetString(NextRewardTimePPK, string.Empty);

            if (!string.IsNullOrEmpty(storedTime))
                return DateTime.FromBinary(Convert.ToInt64(storedTime));
            else
                return DateTime.Now;
        }
    }
}
