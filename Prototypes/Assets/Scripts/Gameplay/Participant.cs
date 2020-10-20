using System.Collections;
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
        public List<Card> aHand = new List<Card>();
        public int coins = 0;
        private int health = 0;
        private TextMeshProUGUI coinCounter = null;
        private List<GameObject> coinObjects = new List<GameObject>();
        private List<GameObject> healthObjects = new List<GameObject>();
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
            if (pv.IsMine && coinCounter.text != coins.ToString())
            {
                coinCounter.text = coins.ToString();
            }

            if (pv.IsMine && GameMaster.Instance.isTesting)
            {
                if (Input.GetKeyDown(KeyCode.KeypadEnter))
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

                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                        GameMaster.Instance.EndTurn(false);
                }
            }
        }

        private void FindSlot(bool claimSlot)
        { // this is what gets us our player seat at the start
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (Equals(PhotonNetwork.PlayerList[i], pv.Controller))
                    playerNumber = i;
            }

            PhotonView slot = GameMaster.Instance.playerSlots[playerNumber];
            if (claimSlot)
            {
                slot.TransferOwnership(pv.Controller);
            }
            mySlot = slot.GetComponent<PlayerSlot>();
            mySlot.player = this;
            mySlot.Board.SetActive(true);
            GameMaster.Instance.seatsClaimed++;
        }

        private void GameSetup()
        {
            mySlot.perspective.enabled = true;
            foreach (var tile in mySlot.publicTiles)
            {
                tile.player = this;
            }
            coinCounter = mySlot.coinCounter;
            CursorFollower.Instance.playerCam = mySlot.perspective;
            UIManager.Instance.participant = this;
            UIManager.Instance.playerCamera = mySlot.perspective;
            UIManager.Instance.player = pv.Controller;
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

        public void DrawACard(GameMaster.CardType type)
        {
            int newCardIndex = GameMaster.Instance.DrawCard(type);
            GameObject newCard = GameMaster.Instance.ConstructCard(type, newCardIndex);
            int handSize = aHand.Count;
            newCard.transform.position = mySlot.aACardLocation.position + new Vector3(.2f*handSize,.3f,.2f*handSize);
            newCard.transform.rotation = mySlot.aACardLocation.rotation;
            Card cardPart = newCard.GetComponent<Card>();
            cardPart.hoverLocation = mySlot.hoverLocation;
            aHand.Add(cardPart);
        }

        private void AddHealth(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                healthObjects.Add(PhotonNetwork.Instantiate(healthObject.name, mySlot.healthLocation.position + new Vector3(0.3f*((health+1)/2*Mathf.Pow(-1, health)), .1f, 0),
                    Quaternion.identity));
                health++;
            }
        }

        private void RemoveHealth(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                PhotonNetwork.Destroy(healthObjects[healthObjects.Count-1]);
                healthObjects.RemoveAt(healthObjects.Count-1);
                health--;
            }

            if (health < 1)
            {
                //TODO: player death
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
        
        IEnumerator PrepAndStart()
        {
            yield return new WaitForSeconds(1f);
            UIManager.Instance.UpdateSelectionNames();
            if (PhotonNetwork.IsMasterClient)
            {
                GameMaster.Instance.EndTurn(true);
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
        public void RpcAddPiece(byte pieceIndex, int amount)
        {
            if (pv.IsMine)
            {
                for (int i = 0; i < amount; i++)
                {
                    AddPiece((GameMaster.PieceType)pieceIndex, false);
                }
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
        public void RpcRemoveHealth(byte amount)
        {
            if (pv.IsMine)
            {
                RemoveHealth(amount);
            }
        }

        [PunRPC]
        public void RpcAssignRoleAndChar(byte roleIndex, int charIndex)
        {
            character = (GameMaster.Character) charIndex;
            GameMaster.Instance.characterIndex.Add(character, this);
            
            role = (GameMaster.Role) roleIndex;
            GameMaster.Instance.playerRoles.Add(role, playerNumber);

            if (pv.IsMine)
            {
                Decklist.Instance.characterCards.TryGetValue(character, out var tempCard);
                GameObject charCard = GameMaster.Instance.ConstructCard(GameMaster.CardType.Character, (int) character);
                var position = mySlot.rCCardLocation.position;
                charCard.transform.position = position + new Vector3(0,.3f,0);
                var rotation = mySlot.rCCardLocation.rotation;
                charCard.transform.rotation = rotation;
                charCard.GetComponent<Card>().hoverLocation = mySlot.hoverLocation;
                AddCoin(tempCard.wealth);
                AddHealth(tempCard.health);
                
                GameObject roleCard = GameMaster.Instance.ConstructCard(GameMaster.CardType.Role, (int) role);
                roleCard.transform.position = position + new Vector3(.5f,.3f,.5f);
                roleCard.transform.rotation = rotation;
                roleCard.GetComponent<Card>().hoverLocation = mySlot.hoverLocation;
                
                if (role == GameMaster.Role.Leader)
                {
                    AddHealth(1);
                }
                
                if (character == GameMaster.Character.Adventurer)
                {
                    DrawACard(GameMaster.CardType.Artifact);
                    DrawACard(GameMaster.CardType.Artifact);
                    DrawACard(GameMaster.CardType.Artifact);
                }

                StartCoroutine(PrepAndStart());
            }
        }

        [PunRPC]
        public void RpcEndTurn(bool isFirst)
        {
            if (pv.IsMine)
            {
                if (!isFirst)
                {
                    UIManager.Instance.StartSelection(UIManager.SelectionType.PostTurnPay, null);

                    switch (character)
                    {
                        case GameMaster.Character.Poisoner:
                            UIManager.Instance.StartSelection(UIManager.SelectionType.Poisoner, null);
                            break;
                        case GameMaster.Character.Scion:
                            AddCoin(2);
                            break;
                        case GameMaster.Character.Seducer:
                            UIManager.Instance.StartSelection(UIManager.SelectionType.Seducer, null);
                            break;
                        case GameMaster.Character.PitFighter:
                            AddPiece(GameMaster.PieceType.Thug, false);
                            break;
                    }
                    
                    // TODO add the revealed gangster here

                    foreach (var tile in FindObjectsOfType<Tile>())
                    {
                        if (tile.isUsed)
                        {
                            tile.ToggleUsed();
                        }
                    }
                
                    // TODO: resolve threat & threat pieces
                }
                
                if (role == GameMaster.Role.Leader)
                {
                    switch (GameMaster.Instance.turnCounter)
                    {
                        case 0:
                        case 2:
                        case 4:
                            UIManager.Instance.StartSelection(UIManager.SelectionType.JobAssignment, null);
                            goto case 3;
                        case 1:
                        case 3:
                            // TODO: draw new threats
                            UIManager.Instance.StartSelection(UIManager.SelectionType.WorkerAssignment, null);
                            break;
                        case 5:
                            // TODO: end game
                            break;
                    }
                }
                GameMaster.Instance.turnCounter++;
            }
        }
    }
}
