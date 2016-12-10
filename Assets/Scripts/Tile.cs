﻿using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum State
    {
        Wall,
        Platform,
        Pickup,
        Moving
    }

    public enum TransisionState
    {
        None,
        Background,
        Wall,
        Foreground
    }

    public struct TransisionStateData
    {
        public Vector3 position;
        public Color color;
    }



    private State state = State.Wall;
    private State previousState;
    private TransisionState transisionState = TransisionState.None;

    private Vector3 previousPosition;
    private Vector3 targetPosition;

    private Color previousColor;
    private Color targetColor;

    private Vector3 basePosition;

    private bool movingToForeground;

    private float lerpTime;
    private float lerpValue;
    public float timeToMove = 1f;

    public AnimationCurve curve;

    private Dictionary<TransisionState, Vector3> targetPositions = new Dictionary<TransisionState, Vector3>
    {
        {TransisionState.Background, new Vector3(0, 0, 1)},
        {TransisionState.Foreground, new Vector3(0, 0, -1)},
        {TransisionState.Wall, new Vector3(0, 0, 0)}
    };

    private Dictionary<TransisionState, Color> targetColors = new Dictionary<TransisionState, Color>
    {
        {TransisionState.Background, new Color(0.0f, 0.0f, 0.0f)},
        {TransisionState.Foreground, new Color(1f, 1f, 1f)},
        {TransisionState.Wall, new Color(0.7f, 0.7f, 0.7f)}
    };

    private Renderer renderer;

    public BoxCollider2D collider;

    // Use this for initialization
	public void Start ()
	{
	    renderer = GetComponent<Renderer>();
	    renderer.material.SetColor("_Color", targetColors[TransisionState.Wall]);

	    basePosition = transform.position;

	    targetPositions[TransisionState.Background] += basePosition;
	    targetPositions[TransisionState.Foreground] += basePosition;
	    targetPositions[TransisionState.Wall] += basePosition;
	}


    private bool transisionFinished;
	public void Update ()
	{
	    if (transisionState == TransisionState.None) return;

	    lerpTime += 1f / timeToMove * Time.deltaTime;

	    lerpValue = curve.Evaluate(lerpTime);

        if (lerpTime >= 1)
	    {
	        lerpTime = 1;
	        transisionFinished = true;
	    }

	    lerpValue = curve.Evaluate(lerpTime);
	    Debug.Log(lerpValue);

	    transform.position = Vector3.LerpUnclamped(previousPosition, targetPosition, lerpValue);
	    renderer.material.SetColor("_Color", Color.LerpUnclamped(previousColor, targetColor, lerpValue));

	    if (transisionFinished)
	    {
	        lerpTime = 0;

	        Debug.Log(transisionState);

	        if (state == State.Pickup && transisionState == TransisionState.Background)
	        {
	            BeginTransision(TransisionState.Wall);
	        }
	        else
	        {
	            transisionState = TransisionState.None;
	        }

	        transisionFinished = false;
	    }

	    if (state != State.Platform || collider.enabled) return;
	    if (lerpValue >= 0.5f && !collider.enabled)
	    {
	        collider.enabled = true;
	    }
	    else if (lerpValue < 0.5f && collider.enabled)
	    {
	        collider.enabled = false;
	    }
	}

    public void BeginTransision(TransisionState transisionState)
    {
        targetPosition = targetPositions[transisionState];
        previousPosition = transform.position;

        targetColor = targetColors[transisionState];
        previousColor = renderer.material.GetColor("_Color");

        this.transisionState = transisionState;
    }

    private void OnMouseDown()
    {
        Debug.Log("Mouse down");

        if (transisionState != TransisionState.None) return;

        switch (state)
        {
            case State.Wall:
                GotoState(State.Pickup);
                break;
            case State.Platform:
                GotoState(State.Wall);
                break;
            case State.Pickup:
                GotoState(State.Platform);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void GotoState(State newState)
    {
        switch (newState)
        {
            case State.Wall:
                BeginTransision(TransisionState.Wall);
                break;
            case State.Platform:
                BeginTransision(TransisionState.Foreground);
                break;
            case State.Pickup:
                BeginTransision(TransisionState.Background);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        state = newState;
    }
}
