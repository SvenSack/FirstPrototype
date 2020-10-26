using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private GameObject highlighter;
        [SerializeField] private Rigidbody cardBody;
        
        public int cardIndex;
        public GameMaster.CardType cardType;
        public bool isPrivate = true;
        public PhotonView pv;
        public TextMeshProUGUI cardName;
        public TextMeshProUGUI text;
        public TextMeshProUGUI extraText1;
        public TextMeshProUGUI extraText2;
        public Image illustration;
        public Image icon;
        public BoxCollider cardCollider;
        public bool showing { get; private set; }
        public Transform hoverLocation { set; private get; }
        public Threat threat;
        
        private Transform cardTransform;
        private Vector3 originPosition = Vector3.zero;
        private Quaternion originRotation = Quaternion.identity;

        private void Start()
        {
            pv = GetComponent<PhotonView>();
            cardTransform = cardBody.transform;
            cardCollider = cardBody.GetComponent<BoxCollider>();
            highlighter.SetActive(false);
        }

        public void ToggleSelector(bool doubleCheck)
        { // used by the UImanager to select cards
            highlighter.SetActive(!highlighter.activeSelf);
            if (doubleCheck && highlighter.activeSelf)
            {
                highlighter.SetActive(false);
            }
            if (UIManager.Instance.isSelectingACard)
            {
                if (cardType == GameMaster.CardType.Action || cardType == GameMaster.CardType.Artifact)
                {
                    CursorFollower.Instance.hoveredCard = this;
                    CursorFollower.Instance.isHoveringACard = highlighter.activeSelf;
                }
            }
            else
            {
                
                if (cardType == GameMaster.CardType.Threat)
                {
                    CursorFollower.Instance.hoveredCard = this;
                    CursorFollower.Instance.isHoveringTCard = highlighter.activeSelf;
                }
                else
                {
                
                    if (cardType == GameMaster.CardType.Role)
                    {
                        CursorFollower.Instance.hoveredCard = this;
                        CursorFollower.Instance.isHoveringRCard = highlighter.activeSelf;
                    }
                }
            }
        }

        public void ToggleShowCard()
        { // used to show cards for closer inspection
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
