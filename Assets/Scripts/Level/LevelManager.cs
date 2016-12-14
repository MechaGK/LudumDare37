using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public bool isEditor;

    public delegate void StateChangedHandler(int stateIndex);
    public static event StateChangedHandler OnStateChanged;

    public delegate void NoStatesLeftHandler();
    public static event NoStatesLeftHandler OnNoStatesLeft;

    Level level;
    private int currentStateIndex;

    TileGenerator tileGenerator;
    private Tile[] tiles;

    private bool isChanging;

    private int countDeath;
    private bool hasGenerated;

    public Tile[] Tiles
    {
        get { return tiles; }
    }

    private void Start ()
    {
        tileGenerator = GetComponent<TileGenerator>();
    }

    public IEnumerator Setup(Level level = null, bool gotoFirstState = false)
    {
        yield return null;

        if (!hasGenerated)
        {
            tiles = tileGenerator.GenerateTiles();
            hasGenerated = true;
        }

    currentStateIndex = 0;

        this.level = level;

        yield return null;

        if (gotoFirstState)
        {
            AdvanceState();
        }
    }
    
    public void AdvanceState()
    {
        if (ChangeState(currentStateIndex, !isEditor))
        {
            if (OnStateChanged != null)
            {
                OnStateChanged(currentStateIndex);
            }

            ++currentStateIndex;
        }
        else if (OnNoStatesLeft != null)
        {
            OnNoStatesLeft();
            Debug.Log("No states left!");
        }
    }

    public void ChangeState(Tile.State[] tileStates, bool asEditor = false)
    {
        for (var i = 0; i < tileStates.Length; ++i)
        {
            var tileState = tileStates[i];
            var tile = tiles[i];

            tile.GotoState(tileState, asEditor);
        }
        SoundManager.single.PlayAdvanceSound();
    }

    public void ChangeState(LevelState state)
    {
        ChangeState(state.tileStates);
    }

    public bool ChangeState(int stateIndex, bool movePlayer)
    {
        if (stateIndex < level.StatesCount)
        {
            ChangeState(level.GetState(stateIndex));
            currentStateIndex = stateIndex;
            return true;
        }

        return false;
    }

    public Vector3 PlayerStartPosition()
    {
        var index = level.GetPlayerStartIndex(currentStateIndex);
        return tiles[index].transform.position;
    }
}
