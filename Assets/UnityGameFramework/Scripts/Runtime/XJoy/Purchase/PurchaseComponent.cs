using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Scripts.Runtime.Purchase
{
    public class PurchaseComponent : GameFrameworkComponent
    {
        IStoreService _storeService;
        IProductService _productService;
        IPurchaseService _purchasingService;

        ICatalogProvider _catalogProvider = new CatalogProvider();
        CrossPlatformValidator _crossPlatformValidator;
        
        readonly IAPPaywallCallbacks _iapPaywallCallbacks;
        public PurchaseComponent()
        {
            _iapPaywallCallbacks = new IAPPaywallCallbacks(this);
        }

        // Here we create the services that will be used by the PaywallManager.
        protected override void Awake()
        {
            base.Awake();
            CreateServices();
        }

        // Here we initialize the catalog, the IAP service, the cross platform validator and connect to the store.
        // If you want to initialize this automatically, change the function signature to "void Start()"
        public void Initialize(Dictionary<string, ProductType> products)
        {
            InitCatalog(products);
            InitializeIapService();
            CreateCrossPlatformValidator();

            ConnectToStore();
        }

        void InitCatalog(Dictionary<string, ProductType> products)
        {
            var initialProductsToFetch = new List<ProductDefinition>();
            
            foreach (var product in products)
            {
                initialProductsToFetch.Add(new ProductDefinition(product.Key, product.Value));
            }

            _catalogProvider.AddProducts(initialProductsToFetch);
        }

        void CreateServices()
        {
            _storeService = UnityIAPServices.DefaultStore();
            _productService = UnityIAPServices.DefaultProduct();
            _purchasingService = UnityIAPServices.DefaultPurchase();

            ConfigureServiceCallbacks();
        }

        void ConfigureServiceCallbacks()
        {
            ConfigureProductServiceCallbacks();
            ConfigurePurchasingServiceCallbacks();
        }

        void ConfigureProductServiceCallbacks()
        {
            _productService.OnProductsFetched += _iapPaywallCallbacks.OnInitialProductsFetched;
            _productService.OnProductsFetchFailed += _iapPaywallCallbacks.OnInitialProductsFetchFailed;
        }

        void ConfigurePurchasingServiceCallbacks()
        {
            _purchasingService.OnPurchasesFetched += _iapPaywallCallbacks.OnExistingPurchasesFetched;
            _purchasingService.OnPurchasesFetchFailed += _iapPaywallCallbacks.OnExistingPurchasesFetchFailed;
            _purchasingService.OnPurchasePending += _iapPaywallCallbacks.OnPurchasePending;
            _purchasingService.OnPurchaseConfirmed += _iapPaywallCallbacks.OnPurchaseConfirmed;
            _purchasingService.OnPurchaseFailed += _iapPaywallCallbacks.OnPurchaseFailed;
            _purchasingService.OnPurchaseDeferred += _iapPaywallCallbacks.OnOrderDeferred;
        }

        public void FetchExistingPurchases()
        {
            _purchasingService.FetchPurchases();
        }

        public void RestorePurchases()
        {
            _purchasingService.RestoreTransactions(OnTransactionsRestored);
        }

        void OnTransactionsRestored(bool success, string error)
        {
            Log.Info($"OnTransactionsRestored, success: {success}, {error}");
        }

        public static bool IsReceiptAvailable(Orders existingOrders)
        {
            return existingOrders != null &&
                   (existingOrders.ConfirmedOrders.Any(order => !string.IsNullOrEmpty(order.Info.Receipt)) ||
                    existingOrders.PendingOrders.Any(order => !string.IsNullOrEmpty(order.Info.Receipt)));
        }

        void InitializeIapService()
        {
            IAPService.Initialize(OnServiceInitialized, (message) =>
            {
                Log.Error($"Initialization failed, {message}");
            });
        }

        void CreateCrossPlatformValidator()
        {
#if !UNITY_EDITOR
        try
        {
            if (CanCrossPlatformValidate())
            {
                _crossPlatformValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
            }
        }
        catch (Exception exception)
        {
            Log.Error($"Cross Platform Validator Not Implemented: {exception}");
        }
#endif
        }

        void OnServiceInitialized()
        {
            Log.Info("Unity Service initialized.");
        }

        async void ConnectToStore()
        {
            _storeService.OnStoreDisconnected += description =>
            {
                Log.Error($"Store disconnected: {description}");
            };
            await _storeService.Connect();
            Log.Info("Store Connected.");
            FetchInitialProducts();
        }

        void FetchInitialProducts()
        {
            _catalogProvider.FetchProducts(_productService.FetchProductsWithNoRetries, DefaultStoreHelper.GetDefaultStoreName());
        }
        
        public string GetLocalPriceString(string productId)
        {
            string defaultPriceString = "???";
            if (_productService == null)
                return defaultPriceString;

            var product = _productService.GetProductById(productId);
            if (product == null)
                return defaultPriceString;

            return product.metadata.localizedPriceString;
        }
        
        public decimal GetLocalPrice(string productId)
        {
            var defaultPrice = 0;
            if (_productService == null)
                return defaultPrice;

            var product = _productService.GetProductById(productId);
            if (product == null)
                return defaultPrice;

            return product.metadata.localizedPrice;
        }

        public void Purchase(string productId)
        {
            var product = FindProduct(productId);

            if (product != null)
            {
                _purchasingService?.PurchaseProduct(product);
            }
            else
            {
                PurchaseResult result = new PurchaseResult()
                {
                    ProductId = productId,
                    IsSuccessful = false,
                    Message = $"The product service has no product with the ID {productId}"
                };
                StartCoroutine(PurchaseResult(result));
            }
        }

        private IEnumerator PurchaseResult(PurchaseResult result)
        {
            yield return new WaitForSeconds(1);
            GameEntry.GetComponent<EventComponent>().Fire(this, PurchaseResultEventArgs.Create(result));
        }

        public Product FindProduct(string productId)
        {
            return GetFetchedProducts()?.FirstOrDefault(product => product.definition.id == productId);
        }

        public ReadOnlyObservableCollection<Product> GetFetchedProducts()
        {
            return _productService?.GetProducts();
        }

        public void ConfirmOrderIfAutomatic(PendingOrder order)
        {
            if (ShouldConfirmOrderAutomatically(order))
            {
                ConfirmOrder(order);
            }
        }

        bool ShouldConfirmOrderAutomatically(PendingOrder order)
        {
            // var containsItemToNotAutoConfirm = false;
            // var containsItemToAutoConfirm = false;
            //
            // foreach (var cartItem in order.CartOrdered.Items())
            // {
            //     var matchingButton = FindMatchingButtonByProduct(cartItem.Product.definition.id);
            //
            //     if (matchingButton)
            //     {
            //         if (matchingButton.consumePurchase)
            //         {
            //             containsItemToAutoConfirm = true;
            //         }
            //         else
            //         {
            //             containsItemToNotAutoConfirm = true;
            //         }
            //     }
            // }
            //
            // if (containsItemToNotAutoConfirm && containsItemToAutoConfirm)
            // {
            //     m_IAPLogger.LogConsole("===========");
            //     m_IAPLogger.LogConsole("Pending Order contains some products to not confirm. Confirming by default!");
            // }
            //
            // return containsItemToAutoConfirm;
            return true;
        }
        

        void ConfirmOrder(PendingOrder pendingOrder)
        {
            _purchasingService.ConfirmPurchase(pendingOrder);
        }

        public void ConfirmPendingPurchaseForId(string id)
        {
            var product = FindProduct(id);
            var order = product != null ? GetPendingOrder(product) : null;

            if (order != null)
            {
                ConfirmOrder(order);
            }
        }

        PendingOrder GetPendingOrder(Product product)
        {
            var orders = _purchasingService.GetPurchases();

            foreach (var order in orders)
            {
                if (order is PendingOrder pendingOrder &&
                    pendingOrder.CartOrdered.Items().First()?.Product.definition.storeSpecificId == product.definition.storeSpecificId)
                {
                    return pendingOrder;
                }
            }

            return null;
        }

        public void ValidatePurchaseIfPossible(IOrderInfo orderInfo)
        {
            if (CanCrossPlatformValidate())
            {
                ValidatePurchase(orderInfo);
            }
        }

        bool CanCrossPlatformValidate()
        {
            return IsGooglePlay() ||
                   Application.platform == RuntimePlatform.IPhonePlayer ||
                   Application.platform == RuntimePlatform.OSXPlayer ||
                   Application.platform == RuntimePlatform.tvOS;
        }

        void ValidatePurchase(IOrderInfo orderInfo)
        {
            try
            {
                var result = _crossPlatformValidator.Validate(orderInfo.Receipt);

                if (IsGooglePlay())
                {
                    Log.Info("Validated Receipt. Contents:");
                    foreach (IPurchaseReceipt productReceipt in result)
                    {
                        Log.Info($"ValidatePurchase, {productReceipt.productID}, {productReceipt.purchaseDate}");
                    }
                }
                else
                {
                    Log.Info($"Validated Receipt.");
                }
            }
            catch (IAPSecurityException ex)
            {
                Log.Error("Invalid receipt, not unlocking content. " + ex);
            }
        }

        bool IsGooglePlay()
        {
            return Application.platform == RuntimePlatform.Android && DefaultStoreHelper.GetDefaultStoreName() == UnityEngine.Purchasing.GooglePlay.Name;
        }
    }
}
