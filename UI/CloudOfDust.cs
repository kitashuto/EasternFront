using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudOfDust : MonoBehaviour
{
    public float fadeTime = 0.5f;
    private float time;
    private SpriteRenderer render;

    float delta = 0;
    float span = 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(-0.003f, 0, 0);
        gameObject.transform.localScale += new Vector3(0.003f, 0.003f);


        delta += Time.deltaTime;
        if (delta > span)
        {
            time += Time.deltaTime / 10;
            if (time < fadeTime)
            {
                float alpha = 1.0f - time / fadeTime;
                Color color = render.color;
                color.a = alpha;
                render.color = color;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
