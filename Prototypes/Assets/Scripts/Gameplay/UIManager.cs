﻿using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameplay
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        public bool isGrabbingPiece;
        private LayerMask piecesMask;
        public Camera playerCamera;
        public Player player;
        public Participant participant;
        public bool isSelecting;
        public Tile selectingTile;
        public bool isSelectingACard;
        public bool isSelectingAPlayer;
        public bool isSelectingDistribution;
        public bool isSelectingThreatAssignment;
        private bool isGrabbingUI;
        public SelectionType typeOfSelection;
        private TargetingReason typeOfTargeting;
        [SerializeField] GameObject[] SelectionPopUps = new GameObject[2];
        [SerializeField] private Transform playedCardsLocation;
        [SerializeField] private AssignmentChoice postTurnPayAssigner;
        [SerializeField] private GameObject[] playerSelectorsChar = new GameObject[6];
        [SerializeField] private GameObject playerSelectorChar;
        [SerializeField] private GameObject workerUIPrefab;
        [SerializeField] private GameObject[] jobSelectionUIPrefabs = new GameObject[5];
        [SerializeField] private TextMeshProUGUI cardTargetingText;
        [SerializeField] private TextMeshProUGUI baubleDecisionText;
        [SerializeField] private AssignmentChoice bowDecider;
        public ThreatAssignmentPool[] threatAssignmentPools = new ThreatAssignmentPool[6];
        [SerializeField] private DialUI[] threatResourceDials = new DialUI[2];
        private List<SelectionType> selectionBuffer = new List<SelectionType>();
        public DistributionPool[] workerDistributionPools = new DistributionPool[7];
        public DistributionPool[] jobDistributionPools = new DistributionPool[7];
        private GraphicRaycaster gRayCaster;
        private EventSystem eventSys;
        private DistributionPieceUI grabbedUI;
        private Participant inquirer;
        private Threat targetedThreat;
        private int threatResolutionCardTargets;
        private int[] threatContributedValues = new int[6];

        public enum SelectionType
        {
            BlackMarket,
            ThievesGuild,
            SellArtifacts,
            PostTurnPay,
            Poisoner,
            Seducer,
            WorkerAssignment,
            JobAssignment,
            CardPlayerTargeting,
            BaubleDecision,
            SerumPopUp,
            BowTargetAssignment,
            ThreatCardAssignment,
            ThreatCardACardAssignment,
            
        }

        public enum TargetingReason
        {
            Ball,
            Bow,
            Periapt,
            Scepter,
            Potion,
            Serum,
            Poison,
            Seduction
        }
    
        void Start()
        {
            Instance = this;
            piecesMask = LayerMask.GetMask("Pieces");
            foreach (var popUp in SelectionPopUps)
            {
                popUp.SetActive(false);
            }

            foreach (var selector in playerSelectorsChar)
            {
               selector.SetActive(false); 
            }
            playerSelectorChar.SetActive(false);
            gRayCaster = GetComponent<GraphicRaycaster>();
            foreach (var wdp in workerDistributionPools)
            {
                if (wdp.isFlex)
                {
                    wdp.gameObject.SetActive(false);
                }
            }
            foreach (var jdp in jobDistributionPools)
            {
                if (jdp.isFlex)
                {
                    jdp.gameObject.SetActive(false);
                }
            }
        }

        void Update()
        {
            if (!isSelecting && selectionBuffer.Count != 0)
            {
                StartSelection(selectionBuffer[0], null);
                selectionBuffer.RemoveAt(0);
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (!isGrabbingPiece && !isSelecting)
                {
                    LookForPieceGrab();
                }

                if (isSelectingACard && CursorFollower.Instance.isHoveringACard)
                {
                    switch (typeOfSelection)
                    {
                        case SelectionType.ThievesGuild:
                            SelectACardTG(CursorFollower.Instance.hoveredCard);
                            break;
                        case SelectionType.SellArtifacts:
                            if (!CursorFollower.Instance.hoveredCard.isPrivate && CursorFollower.Instance.hoveredCard.cardType == GameMaster.CardType.Artifact)
                            {
                                SelectACardSA(CursorFollower.Instance.hoveredCard);
                            }
                            break;
                        case SelectionType.ThreatCardACardAssignment:
                            if (CursorFollower.Instance.hoveredCard.cardType == GameMaster.CardType.Artifact)
                            {
                                SelectACardTCS(CursorFollower.Instance.hoveredCard);
                            }

                            break;
                    }
                }
                
                if (!isGrabbingPiece && !isSelecting && CursorFollower.Instance.isHoveringTCard)
                {
                    StartSelection(SelectionType.ThreatCardAssignment, null);
                }

                if (isSelectingDistribution && !isGrabbingUI)
                {
                    PointerEventData eventData = new PointerEventData(eventSys) {position = Input.mousePosition};
                    List<RaycastResult> results = new List<RaycastResult>();
                    gRayCaster.Raycast(eventData, results);
                    foreach (RaycastResult result in results)
                    {
                        if (result.gameObject.CompareTag("DistributionItem"))
                        {
                            grabbedUI = result.gameObject.GetComponent<DistributionPieceUI>();
                            isGrabbingUI = true;
                            grabbedUI.Grab();
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (isSelectingDistribution && isGrabbingUI)
                {
                    PointerEventData eventData = new PointerEventData(eventSys) {position = Input.mousePosition};
                    List<RaycastResult> results = new List<RaycastResult>();
                    gRayCaster.Raycast(eventData, results);
                    foreach (RaycastResult result in results)
                    {
                        if (result.gameObject.CompareTag("DistributionPanel"))
                        {
                            isGrabbingUI = false;
                            grabbedUI.Release(result.gameObject.GetComponent<DistributionPool>());
                            grabbedUI = null;
                        }
                    }

                    if (isGrabbingUI)
                    {
                        grabbedUI.Release(null);
                    }
                }
            }
        }

        private void LookForPieceGrab()
        {
            if (Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit pieceHit, 100f, piecesMask))
            {
                pieceHit.transform.gameObject.GetComponent<Piece>().TryPickup(player);
            }
        }

        private void ResetAfterSelect()
        {
            SelectionPopUps[(int)typeOfSelection].SetActive(false);
            isSelecting = false;
            if (isSelectingACard)
            {
                isSelectingACard = false;
                CursorFollower.Instance.hoveredCard = null;
                CursorFollower.Instance.isHoveringACard = false;
            }

            if (isSelectingAPlayer)
            {
                isSelectingAPlayer = false;
                playerSelectorChar.SetActive(false);
            }

            if (isSelectingDistribution)
            {
                isSelectingDistribution = false;
                if (isGrabbingUI)
                {
                    grabbedUI.Release(null);
                }
                isGrabbingUI = false;
                grabbedUI = null;
            }

            if (isSelectingThreatAssignment)
            {
                foreach (var pool in threatAssignmentPools)
                {
                  pool.DropPool();
                  pool.gameObject.SetActive(true);  
                }

                foreach (var dial in threatResourceDials)
                {
                    if (dial.gameObject.activeSelf)
                    {
                        dial.Reset();
                    }
                    dial.gameObject.SetActive(true);
                }
            }
        }

        public void StartSelection(SelectionType type, Tile thisTile)
        {
            if (!isSelecting)
            {
                selectingTile = thisTile;
                isSelecting = true;
                SelectionPopUps[(int)type].SetActive(true);
                typeOfSelection = type;
                switch (type)
                { 
                    case SelectionType.BaubleDecision:
                    case SelectionType.SerumPopUp:
                    case SelectionType.BlackMarket:
                        break;
                    // Above are popup with only buttons
                    case SelectionType.ThievesGuild:
                    case SelectionType.SellArtifacts:
                    case SelectionType.ThreatCardACardAssignment:
                        isSelectingACard = true;
                        break;
                    // Above are card selection popups
                    case SelectionType.PostTurnPay:
                        postTurnPayAssigner.CreateToggles();
                        break;
                    case SelectionType.BowTargetAssignment:
                        bowDecider.CreateToggles();
                        break;
                    // Above are two list assignment popups
                    case SelectionType.Poisoner:
                    case SelectionType.Seducer:
                    case SelectionType.CardPlayerTargeting:
                        isSelectingAPlayer = true;
                        playerSelectorChar.SetActive(true);
                        break;
                    // Above are player selection popups with only one option allowed
                    case SelectionType.WorkerAssignment:
                        isSelectingDistribution = true;
                        int workerAmount = GameMaster.Instance.turnCounter + GameMaster.Instance.seatsClaimed * 3;
                        // TODO: add revealed mechanic paladin here
                        for (int i = 0; i < workerAmount; i++)
                        {
                            workerDistributionPools[0].ChangeItem(Instantiate(workerUIPrefab, transform), true);
                        }
                        break;
                    case SelectionType.JobAssignment:
                        isSelectingDistribution = true;
                        foreach (var t in jobSelectionUIPrefabs)
                        {
                            jobDistributionPools[0].ChangeItem(Instantiate(t, transform), true);
                        }
                        break;
                    // Above are player pool assignment popups
                    case SelectionType.ThreatCardAssignment:
                        CreateThreatAssignmentUI();
                        isSelectingDistribution = true;
                        isSelectingThreatAssignment = true;
                        targetedThreat = CursorFollower.Instance.hoveredCard.threat;
                        break;
                    // Above is a unique selection type with multiple functionalities
                }
            }
            else
            {
                selectionBuffer.Add(type);
            }
        }

        private void CreateThreatAssignmentUI()
        {
            int[] values = CursorFollower.Instance.hoveredCard.threat.threatValues;
            for (int j = 0; j < 2; j++)
            {
                if (values[j*3] != 0 || values[j*3+1] != 0)
                {
                    threatAssignmentPools[j+3].PopulatePool();
                    for (int i = 0; i < 2; i++)
                    {
                        if (values[i+j*3] == 0)
                        {
                            threatAssignmentPools[i+1+j*3].gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 3+j*3; i++)
                    {
                        threatAssignmentPools[i].gameObject.SetActive(false);
                    }
                }
            }
            
            if (values[4] != 0)
            {
                int totalCoins = participant.coins;
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfCoin).playerNumber ==
                    participant.playerNumber)
                {
                    totalCoins += GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfCoin].GetComponent<Board>().coins;
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber ==
                    participant.playerNumber)
                {
                    totalCoins += GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods].GetComponent<Board>().coins;
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfClubs).playerNumber ==
                    participant.playerNumber)
                {
                    totalCoins += GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfClubs].GetComponent<Board>().coins;
                }
                threatResourceDials[0].maxAmount = totalCoins;
            }
            else
            {
                threatResourceDials[0].gameObject.SetActive(false);
            }
            if (values[5] != 0)
            {
                int totalCards = 0;
                foreach (var card in participant.aHand)
                {
                    if (card.cardType == GameMaster.CardType.Artifact)
                    {
                        totalCards++;
                    }
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber ==
                    participant.playerNumber)
                {
                    totalCards += GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods].GetComponent<Board>().artifactHand.Count;
                }
                threatResourceDials[1].maxAmount = totalCards;
            }
            else
            {
                threatResourceDials[1].gameObject.SetActive(false);
            }
        }

        public void ConfirmThreatSelection()
        {
            threatContributedValues = new int[6];
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    threatContributedValues[i+j] = threatAssignmentPools[i + 1 + j * 3].objectsHeld.Count;
                    switch (i)
                    {
                        case 0:
                            foreach (var piece in threatAssignmentPools[i + 1 + j * 3].objectsHeld)
                            {
                               piece.representative.ToggleUse(); 
                            }

                            break;
                        case 1:
                            foreach (var piece in threatAssignmentPools[i + 1 + j * 3].objectsHeld)
                            {
                                PhotonNetwork.Destroy(piece.representative.pv);
                            }

                            break;
                    }
                }
            }


            if (threatResourceDials[0].amount != 0)
            {
                int owed = threatResourceDials[0].amount;
                threatContributedValues[4] = owed;
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfCoin).playerNumber ==
                    participant.playerNumber)
                {
                   Board mocB = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfCoin].GetComponent<Board>();
                   if (mocB.coins >= owed)
                   {
                       mocB.RemoveCoins(owed);
                       owed = 0;
                   }
                   else
                   {
                       owed -= mocB.coins;
                       mocB.RemoveCoins(mocB.coins);
                   }
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber ==
                    participant.playerNumber && owed != 0)
                {
                    Board mogB = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods].GetComponent<Board>();
                    if (mogB.coins >= owed)
                    {
                        mogB.RemoveCoins(owed);
                        owed = 0;
                    }
                    else
                    {
                        owed -= mogB.coins;
                        mogB.RemoveCoins(mogB.coins);
                    }
                }
                if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfClubs).playerNumber ==
                    participant.playerNumber && owed != 0)
                {
                    Board moclB = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfClubs].GetComponent<Board>();
                    if (moclB.coins >= owed)
                    {
                        moclB.RemoveCoins(owed);
                        owed = 0;
                    }
                    else
                    {
                        owed -= moclB.coins;
                        moclB.RemoveCoins(moclB.coins);
                    }
                }

                if (owed != 0)
                {
                    participant.RemoveCoins(owed);
                }
            }

            if (threatResourceDials[1].amount != 0)
            {
                threatResolutionCardTargets = threatResourceDials[1].amount;
                ResetAfterSelect();
                for (int i = 0; i < threatResolutionCardTargets; i++)
                {
                    StartSelection(SelectionType.ThreatCardACardAssignment, null);
                }
            }
            else
            {
                ResetAfterSelect();
                targetedThreat.pv.RPC("Contribute", RpcTarget.All, participant.playerNumber, threatContributedValues);
                
            }
        }
        
        #region SelectionAndConfirmation

        private void CardSelectTarget(TargetingReason reason)
        {
            typeOfTargeting = reason;
            string newText = "";
            switch (reason)
            {
                case TargetingReason.Ball:
                    newText = "Select who to spy on";
                    break;
                case TargetingReason.Bow:
                    newText = "Select whose henchmen you want to hunt";
                    break;
                case TargetingReason.Periapt:
                    newText = "Select whose mind to read";
                    break;
                case TargetingReason.Potion:
                    newText = "Select who to heal";
                    break;
                case TargetingReason.Scepter:
                    newText = "Select who to strike with lightning";
                    break;
                case TargetingReason.Serum:
                    newText = "Select who to interrogate";
                    break;
            }
            cardTargetingText.text = newText;
            StartSelection(SelectionType.CardPlayerTargeting, null);
        }
        
        public void BaubleDecisionSelect(TargetingReason reason, Participant _inquirer)
        {
            typeOfTargeting = reason;
            string newText = CreateCharPlayerString(_inquirer);
            switch (reason)
            {
                case TargetingReason.Ball:
                    newText += " wants to spy on you,";
                    break;
                case TargetingReason.Bow:
                    newText += " wants to hunt your men,";
                    break;
                case TargetingReason.Periapt:
                    newText += " wants to read your mind and find out what your role is,";
                    break;
                case TargetingReason.Scepter:
                    newText += " wants to strike you with lightning,";
                    break;
                case TargetingReason.Serum:
                    newText += " wants to interrogate you,";
                    break;
            }
            baubleDecisionText.text = newText +  " do you want to use your Bauble of Shielding to block it ?";
            inquirer = _inquirer;
            StartSelection(SelectionType.BaubleDecision, null);
        }

        public void UpdateSelectionNames()
        {
            for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
            {
                Participant part = GameMaster.Instance.FetchPlayerByNumber(i);
                playerSelectorsChar[i].SetActive(true);
                string nameText = CreateCharPlayerString(part);
                playerSelectorsChar[i].GetComponentInChildren<TextMeshProUGUI>().text = nameText;
                workerDistributionPools[i + 1].gameObject.SetActive(true);
                workerDistributionPools[i + 1].labelText.text = nameText;
                jobDistributionPools[i + 1].gameObject.SetActive(true);
                jobDistributionPools[i + 1].labelText.text = nameText;
            }
        }

        private string CreateCharPlayerString(Participant player)
        {
            string playerName = "";
            if (player.pv.IsMine)
            {
                playerName = "You";
            }
            else
            {
                playerName = player.pv.Controller.NickName;
            }
            Decklist.Instance.characterNames.TryGetValue(player.character, out string charName);
            return charName + "(" + playerName + ")";
        }

        public void SelectACardBM(bool isAction)
        {
            if (isAction)
            {
                selectingTile.GiveCoinToOwner(1, GameMaster.Job.MasterOfWhispers);
                selectingTile.player.DrawACard(GameMaster.CardType.Action);
            }
            else
            {
                selectingTile.GiveCoinToOwner(1, GameMaster.Job.MasterOfGoods);
                selectingTile.player.DrawACard(GameMaster.CardType.Artifact);
            }

            ResetAfterSelect();
        }

        private void SelectACardTG(Card hoveredCard)
        {
            // TODO implement effects here
            switch (hoveredCard.cardType)
            {
                case GameMaster.CardType.Artifact:
                    switch ((GameMaster.Artifact)hoveredCard.cardIndex)
                    {
                        case GameMaster.Artifact.Ball:
                            CardSelectTarget(TargetingReason.Ball);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Artifact.Bauble:
                            break;
                        case GameMaster.Artifact.Bow:
                            CardSelectTarget(TargetingReason.Bow);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Artifact.Dagger:
                            participant.AddPiece(GameMaster.PieceType.Assassin, false);
                            participant.AddPiece(GameMaster.PieceType.Assassin, false);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Artifact.Periapt:
                            CardSelectTarget(TargetingReason.Periapt);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Artifact.Potion:
                            CardSelectTarget(TargetingReason.Potion);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Artifact.Scepter:
                            CardSelectTarget(TargetingReason.Scepter);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Artifact.Serum:
                            CardSelectTarget(TargetingReason.Serum);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Artifact.Venom:
                            Board mokBoard = GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfKnives]
                                .GetComponent<Board>();
                            if (mokBoard.pv.IsMine)
                            {
                                GameObject potPiece = mokBoard.LookForPiece(GameMaster.PieceType.Assassin, true);
                                if (potPiece != null)
                                {
                                    potPiece.GetComponent<PhotonView>().RPC("ActivatePoison", RpcTarget.All);
                                    PlayCard(hoveredCard);
                                    break;
                                }
                            }
                            GameObject potPiece2 = participant.LookForPiece(GameMaster.PieceType.Assassin, true);
                            if (potPiece2 != null)
                            {
                                potPiece2.GetComponent<PhotonView>().RPC("ActivatePoison", RpcTarget.All);
                                PlayCard(hoveredCard);
                                break;
                            }
                            else
                            {
                                break;
                            }
                        case GameMaster.Artifact.Wand:
                            // TODO implement this once threat is in
                            PlayCard(hoveredCard);
                            break;
                    }
                    break;
                case GameMaster.CardType.Action:
                    switch ((GameMaster.Action)hoveredCard.cardIndex)
                    {
                        case GameMaster.Action.Improvise:
                            // TODO card drawn, hovered, popup with selection, then loop back here if confirmed, execute twice
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.DoubleAgent:
                            // TODO implement once threat is in
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.SecretCache:
                            participant.DrawACard(GameMaster.CardType.Artifact);
                            participant.DrawACard(GameMaster.CardType.Artifact);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.AskForFavours:
                            // TODO selection UI with decision, then Leader pays and you gain
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.CallInBackup:
                            participant.AddPiece(GameMaster.PieceType.Thug, false);
                            participant.AddPiece(GameMaster.PieceType.Thug, false);
                            participant.AddPiece(GameMaster.PieceType.Thug, false);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.ExecuteAHeist:
                            // TODO multi toggle selection UI with decision, then those selected get coins
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.RunForOffice:
                            // TODO selection UI with number input field and confirm, upon confirm you lose that much and the leader gets a bool flagged which acts at end of turn
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.SwearTheOaths:
                            participant.AddPiece(GameMaster.PieceType.Assassin, false);
                            participant.AddPiece(GameMaster.PieceType.Assassin, false);
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.BribeTheTaxOfficer:
                            for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
                            {
                                if (i != participant.playerNumber)
                                {
                                    Participant target = GameMaster.Instance.FetchPlayerByNumber(i);
                                    if (target.coins > 0)
                                    {
                                        if (target.coins > 1)
                                        {
                                            target.pv.RPC("RpcRemoveCoin", RpcTarget.Others, 1);
                                            participant.AddCoin(1);
                                        }
                                        else
                                        {
                                            target.pv.RPC("RpcRemoveCoin", RpcTarget.Others, 2);
                                            participant.AddCoin(2);
                                        }
                                    }
                                    else
                                    {
                                        // TODO add this once info is in
                                    }
                                }
                            }
                            PlayCard(hoveredCard);
                            break;
                        case GameMaster.Action.DealWithItYourself:
                            // TODO add this once threat cards are in
                            PlayCard(hoveredCard);
                            break;
                    }
                    break;
            }
            
        }

        private void PlayCard(Card card)
        {
            card.transform.position = playedCardsLocation.position + Vector3.up * .5f;
            participant.aHand.Remove(card);
            card.cardCollider.enabled = false;
            ResetAfterSelect();
        }

        private void SelectACardSA(Card hoveredCard)
        {
            PhotonNetwork.Destroy(hoveredCard.GetComponent<PhotonView>());
            ResetAfterSelect();
            participant.AddCoin(4);
        }
        
        private void SelectACardTCS(Card hoveredCard)
        {
            Decklist.Instance.artifactCards.TryGetValue((GameMaster.Artifact) hoveredCard.cardIndex,
                out ArtifactCard temp);
            threatContributedValues[5] += temp.weaponStrength;
            PhotonNetwork.Destroy(hoveredCard.GetComponent<PhotonView>());
            threatResolutionCardTargets--;
            if (threatResolutionCardTargets == 0)
            {
                targetedThreat.pv.RPC("Contribute", RpcTarget.All, participant.playerNumber, threatContributedValues);
            }
            ResetAfterSelect();
        }

        public void ConfirmPostTurnPay()
        {
            participant.RemoveCoins(postTurnPayAssigner.TallyAndClean(out int thugAmount));
            if (GameMaster.Instance.characterIndex.ContainsKey(GameMaster.Character.Sheriff))
            {
                GameMaster.Instance.characterIndex.TryGetValue(GameMaster.Character.Sheriff, out Participant part);
                part.pv.RPC("RpcAddCoin", RpcTarget.Others, thugAmount/2);
            }
            ResetAfterSelect();
        }

        public void ConfirmWorkerDistribution()
        {
            for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
            {
                int amount = workerDistributionPools[i + 1].objectsHeld.Count;
                if (GameMaster.Instance.FetchPlayerByNumber(i).character == GameMaster.Character.Necromancer)
                {
                    amount++;
                }
                GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("RpcAddPiece", RpcTarget.All, (byte)GameMaster.PieceType.Worker, amount);
                workerDistributionPools[i+1].DropPool();
            }
            ResetAfterSelect();
        }

        public void ConfirmJobDistribution()
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                Participant target = GameMaster.Instance.FetchPlayerByNumber(i);
                for (byte j = 0; j < jobDistributionPools[i+1].objectsHeld.Count; j++)
                {
                    JobPieceUI jpui = jobDistributionPools[i+1].objectsHeld[j].GetComponent<JobPieceUI>();
                    Board targetBoard = GameMaster.Instance.jobBoards[(int) jpui.representedJob].GetComponent<Board>();
                    targetBoard.pv.TransferOwnership(target.pv.Controller);
                    targetBoard.pv.RPC("ChangeJobHolder", RpcTarget.All, j);
                }
                jobDistributionPools[i+1].DropPool();
            }
            
            ResetAfterSelect();
        }

        public void NotBaubledResults(TargetingReason typeOfInquiry, Participant inquiringPlayer)
        { // fast track version
            switch (typeOfInquiry)
            {
                case TargetingReason.Ball:
                    // TODO rpc the lookbehindscreen or the information producing effect it creates
                    break;
                case TargetingReason.Bow:
                    StartSelection(SelectionType.BowTargetAssignment, null);
                    break;
                case TargetingReason.Periapt:
                    // TODO add once info is in
                    break;
                case TargetingReason.Scepter:
                    participant.RpcRemoveHealth(3);
                    break;
                case TargetingReason.Serum:
                    StartSelection(SelectionType.SerumPopUp, null);
                    break;
            }
        }

        public void ConfirmBowChoice()
        {
            if (bowDecider.Clean())
            {
                // this is the nice state
            }
            else
            {
                // TODO once info is in, give the inquirer lookbehindscreen
            }
            ResetAfterSelect();
            
        }

        public void ConfirmBaubleChoice(bool decision)
        {
            if (decision)
            {
                for (var i = 0; i < participant.aHand.Count; i++)
                {
                    if (participant.aHand[i].cardType == GameMaster.CardType.Artifact &&
                        participant.aHand[i].cardIndex == (int) GameMaster.Artifact.Bauble)
                    {
                        PlayCard(participant.aHand[i]);
                    }
                }
            }
            else
            {
                NotBaubledResults(typeOfTargeting, inquirer);
                ResetAfterSelect();
            }
        }

        public void ClosePopup()
        {
            ResetAfterSelect();
        }

        private void LookBehindScreen(Participant target)
        {
            // TODO: implement this once info is in
        }

        public void ConfirmCharSelection()
        {
            for (int i = 0; i < playerSelectorsChar.Length; i++)
            {
                if (playerSelectorsChar[i].activeSelf)
                {
                    if (playerSelectorsChar[i].GetComponent<Toggle>().isOn)
                    {
                        Participant target = GameMaster.Instance.FetchPlayerByNumber(i);
                        switch (typeOfTargeting)
                        {
                            case TargetingReason.Poison:
                                target.pv.RPC("RpcRemoveHealth", RpcTarget.Others, 1);
                                break;
                            case TargetingReason.Seduction:
                                LookBehindScreen(target);
                                break;
                            case TargetingReason.Potion:
                                target.pv.RPC("RpcAddHealth", RpcTarget.All, 2);
                                break;
                            default:
                                target.pv.RPC("BaubleInquiry", RpcTarget.Others, (byte)typeOfTargeting, (byte)participant.playerNumber);
                                break;
                        }
                    }
                }
            }
            ResetAfterSelect();
        }

        #endregion
    }
}
