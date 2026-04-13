using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Drag the Boss's projectile prefab here
    public Transform firePoint;         // A child empty object at the gun tip
    public float projectileSpeed = 25f;
    public float damage = 20f;
    public float fireRate = 0.5f;

    [Header("Aiming Settings")]
    public float maxRange = 50f;
    public LayerMask hitMask;           // What can the bullet target?

    private float nextFireTime;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // 1. Calculate the target point (center of screen)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(maxRange); // If no hit, aim far away
        }

        // 2. Spawn the projectile
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // 3. Aim the projectile at the targetPoint
        bullet.transform.LookAt(targetPoint);

        // 4. Set the damage and speed (Reusing the Boss's script component)
        Projectile projScript = bullet.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.damage = damage;
            projScript.speed = projectileSpeed;
            projScript.firedBy = ProjectileSource.Player;
        }

        Debug.Log("<color=cyan>Player: Projectile Fired towards crosshair!</color>");
    }
}