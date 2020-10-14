using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay;
using Photon.Realtime;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public bool isGrabbingPiece;
    private LayerMask piecesMask;
    public Camera playerCamera;
    public Player player;
    
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        piecesMask = LayerMask.GetMask("Pieces");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isGrabbingPiece)
            {
                if (LookForPieceGrab())
                {
                }
                else
                {
                
                }
                
            }
        }
    }

    private bool LookForPieceGrab()
    {
        if (Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit pieceHit, 100f, piecesMask))
        {
            return pieceHit.transform.gameObject.GetComponent<Piece>().TryPickup(player);
        }
        return false;
    }

    
}
