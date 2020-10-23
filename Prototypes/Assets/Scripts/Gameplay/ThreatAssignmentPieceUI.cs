using UnityEngine;

namespace Gameplay
{
    public class ThreatAssignmentPieceUI : DistributionPieceUI
    {
        [SerializeField] private GameObject jobMarker;
        
        public ThreatAssignmentPool currntPool;
        public bool isPrivate;
        public Piece representative;

        public override void Start()
        {
            base.Start();
            if (isPrivate)
            {
                jobMarker.SetActive(false);
            }
        }

        public void Release(ThreatAssignmentPool newPool)
        {
            if (newPool.acceptedPieces == representative.type)
            {
                currntPool.ChangeItem(gameObject, false);
                if (newPool == null)
                {
                    currntPool.ChangeItem(gameObject, true);
                }
                else
                {
                    newPool.ChangeItem(gameObject, true);
                }

                isGrabbed = false;
            }
            else
            {
                currntPool.ChangeItem(gameObject, true);
            }
        }

        public override void Grab()
        {
            isGrabbed = true;
            if (representative.type == GameMaster.PieceType.Assassin)
            {
                transform.parent = UIManager.Instance.threatAssignmentPools[0].transform;
            }
            else
            {
                transform.parent = UIManager.Instance.threatAssignmentPools[3].transform;
            }
        }
    }
}
