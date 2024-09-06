using UnityEngine;
using System;
using System.Collections;
using Cysharp.Threading.Tasks;

public class LoginBonusSystem : MonoBehaviour
{
    [SerializeField] private UIController uiController;
    private const string LastLoginKey = "LastLoginDate";
    private const string LoginStreakKey = "LoginStreak";

    private async void Start()
    {
        await UniTask.WaitUntil(() => uiController != null);
        await UniTask.DelayFrame(1);
        CheckLoginBonus();
    }

    private void CheckLoginBonus()
    {
        DateTime currentDate = DateTime.Now;
        DateTime lastLoginDate = GetLastLoginDate();

        if (IsNewDay(currentDate, lastLoginDate))
        {
            int loginStreak = UpdateLoginStreak(currentDate, lastLoginDate);
            GiveLoginBonus(loginStreak);
            SaveLoginDate(currentDate);
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

    private void GiveLoginBonus(int loginStreak)
    {
        int bonusAmount = CalculateBonusAmount(loginStreak);
        Debug.Log($"ログインボーナスを付与しました！ ログイン{loginStreak}日目: {bonusAmount}クレジット");
        if (uiController != null)
        {
            uiController.AddPullCredit(bonusAmount).Forget();
        }
        else
        {
            Debug.LogError("UIController is not set!");
        }
    }

    private int CalculateBonusAmount(int loginStreak)
    {
        // 連続ログイン日数に応じてボーナス額を計算
        switch (loginStreak)
        {
            case 1: return 1;  // 1日目: 1クレジット
            case 2: return 2;  // 2日目: 2クレジット
            case 3: return 2;  // 3日目: 3クレジット
            case 4: return 3;  // 4日目: 4クレジット
            case 5: return 3;  // 5日目: 5クレジット
            case 6: return 4;  // 6日目: 6クレジット
            case 7: return 5; // 7日目: 10クレジット（週間ボーナス）
            default: return 3; // 8日目以降: 5クレジット
        }
    }

    private void SaveLoginDate(DateTime date)
    {
        PlayerPrefs.SetString(LastLoginKey, date.ToString());
        PlayerPrefs.Save();
    }

    private int UpdateLoginStreak(DateTime currentDate, DateTime lastLoginDate)
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

        return currentStreak;
    }
}