using System;
using banging_code.runs_system;
using MothDIed;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class CheeseDisplay : MonoBehaviour
{
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        text.text = Game.G<RunSystem>().Data.Cheese + "$";
    }
}
