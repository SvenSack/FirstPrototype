using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class Decklist : MonoBehaviour
    {
    
        public static Decklist Instance;
        public Sprite[] characterSprites = new Sprite[10];
        public Sprite[] roleSprites = new Sprite[6];
        public Sprite[] actionSprites = new Sprite[10];
        public Sprite[] artifactSprites = new Sprite[10];
        public Sprite[] threatSprites = new Sprite[20];
        
        public Dictionary<GameMaster.Character, CharacterCard> characterCards;
        public Dictionary<GameMaster.Character, string> characterNames;
        public Dictionary<GameMaster.Role, RoleCard> roleCards;
        public Dictionary<GameMaster.Artifact, ArtifactCard> artifactCards;
        public Dictionary<GameMaster.Action, ActionCard> actionCards;
        public Dictionary<GameMaster.Threat, ThreatCard> threatCards;

        void Start()
        { // all this class is, is an outsourced location for the card lists and their respective properties
            Instance = this;
            characterCards = new Dictionary<GameMaster.Character, CharacterCard>
            {
                {GameMaster.Character.Adventurer, new CharacterCard("Ott, the Retired Adventurer", "At the start of the game, Ott draws 3 artifacts. Ott also never takes damage from Thugs.", characterSprites[0], 6, 3)},
                {GameMaster.Character.Necromancer, new CharacterCard("Aria, the Backalley Necromancer", "At the start of each turn, Aria gains an additional worker piece.", characterSprites[1], 4, 2)},
                {GameMaster.Character.Poisoner, new CharacterCard("Aden, the Poisoner", "At the start of each turn, Aden may reduce the health of a character of their choice by 1.", characterSprites[2], 4, 4)},
                {GameMaster.Character.Ruffian, new CharacterCard("Mary, the Ruffian", "At the end of the turn, Mary does not need to pay for up to 5 Thugs. Any additional Thugs require payment as usual.", characterSprites[3], 5, 2)},
                {GameMaster.Character.Scion, new CharacterCard("Adeline, the Noble Scion ", "At the start of each turn, Adeline adds 2 coins to her character pool.", characterSprites[4], 4, 6)},
                {GameMaster.Character.Seducer, new CharacterCard("Harkon, the Seducer", "At the end of each turn, Harkon may look behind the screen of any one player of his choice.", characterSprites[5], 4, 3)},
                {GameMaster.Character.Sheriff, new CharacterCard("John, the Corrupt Sheriff", "At the end of each turn, when Thugs are paid, John gets half of those coins. This does not apply to his own Thugs.", characterSprites[6], 5, 2)},
                {GameMaster.Character.BurglaryAce, new CharacterCard("Steven, the Burglary Ace", "Whenever Steven robs the Bank tile, he gets twice as much gold for himself and the Leader. ", characterSprites[7], 4, 3)},
                {GameMaster.Character.OldFox, new CharacterCard("Selene, the Old Fox", "At the start of any turn, where jobs would be assigned, Selene may pick one for herself before the Leader starts to assign. This applies even if she is Leader and does not count against the usual rules for Leaders picking jobs.", characterSprites[8], 5, 5)},
                {GameMaster.Character.PitFighter, new CharacterCard("Ruko, the Priced Pitfighter", "At the end of each turn, Ruko may add a Thug to his character pool. He does not have to pay for that Thug on that turn.", characterSprites[9], 5, 3)}
            };
            roleCards = new Dictionary<GameMaster.Role, RoleCard>
            {
                {GameMaster.Role.Leader, new RoleCard("The Leader", "You win if by the end of the game you are still the Leader. Reveal: gain1 extra HP; you must reveal at the start of the game. Revealed: You assign jobs each round. When you die, the character with the most threat potential gets this role", roleSprites[0], true)},
                {GameMaster.Role.Rogue, new RoleCard("The Greedy Rogue", "You win if by the end of the game you have 20 coins in your character pool. Revealed: Whenever you would get the Leader a benefit, you can keep it instead. ", roleSprites[1], true)},
                {GameMaster.Role.Paladin, new RoleCard("The Hidden Paladin", "You win if the thieves guild loses. Reveal: next turn, one more threat is drawn Revealed: the Leader must assign you at least 2 workers", roleSprites[2], false)},
                {GameMaster.Role.Gangster, new RoleCard("The Ambitious Gangster", "You can only win by becoming the Leader. Reveal: for the remainder of this turn, treat all your Thugs as Assassins Revealed: At the start of each turn, take a Thug for your character pool", roleSprites[3], true)},
                {GameMaster.Role.Vigilante, new RoleCard("The Vengeful Vigilante", "You win if all guild members are dead. If they lose through a threat, you still lose. Reveal: Increase the damage dealt by threatening pieces to a character of your choice by 2. Revealed: At the start of each turn, take an Assassin for your character pool, you no longer need to pay for Assassins at the end of the turn.", roleSprites[4], false)},
                {GameMaster.Role.Noble, new RoleCard("The Scheming Noble", "You win if by the end of the game no one has evidence against you and is still alive. Reveal: gain 10 coins.Revealed:  At the start of each turn, add 2 coins to your character pool", roleSprites[5], true)}
            };
            actionCards = new Dictionary<GameMaster.Action, ActionCard>
            {
                {GameMaster.Action.Improvise, new ActionCard("Improvise", "Reveal a total of 2 cards from either the artifact or action deck. Immediately use them or discard them.", actionSprites[0])},
                {GameMaster.Action.DoubleAgent, new ActionCard("Double Agent", "Move any one piece that threatens you into your character pool instead.", actionSprites[1])},
                {GameMaster.Action.SecretCache, new ActionCard("Secret Cache", "Take 2 artifacts.", actionSprites[2])},
                {GameMaster.Action.AskForFavours, new ActionCard("Ask for Favours", "The Leader must give you your choice of: - an artifact - any specific kind of piece - 2 coins - an action card", actionSprites[3])},
                {GameMaster.Action.CallInBackup, new ActionCard("Call in Backup", "Take 3 Thugs and add them to your character pool, you may immediately use them on this turn.", actionSprites[4])},
                {GameMaster.Action.ExecuteAHeist, new ActionCard("Execute a Heist", "You and any number of other characters of your choice each gain 3 coins.", actionSprites[5])},
                {GameMaster.Action.RunForOffice, new ActionCard("Run for Office", "Pay any amount of coins. Unless the Leader pays more at the end of this turn, you become Leader instead, they remain in the game without a role card.", actionSprites[6])},
                {GameMaster.Action.SwearTheOaths, new ActionCard("Swear the Oaths", "Take 2 Assassins and add them to your character pool. You may immediately use them on this turn.", actionSprites[7])},
                {GameMaster.Action.BribeTheTaxOfficer, new ActionCard("Bribe the Tax Officer", "All other characters must give you 2 coins. If they cannot, they must reveal their character pool to prove it.", actionSprites[8])},
                {GameMaster.Action.DealWithItYourself, new ActionCard("Deal with it Yourself", "Discard any one open threat card, and all things placed on it. You lose 1 HP.", actionSprites[9])}
            };
            artifactCards = new Dictionary<GameMaster.Artifact, ArtifactCard>
            {
                {GameMaster.Artifact.Ball, new ArtifactCard("Crystal Ball", "You may look behind the screen of any one player of your choice.", artifactSprites[0], 1)},
                {GameMaster.Artifact.Bauble, new ArtifactCard("Bauble of Shielding", "Instead of playing this card, you may discard it to negate the effect of any one artifact which targets you.", artifactSprites[1], 2)},
                {GameMaster.Artifact.Bow, new ArtifactCard("Bow of the Manhunter", "Any one character of your choice immediately discards up to 3 Assassins or Thugs.", artifactSprites[2], 4)},
                {GameMaster.Artifact.Dagger, new ArtifactCard("Ritual Dagger", "You may immediately take and use 2 Assassins for your character pool.", artifactSprites[3], 3)},
                {GameMaster.Artifact.Periapt, new ArtifactCard("Periapt of the Mindthief", "You may secretly look at the Role card of any one player of your choice.", artifactSprites[4], 2)},
                {GameMaster.Artifact.Potion, new ArtifactCard("Potion of Healing", "Restore up to 3 HP to a character of your choice", artifactSprites[5], 1)},
                {GameMaster.Artifact.Serum, new ArtifactCard("Serum of Truth", "A player of your choice must immediately answer a yes or no question truthfully.", artifactSprites[6], 1)},
                {GameMaster.Artifact.Scepter, new ArtifactCard("Scepter of Lightning", "A character of your choise immediately loses 3 health.", artifactSprites[7], 5)},
                {GameMaster.Artifact.Venom, new ArtifactCard("Mantikor Venom", "Play this artifact together with an Assassin. For the remainder of this turn, that Assassin has a threat value of 5.", actionSprites[8], 3)},
                {GameMaster.Artifact.Wand, new ArtifactCard("Wand of Fireballs", "Discard any number of pieces which threaten you.", artifactSprites[9], 5)}
            };
            characterNames = new Dictionary<GameMaster.Character, string>
            {
                {GameMaster.Character.Adventurer, "Ott"},
                {GameMaster.Character.Necromancer, "Aria"},
                {GameMaster.Character.Poisoner, "Aden"},
                {GameMaster.Character.Ruffian, "Mary"},
                {GameMaster.Character.Scion, "Adeline"},
                {GameMaster.Character.Seducer, "Harkon"},
                {GameMaster.Character.Sheriff, "John"},
                {GameMaster.Character.BurglaryAce, "Steven"},
                {GameMaster.Character.OldFox, "Selene"},
                {GameMaster.Character.PitFighter, "Ruko"}
            };
            threatCards = new Dictionary<GameMaster.Threat, ThreatCard>
            {
                {GameMaster.Threat.NewKnives, new ThreatCard("New Knives are in Order", threatSprites[0], new []{0,0,0,0,0,5})},
                {GameMaster.Threat.ZealEbbing, new ThreatCard("The Zeal is Ebbing up", threatSprites[1], new []{0,0,0,2,4,0})},
                {GameMaster.Threat.LocalHeroes, new ThreatCard("Local Heroes on the Trail", threatSprites[2], new []{0,2,2,0,0,0})},
                {GameMaster.Threat.ProblematicPolitician, new ThreatCard("Problematic Politician", threatSprites[3], new []{0,0,2,0,0,1})},
                {GameMaster.Threat.BattleGuard, new ThreatCard("Battle with the Guard", threatSprites[4], new []{2,1,0,0,0,3})},
                {GameMaster.Threat.WaningDominance, new ThreatCard("Market Dominance Waning", threatSprites[5], new []{0,0,0,0,10,2})},
                {GameMaster.Threat.AmbitionsDoom, new ThreatCard("Ambitions of Doom", threatSprites[6], new []{0,0,0,0,0,8})},
                {GameMaster.Threat.ProblematicMayor, new ThreatCard("Problematic Mayor", threatSprites[7], new []{2,0,2,1,0,3})},
                {GameMaster.Threat.StoppedFearing, new ThreatCard("The People Stopped Fearing us", threatSprites[8], new []{2,0,1,0,0,0})},
                {GameMaster.Threat.TurfWar, new ThreatCard("Turf War", threatSprites[9], new []{3,0,0,0,0,2})},
                {GameMaster.Threat.NewSheriff, new ThreatCard("A new Sheriff in Town", threatSprites[10], new []{1,0,0,0,4,0})},
                {GameMaster.Threat.ArtifactMaintenance, new ThreatCard("Artifact Maintenance Required", threatSprites[11], new []{2,0,0,0,15,0})},
                {GameMaster.Threat.DraconicDemands, new ThreatCard("Draconic Demands", threatSprites[12], new []{0,2,0,0,20,0})},
                {GameMaster.Threat.SecretCaches, new ThreatCard("Secret Caches Depleted", threatSprites[13], new []{0,0,0,0,8,0})},
                {GameMaster.Threat.RoyalDecree, new ThreatCard("Looming Royal Decree", threatSprites[14], new []{0,4,3,0,0,5})},
                {GameMaster.Threat.ReligiousUnrest, new ThreatCard("Religious Unrest Brewing", threatSprites[15], new []{0,0,4,0,0,1})},
                {GameMaster.Threat.WarWatch, new ThreatCard("War with the Watch", threatSprites[16], new []{6,3,0,0,0,5})},
                {GameMaster.Threat.CivilianUnrest, new ThreatCard("Mounting Civilian Unrest", threatSprites[17], new []{5,0,0,0,8,0})},
                {GameMaster.Threat.ShowForce, new ThreatCard("Public Show of Force needed", threatSprites[18], new []{0,0,1,0,0,6})},
                {GameMaster.Threat.CrimesRevealed, new ThreatCard("Crimes were Revealed", threatSprites[19], new []{0,1,0,0,0,0})},
            };
        }
    }

    public class CharacterCard
    {
        public readonly string name;
        public readonly string effectText;
        public readonly Sprite illustration;
        public readonly int health;
        public readonly int wealth;

        public CharacterCard(string _name, string _effectText, Sprite _illustration, int _health, int _wealth)
        {
            name = _name;
            effectText = _effectText;
            illustration = _illustration;
            health = _health;
            wealth = _wealth;
        }
    }

    public class RoleCard
    {
        public readonly string name;
        public readonly string effectText;
        public readonly Sprite illustration;
        public readonly bool isGuild;

        public RoleCard(string _name, string _effectText, Sprite _illustration, bool _isGuild)
        {
            name = _name;
            effectText = _effectText;
            illustration = _illustration;
            isGuild = _isGuild;
        }
    }

    public class ActionCard
    {
        public readonly string name;
        public readonly string effectText;
        public readonly Sprite illustration;

        public ActionCard(string _name, string _effectText, Sprite _illustration)
        {
            name = _name;
            effectText = _effectText;
            illustration = _illustration;
        }
    }

    public class ArtifactCard
    {
        public readonly string name;
        public readonly string effectText;
        public readonly Sprite illustration;
        public readonly int weaponStrength;

        public ArtifactCard(string _name, string _effectText, Sprite _illustration, int _weaponStrength)
        {
            name = _name;
            effectText = _effectText;
            illustration = _illustration;
            weaponStrength = _weaponStrength;
        }
    }
    
    public class ThreatCard
    {
        public readonly string name;
        public readonly string requirementText = "";
        public readonly Sprite illustration;
        public readonly int[] requirements = new int[6]; // thugs, thugs(D), assassins, assassins(D), coins, weaponstrength

        public ThreatCard(string _name, Sprite _illustration, int[] _requirements)
        {
            name = _name;
            illustration = _illustration;
            requirements = _requirements;
            if (requirements[0] != 0)
            {
                requirementText += requirements[0].ToString() + " Thugs, ";
            }
            if (requirements[1] != 0)
            {
                requirementText += requirements[1].ToString() + " Thugs(D), ";
            }
            if (requirements[2] != 0)
            {
                requirementText += requirements[2].ToString() + " Assassins, ";
            }
            if (requirements[3] != 0)
            {
                requirementText += requirements[3].ToString() + " Assassins(D), ";
            }
            if (requirements[4] != 0)
            {
                requirementText += requirements[4].ToString() + " Coins, ";
            }
            if (requirements[5] != 0)
            {
                requirementText += requirements[5].ToString() + " Weaponstrength, ";
            }

            requirementText = requirementText.Remove(requirementText.Length - 2);
        }
    }
}