using System;
using UnityEngine;

namespace Gameplay
{
    public class AssignmentToggle : MonoBehaviour
    {
        [SerializeField] private GameObject jobMarker;
        
        public bool isAssigned = false;
        public bool isPrivate;
        public AssignmentChoice assigner;
        public Piece representative;

        private void Start()
        {
            if (isPrivate)
            {
                jobMarker.SetActive(false);
            }
        }

        public void ToggleAssignment()
        {
            assigner.SwitchAssignment(this);
            isAssigned = !isAssigned;
        }
    }
}
