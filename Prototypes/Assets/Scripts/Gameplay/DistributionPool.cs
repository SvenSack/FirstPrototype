using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class DistributionPool : MonoBehaviour
    {
        [SerializeField] private float rowSize;
        [SerializeField] private float columnSize;
        [SerializeField] private int columns;
        [SerializeField] private int rows;
        [SerializeField] private Vector2 firstPostion;
        [SerializeField] private Button confirmButton;
        [SerializeField] private bool isJobPool;
        
        public List<DistributionPieceUI> objectsHeld = new List<DistributionPieceUI>();
        public bool isFlex;
        public TextMeshProUGUI labelText;
        
        private float width;
        private float height;
        private bool flaggedForAdjustment;
        private List<DistributionPool> activePlayerPools = new List<DistributionPool>();
        private float originalRowSize;
        private float originalColumSize;
        private int originalColumns;
    
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
                        if (GameMaster.Instance.FetchLeader().character == GameMaster.Character.OldFox)
                        {
                            // this is ok I think
                        }
                        else
                        {
                            if (GameMaster.Instance.seatsClaimed != 1)
                            {
                                confirmButton.interactable = false;
                            }
                        }
                    }
                    // TODO add explanation hover in game UI of confirm button to make clear why this happens
                }
            }
            else if(confirmButton != null && confirmButton.interactable)
            {
                confirmButton.interactable = false;
            }
        }

        public void ChangeItem(GameObject item, bool isAdded)
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
                
                // TODO also flex the column and row sizes to match based on factor
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

        public void DropPool()
        { // this is used by the UImanager to reset the pools
            foreach (var obj in objectsHeld)
            {
                Destroy(obj.gameObject);
            }
            objectsHeld = new List<DistributionPieceUI>();
        }
        
        private void AdjustPositions()
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
