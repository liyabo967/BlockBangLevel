using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Scripts.Runtime.Purchase
{
    public class IAPPaywallCallbacks
    {
        private PurchaseComponent _purchaseComponent;
        
        public IAPPaywallCallbacks(PurchaseComponent paywallManager)
        {
            _purchaseComponent = paywallManager;
        }
        
        public void OnInitialProductsFetched(List<Product> products)
        {
            _purchaseComponent.FetchExistingPurchases();
        }
        public void OnInitialProductsFetchFailed(ProductFetchFailed failure)
        {
            Debug.LogError($"OnInitialProductsFetchFailed: {failure.FailureReason}");
        }
        public void OnExistingPurchasesFetched(Orders existingOrders)
        {
            Log.Info(PurchaseComponent.IsReceiptAvailable(existingOrders) ? "Success - Found Existing Orders with receipts" : "Notice: - No Existing Orders with receipts");
        }
        public void OnExistingPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.LogError($"OnExistingPurchasesFetchFailed: {failure.Message}");
        }
        public void OnPurchasePending(PendingOrder order)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;
                Log.Info($"CompletedPurchase: {product.definition.id}");
                // _purchaseComponent.m_IAPLogger.LogCompletedPurchase(product, order.Info);
                _purchaseComponent.ValidatePurchaseIfPossible(order.Info);
            }

            _purchaseComponent.ConfirmOrderIfAutomatic(order);
        }
        public void OnPurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case FailedOrder failedOrder:
                    OnConfirmationFailed(failedOrder);
                    break;
                case ConfirmedOrder confirmedOrder:
                    OnPurchaseConfirmed(confirmedOrder);
                    break;
            }
        }

        void OnConfirmationFailed(FailedOrder failedOrder)
        {
            var reason = failedOrder.FailureReason;

            foreach (var cartItem in failedOrder.CartOrdered.Items())
            {
                Log.Error($"OnConfirmationFailed: {cartItem.Product}, {reason}");
                // _purchaseComponent.m_IAPLogger.LogFailedConfirmation(cartItem.Product, reason);
                
                PurchaseResult result = new PurchaseResult()
                {
                    ProductId = cartItem.Product.definition.id,
                    IsSuccessful = false,
                    Message = $"The product service has no product with the ID {cartItem.Product.definition.id}"
                };
                GameEntry.GetComponent<EventComponent>().Fire(this, PurchaseResultEventArgs.Create(result));
            }
        }

        public void OnPurchaseConfirmed(ConfirmedOrder order)
        {
            foreach (var cartItem in order.CartOrdered.Items())
            {
                var product = cartItem.Product;
                Log.Info($"OnPurchaseConfirmed: {product.definition.id}");
                // _purchaseComponent.m_IAPLogger.LogConfirmedOrder(product, order.Info);
                
                PurchaseResult result = new PurchaseResult()
                {
                    ProductId = product.definition.id,
                    IsSuccessful = true,
                    Message = "Success"
                };
                GameEntry.GetComponent<EventComponent>().Fire(this, PurchaseResultEventArgs.Create(result));
            }
        }
        public void OnPurchaseFailed(FailedOrder failedOrder)
        {
            var reason = failedOrder.FailureReason;

            foreach (var cartItem in failedOrder.CartOrdered.Items())
            {
                Log.Info($"OnPurchaseFailed: {cartItem.Product.definition.id}, {reason}");
                // _purchaseComponent.m_IAPLogger.LogFailedPurchase(cartItem.Product, reason);
                PurchaseResult result = new PurchaseResult()
                {
                    ProductId = cartItem.Product.definition.id,
                    IsSuccessful = false,
                    Message = $"OnPurchaseFailed, {reason}"
                };
                GameEntry.GetComponent<EventComponent>().Fire(this, PurchaseResultEventArgs.Create(result));
            }
        }
        
        public void OnOrderDeferred(DeferredOrder deferredOrder)
        {
            foreach (var cartItem in deferredOrder.CartOrdered.Items())
            {
                // _purchaseComponent.m_IAPLogger.LogDeferredPurchase(cartItem.Product);
                Log.Info($"OnOrderDeferred: {cartItem.Product.definition.id}");
            }
        }
    }
}
