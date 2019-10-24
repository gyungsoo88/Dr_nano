using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;


namespace JicaGames
{
    public class UnityAdController : MonoBehaviour//, IUnityAdsListener
    {
        public static UnityAdController Instance { get; set; }

        [SerializeField] private string unityAdID = "1611450";
        [SerializeField] private string bannerAdPlacementID = "banner";
        [SerializeField] private string videoAdPlacementID = "video";
        [SerializeField] private string rewardedVideoAdPlacementID = "rewardedVideo";
        [SerializeField] private bool enableTestMode = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            Advertisement.Initialize(unityAdID, enableTestMode);
           // Advertisement.AddListener(this);
        }



        /// <summary>
        /// Show the banner ad with given delay time
        /// </summary>
        public void ShowBanner(float delay)
        {
            StartCoroutine(CRShowBanner(delay));
        }

        /// <summary>
        /// Hide the current banner
        /// </summary>
        /// <param name="destroyBanner"></param>
        public void HideBanner(bool destroyBanner)
        {
            //Advertisement.Banner.Hide(destroyBanner);
        }


        private IEnumerator CRShowBanner(float delay)
        {
            yield return new WaitForSeconds(delay);
            //float timer = 0;
            //while (!Advertisement.Banner.isLoaded)
            //{
            //    yield return null;
            //    timer += Time.deltaTime;
            //    if (timer >= 5f)
            //    {
            //        timer = 0;
            //        Advertisement.Banner.Load(bannerAdPlacementID);
            //    }
            //}

            //Advertisement.Banner.Show(bannerAdPlacementID);
        }




        /// <summary>
        /// Determine whether the interstitial ad is ready
        /// </summary>
        /// <returns></returns>
        public bool IsInterstitialReady()
        {
            return Advertisement.IsReady(rewardedVideoAdPlacementID);
        }


        /// <summary>
        /// Show interstitial ad given given delay time
        /// </summary>
        /// <param name="delay"></param>
        public void ShowInterstitial(float delay)
        {
            StartCoroutine(CRShowInterstitial(delay));
        }
        private IEnumerator CRShowInterstitial(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (Advertisement.IsReady(videoAdPlacementID))
            {
                Advertisement.Show(videoAdPlacementID);
            }
        }


        /// <summary>
        /// Determine whether the rewarded video ad is ready
        /// </summary>
        /// <returns></returns>
        public bool IsRewardedVideoReady()
        {
            return Advertisement.IsReady(rewardedVideoAdPlacementID);
        }

        /// <summary>
        /// Show rewarded video with given delay time
        /// </summary>
        /// <param name="delay"></param>
        public void ShowRewardedVideo(float delay)
        {
            StartCoroutine(CRShowRewardedVideo(delay));
        }
        private IEnumerator CRShowRewardedVideo(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowOptions showOptions = new ShowOptions();
            showOptions.resultCallback = HandleRewardedVideoShowResult;
            Advertisement.Show(rewardedVideoAdPlacementID, showOptions);
        }
        public void HandleRewardedVideoShowResult(ShowResult showResult)
        {
            switch (showResult)
            {
                case ShowResult.Finished:
                    AdManager.Instance.OnRewardedVideoClosed(true);
                    break;
                case ShowResult.Skipped:
                case ShowResult.Failed:
                    AdManager.Instance.OnRewardedVideoClosed(false);
                    break;
            }
        }
        public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        {
            if (placementId.Equals(rewardedVideoAdPlacementID))
            {
                switch (showResult)
                {
                    case ShowResult.Finished:
                        {
                            AdManager.Instance.OnRewardedVideoClosed(true);
                            break;
                        }
                }
            }
        }

        public void OnUnityAdsReady(string placementId)
        {

        }

        public void OnUnityAdsDidError(string message)
        {

        }

        public void OnUnityAdsDidStart(string placementId)
        {

        }

    }

}