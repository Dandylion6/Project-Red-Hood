using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("References")]
    [SerializeField] private Transform player = null;


    public Transform Player => player;
}
