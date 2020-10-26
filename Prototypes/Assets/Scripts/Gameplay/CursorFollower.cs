using UnityEngine;

namespace Gameplay
{
    public class CursorFollower : MonoBehaviour
    {
        public static CursorFollower Instance;
        public bool IsHovering { get; private set; }
        public Camera playerCam;
        public bool isHoveringACard;
        public bool isHoveringTCard;
        public bool isHoveringRCard;
        public Card hoveredCard;
        public bool active;
    
        private LayerMask tableMask;
        
        void Start()
        {
            tableMask = LayerMask.GetMask("Table");
            Instance = this;
        }

        void Update()
        {
            if (active)
            {
                AdjustPosition();
            }
        }

        private void AdjustPosition()
        { // this method is just the update functionality that lets it follow the cursor as projected onto the table surface, this object is then used for trigger collisions to get
          // worldspace UI interactions such as with the cards and hovers
            if (Physics.Raycast(playerCam.ScreenPointToRay(Input.mousePosition), out RaycastHit tableHit, 100f, tableMask))
            {
                Vector3 cursorTarget = tableHit.point;
                transform.position = Vector3.Lerp(transform.position, cursorTarget, .5f);
            }
            else
            {
                if (IsHovering)
                {
                    IsHovering = false;
                }
            }
        }

        public void ToggleHover()
        {
            IsHovering = !IsHovering;
        }
    }
}
