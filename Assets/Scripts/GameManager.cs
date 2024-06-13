using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum GameState { Initi, Playing, Dead };
public enum Rail { Left, Center, Right };

public class GameManager : MonoBehaviour
{
    GameState state;

    [Header("Path Configuration")]
    public List<GameObject> allPaths;
    public int firstPaths;
    public List<int> firstIndexEnabled;

    Transform lastPath, currentPath, parentRail, childRail;
    public Rail currentRail;

    [SerializeField] List<GameObject> paths = new List<GameObject>();
    bool turnActive, pathT, isPathT;
    Transform newPaths, newEnds;

    [Header("Player Values")]
    public CharacterController player;
    Vector3 moveDir;
    public int lifes;
    public float moveSpeed, horizontalSpeed, gravity, jumpForce;
    public int score;
    float timer;
    public TextMeshProUGUI scoreText, lifeText;

    public Animator anim;
    [SerializeField] public AudioSource coinFX;



    void Start()
    {
        state = GameState.Initi;
        lastPath = transform;
        FirstGenerate();
    }


    void FirstGenerate()
    {
        for (int i = 0; i < firstPaths; i++)
        {
            GameObject newPath = Instantiate(allPaths[GetRandomIndexPath()], lastPath.position, lastPath.rotation);
            newPath.AddComponent<PathControl>().Init(this);

            lastPath = newPath.transform.Find("Ends").GetChild(0);
            paths.Add(newPath);
        }
        currentPath = paths[0].transform;
        currentRail = Rail.Center;

        parentRail = currentPath.Find("Paths").GetChild(0);
        childRail = parentRail.GetChild((int)currentRail);

        player.transform.SetParent(parentRail);
        player.transform.forward = parentRail.forward;
    }

    int GetRandomIndexPath()
    {
        int randomIndex = Random.Range(0, allPaths.Count);
        while (firstIndexEnabled.Contains(randomIndex) == false)
        {
            randomIndex = Random.Range(0, allPaths.Count);
        }
        return randomIndex;
    }

    public void GetRnadomNewPath(GameObject oldPath)
    {
        moveSpeed += 0.1f;
        paths.Remove(oldPath);
        Destroy(oldPath);

        if (pathT == false)
        {
            //Obtiene el indice de un camino aleatorio de los que existen
            int randomIndex = Random.Range(0, allPaths.Count);
            GameObject newPath = Instantiate(allPaths[randomIndex], lastPath.position, lastPath.rotation);
            newPath.AddComponent<PathControl>().Init(this);

            lastPath = newPath.transform.Find("Ends").GetChild(0);
            //Añade el nuevo camino a la lista de Paths
            paths.Add(newPath);

            if (randomIndex == 1) pathT = true;
        }

        currentPath = paths[0].transform;
        parentRail = currentPath.Find("Paths").GetChild(0);

        player.transform.SetParent(parentRail);
        player.transform.forward = parentRail.forward;
    }
    // Cuando el usuario haya seleccionado el giro en un PathT
    void SetNewPathT()
    {
        pathT = false;
        isPathT = false;
        lastPath = newEnds;

        for (int i = 0; i < (firstPaths - 1); i++)
        {
            GameObject newPath = Instantiate(allPaths[GetRandomIndexPath()], lastPath.position, lastPath.rotation);
            newPath.AddComponent<PathControl>().Init(this);

            lastPath = newPath.transform.Find("Ends").GetChild(0);
            paths.Add(newPath);
        }
    }


    void Update()
    {
        switch (state)
        {
            case GameState.Initi:
                if (Input.GetButtonDown("Vertical"))
                {
                    //TODO: Desactivar texto de inicio
                    state = GameState.Playing;
                }
                break;

            case GameState.Playing:
                anim.SetBool("isGrounded", player.isGrounded);
                anim.SetFloat("Speed", 1);
                timer += Time.deltaTime;
                if (timer > 1)
                {
                    score++;
                    scoreText.text = score.ToString();
                    timer = 0;
                }

                moveDir = new Vector3(0, moveDir.y, moveSpeed); //Direccion Global
                moveDir = player.transform.TransformDirection(moveDir); //Direccion Local

                if (player.isGrounded)
                {
                    if (Input.GetButton("Jump"))
                    {
                        anim.SetTrigger("Jump");
                        moveDir.y = jumpForce;
                    }
                }
                else
                {
                    moveDir.y -= gravity * Time.deltaTime;
                }

                if (Input.GetButtonDown("Horizontal"))
                {
                    int indexButton = (int)Input.GetAxisRaw("Horizontal");
                    switch (currentRail)
                    {
                        case Rail.Left:
                            if (indexButton > 0) currentRail = Rail.Center;
                            if (indexButton < 0 && turnActive)
                            {
                                parentRail = newPaths;
                                childRail = parentRail.GetChild((int)currentRail);

                                player.transform.SetParent(parentRail);
                                player.transform.forward = parentRail.forward;

                                if (isPathT) SetNewPathT();
                            }
                            break;

                        case Rail.Center:
                            if (indexButton < 0) currentRail = Rail.Left;
                            else if (indexButton > 0) currentRail = Rail.Right;
                            break;

                        case Rail.Right:
                            if (indexButton < 0) currentRail = Rail.Center;
                            if (indexButton > 0 && turnActive)
                            {
                                parentRail = newPaths;
                                childRail = parentRail.GetChild((int)currentRail);

                                player.transform.SetParent(parentRail);
                                player.transform.forward = parentRail.forward;

                                if (isPathT) SetNewPathT();
                            }
                            break;
                    }
                    childRail = parentRail.GetChild((int)currentRail);
                }
                // Muerte por caida
                if (player.transform.position.y < 0)
                {
                    SetDeadPlayer();
                }
                break;

            case GameState.Dead:
                moveDir *= 0;
                break;
        }
    }

    private void FixedUpdate()
    {
        player.Move(moveDir * Time.deltaTime);

        if (childRail != null)
        {
            Vector3 finalPos = player.transform.localPosition;
            finalPos.x = childRail.localPosition.x;
            player.transform.localPosition = Vector3.MoveTowards(player.transform.localPosition, finalPos, horizontalSpeed * Time.deltaTime);
        }
    }


    public void GetScore(int value)
    {
        score += value;
        scoreText.text = score.ToString();
    }

    public void GetDamage()
    {
        anim.GetComponent<DamageControl>().ActiveDamage();

        lifes--;
        lifeText.text = lifes.ToString();
        if (lifes < 0)
        {
            SetDeadPlayer();
        }
    }

    void SetDeadPlayer()
    {
        GetComponent<DataManager>().GameOver(score);
        state = GameState.Dead;
    }

    /// <summary>
    /// Es llamada cuando el Player detecta una curva simple
    /// </summary>
    /// <param name="active">Para activar la opcion de gito</param>
    /// <param name="newPaths">Los nuevos carriles que ha de seguir</param>
    public void ActiveTurn(bool active, Transform newPath)
    {
        turnActive = active;
        newPaths = newPath;
    }
    /// <summary>
    /// Es llamada cuando el Player detecta una curva en T
    /// </summary>
    /// <param name="active">Para activar la opcion de gito</param>
    /// <param name="newPaths">Los nuevos carriles que ha de seguir</param>
    /// <param name="newEnd">El nuevo final desde el que se imprimen los nevos caminos</param>
    public void ActiveTurn(bool active, Transform newPath, Transform newEnd)
    {
        turnActive = active;
        newPaths = newPath;
        isPathT = active;
        newEnds = newEnd;
    }
}
