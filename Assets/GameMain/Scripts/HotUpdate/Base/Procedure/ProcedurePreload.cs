using System;
using GameFramework.Event;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Purchasing;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Quester
{
    public class ProcedurePreload : ProcedureBase
    {
        private bool _loaded;

        private Dictionary<PreloadKey, float> _preloadProgressDict = new()
        {
            { PreloadKey.SeasonTime, 1f},
            { PreloadKey.AssetCheck, 1f}
        };

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            Log.Info("ProcedurePreload OnEnter");
            base.OnEnter(procedureOwner);
            GameEntry.Event.Subscribe(NetworkEventArgs.EventId, OnNetworkEvent);
            GameEntry.Event.Subscribe(PreloadSuccessEventArgs.EventId, OnPreloadSuccess);
            SetProgress();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.Event.Unsubscribe(NetworkEventArgs.EventId, OnNetworkEvent);
            GameEntry.Event.Unsubscribe(PreloadSuccessEventArgs.EventId, OnPreloadSuccess);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!_loaded)
            {
                return;
            }
            
            InitPurchase();
            procedureOwner.SetData<VarInt32>("NextSceneId", 1);
            procedureOwner.SetData<VarBoolean>("FromLaunch", true);
            ChangeState<ProcedureChangeScene>(procedureOwner);
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

        private void ShowNetworkErrorDlg()
        {
            GameEntry.UI.OpenNetworkErrorDialog((obj) =>
            {
                
            });
        }

        private void OnPreloadSuccess(object sender, GameEventArgs e)
        {
            _loaded = true;
        }

        private void OnNetworkEvent(object sender, GameEventArgs e)
        {
           
        }

        private void SetProgress()
        {
            var progress = 0f;
            foreach (var keyValuePair in _preloadProgressDict)
            {
                progress += keyValuePair.Value / _preloadProgressDict.Count;
            }

            GameEntry.Event.Fire(this, ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.Preload, progress));
        }

        private enum PreloadKey
        {
            SeasonTime,
            AssetCheck
        }
    }
}