using System;
using System.Collections;
using GameFramework.Event;
using System.Collections.Generic;
using System.Globalization;
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
            
            procedureOwner.SetData<VarInt32>("NextSceneId", 1);
            procedureOwner.SetData<VarBoolean>("FromLaunch", true);
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }

        public void InitUserData()
        {
            UserDataManager.Instance.Load();
            UserDataManager.Instance.SetLastLaunchTimestamp(DateTimeOffset.Now.ToUnixTimeSeconds());
        }

        private void InitSeasonTime()
        {
            // TimeManager.SetSeasonTime((result) =>
            // {
            //     UpdateProgress(PreloadKey.SeasonTime, 1f);
            //     CheckPicture();
            // });
            if (UserDataManager.Instance.UnlockAdventureTimestamp == 0 && UserDataManager.Instance.AdventureState == 2)
            {
                UserDataManager.Instance.SetUnlockAdventureTimestamp(DateTimeOffset.Now.ToUnixTimeSeconds());
            }
            TimeManager.SetSeasonTimeByUnlockAdventureTime();
            UpdateProgress(PreloadKey.SeasonTime, 1f);
            CheckPicture();
        }

        private bool HaveSpecialPictures(string countryCode)
        {
            HashSet<string> countryCodeSet = new HashSet<string>()
            {
                "US"
            };
            return countryCodeSet.Contains(countryCode);
        }

        private void CheckPicture()
        {
            var pictureName = $"{UserDataManager.Instance.PictureCount}.jpg";
            var countryCode = CountryHelper.GetCountryCode();
            if (!string.IsNullOrEmpty(countryCode) && HaveSpecialPictures(countryCode))
            {
                pictureName = $"{countryCode}/{pictureName}";
            }
            else
            {
                pictureName = $"US/{pictureName}";
            }
            var pictureUrl = $"https://assets-1301567094.cos.ap-beijing.myqcloud.com/block-bang/pictures/{pictureName}";
            Debug.Log($"pictureUrl: {pictureUrl}");
            string savePath = Path.Combine(Application.persistentDataPath, "Pictures", pictureName);
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