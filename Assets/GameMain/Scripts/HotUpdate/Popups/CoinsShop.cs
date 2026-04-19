// // ©2015 - 2026 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GUI.Labels;
using BlockPuzzleGameToolkit.Scripts.Services.IAP;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.System;
using GameFramework.Event;
using Quester;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityGameFramework.Scripts.Runtime.Purchase;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class CoinsShop : UGuiForm
    {
        public ItemPurchase[] packs;
        // private CoinsShopSettings shopSettings;

        [SerializeField]
        private ItemPurchase watchAd;

        private Dictionary<string, DRShopProduct> _shopProductDict = new ();
        
        private void OnEnable()
        {
            GameEntry.Event.Subscribe(PurchaseResultEventArgs.EventId, OnPurchaseResult);
        }

        private void OnDisable()
        {
            GameEntry.Event.Unsubscribe(PurchaseResultEventArgs.EventId, OnPurchaseResult);
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            // shopSettings = Addressables.LoadAssetAsync<CoinsShopSettings>("Assets/GameMain/Settings/Game/CoinsShopSettings.asset").WaitForCompletion();
            var shopProductList = GameEntry.DataTable.GetDataTable<DRShopProduct>().ToList();
            
            foreach (var drShopProduct in shopProductList)
            {
                _shopProductDict[drShopProduct.ProductId] = drShopProduct;
            }
            
            InstantiateMissingItems(shopProductList);
            foreach (var itemPurchase in packs)
            {
                if (shopProductList.Count > 0)
                {
                    var productID = itemPurchase.productID;
                    if (_shopProductDict.TryGetValue(productID, out DRShopProduct drShopProduct))
                    {
                        // itemPurchase.settingsShopItem = settingsShopItem;
                        itemPurchase.count.text = drShopProduct.Content.Split("_")[1];
                        
                        var productType = (ProductTypeWrapper.ProductType)Enum.Parse(typeof(ProductTypeWrapper.ProductType), drShopProduct.ProductType);
                        if (productType == ProductTypeWrapper.ProductType.NonConsumable)
                        {
                            if (UserDataManager.Instance.IsPurchasedProductId(productID))
                            {
                                itemPurchase.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            if (!productID.Contains("noads"))
                            {
                                itemPurchase.price.text = GameEntry.Purchase.GetLocalPriceString(productID);
                            }
                        }
                    }
                }
            }

            watchAd.count.text = GameManager.instance.GameSettings.coinsForAd.ToString();
        }

        private void OnPurchaseResult(object sender, GameEventArgs e)
        {
            if (e is PurchaseResultEventArgs eventArgs)
            {
                GameEntry.UI.CloseUIForm(UIFormId.Processing);
                Debug.Log($"OnPurchaseResult: {eventArgs.PurchaseResult.IsSuccessful}");
                if (eventArgs.PurchaseResult.IsSuccessful)
                {
                    PurchaseSuccess(eventArgs.PurchaseResult.ProductId);
                }
                else
                {
                    Debug.LogWarning(eventArgs.PurchaseResult.Message);
                }
            }
        }

        private void InstantiateMissingItems(List<DRShopProduct> shopProductList)
        {
            var existingProductIds = packs.Where(p => p.productID != null).Select(p => p.productID).ToList();
            // for (var i = 0; i < existingProductIds.Count; i++)
            // {
            //     Debug.LogWarning($"{i}, Product ID: {existingProductIds[i]}");
            // }
            foreach (var shopItem in shopProductList)
            {
                var shopProductId = shopItem.ProductId;
                if (shopProductId.Contains("noads"))
                {
                    continue;
                }
                if (!existingProductIds.Contains(shopProductId))
                {
                    Debug.LogWarning($"{shopProductId} doesn't exist.");
                    var prefab = packs.Last();
                    if (prefab == null)
                        continue;

                    var parent = packs.Length > 0 ? prefab.transform.parent : transform;
                    
                    var newItem = Instantiate(prefab, parent);
                    newItem.productID = shopProductId;
                    // newItem.settingsShopItem = shopItem;
                    newItem.count.text = shopItem.Content.Split("_")[1];
                    newItem.tag.gameObject.SetActive(false);

                    var packsList = packs.ToList();
                    packsList.Add(newItem);
                    packs = packsList.ToArray();
                }
            }
        }

        private void PurchaseSuccess(string id)
        {
            var shopItem = packs.First(i => i.productID == id);
            if (shopItem)
            {
                var count = int.Parse(shopItem.count.text);
                LabelAnim.AnimateForResource(shopItem.resource, shopItem.buyButton.transform.position, "+" + count, SoundBase.instance.coins, () =>
                {
                    ResourceManager.instance.GetResource("Coins").Add(count);
                });

                // If the item is non-consumable, mark it as purchased
                if (_shopProductDict[id].ProductType == nameof(ProductTypeWrapper.ProductType.NonConsumable))
                {
                    // PlayerPrefs.SetInt("Purchased_" + id, 1);
                    // PlayerPrefs.Save();
                    UserDataManager.Instance.SetPurchasedProductId(id);

                    // Disable the button for this item
                    var pack = shopItem;
                    if (pack.buyButton != null)
                    {
                        pack.buyButton.interactable = false;
                    }
                }
            }
            else
            {
                Debug.LogError($"not found shop item: {id}");
            }
            
        }

        public void BuyCoins(string id)
        {
            GameEntry.UI.OpenUIForm(UIFormId.Processing);
            GameEntry.Purchase.Purchase(id);
        }

        public void AwawrdCoins()
        {
            var coins = GameManager.instance.GameSettings.coinsForAd;
            var resourceObject = ResourceManager.instance.GetResource("Coins");
            LabelAnim.AnimateForResource(resourceObject, watchAd.buyButton.transform.position, "+" + coins, SoundBase.instance.coins, () =>
            {
                resourceObject.Add(coins);
            });
        }
    }
}