using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPool<T> where T : new()
{
    private readonly Stack<T> m_pool = new Stack<T>();
    private readonly UnityAction<T> m_actionOnGet;
    private readonly UnityAction<T> m_actionOnRelease;
    private object m_lock = new object();

    public int CountAll { get; private set; }
    public int CountActive { get { return CountAll - CountInactive; } }
    public int CountInactive { get { return m_pool.Count; } }

    public ObjectPool(UnityAction<T> actionOnGet = null, UnityAction<T> actionOnRelease = null)
    {
        m_actionOnGet = actionOnGet;
        m_actionOnRelease = actionOnRelease;
    }

    public T Get()
    {
        T element;

        lock (m_lock)
        {
            if (m_pool.Count == 0)
            {
                element = new T();
                CountAll++;
            }
            else
            {
                element = m_pool.Pop();
            }
        }

        if (m_actionOnGet != null)
            m_actionOnGet(element);

        return element;
    }

    public void Release(T element)
    {
        lock (m_lock)
        {
            if (m_pool.Count > 0 && ReferenceEquals(m_pool.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
        }

        if (m_actionOnRelease != null)
            m_actionOnRelease(element);

        lock (m_lock)
        {
            m_pool.Push(element);
        }
    }
}