using GameMain.Scripts.HotUpdate.Base.DataStore;
using UnityEngine;
using UnityEngine.UI;

namespace Quester
{
    public class CommonButton : Button
    {
        public float interval = 1f;
        private float _lastClickTime;

        public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (Time.time - _lastClickTime < interval)
                return;

            _lastClickTime = Time.time;
            if (DataManager.Instance.PlayerData.soundEnabled)
            {
                GameEntry.Sound.PlaySound(SoundId.ClickUI);
            }
            base.OnPointerClick(eventData);
        }
    }
}