//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace Quester
{
    /// <summary>
    /// 界面编号。
    /// </summary>
    public enum UIFormId : byte
    {
        Undefined = 0,

        /// <summary>
        /// 弹出框。
        /// </summary>
        Dialog,
        
        Launch,

        /// <summary>
        /// 主页面
        /// </summary>
        MainUI,
        
        MapDlg,
        
        CoinsShop,
        
        GDPR,
        
        LuckySpin,
        
        Settings,
        
        SettingsGame,
        
        Processing,
        
        DailyBonus,
        
        NoAds,
        
        Quit,
        
        // LevelTypes目录中的配置文件使用了这个枚举，添加 UI 需要在这几个后面添加，否则枚举索引会错误
        PrePlayBonus,
        PrePlayScore,
        PreWinBonus,
        PreWinScore,
        PreFailed,
        FailedBonus,
        FailedScore,
        FailedClassic,
        
        PurchaseResultUI,
        Tips,
        CollectionDlg
    }
}
