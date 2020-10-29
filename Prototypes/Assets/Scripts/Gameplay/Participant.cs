using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

namespace Gameplay
{ 
    public class Participant : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private GameObject coinObject = null;
        [SerializeField] private GameObject healthObject = null;
        
        public PhotonView pv;
        public PlayerSlot mySlot;
        public int playerNumber = -1;
        public GameMaster.Character character;
        public GameMaster.Role role;
        public List<Card> aHand = new List<Card>();
        public List<Card> tHand = new List<Card>();
        public int coins = 0;
        public bool hasZeal;
        public List<GameObject> pieces = new List<GameObject>();
        public List<InformationPiece> informationHand = new List<InformationPiece>();
        public bool roleRevealed;
        public List<ThreatPiece> piecesThreateningMe = new List<ThreatPiece>();
        public bool isDead { get; private set; }
        public int[] officeCampaign = {0, 0, 0};
        
        private int health = 0;
        private TextMeshProUGUI coinCounter = null;
        private List<GameObject> coinObjects = new List<GameObject>();
        private List<GameObject> healthObjects = new List<GameObject>();
        private bool isWaitingForNobleWin;
        private Coroutine nobleWin;

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

            foreach (var card in aHand)
            {
                if (card == null)
                {
                    aHand.Remove(card);
                }
            }
            
