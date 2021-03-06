﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    public Text timer;
    public Text deathCounter;

    private string timerTemplate;
    private string deathCounterTemplate;

    private float startTime;
    private float elapsedTime;
    private bool gameStarted;

    private int countDeath;

    private float miliseconds, seconds, minutes, time;

    private CanvasGroup _canvasGroup;

    private bool paused;

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        PlayerScript.OnDeath += OnPlayerDeath;
        PlayerRigidbody.OnDeath += OnPlayerDeath;
        GameManager.OnGameEnd += OnGameEnd;

        GameManager.paused += GameManagerOnPaused;
        GameManager.resume += GameManagerOnResume;
    }

    private void GameManagerOnResume(float timeStart, float timeAdd)
    {
        paused = false;
        startTime = timeStart;
        elapsedTime = timeAdd;
    }

    private void GameManagerOnPaused()
    {
        paused = true;
    }

    private void OnPlayerDeath()
    {
        countDeath++;

        deathCounter.text = string.Format(deathCounterTemplate, countDeath);
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        PlayerScript.OnDeath -= OnPlayerDeath;
        PlayerRigidbody.OnDeath -= OnPlayerDeath;
        GameManager.OnGameEnd -= OnGameEnd;

        GameManager.paused -= GameManagerOnPaused;
        GameManager.resume -= GameManagerOnResume;
    }

    private void Start()
    {
        timerTemplate = timer.text;
        deathCounterTemplate = deathCounter.text;

        _canvasGroup = GetComponent<CanvasGroup>();

        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        deathCounter.text = string.Format(deathCounterTemplate, 0);
    }

    private void Update()
    {
        if (!gameStarted || paused) return;

        time = Time.time - startTime + elapsedTime;

        minutes = Mathf.FloorToInt(time) / 60;
        seconds = Mathf.Floor(time) % 60;
        miliseconds = time * 1000 % 1000;

        timer.text = string.Format(timerTemplate, minutes, seconds, miliseconds);
    }

    private void OnGameStart(float time)
    {
        startTime = time;
        gameStarted = true;

        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
    }

    private void OnGameEnd(float time, int deaths)
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }
}
