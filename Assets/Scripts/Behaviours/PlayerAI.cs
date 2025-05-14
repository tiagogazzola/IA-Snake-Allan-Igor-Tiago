using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AIBehaviours/PlayerAI")]
public class PlayerAI : AIBehaviour
{
    public string tagAlvo = "Orb";
    public float raioDeteccao = 1000000f;
    public float speed = 500f;
    public float speedNormal = 50f;

    public Vector2 minLimite = new Vector2(-50f, -50f);
    public Vector2 maxLimite = new Vector2(50f, 50f);
    //public LayerMask layerAlvo;

    private Transform alvoMaisProximo;

    public override void Init(GameObject own, SnakeMovement ownMove)
    {
        base.Init(own, ownMove);
        ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(timeChangeDir));
    }

    public override void Execute()
    {
        alvoMaisProximo = EncontrarMaisProximoNoRaio();

        if (alvoMaisProximo != null)
        {
            ownerMovement.speed = speed;
            Vector2 novaPosicao = Vector2.MoveTowards(
                owner.transform.position,
                alvoMaisProximo.position,
                speed * Time.deltaTime
            );
            novaPosicao = LimitarDentroDosLimites(novaPosicao);
            owner.transform.position = new Vector3(novaPosicao.x, novaPosicao.y, owner.transform.position.z);
        }
        else
        {
            ownerMovement.speed = speedNormal;
            MoveForward();
        }
    }

    void MoveForward()
    {
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, rotation, ownerMovement.speed * Time.deltaTime);

        Vector2 novaPosicao = Vector2.MoveTowards(owner.transform.position, randomPoint, ownerMovement.speed * Time.deltaTime);
        novaPosicao = LimitarDentroDosLimites(novaPosicao);
        owner.transform.position = new Vector3(novaPosicao.x, novaPosicao.y, owner.transform.position.z);
    }

    IEnumerator UpdateDirEveryXSeconds(float x)
    {
        yield return new WaitForSeconds(x);
        ownerMovement.StopCoroutine(UpdateDirEveryXSeconds(x));
        randomPoint = new Vector3(
                Random.Range(
                    Random.Range(owner.transform.position.x - 10, owner.transform.position.x - 5),
                    Random.Range(owner.transform.position.x + 5, owner.transform.position.x + 10)
                ),
                Random.Range(
                    Random.Range(owner.transform.position.y - 10, owner.transform.position.y - 5),
                    Random.Range(owner.transform.position.y + 5, owner.transform.position.y + 10)
                ),
                0
            );
        direction = randomPoint - owner.transform.position;
        direction.z = 0.0f;

        ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(x));
    }

    //Procurar pelos orbs

    Transform EncontrarMaisProximoNoRaio()
    {
        Collider2D[] coliders = Physics2D.OverlapCircleAll(owner.transform.position, raioDeteccao);
        Transform maisProximo = null;
        float menorDistancia = Mathf.Infinity;

        foreach (Collider2D col in coliders)
        {
            if (col.CompareTag(tagAlvo))
            {
                float distancia = Vector2.Distance(owner.transform.position, col.transform.position);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    maisProximo = col.transform;
                }
            }
        }

        return maisProximo;
    }

    Vector2 LimitarDentroDosLimites(Vector2 pos)
    {
        float x = Mathf.Clamp(pos.x, minLimite.x, maxLimite.x);
        float y = Mathf.Clamp(pos.y, minLimite.y, maxLimite.y);
        return new Vector2(x, y);
    }
}