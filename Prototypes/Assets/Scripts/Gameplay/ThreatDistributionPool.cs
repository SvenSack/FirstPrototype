using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class ThreatDistributionPool : DistributionPool
    {
        public List<ThreatDistributionPieceUI> heldItems = new List<ThreatDistributionPieceUI>();
        public List<ThreatDistributionPool> aPlayerPools = new List<ThreatDistributionPool>();
        public override void Update()
        {
            if (isJobPool && aPlayerPools.Count == 0)
            {
                foreach (var pool in UIManager.Instance.threatPieceDistributionPools)
                {
                    if (pool.isFlex && pool.gameObject.activeSelf)
                    {
                        activePlayerPools.Add(pool);
                    }
                }
            }
            
            if (flaggedForAdjustment)
            { // this is done to avoid unneeded updates to positions while other stuff happens. we only need to change it before a frame
                AdjustPositions();
                flaggedForAdjustment = false;
            }
        }

        public override void ChangeItem(GameObject item, bool isAdded)
        { // this is what actually drops the item into pools
            ThreatDistributionPieceUI tdPUI = item.GetComponent<ThreatDistributionPieceUI>();
            if (isAdded)
            {
                heldItems.Add(tdPUI);
                item.transform.parent = transform;
                tdPUI.currentPool = this;
            }
            else
            {
                heldItems.Remove(tdPUI);
            }
            
            if (isFlex && heldItems.Count > 0)
            {
                if (heldItems.Count > columns * rows)
                {
                    if (heldItems[0].currentHeight * rows + 1 < height)
                    {
                        rows++;
                    }
                    else
                    {
                        columns++;
                    }
                }
                
                if (width < heldItems[0].currentWidth * columns)
                {
                    float betterWidth = width / columns;
                    foreach (var obj in heldItems)
                    {
                        obj.Resize(betterWidth);
                    }
                }
                
            }
            else if(isFlex)
            {
                tdPUI.ResetSize();
                columnSize = originalColumSize;
                columns = originalColumns;
                rowSize = originalRowSize;
            }

            flaggedForAdjustment = true;
        }

        public override void DropPool()
        { // this is used by the UImanager to reset the pools
            foreach (var obj in heldItems)
            {
                Destroy(obj.gameObject);
            }
            heldItems = new List<ThreatDistributionPieceUI>();
        }
        
        public override void AdjustPositions()
        { // this places objects correctly to avoid gaps
            for (int i = 0; i < heldItems.Count; i++)
            {
                int row = Mathf.FloorToInt(i / (float)columns);
                int column = i - columns*row;
                heldItems[i].transform.position = transform.position + new Vector3(firstPostion.x + columnSize*column, firstPostion.y - rowSize*row, -.1f);
            }
        }

        public void PopulatePool()
        {
            Piece[] pieces = FindObjectsOfType<Piece>();
            foreach (var piece in pieces)
            {
                if (piece.pv.IsMine && !piece.isUsed)
                {
                    GameObject inst = null;
                    switch (piece.type)
                    {
                        case GameMaster.PieceType.Assassin:
                            inst = Instantiate(UIManager.Instance.pieceDistributionUIPrefabs[2], transform);
                            break;
                        case GameMaster.PieceType.Thug:
                            inst = Instantiate(UIManager.Instance.pieceDistributionUIPrefabs[1], transform);
                            break;
                        default:
                            continue;
                    }
                    ThreatDistributionPieceUI tdp = inst.GetComponent<ThreatDistributionPieceUI>();
                    tdp.cPool = this;
                    tdp.represents = piece.GetComponent<ThreatPiece>();
                    tdp.isPrivate = piece.isPrivate;
                    heldItems.Add(tdp);
                }
            }
            AdjustPositions();
        }
    }
}
