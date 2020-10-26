using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class GameMaster : MonoBehaviourPunCallbacks
    {
        #region Lists

        public List<Character> characterDeck = new List<Character> {
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
        public List<Role> roleDeck = new List<Role>
        {
            Role.Leader,
            Role.Rogue,
            Role.Paladin,
            Role.Gangster,
            Role.Vigilante,
            Role.Noble
        };
        public List<Action> actionDeck = new List<Action>
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
        public List<Artifact> artifactDeck = new List<Artifact>
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
        public List<Threat> threatDeck = new List<Threat>
        {
            Threat.AmbitionsDoom, Threat.ArtifactMaintenance,
            Threat.BattleGuard, Threat.CivilianUnrest,
            Threat.CrimesRevealed, Threat.DraconicDemands,
            Threat.LocalHeroes, Threat.NewKnives,
            Threat.NewSheriff, Threat.ProblematicMayor,
            Threat.ProblematicPolitician, Threat.ReligiousUnrest,
            Threat.RoyalDecree, Threat.SecretCaches, 
            Threat.ShowForce, Threat.StoppedFearing,
            Threat.TurfWar, Threat.WaningDominance,
            Threat.WarWatch, Threat.ZealEbbing
        };

            #endregion
        
        [SerializeField] private GameObject[] cardPrefabs = new GameObject[5];
        [SerializeField] private GameObject[] piecePrefabs = new GameObject[3];
        [SerializeField] private GameObject participantObject = null;
        
        public static GameMaster Instance;
        public PhotonView[] playerSlots;
        public PhotonView[] jobBoards = new PhotonView[5];
        public bool isTesting;
        public Dictionary<Role, int> playerRoles = new Dictionary<Role, int>();
        public Dictionary<Character, Participant> characterIndex = new Dictionary<Character, Participant>();
        public List<Piece> workerPieces = new List<Piece>();
        public byte turnCounter;
        public byte seatsClaimed;
        public PhotonView pv;
        public GameObject threatObject = null;
        public bool[] passedPlayers;
        public int[] roleRevealTurns = new int[7];

        #region Enums

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

        public enum Threat
        {
            NewKnives,
            ZealEbbing,
            LocalHeroes,
            ProblematicPolitician,
            BattleGuard,
            WaningDominance,
            AmbitionsDoom,
            ProblematicMayor,
            StoppedFearing,
            TurfWar,
            NewSheriff,
            ArtifactMaintenance,
            DraconicDemands,
            SecretCaches,
            RoyalDecree,
            ReligiousUnrest,
            WarWatch,
            CivilianUnrest,
            ShowForce,
            CrimesRevealed
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

        #endregion
        
        private void Start()
        {
            for (int i = 0; i < 6-PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                roleDeck.RemoveAt(roleDeck.Count-1);
            }
            pv = GetComponent<PhotonView>();
            Instance = this;
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(CommenceGame());
            }
            PhotonNetwork.Instantiate(participantObject.name, Vector3.zero, Quaternion.identity, 0);
            CursorFollower.Instance.active = true;
            passedPlayers = new bool[PhotonNetwork.CurrentRoom.PlayerCount];
        }

        IEnumerator CommenceGame()
        { // this is used to avoid network latency interfering. I wanted to use buffered RPC calls, but their order was not guaranteed, this is a bit ugly in my opinion
            yield return new WaitUntil(StartCondition);
            yield return new WaitForSeconds(1);
            Participant[] players = FindObjectsOfType<Participant>();
            foreach (var player in players)
            {
                byte roleNum = (byte) DrawCard(CardType.Role);
                int charNum = DrawCard(CardType.Character);
                player.pv.RPC("RpcAssignRoleAndChar", RpcTarget.AllBufferedViaServer, roleNum, charNum);
            }
        }

        private bool StartCondition()
        {
            return seatsClaimed == PhotonNetwork.CurrentRoom.PlayerCount;
        }
        
        private void Update()
        {
            if (artifactDeck.Count < 1)
            {
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
            }
            if (actionDeck.Count < 1)
            {
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
            }

            if (!passedPlayers.Contains(false))
            {
                for (var i = 0; i < passedPlayers.Length; i++)
                {
                    passedPlayers[i] = false;
                }
                EndTurn(false);
            }
        }

        public GameObject CreatePiece(PieceType type)
        { // used by other classes to create pieces
             GameObject piece = PhotonNetwork.Instantiate(piecePrefabs[(int) type].name, transform.position, Quaternion.identity);
             if (type == PieceType.Worker)
             {
                 workerPieces.Add(piece.GetComponent<Piece>());
             }
             return piece;
        }

        public int DrawCard(CardType type)
        {// used by other classes to draw cards from the central deck
            switch (type)
            {
                case CardType.Character:
                    int randomPick = Random.Range(0, characterDeck.Count);
                    Character returnValue = characterDeck[randomPick];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue);
                    return (int)returnValue;
                case CardType.Role:
                    int randomPick2 = Random.Range(0, roleDeck.Count);
                    Role returnValue2 = roleDeck[randomPick2];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue2);
                    return (int)returnValue2;
                case CardType.Artifact:
                    int randomPick3 = Random.Range(0, artifactDeck.Count);
                    Artifact returnValue3 = artifactDeck[randomPick3];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue3);
                    return (int)returnValue3;
                case CardType.Action:
                    int randomPick4 = Random.Range(0, actionDeck.Count);
                    Action returnValue4 = actionDeck[randomPick4];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue4);
                    return (int)returnValue4;
                case CardType.Threat:
                    int randomPick5 = Random.Range(0, threatDeck.Count);
                    Threat returnValue5 = threatDeck[randomPick5];
                    pv.RPC("RemoveFromDeck", RpcTarget.All,(int)type, (int)returnValue5);
                    return (int)returnValue5;
                default: return -1;
            }
        }

        [PunRPC]
        public void RemoveFromDeck(int cardType, int index)
        { // used to synchronize the deck on all clients
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
                            Debug.LogAssertion("removed role " + target2);
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
                case CardType.Threat:
                    Threat target5 = (Threat) index;
                    for (int i = 0; i < threatDeck.Count; i++)
                    {
                        if (threatDeck[i] == target5)
                        {
                            threatDeck.RemoveAt(i);
                            break;
                        }
                    }
                    break;
            }
            
        }

        public GameObject ConstructCard(CardType type, int enumIndex)
        { // used by other classes to create card instances, this could be moved somewhere else maybe
            Decklist dL = Decklist.Instance;
            GameObject card = null;
            switch (type)
            {
                case CardType.Action:
                    card = Instantiate(cardPrefabs[(int) type]);
                    Card parts = card.GetComponent<Card>();
                    if (dL.actionCards.TryGetValue((Action) enumIndex, out ActionCard thisCard))
                    {
                        parts.cardType = type;
                        parts.illustration.sprite = thisCard.illustration;
                        parts.cardName.text = thisCard.name;
                        parts.text.text = thisCard.effectText;
                        parts.cardIndex = enumIndex;
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
                        partsA.cardType = type;
                        partsA.illustration.sprite = thisACard.illustration;
                        partsA.cardName.text = thisACard.name;
                        partsA.text.text = thisACard.effectText;
                        partsA.extraText1.text = "Strength: " + thisACard.weaponStrength;
                        partsA.cardIndex = enumIndex;
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
                        partsC.cardType = type;
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
                        partsR.cardType = type;
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
                
                case CardType.Threat:
                    card = Instantiate(cardPrefabs[(int) type]);
                    Card partsT = card.GetComponent<Card>();
                    if (dL.threatCards.TryGetValue((Threat) enumIndex, out ThreatCard thisTCard))
                    {
                        partsT.cardType = type;
                        partsT.illustration.sprite = thisTCard.illustration;
                        partsT.cardName.text = thisTCard.name;
                        partsT.text.text = thisTCard.requirementText;
                    }
                    else
                    {
                        Debug.LogWarning("PANIC");
                    }
                    break;
            }
            return card;
        }
        
        public void EndTurn(bool isFirst)
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                FetchPlayerByNumber(i).pv.RPC("RpcEndTurn", RpcTarget.All, isFirst);
            }
        }

        #region Fetches
        // all of these are helper functions for other classes to access the correct participants. this happens very often since I am using their index number when sending data via RPC
        public Participant FetchPlayerByNumber(int playerNumber)
        {
            return playerSlots[playerNumber].gameObject.GetComponent<PlayerSlot>().player;
        }

        public Participant FetchPlayerByPlayer(Player player)
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
            if (jobBoards[(int) job] != null)
            {
                Player player = jobBoards[(int) job].Controller;
                return FetchPlayerByPlayer(player);
            }
            else
            {
                return FetchLeader();
            }
        }

        public Participant FetchLeader()
        {
            playerRoles.TryGetValue(Role.Leader, out int playerNumber);
            return FetchPlayerByNumber(playerNumber);
        }
    }

    #endregion
    
}
