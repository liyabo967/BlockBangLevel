using GameFramework;
using GameFramework.Event;

namespace Quester
{
    public class LanguageEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LanguageEventArgs).GetHashCode();

        public override int Id
        {
            get { return EventId; }
        }
       
        public static LanguageEventArgs Create()
        {
            var eventArgs = ReferencePool.Acquire<LanguageEventArgs>();
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