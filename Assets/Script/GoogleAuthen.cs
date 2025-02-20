using System;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GoogleAuthen : MonoBehaviour
{
    public TMP_Text statusText;
    private string clientId = "382397535757-jlr6pk7k9ibtdja6mustqm1p426t4c1j.apps.googleusercontent.com";
    private string redirectUri = "http://localhost:5000/callback";
    private string authUrl;
    private string serverUrl = "http://localhost:5000/register";
    public string nextScene = "MainScene";
    public string loginScene = "LoginScene"; // เปลี่ยนกลับไปหน้าล็อกอิน

    void Start()
    {
        authUrl = "https://accounts.google.com/o/oauth2/auth" +
                  "?client_id=" + clientId +
                  "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                  "&response_type=token" +
                  "&scope=email%20profile%20openid" +
                  "&prompt=select_account"; // บังคับให้เลือกบัญชีใหม่ทุกครั้ง

        Application.deepLinkActivated += OnDeepLink;

        // สำหรับทดสอบใน Editor (จำลอง deep link)
#if UNITY_EDITOR
        // Uncomment บรรทัดด้านล่างเพื่อทดสอบ deep link ใน Editor ได้เลย
        // SimulateDeepLink("unitydl://auth?access_token=TEST_TOKEN_EDITOR");
#endif
    }

    public void OnSignIn()
    {
        Debug.Log("🔹 Opening Google Login: " + authUrl);
        Application.OpenURL(authUrl);
    }

    public void OnLogout()
    {
        Debug.Log("🔹 Logging out...");

        // ลบ Token ที่ถูกเก็บไว้
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.Save();

        // ดีเลย์ 2 วินาทีก่อนเปลี่ยนกลับไปหน้าล็อกอิน
        StartCoroutine(LogoutAndSwitchScene());
    }

    IEnumerator LogoutAndSwitchScene()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(loginScene);
    }

    void OnDeepLink(string url)
    {
        Debug.Log("🔹 Received Deep Link: " + url);
        string token = ExtractTokenFromURL(url);

        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("✅ Extracted Token: " + token);
            PlayerPrefs.SetString("accessToken", token); // เก็บ Token ไว้
            PlayerPrefs.Save();
            StartCoroutine(SendUserDataToServer(token));
        }
        else
        {
            Debug.LogError("❌ Failed to extract token from URL");
            UpdateStatusText("❌ Token extraction failed.");
        }
    }

    // ฟังก์ชันจำลอง deep link สำหรับทดสอบใน Editor
    void SimulateDeepLink(string url)
    {
        Debug.Log("Simulating deep link: " + url);
        OnDeepLink(url);
    }

    IEnumerator SendUserDataToServer(string accessToken)
    {
        WWWForm form = new WWWForm();
        form.AddField("accessToken", accessToken);

        using (UnityWebRequest request = UnityWebRequest.Post(serverUrl, form))
        {
            // ตั้ง header ถ้าจำเป็น (กรณีส่ง JSON ควรใช้ UploadHandlerRaw แต่ที่นี่ใช้ WWWForm)
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError("❌ Failed to send user data: " + request.error);
                UpdateStatusText("❌ Failed to send data: " + request.error);
            }
            else
            {
                Debug.Log("✅ User data sent successfully: " + request.downloadHandler.text);
                UpdateStatusText("✅ Login successful!");
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene(nextScene);
            }
        }
    }

    // ปรับปรุงฟังก์ชันดึง token ให้รองรับทั้ง Fragment และ Query String
    string ExtractTokenFromURL(string url)
    {
        try
        {
            Uri uri = new Uri(url);
            string token = null;
            // ตรวจสอบใน Fragment ก่อน (โดยปกติแล้ว response_type=token จะส่ง token ใน Fragment)
            if (!string.IsNullOrEmpty(uri.Fragment))
            {
                string fragment = uri.Fragment;
                if (fragment.StartsWith("#"))
                {
                    fragment = fragment.Substring(1);
                }

                var queryParams = fragment.Split('&');
                foreach (string param in queryParams)
                {
                    string[] keyValue = param.Split('=');
                    if (keyValue.Length == 2 && keyValue[0] == "access_token")
                    {
                        token = keyValue[1];
                        break;
                    }
                }
            }

            // หากไม่เจอใน Fragment ให้ตรวจสอบใน Query String
            if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(uri.Query))
            {
                string query = uri.Query.TrimStart('?');
                var queryParams = query.Split('&');
                foreach (string param in queryParams)
                {
                    string[] keyValue = param.Split('=');
                    if (keyValue.Length == 2 && keyValue[0] == "access_token")
                    {
                        token = keyValue[1];
                        break;
                    }
                }
            }
            return token;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing URL: " + ex.Message);
            return null;
        }
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    // ฟังก์ชันใหม่สำหรับเปิดเบราว์เซอร์ภายนอกและไปที่ Google
    public void OpenGoogle()
    {
        Debug.Log("🔹 Opening Google in external browser");
        Application.OpenURL("https://www.google.com");
    }
}
