using System;
using Photon.Pun;
using UnityEngine;

namespace Gameplay
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private TileType type;
        [SerializeField] private GameObject nameHover;
        [SerializeField] private GameObject explanationHover;
        [SerializeField] private float[] hoverTimes = new float[2];
        [SerializeField] private GameObject light;
        
        public bool isUsed;
        public float hoverTime = 0;
        public Participant player;
        public Board board;
        

        private enum TileType
        {
            ThievesGuild,
            Market,
            Bank,
            DarkShrine,
            Tavern,
            SkimOffTheTreasury,
            InvestInTheFuture,
            StimulateTheFlow,
            OrganizeOperations,
            PocketSomeMerchandise,
            BuyBulkArtifacts,
            SellExcessArtifacts,
            SmuggleLuxuryGoods,
            EquipSpies,
            ForgeEvidence,
            FalsifyBooks,
            BlackmailNobility,
            MentorTheRookies,
            EnlistNewGang,
            ExtortMembers,
            ExecuteAHeist,
            StageACoupForLeadership,
            IndoctrinateTheFlock,
            StimulateZeal,
            TrainOrphans,
            ThreatenPlayers
        }

        private void Start()
        {
            explanationHover.SetActive(false);
            if (board == null)
            {
                nameHover.SetActive(false);
            }
        }

        private void Update()
        {
            if (nameHover.activeSelf && board!=null)
            {
                if (board.jobHolder != null && board.jobHolder.pv.IsMine)
                {
                    OrientToCamera();
                }
            }
        }

        private void OrientToCamera()
        { // used to ensure that the worldspace UI faces the player after boards are re-assigned
            nameHover.transform.LookAt(player.mySlot.perspective.transform);
            nameHover.transform.Rotate(Vector3.up, 180);
            explanationHover.transform.LookAt(player.mySlot.perspective.transform);
            explanationHover.transform.Rotate(Vector3.up, 180);
        }
        
        private bool PerformTileAction(bool isThug)
        { // this is the logic method behind the board tiles, it just compares against their type and then acts accordingly
            switch (type)
            {
                case TileType.ThievesGuild:
                    if (player.aHand.Count < 1 && (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber == player.playerNumber && GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods].GetComponent<Board>().artifactHand.Count < 1))
                    {
                        return false;
                    }
                    UIManager.Instance.StartSelection(UIManager.SelectionType.ThievesGuild, this);
                    return true;
                    break;
                case TileType.Market:
                    if (player.coins > 0 || (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfCoin).playerNumber == player.playerNumber && GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfCoin].GetComponent<Board>().coins > 0) ||
                        (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber == player.playerNumber && GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods].GetComponent<Board>().coins > 0) ||
                        (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfClubs).playerNumber == player.playerNumber && GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfClubs].GetComponent<Board>().coins > 0))
                    {
                        UIManager.Instance.StartSelection(UIManager.SelectionType.BlackMarket, this);
                        return true;
                    }
                    break;
                case TileType.Bank:
                    if (player.character != GameMaster.Character.BurglaryAce)
                    {
                        player.AddCoin(2);
                        GiveCoinsToLeader(1);
                    }
                    else
                    {
                        player.AddCoin(4);
                        GiveCoinsToLeader(2);
                    }
                    return true;
                case TileType.DarkShrine:
                    if (player.coins > 0)
                    {
                        GiveCoinToOwner(1, GameMaster.Job.MasterOfKnives);
                        player.AddPiece(GameMaster.PieceType.Assassin, true);
                        return true;
                    }
                    break;
                case TileType.Tavern:
                    if (player.coins > 0)
                    {
                        GiveCoinToOwner(1, GameMaster.Job.MasterOfClubs);
                        player.AddPiece(GameMaster.PieceType.Thug, true);
                        return true;
                    }
                    break;
                case TileType.ThreatenPlayers:
                    UIManager.Instance.StartSelection(UIManager.SelectionType.ThreatenPlayerDistribution, null);
                    return true;
                default:
                    if (isThug)
                    {
                        return false;
                    }
                    else
                    {
                        switch (type)
                        {
                            case TileType.SkimOffTheTreasury:
                                if (board.coins > 7)
                                {
                                    board.RemoveCoins(8);
                                    player.AddCoin(8);
                                    GiveCoinsToLeader(2);
                                    ToggleUsed();
                                    return true;
                                }
                                else if(board.coins > 0)
                                {
                                    player.AddCoin(board.coins);
                                    board.RemoveCoins(board.coins);
                                    GiveCoinsToLeader(2);
                                    ToggleUsed();
                                    return true;
                                }
                                else return false;
                            case TileType.InvestInTheFuture:
                                if (player.coins > 1)
                                {
                                    player.RemoveCoins(2);
                                    board.AddCoin(4);
                                    ToggleUsed();
                                    return true;
                                }
                                else return false;
                            case TileType.OrganizeOperations:
                                if (player.coins > 1)
                                {
                                    player.RemoveCoins(2);
                                    board.AddCoin(board.coins);
                                    ToggleUsed();
                                    return true;
                                }
                                else return false;
                            case TileType.StimulateTheFlow:
                                board.AddCoin(4);
                                GiveCoinsToLeader(2);
                                ToggleUsed();
                                return true;
                            case TileType.ExtortMembers:
                                player.AddCoin(4);
                                GiveCoinsToLeader(2);
                                ToggleUsed();
                                return true;
                            case TileType.ExecuteAHeist:
                                GameObject piece = board.LookForPiece(GameMaster.PieceType.Thug, false);
                                if (piece != null)
                                {
                                    PhotonNetwork.Destroy(piece);
                                    board.AddCoin(4);
                                    GiveCoinsToLeader(2);
                                    ToggleUsed();
                                    return true;
                                }
                                else return false;
                            case TileType.EnlistNewGang:
                                board.AddPiece(GameMaster.PieceType.Thug, true);
                                board.AddPiece(GameMaster.PieceType.Thug, true);
                                ToggleUsed();
                                return true;
                            case TileType.MentorTheRookies:
                                for (int i = 0; i < 2; i++)
                                {
                                    GameObject piece1 = board.LookForPiece(GameMaster.PieceType.Thug, true);
                                    if (piece1 != null)
                                    {
                                        PhotonNetwork.Destroy(piece1);
                                    }
                                    player.AddPiece(GameMaster.PieceType.Thug, true);
                                }
                                ToggleUsed();
                                return true;
                            case TileType.StimulateZeal:
                                player.hasZeal = true;
                                ToggleUsed();
                                return true;
                            case TileType.TrainOrphans:
                                board.AddPiece(GameMaster.PieceType.Assassin, true);
                                board.AddPiece(GameMaster.PieceType.Assassin, true);
                                ToggleUsed();
                                return true;
                            case TileType.IndoctrinateTheFlock:
                                board.AddPiece(GameMaster.PieceType.Assassin, true);
                                player.AddPiece(GameMaster.PieceType.Assassin, true);
                                ToggleUsed();
                                return true;
                            case TileType.StageACoupForLeadership:
                                if (board.pieces.Count > 0)
                                {
                                    for (int i = 0; i < board.pieces.Count; i++)
                                    {
                                        GameObject piece3 = board.LookForPiece(GameMaster.PieceType.Assassin, false);
                                        if (piece3 != null)
                                        {
                                            PhotonNetwork.Destroy(piece3);
                                            player.AddPiece(GameMaster.PieceType.Assassin, false);
                                        }
                                    }

                                    ToggleUsed();
                                    return true;
                                }
                                else return false;
                            case TileType.EquipSpies:
                                player.DrawACard(GameMaster.CardType.Action);
                                player.DrawACard(GameMaster.CardType.Action);
                                ToggleUsed();
                                return true;
                            case TileType.ForgeEvidence:
                                UIManager.Instance.StartSelection(UIManager.SelectionType.ForgeEvidence, null);
                                ToggleUsed();
                                return true;
                            case TileType.FalsifyBooks:
                                if (player.informationHand.Count > 0)
                                {
                                    UIManager.Instance.StartInformationSelection(true);
                                    ToggleUsed();
                                    return true;
                                }
                                break;
                            case TileType.BlackmailNobility:
                                if (player.informationHand.Count > 0)
                                {
                                    UIManager.Instance.StartInformationSelection(false);
                                    ToggleUsed();
                                    return true;
                                }
                                break;
                            case TileType.SmuggleLuxuryGoods:
                                board.AddCoin(4);
                                ToggleUsed();
                                return true;
                            case TileType.BuyBulkArtifacts:
                                if (board.coins > 3)
                                {
                                    board.RemoveCoins(4);
                                    board.DrawACard();
                                    board.DrawACard();
                                    board.DrawACard();
                                    GiveArtifactToLeader();
                                    ToggleUsed();
                                    return true;
                                }
                                else if (player.coins > 3)
                                {
                                    UIManager.Instance.PayAmountOwed(4);
                                    board.DrawACard();
                                    board.DrawACard();
                                    board.DrawACard();
                                    GiveArtifactToLeader();
                                    ToggleUsed();
                                    return true;
                                }
                                else return false;
                            case TileType.PocketSomeMerchandise:
                                if (board.coins > 3)
                                {
                                    board.RemoveCoins(4);
                                    board.DrawACard();
                                    player.DrawACard(GameMaster.CardType.Artifact);
                                    GiveArtifactToLeader();
                                    ToggleUsed();
                                    return true;
                                }
                                else if (player.coins > 3 || (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfCoin).playerNumber == player.playerNumber && GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfCoin].GetComponent<Board>().coins > 3) ||
                                         (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber == player.playerNumber && GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods].GetComponent<Board>().coins > 3) ||
                                         (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfClubs).playerNumber == player.playerNumber && GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfClubs].GetComponent<Board>().coins > 3))
                                {
                                    UIManager.Instance.PayAmountOwed(4);
                                    board.DrawACard();
                                    player.DrawACard(GameMaster.CardType.Artifact);
                                    GiveArtifactToLeader();
                                    ToggleUsed();
                                    return true;
                                }
                                else return false;
                            case TileType.SellExcessArtifacts:
                                UIManager.Instance.StartSelection(UIManager.SelectionType.SellArtifacts, this);
                                ToggleUsed();
                                break;
                        }
                    }
                    break;
            }
            
            return false;
        }

        public void ToggleUsed()
        {
            isUsed = !isUsed;
            light.SetActive(!light.activeSelf);
        }



        #region Helpers
        // this is stuff that is used regularly in the tile logic and thus was made into functions for easier viewing/less lines
        private void GiveCoinsToLeader(int amount)
        {
            if (player.Equals(GameMaster.Instance.FetchLeader()) || (player.roleRevealed && (player.role == GameMaster.Role.Rogue || player.role == GameMaster.Role.Paladin || player.role == GameMaster.Role.Vigilante)))
            {
                player.AddCoin(amount);
            }
            else
            {
                GameMaster.Instance.FetchLeader().pv.RPC("RpcAddCoin", RpcTarget.Others,(byte) amount);
            }
        }

        private void GiveArtifactToLeader()
        {
            if (player.Equals(GameMaster.Instance.FetchLeader()) || (player.roleRevealed && (player.role == GameMaster.Role.Rogue || player.role == GameMaster.Role.Paladin || player.role == GameMaster.Role.Vigilante)))
            {
                player.DrawACard(GameMaster.CardType.Artifact);
            }
            else
            {
                GameMaster.Instance.FetchLeader().pv.RPC("RpcAddArtifactCard", RpcTarget.Others);
            }
        }

        private void GiveJobCoin(byte amount, GameMaster.Job target)
        {
            GameMaster.Instance.FetchPlayerByJob(target).pv.RPC("RpcAddCoin", RpcTarget.Others, amount);
        }

        public void GiveCoinToOwner(byte amount, GameMaster.Job owningRole)
        {
            if (player.Equals(GameMaster.Instance.FetchPlayerByJob(owningRole)))
            {
                if (player.Equals(GameMaster.Instance.FetchLeader()))
                {
                                
                }
                else
                {
                    UIManager.Instance.PayAmountOwed(amount);
                    GiveCoinsToLeader(amount);
                }
            }
            else
            {
                UIManager.Instance.PayAmountOwed(amount);
                GiveJobCoin(amount, owningRole);
            }
        }

        #endregion

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Pieces") && player.pv.IsMine)
            {
                Piece piece = other.gameObject.GetComponent<Piece>();
                if (!piece.pv.IsMine)
                {
                    if (type != TileType.ThreatenPlayers)
                    {
                        return;
                    }
                    else
                    {
                        Debug.LogAssertion("Flip to threat");
                        ThreatPiece tp = piece.GetComponent<ThreatPiece>();
                        player.piecesThreateningMe.Add(tp);
                        tp.ToggleThreaten();
                    }
                }
                if (!piece.isPickedUp && !isUsed)
                {
                    if (type == TileType.ThreatenPlayers)
                    {
                        ThreatPiece tp = piece.GetComponent<ThreatPiece>();
                        if (tp.isThreatening)
                        {
                            tp.ToggleThreaten();
                        }
                        Debug.LogAssertion("Open Threat Dialogue");
                        switch (piece.type)
                        {
                            case GameMaster.PieceType.Assassin:
                                if (PerformTileAction(false))
                                {
                                }
                                else
                                {
                                    piece.ResetPiecePosition();
                                }
                                break;
                            case GameMaster.PieceType.Thug:
                                if (PerformTileAction(false))
                                {
                                }
                                else
                                {
                                    piece.ResetPiecePosition();
                                }
                                break;
                            default:
                                piece.ResetPiecePosition();
                                break;
                        }
                    }
                    else
                    {
                        switch (piece.type)
                        {
                            case GameMaster.PieceType.Worker:
                                if (PerformTileAction(false))
                                {
                                    piece.ToggleUse();
                                }
                                else
                                {
                                    piece.ResetPiecePosition();
                                }
                                break;
                            case GameMaster.PieceType.Thug:
                                if (PerformTileAction(true))
                                {
                                    piece.ToggleUse();
                                }
                                else
                                {
                                    piece.ResetPiecePosition();
                                }
                                break;
                            default:
                                piece.ResetPiecePosition();
                                break;
                        }
                    }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("CursorFollower"))
            {
                hoverTime += Time.deltaTime;
                CheckHovers();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("CursorFollower"))
            {
                if (CursorFollower.Instance.IsHovering)
                {
                    ToggleHover(true);
                }

                hoverTime = 0;
            }
        }

        private void ToggleHover(bool withExplanation)
        {
            CursorFollower.Instance.ToggleHover();
            if (board == null)
            {
                nameHover.SetActive(CursorFollower.Instance.IsHovering);
            }
            if (withExplanation)
            {
                explanationHover.SetActive(CursorFollower.Instance.IsHovering);
            }
        }

        private void CheckHovers()
        {
            if (!CursorFollower.Instance.IsHovering && hoverTime >= hoverTimes[0])
            {
                ToggleHover(false);
            }

            if (!explanationHover.activeSelf && hoverTime >= hoverTimes[1])
            {
                explanationHover.SetActive(true);
            }
        }
    }
}
