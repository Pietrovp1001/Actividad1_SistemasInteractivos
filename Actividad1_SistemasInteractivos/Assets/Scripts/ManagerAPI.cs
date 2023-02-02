using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ManagerAPI : MonoBehaviour
{
    [SerializeField] private List<RawImage> YourRawImage;
    
    private string RickAndoMortyApi = "https://rickandmortyapi.com/api";
    private string ServerApiPath = "https://my-json-server.typicode.com/Pietrovp1001/Actividad1_SistemasInteractivos";
    
    private List<int> generatedNumbers = new List<int>();
    public int[] cards;
    private int UserId = 1;
    
    public void GetPlayerDataOnClick() 
    {
        StartCoroutine(GetPlayerData());
    }

    public void RandomID()
    {
        UserId = Random.Range(1, 6); 
        Debug.Log("tu id es:" + UserId);
        StartCoroutine(GetPlayerData());
    }
    
    IEnumerator GetPlayerData()
    {
        string url = ServerApiPath + "/users/" + UserId;
        Debug.Log(url);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR:" + www.error);
        }
        else
        {
            if (www.responseCode == 200)
            {
                UserData user = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
                for (int i = 0; i < user.deck.Length; i++)
                {
                    StartCoroutine(GetCharacter(user.deck[i],i));
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else
            {
                string mensaje = "Status:" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError:" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator GetCharacter(int Id, int Place)
    {
        UnityWebRequest www = UnityWebRequest.Get(RickAndoMortyApi + "/character/" + Id);
        yield return www.Send();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR:" + www.error);
        }
        else
        {
            if (www.responseCode == 200)
            {
                Character character = JsonUtility.FromJson<Character>(www.downloadHandler.text);
                Debug.Log(character.id);
                StartCoroutine(DownloadImage(character.image,Place));
            }
            else
            {
                string mensaje = "Status:" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError:" + www.error;
                Debug.Log(mensaje);
            }
        }
    }

    IEnumerator DownloadImage(string url, int Place)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else YourRawImage[Place].texture = ((DownloadHandlerTexture)request.downloadHandler).texture;                
    }
}

[System.Serializable]
public class Character
{
    public int id;
    public string image;
}
public class UserData
{
    public int[] deck;
}
