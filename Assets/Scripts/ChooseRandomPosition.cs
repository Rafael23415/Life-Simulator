using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ChooseRandomPosition : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private float choosePointRadius = 2.5f;
    [SerializeField] private int timeToChangeBehavior = 1;
    [SerializeField] private int wanderingPercentage = 50;

    [SerializeField] private float health = 100;
    [SerializeField] private Image healthBar;

    [SerializeField] private int waterMeter = 100;
    [SerializeField] private float seeWaterRadius = 5f;
    [SerializeField] private Image waterBar;
    [SerializeField] private LayerMask waterLayerMask;

    private bool isWandering;
    private bool isDrinking;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private IEnumerator Start()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        WaitForSeconds delay = new(timeToChangeBehavior);
        float wanderingProbability = wanderingPercentage / 100f;

        while (true)
        {
            if (agent.remainingDistance < 0.1f && !isDrinking)
            {
                if (waterMeter <= 10)
                    SeekWaterSource();
                else
                {
                    float probability = Random.Range(0f, 1f);
                    if (probability <= wanderingProbability)
                    {
                        isWandering = true;
                        GoToRandomPosition();
                        print("wandering");
                    }
                    else
                    {
                        isWandering = false;
                        agent.destination = transform.position;
                    }
                }
            }

            if(!isDrinking && waterMeter > 0) waterMeter -= 1;
            if (waterMeter <= 0) health -= 1;
            else if (health < 100) health += 1;

            healthBar.fillAmount = health / 100f;
            waterBar.fillAmount = waterMeter / 100f;

            yield return delay;
        }
    }

    private void GoToRandomPosition()
    {
        agent.destination = new Vector2(Random.Range(transform.position.x - choosePointRadius, transform.position.x + choosePointRadius),
                                        Random.Range(transform.position.y - choosePointRadius, transform.position.y + choosePointRadius));
    }

    private void SeekWaterSource()
    {
        Collider2D waterSource = Physics2D.OverlapCircle(transform.position, seeWaterRadius, waterLayerMask);
        if (waterSource)
        {
            agent.destination = waterSource.bounds.ClosestPoint(transform.position);
            StartCoroutine(GoDrink());
        }

        else if (!isDrinking)
        {
            GoToRandomPosition();
            print("searching water");
        }
    }

    private IEnumerator GoDrink()
    {
        WaitForSeconds delay = new(timeToChangeBehavior);
        while (waterMeter < 100)
        {
            if (agent.remainingDistance < 0.1f)
            {
                isDrinking = true;
                waterMeter += 5;
            }
            yield return delay;
        }

        isDrinking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, choosePointRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, seeWaterRadius);
    }
}
