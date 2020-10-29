using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class TradeAssignmentToggle : MonoBehaviour
    {
        [SerializeField] private GameObject textObject;
        [SerializeField] private GameObject poisonedIndicator;
        [SerializeField] private Sprite[] images = new Sprite[8];
        [SerializeField] private Image representation;
        
        public bool isAssigned = false;
        public TradeAssignmentChoice assigner;
        public TradeGood type;
        public Piece representedPiece;
        public Card representedCard;
        public ThreatPiece threateningPiece;
        public InformationPiece infoPiece;
        public bool isHoverName;

        private void Start()
        {
            poisonedIndicator.SetActive(false);
            textObject.SetActive(false);
        }

        public enum TradeGood
        {
            Piece,
            Card,
            ThreateningPiece,
            Information
        }

        public void SetPiece(Piece target)
        {
            type = TradeGood.Piece;
            representedPiece = target;
            if (target.poisoned)
            {
                poisonedIndicator.SetActive(true);
            }

            representation.sprite = images[(int) target.type];
        }

        public void SetCard(Card target)
        {
            type = TradeGood.Card;
            representedCard = target;
            SetText(target.cardName.text);
            isHoverName = true;
            if (target.cardType == GameMaster.CardType.Artifact)
            {
                representation.sprite = images[3];
            }
            else
            {
                representation.sprite = images[4];
            }
        }

        public void SetThreatPiece(ThreatPiece target)
        {
            type = TradeGood.ThreateningPiece;
            threateningPiece = target;
            if (target.thisPiece.type == GameMaster.PieceType.Thug)
            {
                representation.sprite = images[5];
            }
            else
            {
                representation.sprite = images[6];
            }
        }

        public void SetInformation(InformationPiece target)
        {
            type = TradeGood.Information;
            infoPiece = target;
            SetText(target.header);
            isHoverName = true;
            representation.sprite = images[7];
        }
        
        public void ToggleAssignment()
        {
            assigner.SwitchAssignment(this);
            isAssigned = !isAssigned;
            
        }

        public void ToggleHover()
        {
            if (isHoverName)
            {
                textObject.SetActive(!textObject.activeSelf);
            }
        }

        private void SetText(string text)
        {
            textObject.SetActive(true);
            textObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
        }
    }
}
