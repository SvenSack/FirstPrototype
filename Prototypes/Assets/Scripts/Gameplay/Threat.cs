using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Gameplay
{
    public class Threat : MonoBehaviour
    {
        public PhotonView pv;
        public int[] threatValues;
        public GameMaster.Threat threatType;
        public Dictionary<int, int[]> playerContributions = new Dictionary<int, int[]>();
    
        // this class is used to have all the players individual threat cards connect to one root entity
        void Start()
        {
            pv = GetComponent<PhotonView>();
            for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
            {
                playerContributions.Add(i, new []{0,0,0,0,0,0});
            }
        }

        [PunRPC]
        public void Contribute(int playerNumber, int[] contribution)
        {
            for (int i = 0; i < 6; i++)
            {
                playerContributions[playerNumber][i] += contribution[i];
            }
        }

        [PunRPC]
        public void SetThreat(byte threatIndex)
        {
            Decklist.Instance.threatCards.TryGetValue((GameMaster.Threat) threatIndex,
                out ThreatCard thisTCard);
            threatValues = thisTCard.requirements;
            threatType = (GameMaster.Threat) threatIndex;
            StartCoroutine(SetCardThreat());
        }

        IEnumerator SetCardThreat()
        {
            yield return new WaitForSeconds(1);
            foreach (var card in UIManager.Instance.participant.tHand)
            {
                if (card.cardIndex == (int)threatType)
                {
                    card.threat = this;
                }
            }
        }

        public bool Resolve()
        {
            for (int j = 0; j < GameMaster.Instance.seatsClaimed; j++)
            {
                playerContributions.TryGetValue(j, out int[] cont);
                for (int i = 0; i < 6; i++)
                {
                    threatValues[i] -= cont[i];
                }
            }

            foreach (var t in threatValues)
            {
                if (t > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
