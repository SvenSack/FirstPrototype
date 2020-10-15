using UnityEngine;

namespace Gameplay
{
    public class CardHover : MonoBehaviour
    {
        private Card parentCard;
        [SerializeField] private Transform stepParent;
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
            if (other.CompareTag("CursorFollower") && !parentCard.showing)
            {
                parentCard.ToggleShowCard();
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (parentCard.showing)
            {
                parentCard.ToggleShowCard();
            }
        }
    }
}
