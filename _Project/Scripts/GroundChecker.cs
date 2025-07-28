using UnityEngine;

namespace FirstSteps
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] float groundDistance = 0.08f;
        [SerializeField] LayerMask groundLayers;
        
        public bool IsGrounded { get; private set; }

        void Update()
        {
            IsGrounded = Physics.SphereCast(origin: transform.position, radius: groundDistance, direction: Vector3.down, out _, groundDistance, (int)groundLayers);
        }
    }
}