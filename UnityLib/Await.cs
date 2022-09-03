using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Await
{
    class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return x == y;
        }
        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }
    static Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(100, new FloatComparer());

    static WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();
    public static WaitForEndOfFrame WaitForEndOfFrame() => _endOfFrame;

    static WaitForFixedUpdate _fixedUpdate = new WaitForFixedUpdate();
    public static WaitForFixedUpdate WaitForFixedUpdate() => _fixedUpdate;

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        WaitForSeconds wfs;
        if (!_timeInterval.TryGetValue(seconds, out wfs))
            _timeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
        return wfs;
    }

    public static IEnumerator WaitUntil(Func<bool> predicate)
    {
        while (true)
        {
            if (predicate()) yield break;
            yield return WaitForEndOfFrame();
        }
    }

    public void Dispose() { }
}
