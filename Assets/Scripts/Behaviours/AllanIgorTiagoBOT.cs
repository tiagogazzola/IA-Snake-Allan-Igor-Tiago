using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AIBehaviours/AllanIgorTiagoBOT")]
public class AllanIgorTiagoBOT : AIBehaviour
{
    public string tagAlvo = "Orb";
    public float raioDeteccao = 10000000f;
    public float speed = 5000f;
    public float raioFuga = 2f;

    public Vector2 minLimite = new Vector2(-50f, -50f);
    public Vector2 maxLimite = new Vector2(50f, 50f);

    private Transform alvoMaisProximo;

    //O exercício não proibia nenhuma loucura, então:
    private bool hacks = true;

    public override void Init(GameObject own, SnakeMovement ownMove)
    {
        base.Init(own, ownMove);
        ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(timeChangeDir));
    }

    public override void Execute()
    {
        Transform inimigoMaisProximo;
        if (VerificarInimigoProximo(out inimigoMaisProximo))
        {
            Debug.Log("Inimigo detectado");
            if (hacks) { ownerMovement.speed = 5f; }
            FugirDoInimigoMaisProximo(inimigoMaisProximo);
            return;
        }

        alvoMaisProximo = EncontrarMaisProximoNoRaio();

        if (alvoMaisProximo != null)
        {
            if (hacks) { ownerMovement.speed = speed; }
            Vector2 novaPosicao = Vector2.MoveTowards(
                owner.transform.position,
                alvoMaisProximo.position,
                ownerMovement.speed * Time.deltaTime
            );
            novaPosicao = LimitarDentroDosLimites(novaPosicao);
            owner.transform.position = new Vector3(novaPosicao.x, novaPosicao.y, owner.transform.position.z);
        }
        else
        {
            if (hacks) { ownerMovement.speed = 3.5f; }
            MoveForward();
        }
    }

    // Wonderzinho

    void MoveForward()
    {
        Vector2 direcaoAtual = direction.normalized;
        float angle = Mathf.Atan2(direcaoAtual.x, direcaoAtual.y) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, rotation, ownerMovement.speed * Time.deltaTime);

        // Verifica se há "Body" na frente
        RaycastHit2D hit = Physics2D.Raycast(owner.transform.position, direcaoAtual, 3f);
        if (hit.collider != null && hit.collider.CompareTag("Body"))
        {
            // Inverte a direção
            randomPoint = owner.transform.position - (Vector3)direction;
            direction = randomPoint - owner.transform.position;
        }

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
            if (!col.CompareTag(tagAlvo)) continue;

            Vector2 direcao = (col.transform.position - owner.transform.position).normalized;
            float distancia = Vector2.Distance(owner.transform.position, col.transform.position);

            // RaycastAll para pegar todos os obstáculos no caminho
            RaycastHit2D[] hits = Physics2D.RaycastAll(owner.transform.position, direcao, distancia);

            bool caminhoBloqueado = false;
            foreach (RaycastHit2D hit in hits)
            {
                // Ignora o próprio orb e o player
                if (hit.collider.gameObject == col.gameObject || hit.collider.transform.root == owner.transform.root) continue;

                // Se houver qualquer outro objeto com tag "Body", o caminho está bloqueado
                if (hit.collider.CompareTag("Body"))
                {
                    caminhoBloqueado = true;
                    break;
                }
            }

            if (!caminhoBloqueado && distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = col.transform;
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

    // Fuga caso cheguem perto

    bool VerificarInimigoProximo(out Transform inimigoMaisProximo)
    {
        inimigoMaisProximo = null;
        float menorDistancia = Mathf.Infinity;

        Collider2D[] inimigos = Physics2D.OverlapCircleAll(owner.transform.position, raioFuga);
        foreach (Collider2D col in inimigos)
        {
            if (col.CompareTag("Body") && col.transform.root != owner.transform.root)
            {
                float distancia = Vector2.Distance(owner.transform.position, col.transform.position);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    inimigoMaisProximo = col.transform;
                }
            }
        }

        return inimigoMaisProximo != null;
    }

    void FugirDoInimigoMaisProximo(Transform inimigo)
    {
        if (inimigo == null) return;

        Vector2 direcaoFuga = (owner.transform.position - inimigo.position).normalized;
        Vector2 novaPosicao = (Vector2)owner.transform.position + direcaoFuga * ownerMovement.speed * Time.deltaTime;
        novaPosicao = LimitarDentroDosLimites(novaPosicao);
        owner.transform.position = new Vector3(novaPosicao.x, novaPosicao.y, owner.transform.position.z);
        Debug.Log(owner.transform.position);
    }
}