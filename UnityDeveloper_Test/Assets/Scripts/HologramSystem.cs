using UnityEngine;

public class HologramSystem : MonoBehaviour
{
    [SerializeField] private GameObject playerHologram;

    [Header("Settings")]
    [SerializeField] private float offsetDistance = 2.0f;

    private void Start()
    {
        playerHologram.SetActive(false);
    }

    public void ShowHologram(Vector3 playerPosition, Vector3 offsetDirection, Vector3 hologramUp, Vector3 hologramForward)
    {
        //Moving the hologram out towards the wall
        playerHologram.transform.position = playerPosition + (offsetDirection * offsetDistance);

        //Applying the rotation
        playerHologram.transform.rotation = Quaternion.LookRotation(hologramForward, hologramUp);

        playerHologram.SetActive(true);
    }

    public void HideHologram()
    {
        playerHologram.SetActive(false);
    }
}
