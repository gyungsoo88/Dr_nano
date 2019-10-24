using System;
using System.Collections;
using UnityEngine;

#if OG_ADMOB
using GoogleMobileAds.Api;
#endif

namespace JicaGames
{
    public class AdmobController : MonoBehaviour
    {
        public static AdmobController Instance { get; set; }

#if OG_ADMOB
        [Header("Admob Id")]
#if UNITY_ANDROID
        [SerializeField] private string androidAdmobAppId = "ca-app-pub-1064078647772222~8462516402";
#elif UNITY_IOS
        [SerializeField] private string iOSAdmobAppId = "ca-app-pub-1064078647772222~8462516402";
#endif

        [Header("Banner Id")]
#if UNITY_ANDROID
        [SerializeField] private string androidBannerId = "ca-app-pub-1064078647772222/9329609006";
#elif UNITY_IOS
        [SerializeField] private string iOSBannerId = "ca-app-pub-1064078647772222/9329609006";
#endif
        [SerializeField] private AdPosition bannerPosition = AdPosition.Bottom;


        [Header("Interstitial Ad Id")]
#if UNITY_ANDROID
        [SerializeField] private string androidInterstitialId = "ca-app-pub-1064078647772222/2139808686";
#elif UNITY_IOS
        [SerializeField] private string iOSInterstitialId = "ca-app-pub-1064078647772222/2139808686";
#endif

        [Header("Rewarded Base Video Id")]
#if UNITY_ANDROID
        [SerializeField] private string androidRewardedBaseVideoId = "ca-app-pub-1064078647772222/9919321234";

#elif UNITY_IOS
        [SerializeField] private string iOSRewardedBaseVideoId = "ca-app-pub-1064078647772222/9919321234";
#endif
#endif


#if OG_ADMOB
        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardBasedVideoAd rewardBasedVideo;
        private bool isCompletedRewardedVideo = false;
#endif


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

#if OG_ADMOB
#if UNITY_ANDROID
            MobileAds.Initialize(androidAdmobAppId);
#elif UNITY_IOS
        MobileAds.Initialize(iOSAdmobAppId);
#endif
            // Get singleton reward based video ad reference.
            rewardBasedVideo = RewardBasedVideoAd.Instance;

            // RewardBasedVideoAd is a singleton, so handlers should only be registered once.
            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
            rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
#endif
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Hide the current banner ad
        /// </summary>
        public void HideBanner()
        {
#if OG_ADMOB
            bannerView.Hide();
#endif
        }


        /// <summary>
        /// Load and show a banner ad
        /// </summary>
        public void LoadAndShowBanner(float delay)
        {
            StartCoroutine(CRLoadAndShowBanner(delay));
        }
        private IEnumerator CRLoadAndShowBanner(float delay)
        {
            yield return new WaitForSeconds(delay);
#if OG_ADMOB
            // Clean up banner ad before creating a new one.
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            // Create a 320x50 banner at the top of the screen.
#if UNITY_ANDROID
            bannerView = new BannerView(androidBannerId, AdSize.SmartBanner, bannerPosition);
#elif UNITY_IOS
            bannerView = new BannerView(iOSBannerId, AdSize.SmartBanner, bannerPosition);
#endif
            // Load banner ad.
            bannerView.LoadAd(new AdRequest.Builder().Build());
#endif
        }


        /// <summary>
        /// Request interstitial ad
        /// </summary>
        public void RequestInterstitial()
        {
#if OG_ADMOB
            // Clean up interstitial ad before creating a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            // Create an interstitial.
#if UNITY_ANDROID
            interstitial = new InterstitialAd(androidInterstitialId);
#elif UNITY_IOS
            interstitial = new InterstitialAd(iOSInterstitialId);
#endif
            // Register for ad events.
            interstitial.OnAdClosed += HandleInterstitialClosed;

            // Load an interstitial ad.
            interstitial.LoadAd(new AdRequest.Builder().Build());
#endif
        }


        /// <summary>
        /// Request rewarded video ad
        /// </summary>
        public void RequestRewardedVideo()
        {
#if OG_ADMOB
#if UNITY_ANDROID
            rewardBasedVideo.LoadAd(new AdRequest.Builder().Build(), androidRewardedBaseVideoId);
#elif UNITY_IOS
            rewardBasedVideo.LoadAd(new AdRequest.Builder().Build(), iOSRewardedBaseVideoId);
#endif
#endif
        }


        /// <summary>
        /// Determine whether the interstitial ad is ready
        /// </summary>
        /// <returns></returns>
        public bool IsInterstitialReady()
        {
#if OG_ADMOB
            if (interstitial.IsLoaded())
            {
                return true;
            }
            else
            {
                RequestInterstitial();
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// Show interstitial ad with given delay time
        /// </summary>
        /// <param name="delay"></param>
        public void ShowInterstitial(float delay)
        {
            StartCoroutine(CRShowInterstitial(delay));
        }
        private IEnumerator CRShowInterstitial(float delay)
        {
            yield return new WaitForSeconds(delay);
#if OG_ADMOB
            if (interstitial.IsLoaded())
            {
                interstitial.Show();
            }
            else
            {
                RequestInterstitial();
            }
#endif
        }



        /// <summary>
        /// Determine whether the rewarded video ad is ready
        /// </summary>
        /// <returns></returns>
        public bool IsRewardedVideoReady()
        {
#if OG_ADMOB
            if (rewardBasedVideo.IsLoaded())
            {
                return true;
            }
            else
            {
                RequestRewardedVideo();
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// Show rewarded video ad with given delay time 
        /// </summary>
        /// <param name="delay"></param>
        public void ShowRewardedVideo(float delay)
        {
            StartCoroutine(CRShowRewardedVideoAd(delay));
        }
        IEnumerator CRShowRewardedVideoAd(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
#if OG_ADMOB
            if (rewardBasedVideo.IsLoaded())
            {
                rewardBasedVideo.Show();
            }
            else
            {
                RequestRewardedVideo();
            }
#endif
        }





#if OG_ADMOB
        //Events callback
        private void HandleInterstitialClosed(object sender, EventArgs args)
        {
            RequestInterstitial();
        }

        private void HandleRewardBasedVideoRewarded(object sender, Reward args)
        {
            //User watched the whole video
            isCompletedRewardedVideo = true;
        }

        private void HandleRewardBasedVideoClosed(object sender, EventArgs args)
        {
            //User closed the video
            AdManager.Instance.OnRewardedVideoClosed(isCompletedRewardedVideo);
            isCompletedRewardedVideo = false;
            RequestRewardedVideo();
        }
#endif
    }
}

