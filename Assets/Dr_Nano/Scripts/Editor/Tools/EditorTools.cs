using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorTools : EditorWindow {


    [MenuItem("Tools/Reset PlayerPrefs")]
    public static void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("*************** PlayerPrefs Was Deleted ***************");
    }


    [MenuItem("Tools/Capture Screenshot")]
    public static void CaptureScreenshot()
    {
        ScreenCapture.CaptureScreenshot("C:/Users/User/Desktop/screenshot.png");
        Debug.Log("********* C:/Users/User/Desktop/screenshot.png was SAVED *********");
    }

    [MenuItem("Tools/Add 100,000 Coins")]
    public static void AddCoins()
    {
        DataManager.Instance.AddCoins(100000);
        Debug.Log("*************** 100,000 Coins Were Added ***************");
    }
    
    [MenuItem("Tools/Add 100 BoostUps")]
    public static void AddBoostUps()
    {
        DataManager.Instance.AddBomb(100);
        DataManager.Instance.AddLaser(100);
        DataManager.Instance.AddMissile(100);
        Debug.Log("*************** 100 BoostUps Were Added ***************");
    }
}
