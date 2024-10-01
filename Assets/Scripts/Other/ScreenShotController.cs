using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotController : MonoBehaviour
{
    private const string SCREENSHOT_FILE_PATH = "【スクリーンショット保存パス】";

    private void Awake()
    {
        // ゲーム内に一つだけ保持
        if (FindObjectsOfType<ScreenShotController>().Length > 1)
            Destroy(gameObject);
        else
            DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // スペースキーが押されたら
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // スクリーンショットを保存
            System.DateTime date = System.DateTime.Now;
            string fileName = "Screenshot_" + date.ToString("yyyyMMddHHmmss") + ".png";
            ScreenCapture.CaptureScreenshot(SCREENSHOT_FILE_PATH + fileName);
            Debug.Log("Save to " + fileName);
        }
    }
}