using UnityEngine;

namespace FirstSteps
{
    public interface IEntityFactory<T> where T : Entity
    {
        T Create(Transform spawnPoint);
    }
}