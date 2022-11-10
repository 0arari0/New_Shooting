using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isTouchTop;
    public bool isTouchBottom;
    public bool isTouchRight;
    public bool isTouchLeft;

    public int life;
    public int score;
    public float speed;
    public int power;
    public int maxPower;
    public float maxShotDelay;
    public float curShotDelay;

    public GameObject bulletObjA;
    public GameObject bulletObjB;
    public GameObject boomEffect;
    public GameManager manager;
    public bool isHit;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        Fire();
        Reload();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if ((isTouchRight && h == 1) || (isTouchLeft && h == -1))
            h = 0;

        float v = Input.GetAxisRaw("Vertical");
        if ((isTouchTop && v == 1) || (isTouchBottom && v == -1))
            v = 0;

        Vector3 curPos = transform.position;
        Vector3 nextPos = new Vector3(h, v, 0) * speed * Time.deltaTime;

        transform.position = curPos + nextPos;

        if (Input.GetButtonDown("Horizontal") || Input.GetButtonUp("Horizontal"))
        {
            anim.SetInteger("Input", (int)h);
        }
    }

    void Fire()
    {
        if (!Input.GetButton("Fire1"))
            return;

        if (curShotDelay < maxShotDelay)
            return;

        switch (power)
        {
            case 1:
                GameObject bullet = Instantiate(bulletObjA, transform.position, transform.rotation);
                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                Debug.Log("shoot");
                break;
            case 2:
                GameObject bulletR = Instantiate(bulletObjA, transform.position + Vector3.right*0.1f, transform.rotation);
                GameObject bulletL = Instantiate(bulletObjA, transform.position + Vector3.left*0.1f, transform.rotation);
                Rigidbody2D rigidR = bulletR.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidL = bulletL.GetComponent<Rigidbody2D>();
                rigidR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;
            case 3:
                GameObject bulletRR = Instantiate(bulletObjA, transform.position + Vector3.right * 0.25f, transform.rotation);
                GameObject bulletCC = Instantiate(bulletObjB, transform.position, transform.rotation);
                GameObject bulletLL = Instantiate(bulletObjA, transform.position + Vector3.left * 0.25f, transform.rotation);
                Rigidbody2D rigidRR = bulletRR.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidCC = bulletCC.GetComponent<Rigidbody2D>();
                Rigidbody2D rigidLL = bulletLL.GetComponent<Rigidbody2D>();
                rigidRR.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidCC.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                rigidLL.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                break;

        }
        curShotDelay = 0;
    }
    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = true;
                    break;
                case "Bottom":
                    isTouchBottom = true;
                    break;
                case "Right":
                    isTouchRight = true;
                    break;
                case "Left":
                    isTouchLeft = true;
                    break;
            }
        }
        else if (collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "EnemyBullet")
        {
            if (isHit)
                return;
            
            isHit = true;

            life--;
            manager.UpdateLifeIcon(life);

            if(life==0)
            {
                manager.GameOver();
            }
            else
            {
                manager.RespawnPlayer();
            }

            manager.RespawnPlayer();
            gameObject.SetActive(false);
            Destroy(collision.gameObject);
        }
        else if(collision.gameObject.tag == "Item")
        {
            Item item = collision.gameObject.GetComponent<Item>();
            switch (item.type)
            {
                case "Coin":
                    //코인 획득시 1000점 추가
                    score += 1000;
                    break;
                case "Power":
                    //힘이 이미 최대라면 보너스 점수 500점
                    if (power == maxPower)
                        score += 500;
                    //힘 하나씩 올리기 max =3
                    else
                        power++; 
                    break;
                //필살기 : 사용 시 적 비행기+적 총알 모두 삭제
                case "Boom":
                    //#1.이펙트 나타남
                    boomEffect.SetActive(true);
                    //폭탄 스프라이트는 invoke로 시간차 비활성화
                    Invoke("OffBoomEffect", 3f);

                    //#2.적 지우기
                    //해당 태그로 장면의 모든 오브젝트를 추출 , 해당 태그가 배열에 저장됨
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); 
                    //해당 갯수만큼 없애기 위해 접근 for문 사용
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        Enemy enemyLogic = enemies[i].GetComponent<Enemy>();
                        enemyLogic.OnHit(1000); // 폭탄에 맞을 시 데미지 1000 OnHit함수와 연결됨
                    }

                    //#3. 적 총알 지우기
                    GameObject[] bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
                    for (int i = 0; i < bullets.Length; i++)
                    {
                        //총알 모두 삭제
                        Destroy(bullets[i]); 
                    }
                    break;
            }
        }
        //먹은 아이템 삭제
        Destroy(collision.gameObject);
    }
    void OffBoomEffect()
    {
        boomEffect.SetActive(false);
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            switch (collision.gameObject.name)
            {
                case "Top":
                    isTouchTop = false;
                    break;
                case "Bottom":
                    isTouchBottom = false;
                    break;
                case "Right":
                    isTouchRight = false;
                    break;
                case "Left":
                    isTouchLeft = false;
                    break;

            }
        }
    }
}
