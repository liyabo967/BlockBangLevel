using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Scripts.Runtime.Purchase
{
    public class PurchaseResultEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PurchaseResultEventArgs).GetHashCode();

        public PurchaseResultEventArgs()
        {
            PurchaseResult = null;
        }

        public override int Id
        {
            get { return EventId; }
        }

        public PurchaseResult PurchaseResult { get; set; }

        public static PurchaseResultEventArgs Create(PurchaseResult purchaseResult)
        {
            PurchaseResultEventArgs args = ReferencePool.Acquire<PurchaseResultEventArgs>();
            args.PurchaseResult = purchaseResult;
            return args;
        }

        public override void Clear()
        {
            
        }
    }
}