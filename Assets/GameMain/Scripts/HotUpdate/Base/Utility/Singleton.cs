namespace Quester
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new T();
                        _instance.OnInit();
                    }
                }

                return _instance;
            }
        }

        protected Singleton()
        {
        }

        protected virtual void OnInit()
        {
            
        }
    }
}