using GameMain.Scripts.ApiData.Request;
using GameMain.Scripts.ApiData.Response;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Quester
{
    public class ProcedureCheckVersion : ProcedureBase
    {
        private bool _versionChecked = true;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            Log.Info("ProcedureCheckVersion OnEnter");
            base.OnEnter(procedureOwner);
            // 检查是否需要强制更新
            GetVersionData();
        }
        
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!_versionChecked)
            {
                return;
            }
            
            ChangeState<ProcedureUpdateResource>(procedureOwner);
        }

        private void GetVersionData()
        {
            var requestData = new VersionRequest();
            requestData.appVersion = Application.version;
            requestData.platform = Application.platform.ToString();
            // requestData.platform = "ios";
            // var requestConfig = new RequestConfig { Url = Config.VERSION_URL, Method = "POST", Body = requestData, RetryCount = 3};
            // NetworkManager.Instance.Request<ApiResponse<VersionResponse>>(
            //     requestConfig,
            //     res =>
            //     {
            //         if (res.IsSuccess && res.Data.code == 0 && res.Data.data.forceUpdate)
            //         {
            //             ShowForceUpdateDlg(res.Data.data.storeUrl);
            //         }
            //         else
            //         {
            //             _versionChecked = true;
            //         }
            //     }
            // );
        }

        private void ShowForceUpdateDlg(string storeUrl)
        {
            GameEntry.UI.OpenForceUpdateDialog((obj) =>
            {
                // todo: 打开商店
            });
        }
        
    }
}