using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa uma árvore que pode ser cortada pelo jogador
/// </summary>
public class Tree : MonoBehaviour
{
    [SerializeField] private float treeHealth;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private int totalWood;
    [SerializeField] private ParticleSystem leafs;

    private bool isCut;
    /// <summary>
    /// Processa o dano na árvore e spawna madeira quando destruída
    /// </summary>
    private void OnHit()
    {
        treeHealth--;

        anim.SetTrigger("isHit");
        leafs.Play();

        if (treeHealth <= 0)
        {
            // Cria e instancia os drops
            for (int i = 0; i < totalWood; i++)
            {
                Instantiate(
                    woodPrefab,
                    transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f),
                    transform.rotation
                );
            }

            anim.SetTrigger("cut");
            isCut = true;
        }
    }

    /// <summary>
    /// Detecta colisão com machado e processa o corte
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Axe") && !isCut)
        {
            OnHit();
        }
    }
}
