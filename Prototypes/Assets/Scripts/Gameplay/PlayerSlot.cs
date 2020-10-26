using TMPro;
using UnityEngine;
using WebSocketSharp;

namespace Gameplay
{
    public class PlayerSlot : MonoBehaviour
    { 
        public Camera perspective;
        public GameObject Board;
        public Participant player;
        public string playerCharacterName;
        public Transform coinLocation;
        public Transform healthLocation;
        public Transform rCCardLocation;
        public Transform aACardLocation;
        public Transform pieceLocation;
        public Transform threatLocation;
        public TextMeshProUGUI coinCounter;
        public Tile[] publicTiles = new Tile[5];
        public Transform hoverLocation;
        public Transform[] jobLocations = new Transform[5];
        public Transform threateningPiecesLocation;

        // this class, similar to the decklist is just a place to put values, in this case the data for a new participant to interface with the gameworld (mostly locations of stuff)
        
        private void Awake()
        {
            perspective.enabled = false;
            Board.SetActive(false);
        }

        private void Update()
        {
            if(playerCharacterName.IsNullOrEmpty())
            {
                if (player != null)
                {
                    if (Decklist.Instance.characterCards.TryGetValue(player.character, out var tempOut))
                    {
                        playerCharacterName = tempOut.name;
                    }
                }
                
            }
        }
    }
}
