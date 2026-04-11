//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;

namespace Quester
{
    public sealed class PreloadSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PreloadSuccessEventArgs).GetHashCode();

        public PreloadSuccessEventArgs()
        {
            
        }
        
        public override int Id
        {
            get
            {
                return EventId;
            }
        }
   
        public static PreloadSuccessEventArgs Create()
        {
            PreloadSuccessEventArgs loadDataTableSuccessEventArgs = ReferencePool.Acquire<PreloadSuccessEventArgs>();
            return loadDataTableSuccessEventArgs;
        }
        
        public override void Clear()
        {
            
        }
    }
}
