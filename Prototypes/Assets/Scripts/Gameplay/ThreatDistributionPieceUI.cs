using System;
using UnityEngine;

namespace Gameplay
{
    public class ThreatDistributionPieceUI : DistributionPieceUI
    {
        [SerializeField] private GameObject isPrivateMarker;
        
        public ThreatDistributionPool cPool;
        public bool isPrivate;
        public ThreatPiece represents;

        public override void Start()
        {
            base.Start();
            if (isPrivate)
            {
                isPrivateMarker.SetActive(false);
            }
        }

        public override void Grab()
        { // this and the following method are used to make the distribution UI work (the one where you can drag and drop UI elements into pools)
            isGrabbed = true;
            transform.parent = UIManager.Instance.threatPieceDistributionPools[0].transform;
        }

        public override void Release(DistributionPool newPool)
        {
            cPool.ChangeItem(gameObject, false);
            if (newPool == null)
            {
                cPool.ChangeItem(gameObject, true);
            }
            else
            {
                newPool.ChangeItem(gameObject, true);
            }

            isGrabbed = false;
        }
    }
}
