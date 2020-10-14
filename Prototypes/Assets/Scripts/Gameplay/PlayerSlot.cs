﻿using TMPro;
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
        public TextMeshProUGUI coinCounter;
        public Tile[] publicTiles = new Tile[5];

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
