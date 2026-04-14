using System;
using UnityEngine;
using System.Collections;

namespace Quester
{
    public class CoroutineRunner : MonoSingleton<CoroutineRunner>
    {
        public Coroutine StartCo(IEnumerator coroutine)
        {
            return StartCoroutine(coroutine);
        }

        public void Delay(float delay, Action action)
        {
            StartCoroutine(DelayCo(delay, action));
        }

        private IEnumerator DelayCo(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}