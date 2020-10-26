using System;
using UnityEngine;

namespace Gameplay
{
    public class AssignmentToggle : MonoBehaviour
    {
        [SerializeField] private GameObject jobMarker;
        [SerializeField] private GameObject poisonMarker;
        
        public bool isAssigned = false;
        public bool isPrivate;
        public bool isPoisoned;
        public AssignmentChoice assigner;
        public Piece representative;

        private void Start()
        {
            if (isPrivate)
            {
                jobMarker.SetActive(false);
            }

            if (!isPoisoned)
            {
                poisonMarker.SetActive(false);
            }
        }

        public void ToggleAssignment()
        {
            assigner.SwitchAssignment(this);
            isAssigned = !isAssigned;
        }
    }
}
