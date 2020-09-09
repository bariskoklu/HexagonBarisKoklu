using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawScore : MonoBehaviour
{
    private Text text;
    public IntType score;
    void Start()
    {
        text = gameObject.GetComponent<Text>();
    }


    void Update()
    {
        text.text = score.value.ToString();
    }
}
