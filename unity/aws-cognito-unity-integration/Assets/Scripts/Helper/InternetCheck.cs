using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;

public class InternetCheckUI : MonoBehaviour
{
    public Text connectivityStatusText;

    IEnumerator Start()
    {
        // Check for internet connectivity
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            connectivityStatusText.text = "No internet connection available. Please check your network settings.";
        }
        else
        {
            connectivityStatusText.text = "";
        }

        yield return null; 
    }
}
