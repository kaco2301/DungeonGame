using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamageable, IPausable
{
    public Stat stat = new Stat();

    private void OnEnable()
    {
        // 게임 상태 변경 방송을 구독합니다.
        GameStateManager.OnMainStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnMainStateChanged -= OnGameStateChanged;
    }


    public virtual void TakeDamage(float damage)
    {
        //float finalDamage = Mathf.Max(damage - stat.Defense, 0);
        //stat.Hp -= finalDamage;

        //if (stat.Hp <= 0)
        //{
        //    Die();
        //}
        //Debug.Log("asdasdasd");
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

#region GameState
    public void Pause()
    {

        // GetComponent<Animator>().speed = 0;
    }

    public void Resume()
    {
    }

    private void OnGameStateChanged(MainGameState state)
    {
        if (state == MainGameState.Gameplay)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
    #endregion
}
