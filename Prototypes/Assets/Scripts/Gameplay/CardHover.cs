using System;
using UnityEngine;

namespace Gameplay
{
    public class CardHover : MonoBehaviour
    {
        private Card parentCard;
        [SerializeField] private Transform stepParent;
        private float hoverTime;
        [SerializeField] private float hoverTimer = 2f;
        private Rigidbody stepBody;
    
        // Start is called before the first frame update
        void Start()
        {
            parentCard = GetComponentInParent<Card>();
            stepBody = stepParent.GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!parentCard.showing && !stepBody.IsSleeping())
            {
                var trans = transform;
                trans.position = stepParent.position;
                trans.rotation = stepParent.rotation;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("CursorFollower"))
            {
                parentCard.ToggleSelector(false);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("CursorFollower") && !parentCard.showing)
            {
                hoverTime += Time.deltaTime;
                if (hoverTime >= hoverTimer)
                {
                    parentCard.ToggleShowCard();
                    parentCard.ToggleSelector(true);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (parentCard.showing)
            {
                parentCard.ToggleShowCard();
            }
            else
            {
                parentCard.ToggleSelector(true);
            }
            hoverTime = 0;
        }
    }
}
