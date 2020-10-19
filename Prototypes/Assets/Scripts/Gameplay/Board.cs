using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class Board : MonoBehaviour
    {
        public Transform pieceLocation;
        public int coins;
        private TextMeshProUGUI coinCounter;
        private List<GameObject> coinObjects = new List<GameObject>();
        private List<Card> artifactHand = new List<Card>();
        public List<GameObject> pieces = new List<GameObject>();
        public PhotonView pv;
        public Participant jobHolder;

        [SerializeField] private GameObject coinObject = null;
    
        public void AddCoin(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                coinObjects.Add(PhotonNetwork.Instantiate(coinObject.name, pieceLocation.position + new Vector3(.1f*Random.Range(-(float)coins,(float)coins), coins * .2f, .1f*Random.Range(-(float)coins,(float)coins)),
                    Quaternion.identity));
                coins++;
            }
            
        }

        public void RemoveCoins(int amount)
        {
            coins -= amount;
            for (int i = 0; i < amount; i++)
            {
                PhotonNetwork.Destroy(coinObjects[coinObjects.Count-1]);
                coinObjects.RemoveAt(coinObjects.Count-1);
            }
        }
        
        public void AddPiece(GameMaster.PieceType type, bool setUsed)
        {
            GameObject newPiece = GameMaster.Instance.CreatePiece(type);
            newPiece.transform.position = pieceLocation.position +
                                          new Vector3(Random.Range(-.5f, .5f), .5f, Random.Range(-.5f, .5f));
            newPiece.GetComponent<PhotonView>().TransferOwnership(pv.Controller);
            Piece nPPiece = newPiece.GetComponent<Piece>();
            nPPiece.cam = jobHolder.mySlot.perspective;
            nPPiece.isPrivate = false;
            nPPiece.originBoard = this;
            if (setUsed)
            {
                nPPiece.ToggleUse();
            }
            pieces.Add(newPiece);
        }

        public GameObject LookForPiece(GameMaster.PieceType type)
        {
            foreach (var element in pieces)
            {
                Piece piece = element.GetComponent<Piece>();
                if (piece.type == type && !piece.isUsed)
                {
                    return element;
                }
            }
            return null;
        }
        
        public void DrawACard()
        {
            int newCardIndex = GameMaster.Instance.DrawCard(GameMaster.CardType.Artifact);
            GameObject newCard = GameMaster.Instance.ConstructCard(GameMaster.CardType.Artifact, newCardIndex);
            newCard.transform.position = pieceLocation.position + new Vector3(.2f*artifactHand.Count,.3f,.2f*artifactHand.Count);
            newCard.transform.rotation = pieceLocation.rotation;
            Card cardPart = newCard.GetComponent<Card>();
            cardPart.hoverLocation = jobHolder.mySlot.hoverLocation;
            cardPart.isPrivate = false;
            artifactHand.Add(cardPart);
        }
    }
}
