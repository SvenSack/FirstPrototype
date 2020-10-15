using System;
using Photon.Pun;
using UnityEngine;

namespace Gameplay
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private TileType type;
        public bool isUsed;
        [SerializeField] private GameObject nameHover;
        [SerializeField] private GameObject explanationHover;
        public float hoverTime = 0;
        [SerializeField] private float[] hoverTimes = new float[2];
        public Transform pieceLocation;
        public Participant player;
        public Board board;
        [SerializeField] private GameObject light;
        

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
            TrainOrphans
        }

        private void Start()
        {
            explanationHover.SetActive(false);
            nameHover.SetActive(false);
        }

        private void Update()
        {
            if (nameHover.activeSelf)
            {
                OrientToCamera();
            }
        }

        private void OrientToCamera()
        {
            nameHover.transform.LookAt(player.mySlot.perspective.transform);
            nameHover.transform.Rotate(Vector3.up, 180);
            explanationHover.transform.LookAt(player.mySlot.perspective.transform);
            explanationHover.transform.Rotate(Vector3.up, 180);
        }
        
        private bool PerformTileAction(bool isThug)
        {
            switch (type)
            {
                case TileType.ThievesGuild:
                    // TODO: implement card playing here
                    return true;
                    break;
                case TileType.Market:
                    if (player.coins > 0)
                    {
                        // outsource the coin giving to the function maybe ?
                        player.DrawFromMarket();
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
                                GameObject piece = board.LookForPiece(GameMaster.PieceType.Thug);
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
                                    GameObject piece1 = board.LookForPiece(GameMaster.PieceType.Thug);
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
                                        GameObject piece3 = board.LookForPiece(GameMaster.PieceType.Assassin);
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
                            // TODO: implement other whispers jobs when info is in
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
                                    player.RemoveCoins(4);
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
                                else if (player.coins > 3)
                                {
                                    player.RemoveCoins(4);
                                    board.DrawACard();
                                    player.DrawACard(GameMaster.CardType.Artifact);
                                    GiveArtifactToLeader();
                                    ToggleUsed();
                                    return true;
                                }
                                else return false;
                            // TODO: implement the excess artifact selling once selection is in
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

        private void GiveCoinsToLeader(int amount)
        {
            if (player.Equals(GameMaster.Instance.FetchLeader()))
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
            if (player.Equals(GameMaster.Instance.FetchLeader()))
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

        private void GiveCoinToOwner(byte amount, GameMaster.Job owningRole)
        {
            if (player.Equals(GameMaster.Instance.FetchPlayerByJob(owningRole)))
            {
                if (player.Equals(GameMaster.Instance.FetchLeader()))
                {
                                
                }
                else
                {
                    player.RemoveCoins(amount);
                    GiveCoinsToLeader(amount);
                }
            }
            else
            {
                player.RemoveCoins(amount);
                GiveJobCoin(amount, owningRole);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Pieces"))
            {
                Piece piece = other.gameObject.GetComponent<Piece>();
                if (!piece.isPickedUp && !isUsed)
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
                                piece.transform.position = pieceLocation.position+Vector3.up*.3f;
                            }
                            break;
                        case GameMaster.PieceType.Thug:
                            if (PerformTileAction(true))
                            {
                                piece.ToggleUse();
                            }
                            else
                            {
                                piece.transform.position = pieceLocation.position+Vector3.up*.3f;
                            }
                            break;
                        default:
                            piece.transform.position = pieceLocation.position+Vector3.up*.3f;
                            break;
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
            nameHover.SetActive(CursorFollower.Instance.IsHovering);
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
