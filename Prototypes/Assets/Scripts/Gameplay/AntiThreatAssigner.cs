using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Gameplay
{
    public class AntiThreatAssigner : AssignmentChoice
    {
        private int totalThreat;
        
        public override void CreateToggles()
        {
            Piece[] pieces = FindObjectsOfType<Piece>();
            foreach (var piece in pieces)
            {
                if (piece.pv.IsMine)
                {
                    GameObject inst = null;
                    switch (piece.type)
                    {
                        case GameMaster.PieceType.Assassin:
                            inst = Instantiate(togglePrefabs[0], transform);
                            break;
                        case GameMaster.PieceType.Thug:
                            inst = Instantiate(togglePrefabs[1], transform);
                            break;
                    }
                    if (piece.type != GameMaster.PieceType.Worker)
                    {
                        AssignmentToggle toggle = inst.GetComponent<AssignmentToggle>();
                        toggle.assigner = this;
                        toggle.representative = piece;
                        toggle.isPrivate = piece.isPrivate;
                        toggle.isPoisoned = piece.poisoned;
                        ThreatPiece tp = piece.GetComponent<ThreatPiece>();
                        if (UIManager.Instance.participant.character == GameMaster.Character.Adventurer)
                        {
                            tp.damageValue = 0;
                        }
                        if (UIManager.Instance.participant.role == GameMaster.Role.Gangster
                            && GameMaster.Instance.roleRevealTurns[(int) GameMaster.Role.Gangster] == GameMaster.Instance.turnCounter
                            && piece.type == GameMaster.PieceType.Thug)
                        {
                            tp.damageValue = 2;
                        }
                        toggledOff.Add(toggle);
                    }
                }
            }

            foreach (var tp in UIManager.Instance.participant.piecesThreateningMe)
            {
                if (GameMaster.Instance.FetchPlayerByNumber(tp.originPlayerNumber).role == GameMaster.Role.Gangster
                    && GameMaster.Instance.roleRevealTurns[(int) GameMaster.Role.Gangster] == GameMaster.Instance.turnCounter
                    && tp.thisPiece.type == GameMaster.PieceType.Thug)
                {
                    tp.damageValue = 2;
                }
                total += tp.damageValue;
            }
            totalText.text = "Total Damage Taken: " + total;
            totalThreat = total;
            AdjustPositions();
        }

        public override void SwitchAssignment(AssignmentToggle target)
        {
            int multiplier;
            if (target.isAssigned)
            {
                toggledOn.Remove(target);
                toggledOff.Add(target);
                multiplier = 1;
            }
            else
            {
                toggledOff.Remove(target);
                toggledOn.Add(target);
                multiplier = -1;
            }

            total += target.representative.GetComponent<ThreatPiece>().damageValue * multiplier;
            
            totalText.text = "Total Damage Taken: " + total;
            AdjustPositions();
        }

        public int TallyAndClean()
        {
            int payAmount = 0;
            foreach (var obj in toggledOn)
            {
                ThreatPiece tp = obj.representative.GetComponent<ThreatPiece>();
                payAmount += tp.damageValue;
                if (payAmount <= totalThreat)
                {
                    PhotonNetwork.Destroy(obj.representative.pv);
                }
                Destroy(obj.gameObject);
            }
            foreach (var obj in toggledOff)
            {
                Destroy(obj.gameObject);
            }

            int antiPayAmount = 0;
            foreach (var tp in UIManager.Instance.participant.piecesThreateningMe)
            {
                antiPayAmount += tp.damageValue;
                if (antiPayAmount <= payAmount)
                {
                    tp.DestroySelf();
                }
                else
                {
                    tp.ReturnToOwner();
                }
            }
            toggledOn = new List<AssignmentToggle>();
            toggledOff = new List<AssignmentToggle>();
            if (total < 0)
            {
                return 0;
            }
            else
            {
                return total;
            }
        }
    }
}
