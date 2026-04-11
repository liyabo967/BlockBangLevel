using UnityEngine;

namespace Quester
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        // public static BuiltinDataComponent BuiltinData
        // {
        //     get;
        //     private set;
        // }
        
        // public static UtilityToolsComponent UtilityTools
        // {
        //     get;
        //     private set;
        // }

        private static void InitCustomComponents()
        {
            // BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
            // Timer = UnityGameFramework.Runtime.GameEntry.GetComponent<TimerManager>();
            // UtilityTools = UnityGameFramework.Runtime.GameEntry.GetComponent<UtilityToolsComponent>();
        }
        private static void CloseCustomComponents()
        {
            // Timer.Close();
        }
    }
}
