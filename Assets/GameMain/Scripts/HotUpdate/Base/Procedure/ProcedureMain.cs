using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;
using System.Collections.Generic;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;
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

        public void SwitchToGame()
        {
            _procedureOwner.SetData<VarInt32>("NextSceneId", 2);
            ChangeState<ProcedureChangeScene>(_procedureOwner);
        }
    }
}