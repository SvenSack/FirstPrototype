using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class DistributionPool : MonoBehaviour
    {
        [SerializeField] private HoverToolTip hToolTip;
        
        public float rowSize;
        public float columnSize;
        public int columns;
        public int rows;
        public Vector2 firstPostion;
        public Button confirmButton;
        public bool isJobPool;
        public List<DistributionPieceUI> objectsHeld = new List<DistributionPieceUI>();
        public bool isFlex;
        public TextMeshProUGUI labelText;
        public float width;
        public float height;
        public bool flaggedForAdjustment;
        public List<DistributionPool> activePlayerPools = new List<DistributionPool>();
        public float originalRowSize;
        public float originalColumSize;
        public int originalColumns;
    
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

        public virtual void Update()
        {
            if (isJobPool && activePlayerPools.Count == 0)
            {
                foreach (var pool in UIManager.Instance.jobDistributionPools)
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

            if (confirmButton != null && objectsHeld.Count < 1)
            {
                
                if (!confirmButton.interactable)
                {
                    confirmButton.interactable = true;
                    if (hToolTip != null) hToolTip.canHover = false;
                }
                if (isJobPool)
                {
                    int highestValue = 0;
                    foreach (var pool in activePlayerPools)
                    {
                        if (pool.objectsHeld.Count > highestValue)
                        {
                            highestValue = pool.objectsHeld.Count;
                        }
                        else if(highestValue > 1 && pool.objectsHeld.Count == 0)
                        {
                            confirmButton.interactable = false;
                        }
                    }

                    DistributionPool leaderPool = UIManager.Instance.jobDistributionPools[GameMaster.Instance.FetchLeader().playerNumber + 1];
                    if (leaderPool.objectsHeld.Count == highestValue)
                    {
                        if (GameMaster.Instance.seatsClaimed != 1)
                        {
                            confirmButton.interactable = false;
                            if (hToolTip != null) hToolTip.canHover = true;
                        }
                    }
                }
            }
            else if(confirmButton != null && confirmButton.interactable)
            {
                confirmButton.interactable = false;
                if (hToolTip != null) hToolTip.canHover = true;
            }
        }

        public virtual void ChangeItem(GameObject item, bool isAdded)
        { // this is what actually drops the item into pools
            DistributionPieceUI wPUI = item.GetComponent<DistributionPieceUI>();
            if (isAdded)
            {
                objectsHeld.Add(wPUI);
                item.transform.parent = transform;
                wPUI.currentPool = this;
            }
            else
            {
                objectsHeld.Remove(wPUI);
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
                wPUI.ResetSize();
                columnSize = originalColumSize;
                columns = originalColumns;
                rowSize = originalRowSize;
            }

            flaggedForAdjustment = true;
        }

        public virtual void DropPool()
        { // this is used by the UImanager to reset the pools
            foreach (var obj in objectsHeld)
            {
                Destroy(obj.gameObject);
            }
            objectsHeld = new List<DistributionPieceUI>();
        }

        
        
        public virtual void AdjustPositions()
        { // this places objects correctly to avoid gaps
            for (int i = 0; i < objectsHeld.Count; i++)
            {
                int row = Mathf.FloorToInt(i / (float)columns);
                int column = i - columns*row;
                objectsHeld[i].transform.position = transform.position + new Vector3(firstPostion.x + columnSize*column, firstPostion.y - rowSize*row, -.1f);
            }
        }
    }
}
