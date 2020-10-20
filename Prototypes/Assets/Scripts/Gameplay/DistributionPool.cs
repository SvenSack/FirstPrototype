using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class DistributionPool : MonoBehaviour
    {
        public List<DistributionPieceUI> objectsHeld = new List<DistributionPieceUI>();
        public bool isFlex;
        [SerializeField] private float rowSize;
        [SerializeField] private float columnSize;
        private float originalRowSize;
        private float originalColumSize;
        [SerializeField] private int columns;
        private int originalColumns;
        [SerializeField] private int rows;
        [SerializeField] private Vector2 firstPostion;
        [SerializeField] private Button confirmButton;
        public TextMeshProUGUI labelText;
        private float width;
        private float height;
        private bool flaggedForAdjustment;
    
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

            if (confirmButton != null && objectsHeld.Count < 1)
            {
                if (!confirmButton.interactable)
                {
                    confirmButton.interactable = true;
                }
            }
            else if(confirmButton != null && confirmButton.interactable)
            {
                confirmButton.interactable = false;
            }
        }

        public void ChangeItem(GameObject item, bool isAdded)
        {
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
        {
            foreach (var obj in objectsHeld)
            {
                Destroy(obj.gameObject);
            }
            objectsHeld = new List<DistributionPieceUI>();
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
    }
}
