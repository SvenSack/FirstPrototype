using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Card : MonoBehaviour
    {
        public GameMaster.CardType cardType;
        public bool isPrivate = true;
        public TextMeshProUGUI cardName;
        public TextMeshProUGUI text;
        public TextMeshProUGUI extraText1;
        public TextMeshProUGUI extraText2;
        public Image illustration;
        public Image icon;
        [SerializeField] private GameObject highlighter;
        [SerializeField] private Rigidbody cardBody;
        private Transform cardTransform;
        private BoxCollider cardCollider;
        public bool showing { get; private set; }
        private Vector3 originPosition = Vector3.zero;
        private Quaternion originRotation = Quaternion.identity;
        public Transform hoverLocation { set; private get; }

        private void Start()
        {
            cardTransform = cardBody.transform;
            cardCollider = cardBody.GetComponent<BoxCollider>();
            highlighter.SetActive(false);
        }

        public void ToggleSelector(bool doubleCheck)
        {
            highlighter.SetActive(!highlighter.activeSelf);
            if (doubleCheck && highlighter.activeSelf)
            {
                highlighter.SetActive(false);
            }
            if (UIManager.Instance.isSelectingACard)
            {
                if (cardType == GameMaster.CardType.Action || cardType == GameMaster.CardType.Artifact)
                {
                    CursorFollower.Instance.hoveredACard = this;
                    CursorFollower.Instance.isHoveringACard = highlighter.activeSelf;
                }
            }
        }

        public void ToggleShowCard()
        {
            switch (showing)
            {
                case false:
                    cardBody.isKinematic = true;
                    cardCollider.enabled = false;
                    originPosition = cardTransform.position;
                    originRotation = cardTransform.rotation;
                    cardTransform.LeanRotate(hoverLocation.rotation.eulerAngles, .5f);
                    cardTransform.LeanMove(hoverLocation.position, .5f);
                    showing = true;
                    break;
                case true:
                    cardBody.isKinematic = false;
                    cardCollider.enabled = true;
                    LeanTween.cancel(cardTransform.gameObject);
                    cardTransform.LeanRotate(originRotation.eulerAngles, .5f);
                    cardTransform.LeanMove(originPosition, .5f);
                    showing = false;
                    break;
            }
        }
    }
}
