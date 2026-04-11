using GameFramework.Event;
using System.Collections.Generic;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Quester
{
    public class ProcedurePreloadDataTable : ProcedureBase
    {
        private static readonly string[] DataTableNames =
        {
            "Scene",
            "UIForm",
            "Music",
            "Sound",
            "UISound",
            "ShopProduct",
            
            "Element",
            "ElementCategory",
            "CollectElement"
        };
        
        private Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();
        
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            Log.Info("ProcedurePreloadDataTable Entered.");
            base.OnEnter(procedureOwner);
            GameEntry.Event.Subscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Subscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            m_LoadedFlag.Clear();
            LoadLocalization();
            PreloadDataTable();
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            GameEntry.Event.Unsubscribe(LoadConfigSuccessEventArgs.EventId, OnLoadConfigSuccess);
            GameEntry.Event.Unsubscribe(LoadConfigFailureEventArgs.EventId, OnLoadConfigFailure);
            GameEntry.Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            GameEntry.Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            foreach (KeyValuePair<string, bool> loadedFlag in m_LoadedFlag)
            {
                if (!loadedFlag.Value)
                {
                    return;
                }
            }
            
            ChangeState<ProcedureCheckVersion>(procedureOwner);
        }

        private void LoadLocalization()
        {
            GameEntry.Localization.ReadData("Assets/GameMain/Configs/PresetLanguage.txt");
            GameEntry.Localization.ReadData("Assets/GameMain/Configs/LanguageConfig.txt");
            Log.Info("ProcedurePreloadDataTable LoadLocalization.");
        }

        private void PreloadDataTable()
        {
            foreach (string dataTableName in DataTableNames)
            {
                LoadDataTable(dataTableName);
            }
        }

        private void LoadDataTable(string dataTableName)
        {
            var fromBytes = true;
#if UNITY_EDITOR
            fromBytes = false;
#endif
            string dataTableAssetName = AssetUtility.GetDataTableAsset(dataTableName, fromBytes);
            m_LoadedFlag.Add(dataTableAssetName, false);
            GameEntry.DataTable.LoadDataTable(dataTableName, dataTableAssetName, this);
        }

        private void OnLoadConfigSuccess(object sender, GameEventArgs e)
        {
            LoadConfigSuccessEventArgs ne = (LoadConfigSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.ConfigAssetName] = true;
            Log.Info("Load config '{0}' OK.", ne.ConfigAssetName);
        }

        private void OnLoadConfigFailure(object sender, GameEventArgs e)
        {
            LoadConfigFailureEventArgs ne = (LoadConfigFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load config '{0}' from '{1}' with error message '{2}'.", ne.ConfigAssetName,
                ne.ConfigAssetName, ne.ErrorMessage);
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            LoadDataTableSuccessEventArgs ne = (LoadDataTableSuccessEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            m_LoadedFlag[ne.DataTableAssetName] = true;
            
            
            GameEntry.Event.Fire(this, ProgressEventArgs.Create(ProgressEventArgs.ProgressKey.PreLoadDataTable, GetProgress()));
        }

        private float GetProgress()
        {
            var i = 0;
            foreach (var keyValuePair in m_LoadedFlag)
            {
                if (keyValuePair.Value)
                {
                    i++;
                }
            }
            return i / (float)m_LoadedFlag.Count;
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            LoadDataTableFailureEventArgs ne = (LoadDataTableFailureEventArgs)e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName,
                ne.DataTableAssetName, ne.ErrorMessage);
        }
    }
}