using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UserProfilePicture : MonoBehaviour, IPointerClickHandler
{
    static string path;
    static string savePath;
    static string filename = "/profile.png";
    public Image image;

    private void OnEnable() {
        // check if credentialsManager has userid value or credentialsManager has accessKeyId value
        StartCoroutine(waitForAttribute());  
    }

    // Wait for CredentialsManager to set the userid value
    IEnumerator waitForAttribute()
    {
        // update image savePath on local device
        savePath = Application.persistentDataPath + "/images" + CredentialsManager.Userid;   

        // check if file exists
        if (File.Exists(savePath + filename))
        {
            Debug.Log("Profile image found locally: " + savePath + filename);
            // load local image
            loadLocalImage();
        }
        else
        {
            // Don't proceed until values are set
            while (CredentialsManager.Userid == null || CredentialsManager.AccessKeyId == null)
            {
                Debug.Log("waiting for Userid value");
                // wait 200ms
                yield return new WaitForSeconds(0.2f);
            }
            
            // get image from online source
            Debug.Log("Profile image not found locally. Downloading from online source...");
            getImageOnline();
        }
    }
   
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        OpenExplorer();
    }

    async void getImageOnline()
    {
        if (CredentialsManager.authType == "Cognito")
        {
            try
            {
                // Build object key
                string objectKey = ExternalParameters.S3ObjectKeyPrefix + CredentialsManager.IdentityId + "/profile.png";

                // download image from s3
                byte[] data = await UserProfileHelper.S3DownloadProfilePicture(objectKey);
                // save image to persistent data path
                createDirectory();
                File.WriteAllBytes(savePath + filename, data);

                // Create texture from data
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(data);

                // Create sprite from texture
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
                // set sprite on image
                image.sprite = sprite;

            }
            catch (System.Exception e)
            {
                Debug.Log("Error downloading profile picture: " + e.Message);
            }
        }
        else if (CredentialsManager.authType == "Facebook")
        {
            // get image from facebook and set on profile picture image
            PanelFacebookLogin.DownloadProfilePicture("User Profile - Picture");
        }
    }

    // on-click event for image will bring up file explorer
    public void OpenExplorer()
    {
        // TODO : add support for other devices
        #if UNITY_EDITOR
            path = EditorUtility.OpenFilePanel("Select a file", "", ".png, .jpg, .jpeg");
        #else
            path = null;
        #endif

        DisplayIt();
    }
    void DisplayIt()
    {
        if (path != null && path.Length > 0 && File.Exists(path) && (Path.GetExtension(path) == ".png" || Path.GetExtension(path) == ".jpg" || Path.GetExtension(path) == ".jpeg"))
        {
            // start corotine
            StartCoroutine(getImageLocal());
        }
        else
        {
            Debug.Log("Invalid file");
        }
    }

    IEnumerator getImageLocal()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + path);
        yield return www.SendWebRequest();

        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(www.downloadHandler.data);

        // Create sprite from texture
        Sprite _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
        image.sprite = _sprite;
        saveImage();
    }

    // save image to application's persistent data path
    public void saveImage()
    {
        if (path != null)
        {
            createDirectory();
            // copy file to persistent data path
            File.Copy(path, savePath + filename, true);
                    
            // Build object key
            string _S3ObjectKey = ExternalParameters.S3ObjectKeyPrefix + CredentialsManager.IdentityId + filename;
            string _localFilePath = savePath + filename;

            // call upload function
            UserProfileHelper.S3UploadProfilePicture(_localFilePath, _S3ObjectKey);
            EventManager.UserProfile("Profile picture updated");
        }
    }

    // load image from persistent data path
    void loadLocalImage()
    {
        // load image from persistent data path
        byte[] bytes = File.ReadAllBytes(savePath + filename);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);

        // Create sprite from texture
        Sprite _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100);
        image.sprite = _sprite;
    }

    void createDirectory()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/images/" + CredentialsManager.Userid))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/images/" + CredentialsManager.Userid);
        }
    }
    
}