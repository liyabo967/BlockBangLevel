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
    public sealed class ReviveBlocksEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ReviveBlocksEventArgs).GetHashCode();
        
        public override int Id
        {
            get
            {
                return EventId;
            }
        }
   
        public static ReviveBlocksEventArgs Create()
        {
            ReviveBlocksEventArgs eventArgs = ReferencePool.Acquire<ReviveBlocksEventArgs>();
            return eventArgs;
        }
        
        public override void Clear()
        {
            
        }
    }
}
