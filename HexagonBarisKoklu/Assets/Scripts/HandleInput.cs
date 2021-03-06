﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleInput : MonoBehaviour
{
    public TileClassListType selectedTiles;

    private Vector3 fp;   //First touch position
    private Vector3 lp;   //Last touch position
    private float dragDistance;  //minimum distance for a swipe to be registered

    private RotateTiles rotateTilesScript;
    private SelectTiles selectTilesScript;
    void Start()
    {
        rotateTilesScript = gameObject.GetComponent<RotateTiles>();
        selectTilesScript = gameObject.GetComponent<SelectTiles>();
        dragDistance = Screen.height * 15 / 100; //dragDistance is 15% height of the screen
    }

    void Update()
    {
        if (GameManager.instance.currentGameState == GameManager.GameStates.PausedState)
        {
            return;
        }
        if (Input.touchCount == 1) // user is touching the screen with a single touch
        {
            Touch touch = Input.GetTouch(0); // get the touch
            if (touch.phase == TouchPhase.Began) //check for the first touch
            {
                fp = touch.position;
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
            {
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
            {
                lp = touch.position;  //last touch position. Ommitted if you use list

                //Check if drag distance is greater than 20% of the screen height
                if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
                {//It's a drag
                 //check if the drag is vertical or horizontal
                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {   //If the horizontal movement is greater than the vertical movement...
                        if ((lp.x > fp.x))  //If the movement was to the right)
                        {   //Right swipe
                            if (selectedTiles.tileList.Count != 0 && !GameManager.instance.isTilesRotating)
                            {
                                rotateTilesScript.isRotatingCounterClockwise = false;
                                rotateTilesScript.HandleTilesRotation(selectedTiles.tileList, rotateTilesScript.isRotatingCounterClockwise, true);
                            }
                            
                        }
                        else
                        {   //Left swipe
                            if (selectedTiles.tileList.Count != 0 && !GameManager.instance.isTilesRotating)
                            {
                                rotateTilesScript.isRotatingCounterClockwise = true;
                                rotateTilesScript.HandleTilesRotation(selectedTiles.tileList, rotateTilesScript.isRotatingCounterClockwise, true);
                            }
                        }
                    }
                    else
                    {   //the vertical movement is greater than the horizontal movement
                        if (lp.y > fp.y)  //If the movement was up
                        {   //Up swipe
                            Debug.Log("Up Swipe");
                        }
                        else
                        {   //Down swipe
                            Debug.Log("Down Swipe");
                        }
                    }
                }
                else
                {   //It's a tap as the drag distance is less than 20% of the screen height
                    if (!GameManager.instance.isTilesRotating)
                    {
                        selectTilesScript.GetThreeClosestTiles();
                    }
                }
            }
        }
    }
}
