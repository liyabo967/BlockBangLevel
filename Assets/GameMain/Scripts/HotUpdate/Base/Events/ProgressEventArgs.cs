using GameFramework;
using GameFramework.Event;

namespace Quester
{
    public class ProgressEventArgs : GameEventArgs
    {
        public enum ProgressKey
        {
            Unknown,
            HotUpdate,
            PreLoadDataTable,
            Preload,
            UpdateResource
        }
        
        public static readonly int EventId = typeof(ProgressEventArgs).GetHashCode();
        
        private ProgressKey _key;
        private float _progress;

        public ProgressKey Key
        {
            get => _key;
            set => _key = value;
        }

        public float Progress
        {
            get => _progress;
            set => _progress = value;
        }

        public override int Id
        {
            get { return EventId; }
        }
       
        public static ProgressEventArgs Create(ProgressKey key, float progress)
        {
            var eventArgs = ReferencePool.Acquire<ProgressEventArgs>();
            eventArgs.Progress = progress;
            eventArgs.Key = key;
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