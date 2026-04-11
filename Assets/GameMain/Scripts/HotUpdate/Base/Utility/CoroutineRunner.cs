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
    }
}