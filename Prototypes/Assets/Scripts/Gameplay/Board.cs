using System;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Board : MonoBehaviour
    { // TODO this class is pretty similar to the participant, check if inheritance may be possible one way or another
        [SerializeField] private TextMeshProUGUI coinCounter;
        [SerializeField] private Tile[] tiles = new Tile[4];
        [SerializeField] private GameObject coinObject = null;
        
        public Transform pieceLocation;
        public int coins;
        public List<Card> artifactHand = new List<Card>();
        public List<GameObject> pieces = new List<GameObject>();
        public PhotonView pv;
        public Participant jobHolder;
        
        private List<GameObject> coinObjects = new List<GameObject>();

        public void AddCoin(int amount)
        { // adds coins to this board
            for (int i = 0; i < amount; i++)
            {
                coinObjects.Add(PhotonNetwork.Instantiate(coinObject.name, pieceLocation.position + new Vector3(.01f*Random.Range(-(float)coins,(float)coins), coins * .2f, .01f*Random.Range(-(float)coins,(float)coins)),
                    Quaternion.identity));
                coins++;
            }
            coinCounter.text = coins.ToString();
        }

        [PunRPC]
        public void ChangeJobHolder(byte boardIndex, byte newOwnerNumber)
        { // assigns a new owner to the jobboard
            Participant newHolder = GameMaster.Instance.FetchPlayerByNumber(newOwnerNumber);
            pv.TransferOwnership(newHolder.pv.Controller);
            jobHolder = newHolder;
            foreach (var tile in tiles)
            {
                tile.player = newHolder;
            }
            var transform1 = transform;
            transform1.position = newHolder.mySlot.jobLocations[boardIndex].position;
            transform1.rotation = newHolder.mySlot.jobLocations[boardIndex].rotation;
        }

        public void RemoveCoins(int amount)
        { // removes coins from this board
            coins -= amount;
            for (int i = 0; i < amount; i++)
            {
                PhotonNetwork.Destroy(coinObjects[coinObjects.Count-1]);
                coinObjects.RemoveAt(coinObjects.Count-1);
            }
            coinCounter.text = coins.ToString();
        }
        
        public void AddPiece(GameMaster.PieceType type, bool setUsed)
        { // ads pieces to this board
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

        public GameObject LookForPiece(GameMaster.PieceType type, bool careUsed)
        { // looks for pieces matching criteria on this board
            foreach (var element in pieces)
            {
                Piece piece = element.GetComponent<Piece>();
                if (piece.type == type)
                {
                    if (careUsed && !piece.isUsed)
                    {
                        return element;
                    }
                    else if(!careUsed)
                    {
                        return element;
                    }
                }
            }
            return null;
        }
        
        public void DrawACard()
        { // draws a card to this board
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
