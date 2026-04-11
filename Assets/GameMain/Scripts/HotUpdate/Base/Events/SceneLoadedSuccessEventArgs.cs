using GameFramework;
using GameFramework.Event;

namespace Quester
{
    public class SceneLoadedSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(SceneLoadedSuccessEventArgs).GetHashCode();
        
        private string _sceneAssetName;

        public string SceneAssetName
        {
            get => _sceneAssetName;
            set => _sceneAssetName = value;
        }

        public override int Id
        {
            get { return EventId; }
        }
       
        public static SceneLoadedSuccessEventArgs Create(string sceneAssetName)
        {
            var eventArgs = ReferencePool.Acquire<SceneLoadedSuccessEventArgs>();
            eventArgs.SceneAssetName = sceneAssetName;
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