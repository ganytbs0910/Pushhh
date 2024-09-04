using UnityEngine;
using System;
using System.Collections;

public class LoginBonusSystem : MonoBehaviour
{
    [SerializeField] private UIController uiController;
    private const string LastLoginKey = "LastLoginDate";
    private const string LoginStreakKey = "LoginStreak";

    private void Start()
    {
        CheckLoginBonus();
    }

    private void CheckLoginBonus()
    {
        DateTime currentDate = DateTime.Now;
        DateTime lastLoginDate = GetLastLoginDate();

        if (IsNewDay(currentDate, lastLoginDate))
        {
            GiveLoginBonus();
            SaveLoginDate(currentDate);
            UpdateLoginStreak(currentDate, lastLoginDate);
        }
    }

    private DateTime GetLastLoginDate()
    {
        string lastLoginStr = PlayerPrefs.GetString(LastLoginKey, string.Empty);
        if (string.IsNullOrEmpty(lastLoginStr))
        {
            return DateTime.MinValue;
        }
        return DateTime.Parse(lastLoginStr);
    }

    private bool IsNewDay(DateTime currentDate, DateTime lastLoginDate)
    {
        return currentDate.Date > lastLoginDate.Date;
    }

    private void GiveLoginBonus()
    {
        int loginStreak = PlayerPrefs.GetInt(LoginStreakKey, 0);
        // ログインボーナスを付与するロジックをここに実装
        Debug.Log($"ログインボーナスを付与しました！ ログイン{loginStreak + 1}日目");
        uiController.AddPullCredit(1).Forget();
    }

    private void SaveLoginDate(DateTime date)
    {
        PlayerPrefs.SetString(LastLoginKey, date.ToString());
        PlayerPrefs.Save();
    }

    private void UpdateLoginStreak(DateTime currentDate, DateTime lastLoginDate)
    {
        int currentStreak = PlayerPrefs.GetInt(LoginStreakKey, 0);

        if ((currentDate.Date - lastLoginDate.Date).Days == 1)
        {
            // 連続ログイン
            currentStreak++;
        }
        else
        {
            // 連続ログインが途切れた
            currentStreak = 1;
        }

        PlayerPrefs.SetInt(LoginStreakKey, currentStreak);
        PlayerPrefs.Save();
    }
}