using System;
using UnityEngine;

namespace Gameplay
{
    public class CardHover : MonoBehaviour
    {
        [SerializeField] private Transform stepParent;
        [SerializeField] private float hoverTimer = 2f;
        
        private Card parentCard;
        private float hoverTime;
        private Rigidbody stepBody;
    
        // Start is called before the first frame update
        void Start()
        {
            parentCard = GetComponentInParent<Card>();
            stepBody = stepParent.GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        { // this is what I call a step parent transform. this thing follows it, but is not disabled with it as a child would be
            if (!parentCard.showing && !stepBody.IsSleeping())
            {
                var trans = transform;
                trans.position = stepParent.position;
                trans.rotation = stepParent.rotation;
            }
        }

        private void OnTriggerEnter(Collider other)
        { // this and the following 2 methods are all basically just trigger behaviour for the card (as is this entire class),
          // I was too stupid to figure out how to have a separate trigger otherwise so I split it off to here
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
