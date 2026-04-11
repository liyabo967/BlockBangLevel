namespace UnityGameFramework.Scripts.Runtime.Purchase
{
    public class PurchaseResult
    {
        private string _productId;
        private bool _isSuccessful;
        private bool _isRestored;
        private string _message;

        public string ProductId
        {
            get => _productId;
            set => _productId = value;
        }

        public bool IsSuccessful
        {
            get => _isSuccessful;
            set => _isSuccessful = value;
        }

        public bool IsRestored
        {
            get => _isRestored;
            set => _isRestored = value;
        }

        public string Message
        {
            get => _message;
            set => _message = value;
        }
    }
}