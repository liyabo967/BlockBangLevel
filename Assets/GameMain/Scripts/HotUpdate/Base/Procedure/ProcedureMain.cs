using System;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using System.Collections.Generic;
using System.Globalization;
using AppsFlyerSDK;
using BlockPuzzleGameToolkit.Scripts.Data;
using GameFramework.Procedure;
using GameMain.Scripts.HotUpdate.Base.Ads;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityGameFramework.Runtime;
using UnityGameFramework.Scripts.Runtime.Purchase;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Quester
{
    public class ProcedureMain : ProcedureBase
    {
        private ProcedureOwner _procedureOwner;
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            _procedureOwner = procedureOwner;
            Debug.Log("ProcedureMain OnEnter.");
            LoadingForm.Instance.CloseSelf();
            GameEntry.Sound.PlayMusic(MusicId.Main);
            
            GameEntry.Event.Subscribe(PurchaseResultEventArgs.EventId, OnPurchaseResult);
            InitPurchase();
            
            // 设置第一次启动标志
            if (PlayerPrefs.GetString("LaunchVersion") != Application.version)
            {
                PlayerPrefs.SetString("LaunchVersion", Application.version);
            }
            
            AdManager.Instance.OnShown += result =>
            {
                // Debug.Log("Procedure OnAdShown：" + result.AdType);
                if (result.AdType == AdType.Interstitial || result.AdType == AdType.RewardedVideo)
                {
                    GameEntry.Sound.PauseMusic();
                }
            };
            
            AdManager.Instance.OnAdClosed += result =>
            {
                // Debug.Log("Procedure OnAdClosed：" + result.AdType);
                if (result.AdType == AdType.Interstitial || result.AdType == AdType.RewardedVideo)
                {
                    GameEntry.Sound.ResumeMusic();
                }
            };

            AdManager.Instance.OnRevenuePaid += result =>
            {
                Dictionary<string, string> additionalParams = new Dictionary<string, string>();
                additionalParams.Add(AdRevenueScheme.COUNTRY, RegionInfo.CurrentRegion.ThreeLetterISORegionName);
                additionalParams.Add(AdRevenueScheme.AD_UNIT, result.AdUnitId);
                additionalParams.Add(AdRevenueScheme.AD_TYPE, result.AdType.ToString());
                additionalParams.Add(AdRevenueScheme.PLACEMENT, result.PlacementId);
                var logRevenue =
                    new AFAdRevenueData(result.AdNetwork, MediationNetwork.GoogleAdMob, result.Currency, result.Revenue);
                AppsFlyer.logAdRevenue(logRevenue, additionalParams);
            };
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.Event.Unsubscribe(PurchaseResultEventArgs.EventId, OnPurchaseResult);
        }

        private void OnPurchaseResult(object sender, GameEventArgs e)
        {
            if (e is PurchaseResultEventArgs eventArgs)
            {
                if (eventArgs.PurchaseResult.IsSuccessful && eventArgs.PurchaseResult.IsRestored)
                {
                    foreach (var productId in eventArgs.PurchaseResult.ProductIdList)
                    {
                        UserDataManager.Instance.SetPurchasedProductId(productId);
                        if (productId.Contains("noads"))
                        {
                            UserDataManager.Instance.SetNoAds();
                            AdManager.Instance.RemoveAds();
                        }
                    }

// #if UNITY_IPHONE
//                     GameEntry.UI.OpenUIForm(UIFormId.Tips, GameEntry.Localization.GetString("#restore_purchase_success"));
// #endif
                }
            }
        }
        
        private void InitPurchase()
        {
            var productTable = GameEntry.DataTable.GetDataTable<DRShopProduct>();
            var products = new Dictionary<string, ProductType>();
            foreach (var drShopProduct in productTable)
            {
                if (Enum.TryParse(drShopProduct.ProductType, out ProductType productType))
                {
                    products[drShopProduct.ProductId] = productType;
                }
            }

            GameEntry.Purchase.Initialize(products);
        }

        public void SwitchToGame()
        {
            _procedureOwner.SetData<VarInt32>("NextSceneId", 2);
            ChangeState<ProcedureChangeScene>(_procedureOwner);
        }
    }
}