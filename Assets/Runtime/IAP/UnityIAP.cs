#if UNITY_PURCHASING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ProjectX
{
    public class UnityIAP
    {
        public static UnityIAP instance = new UnityIAP();

        public delegate PurchaseProcessingResult ProcessPurchaseHandler(Product product);
        public delegate void PurchaseCompletedHandler(Product product);
        public delegate void PurchaseFailedHandler(Product product, PurchaseFailureReason reason);

        public event PurchaseCompletedHandler onPurchaseCompleted = null;
        public event PurchaseFailedHandler onPurchaseFailed = null;

        private IAPHandler mHandler = new IAPHandler();
        private IStoreController mStore = null;
        private IExtensionProvider mExtensions = null;

        public void Init()
        {
            this.mHandler.onInitSuccess += this.OnInitSuccess;
            this.mHandler.onInitFailed += this.OnInitFailed;
            this.mHandler.onProcessPurchase += this.OnProcessPurchase;
            this.mHandler.onPurchaseFailed += this.OnPurchaseFailed;

            var catalog = ProductCatalog.LoadDefaultCatalog();

            StandardPurchasingModule module = StandardPurchasingModule.Instance();
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
            
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
            IAPHelper.PopulateConfigurationBuilder(ref builder, catalog);
            
            UnityPurchasing.Initialize(this.mHandler, builder);
        }

        public void Quit()
        {
            this.mHandler.onInitSuccess -= this.OnInitSuccess;
            this.mHandler.onInitFailed -= this.OnInitFailed;
            this.mHandler.onProcessPurchase -= this.OnProcessPurchase;
            this.mHandler.onPurchaseFailed -= this.OnPurchaseFailed;
        }

        public void Purchase(string productId)
        {
            if(this.mStore == null)
            {
                this.OnPurchaseFailed(null, PurchaseFailureReason.PurchasingUnavailable);
                return;
            }
            this.mStore.InitiatePurchase(productId);
        }

        public void Restore()
        {
            if (Application.platform == RuntimePlatform.WSAPlayerX86 ||
                Application.platform == RuntimePlatform.WSAPlayerX64 ||
                Application.platform == RuntimePlatform.WSAPlayerARM)
            {
                this.mExtensions.GetExtension<IMicrosoftExtensions>().RestoreTransactions();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.tvOS)
            {
                this.mExtensions.GetExtension<IAppleExtensions>().RestoreTransactions(this.OnTransactionsRestored);
            }
            else if (Application.platform == RuntimePlatform.Android &&
                StandardPurchasingModule.Instance().appStore == AppStore.SamsungApps)
            {
                this.mExtensions.GetExtension<ISamsungAppsExtensions>().RestoreTransactions(this.OnTransactionsRestored);
            }
            else if (Application.platform == RuntimePlatform.Android &&
                StandardPurchasingModule.Instance().appStore == AppStore.CloudMoolah)
            {
                this.mExtensions.GetExtension<IMoolahExtension>().RestoreTransactionID((restoreTransactionIDState) =>
                {
                    this.OnTransactionsRestored(
                        restoreTransactionIDState != RestoreTransactionIDState.RestoreFailed &&
                        restoreTransactionIDState != RestoreTransactionIDState.NotKnown);
                });
            }
            else
            {
                Debug.LogWarning(Application.platform.ToString() + " is not a supported platform for the Codeless IAP restore button");
            }
        }

        public Product GetProductById(string productId)
        {
            if (this.mStore == null || this.mStore.products == null)
                return null;
            return this.mStore.products.WithID(productId);
        }

        public Product GetProductByStoreSpecificId(string storeSpecificId)
        {
            if (this.mStore == null || this.mStore.products == null)
                return null;
            return this.mStore.products.WithStoreSpecificID(storeSpecificId);
        }

        void OnInitSuccess(IStoreController store, IExtensionProvider extensions)
        {
            this.mStore = store;
            this.mExtensions = extensions;
        }

        void OnInitFailed(InitializationFailureReason reason)
        { }

        PurchaseProcessingResult OnProcessPurchase(Product product)
        {
            if (this.onPurchaseCompleted != null)
            {
                this.onPurchaseCompleted(product);
            }

            if (product.definition.type == ProductType.Consumable)
                return PurchaseProcessingResult.Complete;
            else
                return PurchaseProcessingResult.Pending;
        }

        void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            if (this.onPurchaseFailed != null)
            {
                this.onPurchaseFailed(product, reason);
            }
        }

        void OnTransactionsRestored(bool result)
        {
            Debug.Log("Transactions restored: " + result);
        }

        class IAPHandler : IStoreListener
        {
            public event System.Action<IStoreController, IExtensionProvider> onInitSuccess = null;
            public event System.Action<InitializationFailureReason> onInitFailed = null;
            public event ProcessPurchaseHandler onProcessPurchase = null;
            public event PurchaseFailedHandler onPurchaseFailed = null;

            public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
            {
                if(this.onInitSuccess != null)
                {
                    this.onInitSuccess(controller, extensions);
                }
            }

            public void OnInitializeFailed(InitializationFailureReason error)
            {
                Debug.LogErrorFormat("IAP init failed: {0}", error);
                if(this.onInitFailed != null)
                {
                    this.onInitFailed(error);
                }
            }

            public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
            {
                if (this.onProcessPurchase == null)
                    return PurchaseProcessingResult.Pending;
                return this.onProcessPurchase(e.purchasedProduct);
            }

            public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
            {
                if (this.onPurchaseFailed != null)
                {
                    this.onPurchaseFailed(i, p);
                }
            }
        }

        public static class IAPHelper
        {
            /// Populate a ConfigurationBuilder with products from a ProductCatalog
            public static void PopulateConfigurationBuilder(ref ConfigurationBuilder builder, ProductCatalog catalog)
            {
                foreach (var product in catalog.allProducts)
                {
                    IDs ids = null;

                    if (product.allStoreIDs.Count > 0)
                    {
                        ids = new IDs();
                        foreach (var storeID in product.allStoreIDs)
                        {
                            ids.Add(storeID.id, storeID.store);
                        }
                    }

#if UNITY_2017_2_OR_NEWER

                    var payoutDefinitions = new List<PayoutDefinition>();
                    foreach (var payout in product.Payouts)
                    {
                        payoutDefinitions.Add(new PayoutDefinition(payout.typeString, payout.subtype, payout.quantity, payout.data));
                    }
                    builder.AddProduct(product.id, product.type, ids, payoutDefinitions.ToArray());

#else

                builder.AddProduct(product.id, product.type, ids);

#endif
                }
            }
        }
    }
}

#endif
