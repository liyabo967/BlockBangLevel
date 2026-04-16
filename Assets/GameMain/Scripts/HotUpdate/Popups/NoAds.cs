// // ©2015 - 2026 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.Services;
using BlockPuzzleGameToolkit.Scripts.Services.IAP;
using BlockPuzzleGameToolkit.Scripts.System;
using GameFramework.Event;
using GameMain.Scripts.HotUpdate.Base.Ads;
using Quester;
using TMPro;
using UnityEngine;
using UnityGameFramework.Scripts.Runtime.Purchase;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class NoAds : UGuiForm
    {
        public CustomButton removeAdsButton;
        public ProductID productID;
        public TextMeshProUGUI priceText;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            removeAdsButton.onClick.AddListener(RemoveAds);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            GameEntry.Event.Subscribe(PurchaseResultEventArgs.EventId, OnPurchaseResult);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            GameEntry.Event.Unsubscribe(PurchaseResultEventArgs.EventId, OnPurchaseResult);
        }
        
        private void OnPurchaseResult(object sender, GameEventArgs e)
        {
            if (e is PurchaseResultEventArgs eventArgs)
            {
                GameEntry.UI.CloseUIForm(UIFormId.Processing);
                Debug.Log($"OnPurchaseResult: {eventArgs.PurchaseResult.ProductId}, {eventArgs.PurchaseResult.IsSuccessful}");
                if (eventArgs.PurchaseResult.IsSuccessful)
                {
                    PurchaseSucceeded(eventArgs.PurchaseResult.ProductId);
                }
                else
                {
                    Debug.LogWarning(eventArgs.PurchaseResult.Message);
                }
                GameEntry.UI.OpenUIForm(UIFormId.PurchaseResultUI, eventArgs);
            }
        }
        
        private void OnEnable()
        {
            if (priceText != null)
            {
                string price = GameEntry.Purchase.GetLocalPriceString(productID.ID);
                if (!string.IsNullOrEmpty(price))
                {
                    priceText.text = price;
                }
            }
        }

        private void PurchaseSucceeded(string productId)
        {
            if (productId == productID.ID)
            {
                // PlayerPrefs.SetInt("Purchased_" + productId, 1);
                // PlayerPrefs.Save();
                UserDataManager.Instance.SetPurchasedProductId(productId);
                AdManager.Instance.RemoveAds();
                Close();
            }
        }

        private void RemoveAds()
        {
            GameEntry.UI.OpenUIForm(UIFormId.Processing);
            GameEntry.Purchase.Purchase(productID.ID);
        }
    }
}