/*
 * @Author: fasthro
 * @Date: 2019-06-22 16:19:54
 * @Description: Assetbundle 资源
 */
using System.Collections;
using UnityEngine;

namespace RPGGame.ResSystem
{
    public class BundleRes : Res, IPoolable, IPoolBehavior, IRunAsync
    {
        private AssetBundleCreateRequest m_request;

        private BundleRes[] m_dependencies;

        private int m_dependWaitCount;

        public static BundleRes Allocate(ResData data)
        {
            var res = SafeObjectPool<BundleRes>.Instance.Allocate();
            res.Init(data);
            return res;
        }

        #region  IPoolable
        public bool isRecycled { get; set; }

        public void OnRecycled()
        {

        }
        #endregion

        #region IPoolBehavior

        public void Recycle2Cache()
        {
            SafeObjectPool<BundleRes>.Instance.Recycle(this);
        }

        #endregion

        public void Init(ResData data)
        {
            m_bundleName = data.bundleName;
            m_state = ResState.Waiting;
            m_type = ResType.Bundle;
            m_asset = null;
            m_assetBundle = null;
        }

        /// <summary>
        /// 查找依赖
        /// </summary>
        private bool FindDependencies()
        {
            string[] ds = AssetBundleDependencies.GetDependencies(m_bundleName);
            int length = ds.Length;
            m_dependencies = new BundleRes[length];
            for (int i = 0; i < length; i++)
            {
                m_dependencies[i] = ResPoolSystem.Get<BundleRes>(ResData.AllocateBundle(ds[i]), true);
            }
            return length > 0;
        }

        /// <summary>
        /// 同步加载
        /// </summary>
        public override bool LoadSync()
        {
            if (m_state == ResState.Ready)
                return true;
                
            m_state = ResState.Loading;

            // 先加载依赖
            if (FindDependencies())
            {
                bool dpass = true;
                for (int i = 0; i < m_dependencies.Length; i++)
                {
                    if (!m_dependencies[i].LoadSync())
                    {
                        dpass = false;
                    }
                }
                if (!dpass)
                {
                    m_state = ResState.Failed;
                    return false;
                }
            }

            // 加载本体
            var url = AssetBundlePath.GetFullPath(m_bundleName);
            m_assetBundle = AssetBundle.LoadFromFile(url);
            if (m_assetBundle == null)
            {
                m_state = ResState.Failed;
                return false;
            }
            m_state = ResState.Ready;
            return true;
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        public override void LoadAsync()
        {
            if (m_state == ResState.Ready)
            {
                Notification(true);
            }
            else if (m_state == ResState.Waiting || m_state == ResState.Failed)
            {
                m_state = ResState.Loading;

                // 先加载依赖
                if (FindDependencies())
                {
                    m_dependWaitCount = m_dependencies.Length;
                    for (int i = 0; i < m_dependWaitCount; i++)
                    {
                        m_dependencies[i].AddNotification(OnReceiveNotification);
                        m_dependencies[i].LoadAsync();
                    }
                }
                else
                {
                    RunAsyncSystem.PushEnumerator(this);
                }
            }
        }

        /// <summary>
        /// 接收通知处理依赖
        /// </summary>
        /// <param name="readly"></param>
        /// <param name="res"></param>
        private void OnReceiveNotification(bool ready, Res res)
        {
            m_dependWaitCount--;
            res.RemoveNotification(OnReceiveNotification);
            if (m_dependWaitCount <= 0)
            {
                // 依赖加载完毕
                RunAsyncSystem.PushEnumerator(this);
            }
        }

        /// <summary>
        /// 执行异步加载
        /// </summary>
        /// <param name="asyncSystem"></param>
        public IEnumerator RunAsync(IRunAsyncSystem asyncSystem)
        {
            var url = AssetBundlePath.GetFullPath(m_bundleName);
            var request = AssetBundle.LoadFromFileAsync(url);

            m_request = request;
            yield return request;
            m_request = null;

            if (!request.isDone)
            {
                m_state = ResState.Failed;
                asyncSystem.OnRunAsyncComplete();
                Notification(false);
                yield break;
            }

            m_assetBundle = request.assetBundle;

            if (m_assetBundle == null)
            {
                m_state = ResState.Failed;
                asyncSystem.OnRunAsyncComplete();
                Notification(false);
                yield break;
            }

            m_state = ResState.Ready;
            asyncSystem.OnRunAsyncComplete();
            Notification(true);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public override void Unload()
        {
            // 依赖
            if (FindDependencies())
            {
                for (int i = 0; i < m_dependencies.Length; i++)
                {
                    m_dependencies[i].Release();
                }
            }

            // 本体
            Release();
        }

        /// <summary>
        /// 引用次数为0处理
        /// </summary>
        protected override void OnZeroRef()
        {
            if (m_assetBundle != null)
            {
                m_assetBundle.Unload(true);
            }
            m_assetBundle = null;
            m_asset = null;

            m_request = null;
            Recycle2Cache();
        }
    }
}