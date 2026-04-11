using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Quester
{
    public class ProcedureGame : ProcedureBase
    {
        private ProcedureOwner _procedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Debug.Log("ProcedureGame OnEnter.");
            _procedureOwner = procedureOwner;
            GameEntry.UI.CloseAllLoadedUIForms();
            LoadingForm.Instance.CloseSelf();
            // GameEntry.UI.OpenUIForm(UIFormId.GameSceneUI);
        }


        public void SwitchToMain()
        {
            _procedureOwner.SetData<VarBoolean>("FromLaunch", false);
            _procedureOwner.SetData<VarInt32>("NextSceneId", 1);
            ChangeState<ProcedureChangeScene>(_procedureOwner);
        }
    }
}