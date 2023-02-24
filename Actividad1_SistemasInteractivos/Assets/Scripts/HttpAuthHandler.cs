using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class HttpAuthHandler : MonoBehaviour
{
    [SerializeField] private GameObject login,index;
    [SerializeField] private string serverApiURL;
    [SerializeField] private TMP_Text[] Spots ;
    [SerializeField] private TMP_Text error, userName;
    
    public string Token { get; set; }
    public string Username { get; set; }
    private string token ;
    
    
    //Functions
    public void Start()
    {
        List<User> lista = new List<User>();
        List<User> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<User>();
        if (string.IsNullOrEmpty(Token))
        {
            index.SetActive(false);
            login.SetActive(true);
            Debug.Log("No hay token");
        }
        else
        {
            login.SetActive(false);
            index.SetActive(true);
            token = Token;
            Debug.Log(Token);
            Debug.Log(Username);
            StartCoroutine(GetPerfil());
        }
    }
    public void Registrar()
    {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<TMP_InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<TMP_InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Registro(postData));
    }
    public void Ingresar()
    {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<TMP_InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<TMP_InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Login(postData));
    }
    
    public void UploadScore()
    {
        User user = new User();
        user.username = Username;
        if (int.TryParse(GameObject.Find("InputDatascore").GetComponent<TMP_InputField>().text,out _))
        {
            user.data.score = int.Parse(GameObject.Find("InputDatascore").GetComponent<TMP_InputField>().text);
        }
        string postData = JsonUtility.ToJson(user);
        Debug.Log(postData);
        StartCoroutine(UpdateLeaderboard(postData));
    }
    
    //Coroutines
    IEnumerator Registro(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(serverApiURL + "/api/usuarios", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);
                
                Debug.Log(jsonData.usuario.username + " se regitro con id " + jsonData.usuario._id);
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
                error.text = "Error : El usuario ya existe  ";
                StartCoroutine(MessageError());
            }

        }
    }
    IEnumerator Login(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(serverApiURL + "/api/auth/login", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " inicio sesion");

                Token = jsonData.token;
                Username = jsonData.usuario.username;

                PlayerPrefs.SetString("token", Token);
                PlayerPrefs.SetString("username", Username);
                login.SetActive(false);
                index.SetActive(true);
                StartCoroutine(GetScores());
                userName.text = "Usuario :" + jsonData.usuario.username;

            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
                error.text = "Usuario inexistente o contraseña incorrecta";
                StartCoroutine(MessageError());
            }

        }
    }
    IEnumerator GetPerfil()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverApiURL + "/api/usuarios/" + Username);
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            if (www.responseCode == 200)
            {
                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);
                Debug.Log(jsonData.usuario.username + " Sigue con la sesion inciada");
                userName.text = "Usuario :" + jsonData.usuario.username;
                StartCoroutine(GetScores());
            }
            else
            {
                index.SetActive(false);
                login.SetActive(true);
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                error.text = "Error : El usuario anterior a cerrado seccion ";
                StartCoroutine(MessageError());
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator MessageError()
    {
        yield return new WaitForSeconds(5f);
        error.text = "";
    }
    IEnumerator GetScores()
    {
        UnityWebRequest www = UnityWebRequest.Get(serverApiURL + "/api/usuarios");
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {
                userlist jsonList = JsonUtility.FromJson<userlist>(www.downloadHandler.text);
                List<User> list = jsonList.usuarios;
                List<User> listInOrder = list.OrderByDescending(u => u.data.score).ToList<User>();
                int spot=0;
                foreach (User person in listInOrder)
                {
                    if (spot > 4)
                    {

                    }
                    else
                    {
                        string _username = spot + 1 + "-" + person.username + "..........." + person.data.score + "PTS";
                        Spots[spot].text = _username;
                        spot++;
                    }
                }
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
    IEnumerator UpdateLeaderboard(string postData)
    {
        UnityWebRequest www = UnityWebRequest.Put(serverApiURL + "/api/usuarios/", postData);
        www.method = "PATCH";
        www.SetRequestHeader("x-token", Token);
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();
        if (www.isNetworkError)
        {
            index.SetActive(false);
            login.SetActive(true);
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            if (www.responseCode == 200)
            {
                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);
                StartCoroutine(GetScores());
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
}


[System.Serializable]
public class User
{
    public string _id;
    public string username;
    public string password;
    public userData data;

    public User()
    {
        data = new userData();
    }
    public User(string username, string password)
    {
        this.username = username;
        this.password = password;
        data = new userData();
    }
}
[System.Serializable]
public class userData
{
    public int score;
}
public class AuthJsonData
{
    public User usuario;
    public userData data;
    public string token;
}
[System.Serializable]
public class userlist
{
    public List<User> usuarios;
}

