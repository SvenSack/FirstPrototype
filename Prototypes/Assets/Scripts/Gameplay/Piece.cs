using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Gameplay
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private ParticleSystem poisonParticles;
        public bool isUsed { get; private set; }
        public bool isPickedUp;
        public bool isPrivate = true;
        public PhotonView pv;
        public Rigidbody rb;
        public Camera cam;
        public Board originBoard;
        public GameMaster.PieceType type;
        public bool poisoned { private set; get; }
        
        private LayerMask tableMask;
        private int pieceLayer;
    
        void Start()
        {
            pv = GetComponent<PhotonView>();
            rb = GetComponent<Rigidbody>();
            tableMask = LayerMask.GetMask("Table");
            pieceLayer = LayerMask.NameToLayer("Pieces");
            if (poisonParticles != null)
            {
                poisonParticles.Pause();
            }
        }

        void Update()
        {
            
            if (isPickedUp)
            {
                if (Input.GetMouseButton(0))
                {
                    if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit tableHit, 100f, tableMask))
                    {
                        Vector3 cursorTarget = tableHit.point + Vector3.up*.5f;
                        transform.position = Vector3.Lerp(transform.position, cursorTarget, .5f);
                    }
                    else
                    {
                        Debug.Log("dropped off table");
                        ToggleSelfPickup();
                    }
                }
                else
                {
                    Debug.Log("stopped holding");
                    ToggleSelfPickup();
                    UIManager.Instance.defaultUI.SetActive(true);
                } 
            }
            
        }

        private void ToggleSelfPickup()
        {
            rb.isKinematic = !rb.isKinematic;
            isPickedUp = !isPickedUp;
            UIManager.Instance.isGrabbingPiece = !UIManager.Instance.isGrabbingPiece;
        }

        public void ToggleUse()
        {
            if (!isUsed)
            {
                isUsed = true;
                gameObject.layer = 0;
            }
            else
            {
                ResetPiecePosition();
                isUsed = false;
                gameObject.layer = pieceLayer;
            }
        }

        public bool TryPickup(Player owner)
        {
            if (Equals(owner, pv.Controller))
            {
                ToggleSelfPickup();
                return true;
            }

            return false;
        }
        
        public void ResetPiecePosition()
        {
            if (isPrivate)
            {
                transform.position = UIManager.Instance.participant.mySlot.pieceLocation.position + Vector3.up * .3f;
            }
            else
            {
                transform.position = originBoard.pieceLocation.position + Vector3.up * .3f;
            }
        }

        [PunRPC]
        public void ActivatePoison()
        {
            poisoned = true;
            poisonParticles.Play();
            GetComponent<ThreatPiece>().damageValue = 5;
        }
        
        [PunRPC]
        public void DestroyThis()
        {
            if (pv.IsMine)
            {
                PhotonNetwork.Destroy(pv);
            }
        }
    }
}
