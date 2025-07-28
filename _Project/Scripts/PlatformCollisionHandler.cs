using UnityEngine;

namespace FirstSteps
{
    public class PlatformCollisionHandler : MonoBehaviour
    {
        Transform platform;

        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("MovingPlatform"))
            {
                ContactPoint contact = other.GetContact(index: 0);
                if (contact.normal.y < 0.5f) return;
                platform = other.transform;
                transform.SetParent(platform);
            }
        }

        void OnCollisionExit(Collision other)
        {
            if (other.gameObject.CompareTag("MovingPlatform"))
            {
                transform.SetParent(p:null);
                platform = null;
            }
        }
    }
}