using Lockstep.Network;
using UnityFramework;

namespace Lockstep.Game
{
    public class MainUIPlayer : MainUIPlayerBase
    {
        public const string Path = "Prefabs/UI/main_ui_player";
        protected override void DoOpen()
        {
            playerBtn.onClick.AddListener(this.OnPlayerBtn);
        }

        protected override void DoClose()
        {
            playerBtn.onClick.RemoveListener(this.OnPlayerBtn);
        }
        private void OnPlayerBtn()
        {
            LLog.Log("OnPlayerBtn");
        }
    }
}
