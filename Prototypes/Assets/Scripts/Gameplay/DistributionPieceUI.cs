using System;
using UnityEngine;

namespace Gameplay
{
    public class DistributionPieceUI : MonoBehaviour
    {
        public DistributionPool currentPool;
        public bool isGrabbed;
        private float originalWidth;
        private float originalHeight;
        public float currentWidth;
        public float currentHeight;
        private RectTransform recT;

        public virtual void Start()
        {
            recT = GetComponent<RectTransform>();
            var rect = recT.rect;
            originalWidth = rect.width;
            originalHeight = rect.height;
            
            currentWidth = originalWidth;
            currentHeight = originalHeight;
        }


        public virtual void Grab()
        {
            isGrabbed = true;
            transform.parent = UIManager.Instance.workerDistributionPools[0].transform;
        }

        public void Release(DistributionPool newPool)
        {
            currentPool.ChangeItem(gameObject, false);
            if (newPool == null)
            {
                currentPool.ChangeItem(gameObject, true);
            }
            else
            {
                newPool.ChangeItem(gameObject, true);
            }

            isGrabbed = false;
        }

        public void Resize(float newWidth)
        {
            var recTRect = recT.rect;
            recTRect.width = newWidth;
            float scaleFactor = originalWidth / originalHeight;
            currentHeight = newWidth * scaleFactor;
            recTRect.height = currentHeight;
            currentWidth = newWidth;
        }

        public void ResetSize()
        {
            var recTRect = recT.rect;
            recTRect.width = originalWidth;
            recTRect.height = originalHeight;
            currentWidth = originalWidth;
            currentHeight = originalHeight;
        }

        public virtual void Update()
        {
            if (isGrabbed)
            {
                transform.position = Vector3.Slerp(transform.position, Input.mousePosition, .5f);
            }
        }
    }
}
