using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class ThreatAssignmentPool : MonoBehaviour
    {
        [SerializeField] private int rows;
        [SerializeField] private Vector2 firstPostion;
        [SerializeField] private float rowSize;
        [SerializeField] private float columnSize;
        [SerializeField] private int columns;
        [SerializeField] private GameObject populationPiecePrefab;
        
        public List<ThreatAssignmentPieceUI> objectsHeld = new List<ThreatAssignmentPieceUI>();
        public bool isFlex;
        public GameMaster.PieceType acceptedPieces;
        
        private float originalRowSize;
        private float originalColumSize;
        private int originalColumns;
        private float width;
        private float height;
        private bool flaggedForAdjustment;
    
        // this class is basically a copy paste of the normal distributionpools, I wanted to make it a child, but it had too little in common to be feasible that way (which sounds
        // like I planned poorly when writing those...
        void Start()
        {
            if (isFlex)
            {
                var rect = GetComponent<RectTransform>().rect;
                width = rect.width;
                height = rect.height;
                originalColumSize = columnSize;
                originalRowSize = rowSize;
                originalColumns = columns;
            }
        }

        private void Update()
        {
            if (flaggedForAdjustment)
            {
                AdjustPositions();
                flaggedForAdjustment = false;
            }
        }

        public void ChangeItem(GameObject item, bool isAdded)
        {
            ThreatAssignmentPieceUI tapUI = item.GetComponent<ThreatAssignmentPieceUI>();
            if (isAdded)
            {
                objectsHeld.Add(tapUI);
                item.transform.parent = transform;
                tapUI.currntPool = this;
            }
            else
            {
                objectsHeld.Remove(tapUI);
            }
            
            if (isFlex && objectsHeld.Count > 0)
            {
                if (objectsHeld.Count > columns * rows)
                {
                    if (objectsHeld[0].currentHeight * rows + 1 < height)
                    {
                        rows++;
                    }
                    else
                    {
                        columns++;
                    }
                }
                
                if (width < objectsHeld[0].currentWidth * columns)
                {
                    float betterWidth = width / columns;
                    foreach (var obj in objectsHeld)
                    {
                        obj.Resize(betterWidth);
                    }
                }
                
            }
            else if(isFlex)
            {
                tapUI.ResetSize();
                columnSize = originalColumSize;
                columns = originalColumns;
                rowSize = originalRowSize;
            }

            flaggedForAdjustment = true;
        }

        public void DropPool()
        {
            foreach (var obj in objectsHeld)
            {
                Destroy(obj.gameObject);
            }
            objectsHeld = new List<ThreatAssignmentPieceUI>();
        }
        
        private void AdjustPositions()
        {
            for (int i = 0; i < objectsHeld.Count; i++)
            {
                int row = Mathf.FloorToInt(i / (float)columns);
                int column = i - columns*row;
                objectsHeld[i].transform.position = transform.position + new Vector3(firstPostion.x + columnSize*column, firstPostion.y - rowSize*row, -.1f);
            }
        }

        public void PopulatePool()
        {
            Piece[] pieces = FindObjectsOfType<Piece>();
            foreach (var piece in pieces)
            {
                if (piece.pv.IsMine)
                {
                    GameObject inst = null;
                    if (piece.type == acceptedPieces && !piece.isUsed)
                    {
                        inst = Instantiate(populationPiecePrefab, transform);
                        ThreatAssignmentPieceUI tapUI = inst.GetComponent<ThreatAssignmentPieceUI>();
                        tapUI.currntPool = this;
                        tapUI.representative = piece;
                        tapUI.isPrivate = piece.isPrivate;
                        objectsHeld.Add(tapUI);
                    }
                }
            }

            if (!flaggedForAdjustment)
            {
                flaggedForAdjustment = true;
            }
        }
    }
}
