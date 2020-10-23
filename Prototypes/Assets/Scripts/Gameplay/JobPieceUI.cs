using UnityEditor;
using UnityEngine;

namespace Gameplay
{
    public class JobPieceUI : DistributionPieceUI
    {
        [SerializeField] private float hoverTimer = 2f;
        [SerializeField] private GameObject explanationHover;
        
        public GameMaster.Job representedJob;
        
        private float hoverTime;
        private bool isHovered;
        
        // this class is essentially a full copy paste of the normal distributionpiece UI, I wanted to make it a better child, but too much was different except the functionality
        
        public override void Start()
        {
            base.Start();
            explanationHover.SetActive(false);
        }

        public void HoverStart()
        {
            isHovered = true;
        }

        public void HoverEnd()
        {
            isHovered = false;
            hoverTime = 0;
            explanationHover.SetActive(false);
        }

        public override void Grab()
        {
            isGrabbed = true;
            transform.parent = UIManager.Instance.jobDistributionPools[0].transform;
        }

        public override void Update()
        {
            base.Update();
            if (isHovered)
            {
                if (hoverTime >= hoverTimer && !explanationHover.activeSelf)
                {
                    Transform oldParent = transform.parent;
                    transform.parent = null;
                    transform.parent = oldParent;
                    explanationHover.SetActive(true);
                }
                else if(!explanationHover.activeSelf)
                {
                    hoverTime += Time.deltaTime;
                }
            }
        }
    }
}
