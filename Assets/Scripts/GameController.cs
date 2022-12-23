using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private FieldController[] _fields;
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private GameObject _panel;
    [SerializeField] private float _startMessageLifetime;

    private void Awake()
    {
        Utilities.InitializePrologEngine();
    }

    void Start()
    {
        foreach (var field in _fields)
        {
            field.onGameEnded += ShowMessage;
        }

        StartCoroutine(ShowTutorialMessageCoroutine());
    }

    private void ShowMessage(FieldController field)
    {
        _panel.SetActive(true);
        _textMeshPro.text = $"{field.EnemyName} has won";
    }

    private IEnumerator ShowTutorialMessageCoroutine()
    {
        _panel.SetActive(true);
        _textMeshPro.text = "C - build a ship\n" +
                            "F - fix the position of a ship\n" +
                            "R - rotate a ship\n" +
                            "D - delete the last built ship\n" +
                            "LMB - pick a tile to shoot";

        var timer = _startMessageLifetime;
        while (timer > 0)
        {
            yield return new WaitForFixedUpdate();
            timer -= Time.fixedDeltaTime;
        }

        _textMeshPro.text = "";
        _panel.SetActive(false);
    }
}
