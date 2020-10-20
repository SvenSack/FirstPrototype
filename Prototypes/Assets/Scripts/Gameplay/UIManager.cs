using System.Collections.Generic;
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
        private bool isGrabbingUI;
        public SelectionType typeOfSelection;
        [SerializeField] GameObject[] SelectionPopUps = new GameObject[2];
        [SerializeField] private Transform playedCardsLocation;
        [SerializeField] private PayAssignment postTurnPayAssigner;
        [SerializeField] private GameObject[] playerSelectorsChar = new GameObject[5];
        [SerializeField] private GameObject playerSelectorChar;
        [SerializeField] private GameObject workerUIPrefab;
        [SerializeField] private GameObject[] jobSelectionUIPrefabs = new GameObject[5];
        private List<SelectionType> selectionBuffer = new List<SelectionType>();
        public DistributionPool[] workerDistributionPools = new DistributionPool[7];
        public DistributionPool[] jobDistributionPools = new DistributionPool[7];
        private GraphicRaycaster gRayCaster;
        private EventSystem eventSys;
        private DistributionPieceUI grabbedUI;

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
                            SelectACardTG(CursorFollower.Instance.hoveredACard);
                            break;
                        case SelectionType.SellArtifacts:
                            if (!CursorFollower.Instance.hoveredACard.isPrivate && CursorFollower.Instance.hoveredACard.cardType == GameMaster.CardType.Artifact)
                            {
                                SelectACardSA(CursorFollower.Instance.hoveredACard);
                            }
                            break;
                    }
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
                CursorFollower.Instance.hoveredACard = null;
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
                    case SelectionType.BlackMarket:
                        break;
                    case SelectionType.ThievesGuild:
                    case SelectionType.SellArtifacts:
                        isSelectingACard = true;
                        break;
                    case SelectionType.PostTurnPay:
                        postTurnPayAssigner.CreateToggles();
                        break;
                    case SelectionType.Poisoner:
                    case SelectionType.Seducer:
                        isSelectingAPlayer = true;
                        playerSelectorChar.SetActive(true);
                        break;
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
                }
            }
            else
            {
                selectionBuffer.Add(type);
            }
        }

        public void UpdateSelectionNames()
        {
            for (int i = 0; i < GameMaster.Instance.seatsClaimed; i++)
            {
                Participant part = GameMaster.Instance.FetchPlayerByNumber(i);
                if (i != participant.playerNumber)
                {
                    playerSelectorsChar[i].SetActive(true);
                    string playerName = part.pv.Controller.NickName;
                    Decklist.Instance.characterNames.TryGetValue(part.character, out string charName);
                    string nameText = charName + "(" + playerName + ")";
                    playerSelectorsChar[i].GetComponentInChildren<TextMeshProUGUI>().text = nameText;
                    workerDistributionPools[i + 1].gameObject.SetActive(true);
                    workerDistributionPools[i + 1].labelText.text = nameText;
                    jobDistributionPools[i + 1].gameObject.SetActive(true);
                    jobDistributionPools[i + 1].labelText.text = nameText;
                }
                else
                {
                    workerDistributionPools[i + 1].gameObject.SetActive(true);
                    jobDistributionPools[i + 1].gameObject.SetActive(true);
                    Decklist.Instance.characterNames.TryGetValue(part.character, out string charName);
                    workerDistributionPools[i + 1].labelText.text = charName +"(You)";
                    jobDistributionPools[i + 1].labelText.text = charName +"(You)";
                }
            }
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
            // TODO: add simpler card effects
            hoveredCard.transform.position = playedCardsLocation.position + Vector3.up * .3f;
            participant.aHand.Remove(hoveredCard);
            hoveredCard.GetComponent<PhotonView>().TransferOwnership(GameMaster.Instance.playerSlots[0].Controller);
            ResetAfterSelect();
        }

        private void SelectACardSA(Card hoveredCard)
        {
            PhotonNetwork.Destroy(hoveredCard.GetComponent<PhotonView>());
            ResetAfterSelect();
            participant.AddCoin(4);
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
                // TODO implement necromancer here
                GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("RpcAddPiece", RpcTarget.All, (byte)GameMaster.PieceType.Worker, workerDistributionPools[i+1].objectsHeld.Count);
                workerDistributionPools[i+1].DropPool();
            }
            ResetAfterSelect();
        }

        public void ConfirmJobDistribution()
        {
            // TODO implement this
            ResetAfterSelect();
        }

        private void LookBehindScreen(Participant target)
        {
            // TODO: implement this once info is in
        }

        public void ConfirmCharSelection(bool isPoison)
        {
            for (int i = 0; i < playerSelectorsChar.Length; i++)
            {
                if (playerSelectorsChar[i].activeSelf)
                {
                    if (playerSelectorsChar[i].GetComponent<Toggle>().isOn)
                    {
                        if (isPoison)
                        {
                            GameMaster.Instance.FetchPlayerByNumber(i).pv.RPC("RpcRemoveHealth", RpcTarget.Others, 1);
                        }
                        else
                        {
                            LookBehindScreen(GameMaster.Instance.FetchPlayerByNumber(i));
                        }
                    }
                }
            }
            ResetAfterSelect();
        }
    }
}
