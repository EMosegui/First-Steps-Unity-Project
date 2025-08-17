using Assets;
using UnityEngine;
using UnityEngine.AI;

namespace FirstSteps
{
    public class PlayerDetector : MonoBehaviour
    {
        [SerializeField] float detectionAngle = 60f;
        [SerializeField] float detectionRadius = 10f;
        [SerializeField] float innerDetectionRadius = 5f;
        [SerializeField] float detectionCooldown = 1f;
        [SerializeField] float attackRange = 2f;
        
        public Transform Player { get; private set; }
        public Health PlayerHealth { get; private set; }
        
        CountdownTimer detectionTimer;
        
        IDetectionStrategy  detectionStrategy;

        void Awake()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            PlayerHealth = Player.GetComponent<Health>();
        }

        void Start()
        {
            detectionTimer = new CountdownTimer(detectionCooldown);
            detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
        }
        
        void Update() => detectionTimer.Tick(Time.deltaTime);

        public bool CanDetectPlayer()
        {
            return detectionTimer.IsRunning || detectionStrategy.Execute(Player, detector:transform, detectionTimer);
        }

        public bool CanAttackPlayer()
        {
            var directionToPLayer = Player.position - transform.position;
            return directionToPLayer.magnitude <= attackRange;
        }

        public void SetDetectionStrategy(IDetectionStrategy detectionStrategy) => this.detectionStrategy = detectionStrategy;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.DrawWireSphere(transform.position, innerDetectionRadius);
            
            Vector3 forwardConeDirection = Quaternion.Euler(x: 0, y:detectionAngle / 2,  z: 0) * transform.forward * detectionRadius;
            Vector3 backwardConeDirection = Quaternion.Euler(x:0, y:detectionAngle / 2, z:0) * transform.forward * detectionRadius;
            
            Gizmos.DrawLine(transform.position, transform.position + forwardConeDirection);
            Gizmos.DrawLine(transform.position, transform.position + backwardConeDirection);
        }
    }
}