using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : Actor
{
    [Header("Door")]
    [SerializeField] private GameObject endText;

    public override void Die()
    {
        ExplodeSprite();

        StopAllCoroutines();
        SpriteRenderer.enabled = false;

        endText.SetActive(true);
        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
