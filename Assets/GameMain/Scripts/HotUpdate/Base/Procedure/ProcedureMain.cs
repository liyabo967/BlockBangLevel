using System;
using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using System.Collections.Generic;
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
            GameEntry.Sound.PlayMusic(MusicId.BGM01);
            
            GameEntry.Event.Subscribe(PurchaseResultEventArgs.EventId, OnPurchaseResult);
            InitPurchase();
            // var fromLaunch = procedureOwner.GetData<VarBoolean>("FromLaunch");
            // if (fromLaunch)
            // {
            //     GameEntry.UI.OpenUIForm(UIFormId.MainUI);
            //     LoadingForm.Instance.CloseSelf();
            // }
            // else
            // {
            //     GameEntry.UI.CloseAllLoadedUIForms();
            //     GameEntry.UI.OpenUIForm(UIFormId.MainUI);
            // }
            
            // if (UserDataManager.Instance.GetService().MusicEnabled)
            // {
            //     GameEntry.Sound.PlayMusic(MusicId.BGM01);
            // }
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

#if UNITY_IPHONE
                    GameEntry.UI.OpenUIForm(UIFormId.Tips, GameEntry.Localization.GetString("#restore_purchase_success"));
#endif
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