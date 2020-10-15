using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Card : MonoBehaviour
    {
        public TextMeshProUGUI cardName;
        public TextMeshProUGUI text;
        public TextMeshProUGUI extraText1;
        public TextMeshProUGUI extraText2;
        public Image illustration;
        public Image icon;
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
