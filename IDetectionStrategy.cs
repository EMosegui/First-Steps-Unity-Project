using Assets;
using UnityEngine;

namespace FirstSteps
{
    public interface IDetectionStrategy
    {
        bool Execute(Transform player, Transform detector, CountdownTimer timer);
    }
}