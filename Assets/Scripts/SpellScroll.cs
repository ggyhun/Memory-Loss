using UnityEngine;

public class SpellScroll : MonoBehaviour
{
    public SpellData spellData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.LearnSpell(spellData);
            Destroy(gameObject);
        }
    }
}