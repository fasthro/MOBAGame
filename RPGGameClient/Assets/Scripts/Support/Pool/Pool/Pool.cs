/*
 * @Author: fasthro
 * @Date: 2019-06-19 17:39:24
 * @Description: 池基类
 */

using System.Collections.Generic;

namespace RPGGame
{
    public abstract class Pool<T> : IPool<T>, ICountObserveAble
    {
        #region ICountObserverable
        public int count
        {
            get { return m_cacheStack.Count; }
        }
        #endregion

        // 对象工厂
        protected IObjectFactory<T> m_factory;

        // 池数据
        protected readonly Stack<T> m_cacheStack = new Stack<T>();

        // 池默认最大数量
        protected int m_maxCount = 12;

        /// <summary>
        /// 分配对象
        /// </summary>
        public virtual T Allocate()
        {
            return m_cacheStack.Count == 0 ? m_factory.Create() : m_cacheStack.Pop();
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        public abstract bool Recycle(T obj);
    }
}