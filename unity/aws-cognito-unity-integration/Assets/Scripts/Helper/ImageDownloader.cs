using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageDownloader : MonoBehaviour
{
    public IEnumerator DownloadImage(string gameObjectName, string imageUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            byte[] bytes = texture.EncodeToPNG();
            // Create sprite from texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);

            // Set sprite on profile picture image
            Image FbProfilePicture = GameObject.Find(gameObjectName).GetComponent<Image>();
            FbProfilePicture.sprite = sprite;
        }
    }
}