            foreach (var piece in pieces)
            {
                if (piece == null)
                {
                    pieces.Remove(piece);
                }
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

        #region Setup
        // this stuff is used to set up the participant and all game start data
        private void FindSlot(bool claimSlot)
        {
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
            foreach (var tile in mySlot.publicTiles)
            {
                tile.player = this;
            }
            GameMaster.Instance.seatsClaimed++;
        }

        private void GameSetup()
        {
            mySlot.perspective.enabled = true;
            coinCounter = mySlot.coinCounter;
            CursorFollower.Instance.playerCam = mySlot.perspective;
            UIManager.Instance.participant = this;
            UIManager.Instance.playerCamera = mySlot.perspective;
            UIManager.Instance.player = pv.Controller;
        }

        #endregion

        #region Interfacing
        // this stuff is what other classes, mostly the tiles and UImanager, use to interface with the values of the class
        public void AddCoin(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                coinObjects.Add(PhotonNetwork.Instantiate(coinObject.name, mySlot.coinLocation.position + new Vector3(.01f*Random.Range(-(float)coins,coins), coins * .2f, .01f*Random.Range(-(float)coins,(float)coins)),
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
            newCard.transform.position = mySlot.aACardLocation.position + new Vector3(.2f*handSize,.3f,0);
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

        public void RemoveHealth(byte amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (health > 0)
                {
                    PhotonNetwork.Destroy(healthObjects[healthObjects.Count-1]);
                    healthObjects.RemoveAt(healthObjects.Count-1);
                }
                health--;
            }

            if (health < 1)
            {
                GameOver(false);
                Participant[] participants = FindObjectsOfType<Participant>();
                List<Participant> alivePlayers = new List<Participant>();
                foreach (var p in participants)
                {
                    if (!p.isDead)
                    {
                        alivePlayers.Add(p);
                    }
                }

                if (alivePlayers.Count == 1)
                {
                    alivePlayers[0].EndTheGame();
                }
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
            pieces.Add(newPiece);
            if (setUsed)
            {
                nPPiece.ToggleUse();
            }
        }

        public void EndTheGame()
        {
            for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
            {
                GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("EndGame", RpcTarget.AllBuffered);
            }
        }

        private void DenyNobleWin()
        {
            isWaitingForNobleWin = false;
            StopCoroutine(nobleWin);
            GameOver(false);
        }

        IEnumerator WaitForNobleWin()
        {
            isWaitingForNobleWin = true;
            yield return new WaitForSeconds(5f);
            if (isWaitingForNobleWin)
            {
                GameOver(true);
            }
        }

        public void GameOver(bool hasWon)
        {
            isDead = true;
            UIManager.Instance.dead = true;
            UIManager.Instance.ResetAfterSelect();
            if (hasWon)
            {
                
            }
        }

        #endregion
        
        public GameObject LookForPiece(GameMaster.PieceType type, bool careUsed)
        {
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
        
        IEnumerator PrepAndStart()
        {
            yield return new WaitForSeconds(1f);
            UIManager.Instance.UpdateSelectionNames();
            if (PhotonNetwork.IsMasterClient)
            {
                GameMaster.Instance.EndTurn(true);
            }
            if (role == GameMaster.Role.Leader)
            {
                UIManager.Instance.RevealRole(false);
            }

            if (role == GameMaster.Role.Noble)
            {
                string head = "The Noble is suspicious";
                string[] nobleActions =
                {
                    "They slept with the servant girl",
                    "They made a pact with demons for power",
                    "They killed their father to inherit his wealth",
                    "They embezzle most of the cities taxes",
                    "They are a member of the Thieves Guild",
                    "They once drunkenly gambled away their palace"
                };
                for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
                {
                    if (i != playerNumber)
                    {
                        string content = nobleActions[Random.Range(0, 5)];
                        GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("RpcAddEvidence", RpcTarget.Others, content, head, true, playerNumber);
                    }
                }
            }
        }

        #region RPC
        // these are all the RPC functions (what gets used by photon to do remote function calls), many of them are just remote versions of the existing interface functions
        [PunRPC]
        public void RpcAddCoin(byte amount)
        {
            if (pv.IsMine)
            {
                AddCoin(amount);
            }
        }
        
        [PunRPC]
        public void RpcRemoveCoin(byte amount)
        {
            if (pv.IsMine)
            {
                RemoveCoins(amount);
            }
        }

        [PunRPC]
        public void RpcStealPiece(byte pieceType, byte playerIndexWhoSteals)
        {
            if (pv.IsMine)
            {
                GameObject potPiece = LookForPiece((GameMaster.PieceType) pieceType, false);
                if (potPiece != null)
                {
                    PhotonNetwork.Destroy(potPiece);
                    GameMaster.Instance.FetchPlayerByNumber(playerIndexWhoSteals).pv.RPC("RpcAddPiece", RpcTarget.Others, pieceType, 1);
                }
            }
        }
        
        [PunRPC]
        public void RpcStealCard(byte cardType, byte playerIndexWhoSteals)
        {
            if (pv.IsMine)
            {
                // TODO add the ui selection to determine which card to give
            }
        }

        [PunRPC]
        public void AnswerFavourCard(bool isArtifact, byte indexOfPlayerWhoAsks)
        {
            if (pv.IsMine)
            {
                
                foreach (var card in aHand)
                {
                    if (card.cardType == (GameMaster.CardType) 2 + Convert.ToInt32(isArtifact))
                    {
                        UIManager.Instance.AnswerFavourRequest(isArtifact, GameMaster.Instance.FetchPlayerByNumber(indexOfPlayerWhoAsks));
                        break;
                    }   
                }
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
        public void RpcHandCard(byte indexOfCard, byte typeOfCard)
        {
            if (pv.IsMine)
            {
                GameObject newCard = GameMaster.Instance.ConstructCard((GameMaster.CardType)typeOfCard, indexOfCard);
                int handSize = aHand.Count;
                newCard.transform.position = mySlot.aACardLocation.position + new Vector3(.2f*handSize,.3f,0);
                newCard.transform.rotation = mySlot.aACardLocation.rotation;
                Card cardPart = newCard.GetComponent<Card>();
                cardPart.hoverLocation = mySlot.hoverLocation;
                aHand.Add(cardPart);
            }
        }

        [PunRPC]
        public void RpcReturnNobleCheck(byte playerIndexOfNoble)
        {
            if (pv.IsMine)
            {
                foreach (var info in informationHand)
                {
                    if (info.isEvidence && info.evidenceTargetIndex == playerIndexOfNoble)
                    {
                        GameMaster.Instance.FetchPlayerByNumber(playerIndexOfNoble).pv.RPC("NobleWinDenied", RpcTarget.Others);
                        break;
                    }
                }
            }
        }

        [PunRPC]
        public void ReceiveLeaderChallenge(int amountOfCampaign, byte playerIndex)
        {
            if (pv.IsMine)
            {
                officeCampaign[0] = 1;
                officeCampaign[1] = amountOfCampaign;
                officeCampaign[2] = playerIndex;
            }
        }

        [PunRPC]
        public void NobleWinDenied()
        {
            if (pv.IsMine && isWaitingForNobleWin)
            {
                DenyNobleWin();
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
        public void RpcAddHealth(byte amount)
        {
            if (pv.IsMine)
            {
                Decklist.Instance.characterCards.TryGetValue(character, out CharacterCard charCard);
                if (health + amount <= charCard.health)
                {
                    AddHealth(amount);
                }
                else
                {
                    AddHealth(charCard.health - health);
                }
            }
        }
        
        [PunRPC]
        public void DrawTCard(byte cardIndex)
        {
            if (pv.IsMine)
            {
                GameObject newCard = GameMaster.Instance.ConstructCard(GameMaster.CardType.Threat, cardIndex);
                int handSize = tHand.Count;
                newCard.transform.position = mySlot.threatLocation.position + new Vector3(.2f*handSize,.3f,0);
                newCard.transform.rotation = mySlot.threatLocation.rotation;
                Card cardPart = newCard.GetComponent<Card>();
                cardPart.hoverLocation = mySlot.hoverLocation;
                cardPart.cardIndex = cardIndex;
                tHand.Add(cardPart);
            }
        }

        [PunRPC]
        public void RpcRemoveTCard(byte handIndex)
        {
            if (pv.IsMine)
            {
                PhotonNetwork.Destroy(tHand[handIndex].pv);
                tHand.RemoveAt(handIndex);
            }
        }
        
        [PunRPC]
        public void EndGame()
        {
            if (pv.IsMine && health > 0)
            {
                switch (role)
                {
                    case GameMaster.Role.Leader:
                        
                        GameOver(true);
                        break;
                    case GameMaster.Role.Rogue:
                        if (coins >= 20)
                        {
                            GameOver(true);
                        }
                        else
                        {
                            GameOver(false);
                        }
                        break;
                    case GameMaster.Role.Vigilante:
                    case GameMaster.Role.Paladin:
                        for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
                        {
                            Participant p = GameMaster.Instance.FetchPlayerByNumber(i);
                            if (!p.isDead && (p.role == GameMaster.Role.Leader || p.role == GameMaster.Role.Noble ||
                                              p.role == GameMaster.Role.Rogue || p.role == GameMaster.Role.Gangster))
                            {
                                GameOver(false);
                                break;
                            }
                            else
                            {
                                GameOver(true);
                            }
                        }
                        break;
                    case GameMaster.Role.Noble:
                        for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
                        {
                            if (i != playerNumber)
                            {
                                GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("RpcReturnNobleCheck", RpcTarget.Others,(byte) playerNumber);
                            }
                        }

                        nobleWin = StartCoroutine(WaitForNobleWin());
                        break;
                    default:
                        GameOver(false);
                        break;
                }
            }
        }

        [PunRPC]
        public void RpcAssignRoleAndChar(byte roleIndex, int charIndex)
        { // this is a workaround for a latency related issue I had where the deck would not sync up fast enough and two clients could draw the same card
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
        public void BaubleInquiry(byte inquiryIndex, byte inquiringPlayer)
        {
            if (pv.IsMine)
            {
                Participant inquirer = GameMaster.Instance.FetchPlayerByNumber(inquiringPlayer);
                foreach (var card in aHand)
                {
                    if (card.cardType == GameMaster.CardType.Artifact &&
                        card.cardIndex == (int) GameMaster.Artifact.Bauble)
                    {
                        UIManager.Instance.BaubleDecisionSelect((UIManager.TargetingReason) inquiryIndex, inquirer);
                        return;
                    }
                }
                UIManager.Instance.NotBaubledResults((UIManager.TargetingReason) inquiryIndex, inquirer);
            }
        }

        [PunRPC]
        public void LookBehindScreenBy(byte playerIndexOfLooker)
        { // this could have gone somewhere else, but it references so many variables from here that it felt silly to do so
            if (pv.IsMine)
            {
                string playerTurnInfo = GameMaster.Instance.turnCounter + " " + UIManager.Instance.CreateCharPlayerString(this);
                string openingText = "In turn " + playerTurnInfo + " had: \n";
                string headerText = "Behind the screen information: " + playerTurnInfo;
            
                int workerPieces = 0;
                int thugPieces = 0;
                int assassinPieces = 0;
                foreach (var p in pieces)
                {
                    if (p != null)
                    {
                        Piece piece = p.GetComponent<Piece>();
                        switch (piece.type)
                        {
                            case GameMaster.PieceType.Assassin:
                                assassinPieces++;
                                break;
                            case GameMaster.PieceType.Thug:
                                thugPieces++;
                                break;
                            case GameMaster.PieceType.Worker:
                                workerPieces++;
                                break;
                        }
                    }
                }
                string playerPoolInfo =
                    workerPieces + " Workers\n" + thugPieces + " Thugs\n" + assassinPieces + " Assassins\n" + coins + " Coins\n"
                    + aHand.Count + " Cards in their Arsenal\n" + informationHand.Count + " pieces of Information\n" + health + " remaining Health\n";

                string jobInfo = "";
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfClubs).playerNumber == playerNumber)
                {
                    Board moclBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfClubs].GetComponent<Board>();
                    jobInfo += "\n They are Master of Clubs with:\n" + moclBoard.pieces.Count + " Thugs\n" +
                               moclBoard.coins + " Coins\n";
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfCoin).playerNumber == playerNumber)
                {
                    Board mocBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfCoin].GetComponent<Board>();
                    jobInfo += "\n They are Master of Coin with:\n" + mocBoard.coins + " Coins\n";
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber == playerNumber)
                {
                    Board mogBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods].GetComponent<Board>();
                    jobInfo += "\n They are Master of Goods with:\n" + mogBoard.artifactHand.Count +
                               " Artifacts at their disposal\n" + mogBoard.coins + " Coins\n";
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfKnives).playerNumber == playerNumber)
                {
                    Board mokBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfKnives].GetComponent<Board>();
                    jobInfo += "\n They are Master of Knives with:\n" + mokBoard.pieces.Count + " Assassins\n";
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfWhispers).playerNumber == playerNumber)
                {
                    jobInfo += "\n They are Master of Whispers with:\n" + "[REDACTED by the Master of Whispers]\n";
                }
            
                string informationText = openingText + playerPoolInfo + jobInfo;
                GameMaster.Instance.FetchPlayerByNumber(playerIndexOfLooker).pv.RPC("RpcAddEvidence", RpcTarget.Others, informationText, headerText, true, (byte) playerNumber); 
            }
        }

        [PunRPC]
        public void RpcAddEvidence(string content, string header, bool isEvidence, byte targetedPlayer)
        {
            if (pv.IsMine)
            {
                if (targetedPlayer != (byte) playerNumber)
                {
                    string realPlayerString = UIManager.Instance.CreateCharPlayerString(GameMaster.Instance.FetchPlayerByNumber(targetedPlayer));
                    string falsePlayerString = realPlayerString.Remove(realPlayerString.IndexOf("(", StringComparison.Ordinal)+1);
                    falsePlayerString += "You)";
                    content = content.Replace(falsePlayerString, realPlayerString);
                    header = header.Replace(falsePlayerString, realPlayerString);
                }
                informationHand.Add(new InformationPiece(content, header, isEvidence, targetedPlayer));
            }
        }

        [PunRPC]
        public void ReceiveTradeGoods(byte amountOfWorkers, byte amountOfThugs, byte amountOfAssassins,
            byte amountPoisoned, List<byte> artifactsIndices, List<byte> actionsIndices, int coinAmount, byte playerIndex)
        {
            if (pv.IsMine)
            {
                // TODO check to make sure to have the playerindex sent and only then give
                for (int i = 0; i < amountOfWorkers; i++)
                {
                    AddPiece(GameMaster.PieceType.Worker, false);
                }
                for (int i = 0; i < amountOfThugs; i++)
                {
                    AddPiece(GameMaster.PieceType.Thug, false);
                }
                for (int i = 0; i < amountOfAssassins; i++)
                {
                    AddPiece(GameMaster.PieceType.Assassin, false);
                }
                if (amountPoisoned > 0)
                {
                    LookForPiece(GameMaster.PieceType.Assassin, true).GetComponent<Piece>().ActivatePoison();
                }
                foreach (var ind in artifactsIndices)
                {
                    RpcHandCard(ind, 3); 
                }
                foreach (var ind in actionsIndices)
                {
                    RpcHandCard(ind, 2); 
                }
                AddCoin(coinAmount);
            }
        }

        [PunRPC]
        public void RpcMoWTradeSecret(string content, string header, byte targetedPlayer, byte secondaryPlayer)
        {
            if (pv.IsMine)
            {
                
            }
            // TODO create two button UI once the second inquiry is in, then add evidence based on selection to this and the other
        }

        [PunRPC]
        public void PassTurn(byte passingPlayerIndex)
        {
            GameMaster.Instance.passedPlayers[passingPlayerIndex] = true;
        }

        [PunRPC]
        public void RevealRoleOf(byte playerIndexWhoRevealed)
        {
            if (pv.IsMine)
            {
                Participant part = GameMaster.Instance.FetchPlayerByNumber(playerIndexWhoRevealed);
                part.roleRevealed = true;
                GameMaster.Instance.roleRevealTurns[(int) part.role] = GameMaster.Instance.turnCounter;
                if (playerNumber != playerIndexWhoRevealed)
                {
                    Decklist.Instance.roleCards.TryGetValue(part.role, out RoleCard roleCard);
                    string content = UIManager.Instance.CreateCharPlayerString(part) + " is " + roleCard.name;
                    string header = UIManager.Instance.CreateCharPlayerString(part) + " has revealed their role";
                    if (part.role == GameMaster.Role.Noble)
                    {
                        RpcAddEvidence(content, header, GameMaster.Instance.FetchLeader().playerNumber == playerNumber, playerIndexWhoRevealed);
                    }
                    else
                    {
                        RpcAddEvidence(content, header, false, playerIndexWhoRevealed);
                    }
                }
                else
                {
                    if (role == GameMaster.Role.Noble)
                    {
                        AddCoin(10);
                    }
                }
            }
        }


        [PunRPC]
        public void RpcEndTurn(bool isFirst)
        {
            if (pv.IsMine)
            {
                if (!isFirst)
                {
                    UIManager.Instance.StartTurnAgain();
                    Piece[] pieces = FindObjectsOfType<Piece>();
                    foreach (var piece in pieces)
                    {
                        if (piece.pv.IsMine && piece.type != GameMaster.PieceType.Worker)
                        {
                            UIManager.Instance.StartSelection(UIManager.SelectionType.PostTurnPay, null);
                            break;
                        }
                    }

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
                        case GameMaster.Character.OldFox:
                            if (GameMaster.Instance.turnCounter % 2 == 0)
                            {
                                // TODO create ui selection to pick a job, and add a bool to let the leader know this happened
                            }
                            break;
                    }

                    if (role == GameMaster.Role.Gangster && roleRevealed)
                    {
                        AddPiece(GameMaster.PieceType.Thug, false);
                    }
                    
                    if (role == GameMaster.Role.Vigilante && roleRevealed)
                    {
                        AddPiece(GameMaster.PieceType.Assassin, false);
                    }
                    
                    if (role == GameMaster.Role.Noble && roleRevealed)
                    {
                        AddCoin(2);
                    }

                    foreach (var tile in FindObjectsOfType<Tile>())
                    {
                        if (tile.isUsed)
                        {
                            tile.ToggleUsed();
                        }
                    }

                    if (piecesThreateningMe.Count != 0)
                    {
                        UIManager.Instance.StartSelection(UIManager.SelectionType.ThreatenedPlayerResolution, null);
                    }
                }
                
                // TODO add vigilante reveal
                // TODO add a remote pause to the selection events to fix the payment-threat glitch and to make selene work

                if (officeCampaign[0] != 0)
                {
                    UIManager.Instance.RunForOffice();
                }
                
                if (role == GameMaster.Role.Leader)
                {
                    if (!isFirst)
                    {
                        for (var i = 0; i < tHand.Count; i++)
                        {
                            var card = tHand[i];
                            if (!card.threat.Resolve())
                            {
                                for (int e = 0; e < GameMaster.Instance.seatsClaimed; e++)
                                {
                                    GameMaster.Instance.FetchPlayerByNumber(e).pv.RPC("RpcRemoveHealth", RpcTarget.All, (byte)1);
                                }
                            }
                        
                            PhotonNetwork.Destroy(card.threat.pv);
                            for (int j = 0; j < GameMaster.Instance.seatsClaimed; j++)
                            {
                                GameMaster.Instance.FetchPlayerByNumber(j).pv.RPC("RpcRemoveTCard", RpcTarget.All,(byte) i);
                            }
                        
                        } 
                    }

                    switch (GameMaster.Instance.turnCounter)
                    {
                        case 0:
                        case 2:
                        case 4:
                            UIManager.Instance.StartSelection(UIManager.SelectionType.JobAssignment, null);
                            goto case 3;
                        case 1:
                        case 3:
                            int threatAmount = Mathf.CeilToInt(GameMaster.Instance.seatsClaimed / 2f);
                            if (GameMaster.Instance.roleRevealTurns[2] == GameMaster.Instance.turnCounter && GameMaster.Instance.turnCounter > 0)
                            {
                                threatAmount++;
                            }
                            for (int i = 0; i < threatAmount; i++)
                            {
                                byte newThreatIndex = (byte)GameMaster.Instance.DrawCard(GameMaster.CardType.Threat);
                                for (int j = 0; j < GameMaster.Instance.seatsClaimed; j++)
                                {
                                    GameMaster.Instance.FetchPlayerByNumber(j).pv.RPC("DrawTCard", RpcTarget.All, newThreatIndex);
                                }
                                GameObject newThreat = PhotonNetwork.Instantiate(GameMaster.Instance.threatObject.name, Vector3.zero, Quaternion.identity);
                                Threat nT = newThreat.GetComponent<Threat>();
                                nT.GetComponent<PhotonView>().RPC("SetThreat", RpcTarget.All, newThreatIndex);
                            }
                            UIManager.Instance.StartSelection(UIManager.SelectionType.WorkerAssignment, null);
                            break;
                        case 5:
                            EndTheGame();
                            break;
                    }
                }
                GameMaster.Instance.turnCounter++;
            }
        }

        #endregion

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(coins);
                stream.SendNext(isDead);
            }
            else
            {
                coins = (int)stream.ReceiveNext();
                isDead = (bool) stream.ReceiveNext();
            }
        }
    }
}
