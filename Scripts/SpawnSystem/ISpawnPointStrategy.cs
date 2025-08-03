using UnityEngine;

namespace FirstSteps
{
    public interface ISpawnPointStrategy
    {
        Transform NextSpawnPoint();
    }
}