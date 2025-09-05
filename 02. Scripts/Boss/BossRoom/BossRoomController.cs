using System.Collections;
using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    [Header("Room Setup")]
    [SerializeField] private Collider2D entranceTrigger;
    [SerializeField] private GameObject door;
    [SerializeField] private BossStatHandler _bossStat;

    [Header("Door Setting")]
    [SerializeField] private float doorCloseDelay = 0.2f;

    private bool isClosed = false;

    private void Awake()
    {
        //  문 비활성화
        door.SetActive(false);
    }

    private void OnEnable()
    {
        //  보스 사망 이벤트 구독
        _bossStat.OnDeath += OpenDoor;
    }

    private void OnDisable()
    {
        _bossStat.OnDeath -= OpenDoor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isClosed)
        {
            return;
        }

        if (!collision.TryGetComponent<PlayerController>(out var player))
        {
            return;
        }

        StartCoroutine(CloseDoorWithDelay());
    }

    private IEnumerator CloseDoorWithDelay()
    {
        yield return new WaitForSeconds(doorCloseDelay);

        CloseDoor();
    }

    private void OpenDoor()
    {
        door.SetActive(false);

        enabled = false;
    }

    private void CloseDoor()
    {
        door.SetActive(true);
        isClosed = true;
    }
}
