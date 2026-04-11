using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityGameFramework.Scripts.Runtime.Purchase;

namespace Quester
{
    public class PurchaseResultUI : UGuiForm
    {
        public Image successImage;
        public Image failedImage;
        public TextMeshProUGUI tipsText;
        
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (userData is PurchaseResultEventArgs eventArgs)
            {
                var key = "";
                if (eventArgs.PurchaseResult.IsSuccessful)
                {
                    key = "#purchase_success";
                    successImage.gameObject.SetActive(true);
                    failedImage.gameObject.SetActive(false);
                    var shopData = GameEntry.DataTable.GetDataTable<DRShopProduct>();
                    foreach (var drShopProduct in shopData)
                    {
                        if (drShopProduct.ProductId == eventArgs.PurchaseResult.ProductId)
                        {
                            Addressables.LoadAssetAsync<Sprite>($"Assets/GameMain/Sprites/general-ui/{drShopProduct.Icon}.png").Completed += handle =>
                            {
                                if (handle.Status == AsyncOperationStatus.Succeeded)
                                {
                                    successImage.sprite = handle.Result;
                                }
                            };
                            break;
                        }
                    }
                }
                else
                {
                    key = "#purchase_fail";
                    failedImage.gameObject.SetActive(true);
                    successImage.gameObject.SetActive(false);
                }
                
                tipsText.text = GameEntry.Localization.GetString(key);
            }
        }
    }
}