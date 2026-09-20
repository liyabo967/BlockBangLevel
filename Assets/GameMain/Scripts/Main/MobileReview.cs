using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#elif  UNITY_ANDROID
using System.Collections;
using Google.Play.Review;
#endif

namespace GameMain
{
    public class MobileReview : MonoSingleton<MobileReview>
    {
        private bool _reviewRequested;
#if UNITY_ANDROID
        private ReviewManager _reviewManager;
        private PlayReviewInfo _playReviewInfo;
        
#endif
        

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
#if UNITY_ANDROID
            _reviewManager = new ReviewManager();
#endif
        }

        public void RequestReview()
        {
#if UNITY_EDITOR
            return;
#endif
            
#if UNITY_IOS
            RequestIOSReview();
#elif UNITY_ANDROID
            StartCoroutine(RequestAndroidReview());
#endif
        }
        
#if UNITY_IOS
        private void RequestIOSReview()
        {
            if (_reviewRequested == false)
            {
                bool popupShown = Device.RequestStoreReview();
                if (popupShown)
                {
                    // The review popup was presented to the user, set "reviewRequested" to "true" to reflect that
                    // Note: there's no way to check if the user actually gave a review for the app or cancelled the popup.
                    _reviewRequested = true;
                }
                else
                {
                    // The review popup wasn't presented. Log a message and reset "reviewRequested" so you can revisit this in the future.
                    Debug.Log("iOS version is too low or StoreKit framework was not linked.");
                    _reviewRequested = false;
                }
            }
        }
#elif UNITY_ANDROID
        private IEnumerator RequestAndroidReview()
        {
            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                // Log error. For example, using requestFlowOperation.Error.ToString().
                yield break;
            }
            _playReviewInfo = requestFlowOperation.GetResult();
            
            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
            yield return launchFlowOperation;
            _playReviewInfo = null; // Reset the object
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                // Log error. For example, using launchFlowOperation.Error.ToString().
                yield break;
            } 
            // The flow has finished. The API does not indicate whether the user
            // reviewed or not, or even whether the review dialog was shown. Thus, no
            // matter the result, we continue our app flow.
        }
#endif
    }
}