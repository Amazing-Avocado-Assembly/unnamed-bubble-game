using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Player playerPrefab;
    [SerializeField] private CinemachineCamera playerFollowCamera;
    [SerializeField] private Cog cogPrefab;
    public Cog cog;
    public Player Player { get; private set; }

    public void Respawn()
    {
        Kill().onComplete += () =>
        {
            // Spawn a new cog
            Spawn();
        };
    }

    public Tween Kill()
    {
        // Unparent the cog
        cog.transform.SetParent(null);
        // Kill the player
        Destroy(Player.gameObject);
        // Tween the cog down from the screen
        Tween t = cog.transform.DOMoveY(-10, 1.0f).SetEase(Ease.InQuad);
        t.onComplete += () =>
        {
            // Destroy the cog
            Destroy(cog.gameObject);
        };
        return t;
    }

    public void Spawn()
    {
        // Instantiate Player at the spawn point
        Player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        if (playerFollowCamera != null)
        {
            playerFollowCamera.Follow = Player.transform;
        }
        Player.RB.bodyType = RigidbodyType2D.Static;

        Bubble bubble = Player.GetComponentInChildren<Bubble>();
        bubble.volume = 0;

        // Instantiate Cog above the screen and spawn point
        cog = Instantiate(cogPrefab, spawnPoint.position + new Vector3(0, 10, 0), Quaternion.identity);
        // Tween the cog down to the spawn point
        cog.transform.DOMoveY(spawnPoint.position.y, 1.0f).SetEase(Ease.InQuad).onComplete += () =>
        {
            // Set cog's state to START
            cog.state = Cog.State.START;

            // Set parent to RespawnPoint
            cog.transform.SetParent(transform);

            Tween t = bubble.DOVolume(bubble.initialVolume, 2.0f);
            t.SetDelay(2f);
            t.onComplete += () =>
            {
                Player.RB.bodyType = RigidbodyType2D.Dynamic;
                // Add up velocity to the player
                Player.RB.linearVelocity = new Vector2(0, 1);
                // Set cog's parent to the player
                cog.transform.SetParent(Player.transform);
                cog.state = Cog.State.BUBBLE;
            };
        };

        // Get all Particle Systems in the scene and add my Sprite child as a Trigger transform
        ParticleSystem[] particleSystems = Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.trigger.AddCollider(Player.GetComponentInChildren<CircleCollider2D>().transform);
        }
    }
}
