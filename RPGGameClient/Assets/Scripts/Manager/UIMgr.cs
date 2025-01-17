﻿/*
 * @Author: fasthro
 * @Date: 2019-01-10 11:11:21
 * @Description: UI 管理器
 */
using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;
using RPGGame.ResSystem;

namespace RPGGame
{
    public class UIMgr : Singleton<UIMgr>, IManager
    {
        // 所有面板UI
        private Dictionary<int, AbstractPanel> m_panels = new Dictionary<int, AbstractPanel>();

        // 所有UI包
        private Dictionary<string, PackageInfo> m_packages = new Dictionary<string, PackageInfo>();

        private UIMgr() { }

        public void Init()
        {

        }

        public void Update()
        {

        }

        public void FixedUpdate()
        {

        }

        public void LateUpdate()
        {

        }

        /// <summary>
		/// 预加载UI，只加载UI所需的包，其他不做任何处理
		/// </summary>
		/// <param name="uiId"></param>
        public void PreloadUI(int uiId)
        {
            AbstractPanel panel = null;
            if (!m_panels.TryGetValue(uiId, out panel))
            {
                panel = CreateUI(uiId);
            }

            panel.Preload();
        }

        /// <summary>
		/// 打开UI
		/// </summary>
		/// <param name="uiId"></param>
        /// <param name="data"></param>
        public AbstractPanel OpenUI(int uiId, IPanelData data)
        {
            AbstractPanel panel = null;
            if (!m_panels.TryGetValue(uiId, out panel))
            {
                panel = CreateUI(uiId);
            }

            panel.OpenPanel(data);
            return panel;
        }

        /// <summary>
		/// 显示UI
		/// </summary>
		/// <param name="uiId"></param>
        public void ShowUI(int uiId)
        {
            AbstractPanel panel = null;
            if (m_panels.TryGetValue(uiId, out panel))
            {
                panel.ShowPanel();
            }
        }

        /// <summary>
		/// 隐藏UI
		/// </summary>
		/// <param name="uiId"></param>
        public void HideUI(int uiId)
        {
            AbstractPanel panel = null;
            if (m_panels.TryGetValue(uiId, out panel))
            {
                panel.HidePanel();
            }
        }

        /// <summary>
		/// 关闭UI
		/// </summary>
		/// <param name="uiId"></param>
        public void CloseUI(int uiId)
        {
            AbstractPanel panel = null;
            if (m_panels.TryGetValue(uiId, out panel))
            {
                panel.ClosePanel();
                m_panels.Remove(uiId);
            }
        }

        // <summary>
        /// 创建UI
        /// </summary>
        /// <param name="uiId"></param>
        private AbstractPanel CreateUI(int uiId)
        {
            return UIID.GetPanel(uiId);
        }

        /// <summary>
		/// 添加包
		/// </summary>
		/// <param name="packageName"></param>
        /// <param name="completeCallback"></param>
        public void AddPackage(string packageName, Action<UIPackage> completeCallback)
        {
            PackageInfo packageInfo = null;
            if (m_packages.TryGetValue(packageName, out packageInfo))
            {
                packageInfo.rc++;
            }
            else
            {
                packageInfo = new PackageInfo();

#if UNITY_EDITOR
                packageInfo.package = UIPackage.AddPackage(string.Format("Assets/Art/UI/{0}/{1}", packageName, packageName));
                completeCallback.InvokeGracefully(packageInfo.package);
#else
                AssetBundleLoader loader = AssetBundleLoader.Allocate("ui", "", (ready, res) => { 
                    if(ready){
                        packageInfo.package = UIPackage.AddPackage(res.assetBundle);
                        completeCallback.InvokeGracefully(packageInfo.package);
                    }
                });
                loader.LoadAsync();
                packageInfo.loader = loader;
#endif
            }
        }

        /// <summary>
		/// 移除包
		/// </summary>
		/// <param name="packageName"></param>
        public void RemovePackage(string packageName)
        {
            PackageInfo packageInfo = null;
            if (m_packages.TryGetValue(packageName, out packageInfo))
            {
                packageInfo.rc--;
                if (packageInfo.rc <= 0)
                {
                    packageInfo.Release();
                    m_packages.Remove(packageName);
                }
            }
        }

        /// <summary>
		/// 移除所有包
		/// </summary>
		/// <param name="packageName"></param>
        public void RemoveAllPackage()
        {
            UIPackage.RemoveAllPackages();
            m_packages.Clear();
        }
    }
}

