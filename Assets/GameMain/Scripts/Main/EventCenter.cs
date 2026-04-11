using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局事件中心 —— 支持带参数 / 无参数事件，类型安全，无需字符串 key
/// 用法见 EventDemo.cs
/// </summary>
public static class EventCenter
{
    public interface IEvent
    {
        
    }
    
    // ──────────────────────────────────────────────
    //  内部存储：事件类型 → 委托列表
    // ──────────────────────────────────────────────
    private static readonly Dictionary<Type, Delegate> _eventTable = new();

    // ══════════════════════════════════════════════
    //  无参数事件
    // ══════════════════════════════════════════════

    public static void Subscribe<T>(Action handler) where T : IEvent
    {
        var key = typeof(T);
        if (_eventTable.TryGetValue(key, out var existing))
            _eventTable[key] = Delegate.Combine(existing, handler);
        else
            _eventTable[key] = handler;
    }

    public static void Unsubscribe<T>(Action handler) where T : IEvent
    {
        var key = typeof(T);
        if (_eventTable.TryGetValue(key, out var existing))
        {
            var updated = Delegate.Remove(existing, handler);
            if (updated == null) _eventTable.Remove(key);
            else _eventTable[key] = updated;
        }
    }

    public static void Publish<T>() where T : IEvent
    {
        if (_eventTable.TryGetValue(typeof(T), out var d))
            (d as Action)?.Invoke();
    }

    // ══════════════════════════════════════════════
    //  带参数事件
    // ══════════════════════════════════════════════

    public static void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var key = typeof(T);
        if (_eventTable.TryGetValue(key, out var existing))
            _eventTable[key] = Delegate.Combine(existing, handler);
        else
            _eventTable[key] = handler;
    }

    public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        var key = typeof(T);
        if (_eventTable.TryGetValue(key, out var existing))
        {
            var updated = Delegate.Remove(existing, handler);
            if (updated == null) _eventTable.Remove(key);
            else _eventTable[key] = updated;
        }
    }

    public static void Publish<T>(T eventData) where T : IEvent
    {
        if (_eventTable.TryGetValue(typeof(T), out var d))
            (d as Action<T>)?.Invoke(eventData);
    }

    // ══════════════════════════════════════════════
    //  工具方法
    // ══════════════════════════════════════════════

    /// <summary>清空所有事件（场景切换时调用）</summary>
    public static void Clear() => _eventTable.Clear();

    /// <summary>清空某个事件的所有监听</summary>
    public static void Clear<T>() where T : IEvent => _eventTable.Remove(typeof(T));

    /// <summary>查询某事件当前监听数量（调试用）</summary>
    public static int ListenerCount<T>() where T : IEvent
    {
        if (_eventTable.TryGetValue(typeof(T), out var d))
            return d.GetInvocationList().Length;
        return 0;
    }
}
