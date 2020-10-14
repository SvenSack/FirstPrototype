using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

namespace Gameplay
{ 
    public class Participant : MonoBehaviourPunCallbacks
    {
        public PhotonView pv;
        public PlayerSlot mySlot;
        public int playerNumber = -1;
        public GameMaster.Character character;
        public GameMaster.Role role;
        private List<GameMaster.Action> actionHand = new List<GameMaster.Action>();
        private List<GameMaster.Artifact> artifactHand = new List<GameMaster.Artifact>();
        public int coins = 0;
        private int health = 0;
        private TextMeshProUGUI coinCounter = null;
        private List<GameObject> coinObjects = new List<GameObject>();
        public bool hasZeal;

        [SerializeField] private GameObject coinObject = null;
        [SerializeField] private GameObject healthObject = null;


        private void Start()
        {
            pv = GetComponent<PhotonView>();

            if (pv.IsMine)
            { 
                FindSlot(true);
                GameSetup();
            }
            else
            { 
                FindSlot(false);
            }
        }

        private void Update()
        {
            if (coinCounter.text != coins.ToString())
            {
                coinCounter.text = coins.ToString();
            }

            if (pv.IsMine && Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (GameMaster.Instance.isTesting)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject newPiece = GameMaster.Instance.CreatePiece((GameMaster.PieceType) i);
                        newPiece.transform.position = mySlot.pieceLocation.position +
                                                      new Vector3(Random.Range(-.5f, .5f), .5f, Random.Range(-.5f, .5f));
                        newPiece.GetComponent<PhotonView>().TransferOwnership(pv.Controller);
                        newPiece.GetComponent<Piece>().cam = mySlot.perspective;
                    }
                }
            }
        }

        private void FindSlot(bool claimSlot)
        { // this is what gets us our player seat at the start
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (PhotonNetwork.PlayerList[i].Equals(pv.Controller))
                    playerNumber = i;
            }

            PhotonView slot = GameMaster.Instance.playerSlots[playerNumber];
            if (claimSlot)
            {
                slot.TransferOwnership(pv.Controller);
            }
            mySlot = slot.GetComponent<PlayerSlot>();
            mySlot.player = this;
            UIManager.Instance.playerCamera = mySlot.perspective;
            UIManager.Instance.player = pv.Controller;
        }

        private void GameSetup()
        {
            mySlot.perspective.enabled = true;
            mySlot.Board.SetActive(true);
            foreach (var tile in mySlot.publicTiles)
            {
                tile.player = this;
            }

            pv.RPC("SetUpSeat", RpcTarget.AllBuffered, playerNumber);
            character = (GameMaster.Character)GameMaster.Instance.DrawCard(GameMaster.CardType.Character);
            Decklist.Instance.characterCards.TryGetValue(character, out var tempCard);
            GameObject charCard = GameMaster.Instance.ConstructCard(GameMaster.CardType.Character, (int) character);
            var position = mySlot.rCCardLocation.position;
            charCard.transform.position = position + new Vector3(0,.3f,0);
            var rotation = mySlot.rCCardLocation.rotation;
            charCard.transform.rotation = rotation;
            coinCounter = mySlot.coinCounter;
            AddCoin(tempCard.wealth);
            AddHealth(tempCard.health);
            role = (GameMaster.Role) GameMaster.Instance.DrawCard(GameMaster.CardType.Role);
            GameObject roleCard = GameMaster.Instance.ConstructCard(GameMaster.CardType.Role, (int) role);
            roleCard.transform.position = position + new Vector3(.5f,.3f,.5f);
            roleCard.transform.rotation = rotation;
            if (role == GameMaster.Role.Leader)
            {
                AddHealth(1);
            }
            GameMaster.Instance.pv.RPC("RpcAddRoleIndex", RpcTarget.AllBuffered, playerNumber, (int)role);
            if (character == GameMaster.Character.Adventurer)
            {
                DrawACard(GameMaster.CardType.Artifact);
                DrawACard(GameMaster.CardType.Artifact);
                DrawACard(GameMaster.CardType.Artifact);
            }
        }

        public void AddCoin(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                coinObjects.Add(PhotonNetwork.Instantiate(coinObject.name, mySlot.coinLocation.position + new Vector3(.1f*Random.Range(-(float)coins,coins), coins * .2f, .1f*Random.Range(-(float)coins,(float)coins)),
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

        public void DrawFromMarket()
        {
            // TODO: implement this once selection is in
        }

        public void DrawACard(GameMaster.CardType type)
        {
            int newCardIndex = GameMaster.Instance.DrawCard(type);
            GameObject newCard = GameMaster.Instance.ConstructCard(type, newCardIndex);
            int handSize = actionHand.Count + artifactHand.Count;
            newCard.transform.position = mySlot.aACardLocation.position + new Vector3(.2f*handSize,.3f,.2f*handSize);
            newCard.transform.rotation = mySlot.aACardLocation.rotation;
            if (type == GameMaster.CardType.Action)
            {
                actionHand.Add((GameMaster.Action)newCardIndex);
            }
            else
            {
                artifactHand.Add((GameMaster.Artifact)newCardIndex);
            }
        }

        private void AddHealth(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PhotonNetwork.Instantiate(healthObject.name, mySlot.healthLocation.position + new Vector3(0.3f*((health+1)/2*Mathf.Pow(-1, health)), .1f, 0),
                    Quaternion.identity);
                health++;
            }
        }

        public void AddPiece(GameMaster.PieceType type, bool setUsed)
        {
            GameObject newPiece = GameMaster.Instance.CreatePiece(type);
            newPiece.transform.position = mySlot.pieceLocation.position +
                                          new Vector3(Random.Range(-.5f, .5f), .5f, Random.Range(-.5f, .5f));
            newPiece.GetComponent<PhotonView>().TransferOwnership(pv.Controller);
            Piece nPPiece = newPiece.GetComponent<Piece>();
            nPPiece.cam = mySlot.perspective;
            if (setUsed)
            {
                nPPiece.ToggleUse();
            }
        }

        [PunRPC]
        public void RpcAddCoin(byte amount)
        {
            if (pv.IsMine)
            {
                AddCoin(amount);
            }
        }

        [PunRPC]
        public void RpcAddPiece(byte pieceIndex)
        {
            if (pv.IsMine)
            {
                AddPiece((GameMaster.PieceType)pieceIndex, true);
            }
        }

        [PunRPC]
        public void RpcAddArtifactCard()
        {
            if (pv.IsMine)
            {
                DrawACard(GameMaster.CardType.Artifact);
            }
        }

        [PunRPC]
        public void RpcEndTurn(int thugAmount, int assassinAmount)
        {
            if (pv.IsMine)
            {
                int amount = 0;
                if (hasZeal)
                {
                    amount = assassinAmount;
                    hasZeal = false;
                }
                else
                {
                    amount = assassinAmount * 2;
                }

                if (character == GameMaster.Character.Ruffian)
                {
                    if (thugAmount < 6)
                    {
                        // fully absorbed by her ability
                    }
                    else
                    {
                        thugAmount -= 5;
                    }
                }

                amount += thugAmount;
                // then create UI to pay and after paying set actives and positions. check against gamemaster to make sure
                // there is no sheriff, if there is give them half of the thugamount

                switch (character)
                {
                    case GameMaster.Character.Poisoner:
                        // create UI with selection
                        break;
                    case GameMaster.Character.Scion:
                        AddCoin(2);
                        break;
                    case GameMaster.Character.Seducer:
                        // create UI with selection
                        break;
                    case GameMaster.Character.PitFighter:
                        AddPiece(GameMaster.PieceType.Thug, false);
                        break;
                }

                foreach (var tile in FindObjectsOfType<Tile>())
                {
                    if (tile.isUsed)
                    {
                        tile.ToggleUsed();
                    }
                }
                
                // resolve threat & threat pieces ?
                
                GameMaster.Instance.turnCounter++;
                if (role == GameMaster.Role.Leader)
                {
                    switch (GameMaster.Instance.turnCounter)
                    {
                        case 0:
                        case 2:
                        case 4:
                            // assign roles with UI selection
                        case 1:
                        case 3:
                            // draw new threats
                            // assign workers with UI selection
                            break;
                        case 5:
                            // end game
                            break;
                    }
                }
            }
        }
    }
}
