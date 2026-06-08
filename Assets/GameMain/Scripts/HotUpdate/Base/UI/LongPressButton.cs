using UnityEngine;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float holdTime = 5f;
    public UnityEvent onLongPress;

    private Coroutine pressCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        pressCoroutine = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
            pressCoroutine = null;
        }
    }

    private IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(holdTime);
        onLongPress?.Invoke();
    }
}
