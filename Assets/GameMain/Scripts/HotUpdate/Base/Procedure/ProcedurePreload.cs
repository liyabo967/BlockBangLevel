using System;
using System.Collections;
using GameFramework.Event;
using System.Collections.Generic;
using System.IO;
using BlockPuzzleGameToolkit.Scripts.Data;
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
            { PreloadKey.SeasonTime, 0f },
            { PreloadKey.AssetCheck, 0f }
        };

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            Log.Info("ProcedurePreload OnEnter");
            base.OnEnter(procedureOwner);
            InitUserData();
            InitSeasonTime();
            GameEntry.Event.Subscribe(PreloadSuccessEventArgs.EventId, OnPreloadSuccess);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
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

        public void InitUserData()
        {
            UserDataManager.Instance.Load();
        }

        private void InitSeasonTime()
        {
            TimeManager.SetSeasonTime((result) =>
            {
                UpdateProgress(PreloadKey.SeasonTime, 1f);
                CheckPicture();
            });
        }

        private void CheckPicture()
        {
            var picturePath = $"pictures/{TimeManager.SeasonTime.year}/{TimeManager.SeasonTime.week}.jpg";
            var pictureUrl = $"https://assets-1301567094.cos.ap-beijing.myqcloud.com/block-bang/{picturePath}";
            string savePath = Path.Combine(Application.persistentDataPath, picturePath);
            if (File.Exists(savePath))
            {
                CheckPictureCompleted();
                return;
            }

            CoroutineRunner.Instance.StartCo(
                FileDownloader.Instance.Download(pictureUrl, savePath,
                    s => { CheckPictureCompleted(); },
                    s => { CheckPictureCompleted(); }));
        }

        private void CheckPictureCompleted()
        {
            UpdateProgress(PreloadKey.AssetCheck, 1f);
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

        private void OnPreloadSuccess(object sender, GameEventArgs e)
        {
            // 加载页面进度条完成之后，会收到该事件
            _loaded = true;
        }

        private void UpdateProgress(PreloadKey preloadKey, float keyProgress)
        {
            _preloadProgressDict[preloadKey] = keyProgress;
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