using UnityEngine;

/// <summary>
/// Seprate system to show and hide hologram when arrow button clicked and released.
/// To make gravity change work you have to press Enter while selecting the arrow key to change direction,
/// as per the assignment
/// </summary>

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
