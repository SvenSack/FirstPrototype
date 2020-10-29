using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class TradeAssignmentChoice : AssignmentChoice
    {
        [SerializeField] private GameObject togglePrefab;
        
        public List<TradeAssignmentToggle> tOn = new List<TradeAssignmentToggle>();
        public List<TradeAssignmentToggle> tOff = new List<TradeAssignmentToggle>();
        public DialUI coinDial;


        public void SwitchAssignment(TradeAssignmentToggle target)
        {
            if (target.isAssigned)
            {
                tOn.Remove(target);
                tOff.Add(target);
            }
            else
            {
                tOff.Remove(target);
                tOn.Add(target);
            }
            AdjustPositions();
        }
        
        public override void CreateToggles()
        {
            Participant player = UIManager.Instance.participant;
            foreach (var piece in player.pieces)
            {
                Piece pp = piece.GetComponent<Piece>();
                if (pp.isPrivate && !pp.isUsed)
                {
                    CreateAndSetDefault().SetPiece(pp);
                }
            }

            foreach (var card in player.aHand)
            {
                if (card != null)
                {
                    CreateAndSetDefault().SetCard(card);
                }
            }

            foreach (var tp in player.piecesThreateningMe)
            {
                if (tp.isThreatening)
                {
                    CreateAndSetDefault().SetThreatPiece(tp);
                }
            }

            foreach (var info in player.informationHand)
            {
                if (info.isEvidence)
                {
                    CreateAndSetDefault().SetInformation(info);
                }
            }
            AdjustPositions();
            coinDial.maxAmount = player.coins;
        }
        
        public override void AdjustPositions()
        { // called whenever one piece moves
            for (int i = 0; i < tOn.Count; i++)
            {
                int row = Mathf.FloorToInt(i / 5f);
                int column = i - 5*row;
                tOn[i].transform.position = onGroup.position + new Vector3(-160 + 80*column, 230 - 70*row, 0);
            }
            for (int i = 0; i < tOff.Count; i++)
            {
                int row1 = Mathf.FloorToInt(i / 5f);
                int column1 = i - 5*row1;
                tOff[i].transform.position = offGroup.position + new Vector3(-160 + 80*column1, 230 - 70*row1, 0);
            }
        }

        public void DropAll()
        {
            foreach (var obj in tOn)
            {
                Destroy(obj);
            }

            foreach (var obj in tOff)
            {
                Destroy(obj);
            }
            
            tOn = new List<TradeAssignmentToggle>();
            tOff = new List<TradeAssignmentToggle>();
            coinDial.amount = 0;
        }

        private TradeAssignmentToggle CreateAndSetDefault()
        {
            GameObject inst = Instantiate(togglePrefab, transform);
            TradeAssignmentToggle toggle = inst.GetComponent<TradeAssignmentToggle>();
            toggle.assigner = this;
            tOff.Add(toggle);
            return toggle;
        }
    }
}
