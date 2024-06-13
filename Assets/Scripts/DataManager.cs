using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Unity.VisualScripting;

public class DataManager : MonoBehaviour
{
    string urlGetInfo = "http://covalent.x10.mx/runner_game/getleaderboard.php";
    string urlSetInfo = "http://covalent.x10.mx/runner_game/setscore.php";
    string urlRemoveInfo = "http://covalent.x10.mx/runner_game/removeuser.php";

    public UserInfo[] users;

    [Header("Canvas Configs")]
    public Transform parentUsers;
    public GameObject userPrefab;
    public TMP_InputField usernameField;
    public GameObject scorePanel;

    int score;
    public TextMeshProUGUI scoreText;

    [Header("Remove User")]
    public int userID;



    public void GameOver(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
        scorePanel.SetActive(true);
    }


    // Solo se ejecuta cuando se accede a Ranking
    public void GetInfoLeadboard()
    {
        StartCoroutine(cGetData());
    }

    public void PrintLeadboard()
    {
        for (int i = 0; i < users.Length; i++)
        {
            GameObject temp = Instantiate(userPrefab, parentUsers);
            temp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = users[i].username;
            temp.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = users[i].score.ToString();
        }
    }

    // Corutina para obtener datos json
    IEnumerator cGetData()
    {
        UnityWebRequest request = UnityWebRequest.Get(urlGetInfo);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            string jsonData = request.downloadHandler.text;
            RootUsers info = JsonUtility.FromJson<RootUsers>("{\"users\":" + jsonData + "}");
            users = info.users;
            PrintLeadboard();
        }
    }


    public void ShowScorePanel()
    {
        scorePanel.transform.Find("ScoreValue").GetComponent<TextMeshProUGUI>().text = score.ToString();
        scorePanel.SetActive(true);
    }

    public void SetScoreUser()
    {
        if (usernameField.text.Length >= 4)
        {
            Debug.Log("El Nombre tiene menos de 4 caracteres!");
            StartCoroutine(cSetData());
            scorePanel.SetActive(false);
        }
    }

    // Corutina para insertar datos en la BBDD
    IEnumerator cSetData()
    {
        WWWForm form = new WWWForm();
        form.AddField("user", usernameField.text);
        form.AddField("score", score);

        UnityWebRequest request = UnityWebRequest.Post(urlSetInfo, form);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
        }
        ChangeScene(0);
    }



    //private void OnGUI()
    //{
    //    if (GUILayout.Button("Eliminar Usuario"))
    //    {
    //        StartCoroutine(cRemoveData());
    //    }
    //}

    // Corutina para eliminiar datos en la BBDD
    IEnumerator cRemoveData()
    {
        WWWForm form = new WWWForm();
        form.AddField("table", "leaderboard");
        form.AddField("id", userID);

        UnityWebRequest request = UnityWebRequest.Post(urlRemoveInfo, form);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
        }
    }

    public void ChangeScene(int sceneIndex) //Menu:0, Game:1
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}

[System.Serializable]
public class UserInfo
{
    //El nombre de las variables en la BBDD
    public int id;
    public string username;
    public int score;
}

[System.Serializable]
public class RootUsers
{
    public UserInfo[] users;
}