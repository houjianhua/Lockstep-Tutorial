using System;
using System.Collections.Generic;
using Lockstep.Network;
using Lockstep.Util;
using UnityEngine;
using UnityFramework;

namespace Lockstep.Game
{
    public class MainUI : MainUIBase
    {
        public const string Path = "Prefabs/UI/main_ui";
        private List<UIBase> subs = new List<UIBase>();
        protected override void DoOpen()
        {
            goBtn.onClick.AddListener(this.OnGoBtn);
            createBtn.onClick.AddListener(this.CreatePlayer);
            CreatePlayer();
            CreatePlayer();
        }

        protected override void DoClose()
        {
            goBtn.onClick.RemoveListener(this.OnGoBtn);
            createBtn.onClick.RemoveListener(this.CreatePlayer);
            foreach (var sub in subs)
            {
                this.ReleaseUI(sub);
            }
            subs.Clear();
        }
        private void OnGoBtn()
        {
            LLog.Log("Go Btn");
            Launcher.Instance.GetService<IUIService>().CloseWindow(this);
            CreateDelayAction(0.1f, () =>
            {
                LLog.Log("after seconds");

                Launcher.Instance.GetService<IUIService>().OpenWindow(MainUI.Path, EWindowLayer.Normal);
            });
        }

        private void CreatePlayer()
        {
            this.CreateUIAsync(MainUIPlayer.Path, playerRtf, (player) =>
            {
                if (player != null)
                {
                    subs.Add(player);
                    CreateDelayAction(2f, () => this.ReleaseUI(player));
                }
            });


            CreateDelayAction(1f, () =>
            {
                this.CreateUIAsync(MainUIPlayer.Path, playerRtf, (player) =>
                {
                    if (player != null)
                    {
                        subs.Add(player);
                        CreateDelayAction(0.8f, () => this.ReleaseUI(player));
                    }
                });
            });
        }
    }
}
