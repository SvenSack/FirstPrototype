using System;
using UnityEngine;

namespace Gameplay
{
    public class DistributionPieceUI : MonoBehaviour
    {
        public DistributionPool currentPool;
        public bool isGrabbed;
        public float currentWidth { get; private set; }
        public float currentHeight { get; private set; }
        
        private RectTransform recT;
        private float originalWidth;
        private float originalHeight;

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
        { // this and the following method are used to make the distribution UI work (the one where you can drag and drop UI elements into pools)
            isGrabbed = true;
            transform.parent = UIManager.Instance.workerDistributionPools[0].transform;
        }

        public virtual void Release(DistributionPool newPool)
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
        { // this and the following are currently deprecated systems to flex size the elements to make them fit into a container instead of spilling as they do atm
          // I could put this on a TO DO, but honestly it doesnt matter for this prototype 
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
