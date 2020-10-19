using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
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
        public SelectionType typeOfSelection;
        [SerializeField] GameObject[] SelectionPopUps = new GameObject[2];
        [SerializeField] private Transform playedCardsLocation;
        [SerializeField] private PayAssignment postTurnPayAssigner;
        [SerializeField] private GameObject[] playerSelectorsChar = new GameObject[5];
        [SerializeField] private GameObject playerSelectorChar;
        private List<SelectionType> selectionBuffer = new List<SelectionType>();

        public enum SelectionType
        {
            BlackMarket,
            ThievesGuild,
            SellArtifacts,
            PostTurnPay,
            Poisoner,
            Seducer,
            
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
                        UpdateSelectionNames();
                        isSelectingAPlayer = true;
                        playerSelectorChar.SetActive(true);
                        break;
                }
            }
            else
            {
                selectionBuffer.Add(type);
            }
        }

        private void UpdateSelectionNames()
        {
            for (int i = 0; i < GameMaster.Instance.playerNumber; i++)
            {
                Participant part = GameMaster.Instance.FetchPlayerByNumber(i);
                if (part != participant)
                {
                    playerSelectorsChar[i].SetActive(true);
                    string playerName = part.pv.Controller.NickName;
                    Decklist.Instance.characterNames.TryGetValue(part.character, out string charName);
                    playerSelectorsChar[i].GetComponentInChildren<TextMeshProUGUI>().text = charName + "(" + playerName + ")";
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
