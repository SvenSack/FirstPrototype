﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class GameMaster : MonoBehaviourPunCallbacks
    { 
        public List<Character> characterDeck;
        public List<Role> roleDeck;
        public List<Action> actionDeck;
        public List<Artifact> artifactDeck;
        public static GameMaster Instance;
        public PhotonView[] playerSlots;
        public PhotonView[] jobBoards;
        public bool isTesting;
        public Dictionary<Role, int> playerRoles = new Dictionary<Role, int>();
        public List<Piece> workerPieces = new List<Piece>();
        public int turnCounter;

        public PhotonView pv;
        [SerializeField] private GameObject participantObject = null;
        [SerializeField] private GameObject[] cardPrefabs = new GameObject[5];
        [SerializeField] private GameObject[] piecePrefabs = new GameObject[3];

        public enum Character
        {
            Ruffian,
            Scion,
            Seducer,
            BurglaryAce,
            Necromancer,
            Sheriff,
            Poisoner,
            Adventurer,
            PitFighter,
            OldFox
        }
        
        public enum Role
        {
            Leader,
            Rogue,
            Paladin,
            Gangster,
            Vigilante,
            Noble
        }

        public enum Artifact
        {
            Potion,
            Serum,
            Ball,
            Periapt,
            Venom,
            Dagger,
            Bauble,
            Bow,
            Scepter,
            Wand
        }
        
        public enum Action
        {
            SecretCache,
            DoubleAgent,
            RunForOffice,
            CallInBackup,
            DealWithItYourself,
            SwearTheOaths,
            ExecuteAHeist,
            BribeTheTaxOfficer,
            AskForFavours,
            Improvise
        }

        public enum CardType
        {
            Character,
            Role,
            Action,
            Artifact,
            Threat
        }

        public enum PieceType
        {
            Worker,
            Thug,
            Assassin
        }

        public enum Job
        {
            MasterOfCoin,
            MasterOfKnives,
            MasterOfWhispers,
            MasterOfClubs,
            MasterOfGoods
        }

        private void Start()
        {
            pv = GetComponent<PhotonView>();
            for (int i = 0; i < 4; i++)
            {
                CreateDeck((CardType)i);
            }
            Instance = this;
            PhotonNetwork.Instantiate(participantObject.name, Vector3.zero, Quaternion.identity, 0);
        }

        private void Update()
        {
            if (artifactDeck.Count < 1)
            {
                CreateDeck(CardType.Artifact);
            }
            if (actionDeck.Count < 1)
            {
                CreateDeck(CardType.Action);
            }
        }

        public GameObject CreatePiece(PieceType type)
        {
             GameObject piece = PhotonNetwork.Instantiate(piecePrefabs[(int) type].name, transform.position, Quaternion.identity);
             if (type == PieceType.Worker)
             {
                 workerPieces.Add(piece.GetComponent<Piece>());
             }
             return piece;
        }

        public int DrawCard(CardType type)
        {
            switch (type)
            {
                case CardType.Character:
                    int randomPick = Random.Range(0, characterDeck.Count);
                    Character returnValue = characterDeck[randomPick];
                    pv.RPC("RemoveFromDeck", RpcTarget.AllBuffered,(int)type, (int)returnValue);
                    return (int)returnValue;
                case CardType.Role:
                    int randomPick2 = Random.Range(0, roleDeck.Count);
                    Role returnValue2 = roleDeck[randomPick2];
                    pv.RPC("RemoveFromDeck", RpcTarget.AllBuffered,(int)type, (int)returnValue2);
                    return (int)returnValue2;
                case CardType.Artifact:
                    int randomPick3 = Random.Range(0, artifactDeck.Count);
                    Artifact returnValue3 = artifactDeck[randomPick3];
                    pv.RPC("RemoveFromDeck", RpcTarget.AllBuffered,(int)type, (int)returnValue3);
                    return (int)returnValue3;
                case CardType.Action:
                    int randomPick4 = Random.Range(0, actionDeck.Count);
                    Action returnValue4 = actionDeck[randomPick4];
                    pv.RPC("RemoveFromDeck", RpcTarget.AllBuffered,(int)type, (int)returnValue4);
                    return (int)returnValue4;
                default: return -1;
            }
        }

        [PunRPC]
        public void RemoveFromDeck(int cardType, int index)
        {
            switch ((CardType)cardType)
            {
                case CardType.Character:
                    Character target = (Character) index;
                    for (int i = 0; i < characterDeck.Count; i++)
                    {
                        if (characterDeck[i] == target)
                        {
                            characterDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case CardType.Role:
                    Role target2 = (Role) index;
                    for (int i = 0; i < roleDeck.Count; i++)
                    {
                        if (roleDeck[i] == target2)
                        {
                            roleDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case CardType.Action:
                    Action target3 = (Action) index;
                    for (int i = 0; i < actionDeck.Count; i++)
                    {
                        if (actionDeck[i] == target3)
                        {
                            actionDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
                case CardType.Artifact:
                    Artifact target4 = (Artifact) index;
                    for (int i = 0; i < artifactDeck.Count; i++)
                    {
                        if (artifactDeck[i] == target4)
                        {
                            artifactDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
            }
            
        }

        private void CreateDeck(CardType type)
        {
            switch (type)
            {
                case CardType.Character:
                    characterDeck = new List<Character> {
                        Character.Adventurer, 
                        Character.Necromancer,
                        Character.Poisoner,
                        Character.Ruffian,
                        Character.Scion,
                        Character.Seducer,
                        Character.Sheriff,
                        Character.BurglaryAce,
                        Character.OldFox,
                        Character.PitFighter};
                    break;
                case CardType.Role:
                    roleDeck = new List<Role>();
                    for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
                    {
                        roleDeck.Add((Role)i);
                    }

                    break;
                case CardType.Action:
                    actionDeck = new List<Action>
                    {
                        Action.Improvise, Action.Improvise,
                        Action.DoubleAgent, Action.DoubleAgent,
                        Action.SecretCache, Action.SecretCache,
                        Action.AskForFavours, Action.AskForFavours,
                        Action.CallInBackup, Action.CallInBackup,
                        Action.ExecuteAHeist, Action.ExecuteAHeist,
                        Action.RunForOffice, Action.RunForOffice,
                        Action.SwearTheOaths, Action.SwearTheOaths,
                        Action.BribeTheTaxOfficer, Action.BribeTheTaxOfficer,
                        Action.DealWithItYourself, Action.DealWithItYourself
                    };
                    break;
                case CardType.Artifact:
                    artifactDeck = new List<Artifact>
                    {
                        Artifact.Ball, Artifact.Ball, Artifact.Ball, Artifact.Ball,
                        Artifact.Bauble, Artifact.Bauble, Artifact.Bauble, Artifact.Bauble,
                        Artifact.Bow, Artifact.Bow, Artifact.Bow,
                        Artifact.Dagger, Artifact.Dagger, Artifact.Dagger, Artifact.Dagger,
                        Artifact.Periapt, Artifact.Periapt, Artifact.Periapt, Artifact.Periapt,
                        Artifact.Potion, Artifact.Potion, Artifact.Potion, Artifact.Potion, Artifact.Potion,
                        Artifact.Scepter,
                        Artifact.Serum, Artifact.Serum, Artifact.Serum,
                        Artifact.Venom, Artifact.Venom, Artifact.Venom,
                        Artifact.Wand, Artifact.Wand
                    };
                    break;
            }
        }

        public GameObject ConstructCard(CardType type, int enumIndex)
        {
            Decklist dL = Decklist.Instance;
            GameObject card = null;
            switch (type)
            {
                case CardType.Action:
                    card = Instantiate(cardPrefabs[(int) type]);
                    Card parts = card.GetComponent<Card>();
                    if (dL.actionCards.TryGetValue((Action) enumIndex, out ActionCard thisCard))
                    {
                        parts.illustration.sprite = thisCard.illustration;
                        parts.cardName.text = thisCard.name;
                        parts.text.text = thisCard.effectText;
                    }
                    else
                    {
                        Debug.LogWarning("PANIC");
                    }
                    break;
                case CardType.Artifact:
                    card = Instantiate(cardPrefabs[(int) type]);
                    Card partsA = card.GetComponent<Card>();
                    if (dL.artifactCards.TryGetValue((Artifact) enumIndex, out ArtifactCard thisACard))
                    {
                        partsA.illustration.sprite = thisACard.illustration;
                        partsA.cardName.text = thisACard.name;
                        partsA.text.text = thisACard.effectText;
                        partsA.extraText1.text = "Strength: " + thisACard.weaponStrength;
                    }
                    else
                    {
                        Debug.LogWarning("PANIC");
                    }
                    break;
                case CardType.Character:
                    card = Instantiate(cardPrefabs[(int) type]);
                    Card partsC = card.GetComponent<Card>();
                    if (dL.characterCards.TryGetValue((Character) enumIndex, out CharacterCard thisCCard))
                    {
                        partsC.illustration.sprite = thisCCard.illustration;
                        partsC.cardName.text = thisCCard.name;
                        partsC.text.text = thisCCard.effectText;
                        partsC.extraText1.text = thisCCard.health.ToString();
                        partsC.extraText2.text = thisCCard.wealth.ToString();
                    }
                    else
                    {
                        Debug.LogWarning("PANIC");
                    }
                    break;
                case CardType.Role:
                    card = Instantiate(cardPrefabs[(int) type]);
                    Card partsR = card.GetComponent<Card>();
                    if (dL.roleCards.TryGetValue((Role) enumIndex, out RoleCard thisRCard))
                    {
                        partsR.illustration.sprite = thisRCard.illustration;
                        partsR.cardName.text = thisRCard.name;
                        partsR.text.text = thisRCard.effectText;
                        if (!thisRCard.isGuild)
                        {
                            partsR.icon.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("PANIC");
                    }
                    break;
            }
            return card;
        }

        [PunRPC]
        public void EndTurn()
        {
            Piece[] allPieces = FindObjectsOfType<Piece>();
            int[,] bill = new int[7,2];
            foreach (var piece in allPieces)
            {
                switch (piece.type)
                {
                    case PieceType.Thug:
                        bill[FetchPlayerByPlayer(piece.pv.Controller).playerNumber,0]+=1;
                        break;
                    case PieceType.Assassin:
                        bill[FetchPlayerByPlayer(piece.pv.Controller).playerNumber,1]+=1;
                        break;
                    case PieceType.Worker:
                        piece.pv.TransferOwnership(playerSlots[0].Controller);
                        piece.ToggleUse();
                        break;
                }
            }
            
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                FetchPlayerByNumber(i).pv.RPC("RpcEndTurn", RpcTarget.Others, bill[i,0], bill[i,1]);
            }
            
            
        }

        [PunRPC]
        public void RpcAddRoleIndex(int playerNumber, int roleIndex)
        {
            playerRoles.Add((Role)roleIndex, playerNumber);
        }

        public Participant FetchPlayerByNumber(int playerNumber)
        {
            return playerSlots[playerNumber].gameObject.GetComponent<PlayerSlot>().player;
        }

        private Participant FetchPlayerByPlayer(Player player)
        {
            foreach (var slot in playerSlots)
            {
                if (Equals(slot.Controller, player))
                {
                    return slot.gameObject.GetComponent<PlayerSlot>().player;
                }
            }

            return null;
        }

        public Participant FetchPlayerByJob(Job job)
        {
            Player player = jobBoards[(int) job].Controller;
            return FetchPlayerByPlayer(player);
        }

        public Participant FetchLeader()
        {
            playerRoles.TryGetValue(Role.Leader, out int playerNumber);
            return FetchPlayerByNumber(playerNumber);
        }
    }
    
}