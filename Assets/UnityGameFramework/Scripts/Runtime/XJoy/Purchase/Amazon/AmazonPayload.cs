namespace UnityGameFramework.Scripts.Runtime.Purchase.Amazon
{
    public class AmazonPayload
    {
        public string receiptId;
        public string userId;
        public bool isSandbox;
        public AmazonProduct product;
        public AmazonReceiptJson receiptJson;
    }
}