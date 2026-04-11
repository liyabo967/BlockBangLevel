using GameFramework;
using GameFramework.Event;

namespace Quester
{
    public class NetworkEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(NetworkEventArgs).GetHashCode();
        
       
        public override int Id
        {
            get { return EventId; }
        }
       
        public static NetworkEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<NetworkEventArgs>();
            return eventArgs;
        }

        /// <summary>
        /// 清理加载数据表成功事件。
        /// </summary>
        public override void Clear()
        {
            
        }
    }
}