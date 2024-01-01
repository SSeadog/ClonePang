using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelectBorder : MonoBehaviour
{
    [SerializeField] private Image borderImage;
    private float minScale = 0.93f;
    private float maxScale = 1.07f;
    private float animateSpeed = 10f;
    private Coroutine coroutine;


    public void Init()
    {
        borderImage.enabled = true;
        coroutine = StartCoroutine(CoAnimate());
    }

    public void Clear()
    {
        borderImage.enabled = false;
        StopCoroutine(coroutine);
        transform.localScale = Vector3.one;
    }

    private IEnumerator CoAnimate()
    {
        while (true)
        {
            while (transform.localScale.x <= maxScale - 0.005f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * maxScale, Time.deltaTime * animateSpeed);
                yield return null;
            }

            while (transform.localScale.x >= minScale + 0.005f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * minScale, Time.deltaTime * animateSpeed);
                yield return null;
            }
        }
    }
}
