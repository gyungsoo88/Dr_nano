using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using JicaGames;

public class UpgradeController : MonoBehaviour {

    [SerializeField] private GameObject upgradePanel = null;
    [Header("Upgrade References")]
    [SerializeField] private Text coinTxt = null;
    [SerializeField] private Text SS_LevelTxt = null;
    [SerializeField] private Text DP_LevelTxt = null;
    [SerializeField] private Text SSUpgradePriceTxt = null;
    [SerializeField] private Text DPUpgradePriceTxt = null;
    [SerializeField] private Button SS_UpgradeBtn = null;
    [SerializeField] private Button DP_UpgradeBtn = null;

    private void Start()
    {
        Application.targetFrameRate = 60;
        DataManager.Instance.SSUpgradePrice = DataManager.Instance.DefaultUpgradePrice + DataManager.Instance.UpgradeIncrese * DataManager.Instance.Get_SS_Level() *
            (DataManager.Instance.Get_SS_Level() - 1) / 2;
        DataManager.Instance.DPUpgradePrice = DataManager.Instance.DefaultUpgradePrice + DataManager.Instance.UpgradeIncrese * DataManager.Instance.Get_DP_Level() *
            (DataManager.Instance.Get_DP_Level() - 1) / 2;
    }

    void Update ()
    {
        coinTxt.text = DataManager.Instance.Coins.ToString();
        SSUpgradePriceTxt.text = DataManager.Instance.SSUpgradePrice.ToString();
        DPUpgradePriceTxt.text = DataManager.Instance.DPUpgradePrice.ToString();
        SS_LevelTxt.text = "LEVEL: " + DataManager.Instance.Get_SS_Level().ToString();
        DP_LevelTxt.text = "LEVEL: " + DataManager.Instance.Get_DP_Level().ToString();
        if (DataManager.Instance.Coins >= DataManager.Instance.SSUpgradePrice)
            SS_UpgradeBtn.interactable = true;
        else
            SS_UpgradeBtn.interactable = false;
        if (DataManager.Instance.Coins >= DataManager.Instance.DPUpgradePrice)
            DP_UpgradeBtn.interactable = true;
        else
            DP_UpgradeBtn.interactable = false;
	}

    public void Upgrade_SS()
    {
        if (DataManager.Instance.Get_SS_Level() < DataManager.Instance.Max_SS_Level)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
            Increase_SS_Level(1);
            DataManager.Instance.RemoveCoins(DataManager.Instance.SSUpgradePrice);
            DataManager.Instance.SSUpgradePrice = DataManager.Instance.DefaultUpgradePrice + DataManager.Instance.UpgradeIncrese * DataManager.Instance.Get_SS_Level() * 
                (DataManager.Instance.Get_SS_Level() - 1) / 2;
        }
    }
    public void Upgrade_DP()
    {
        if (DataManager.Instance.Get_DP_Level() < DataManager.Instance.Max_DP_Level)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
            Increase_DP_Level(1);
            DataManager.Instance.RemoveCoins(DataManager.Instance.DPUpgradePrice);
            DataManager.Instance.DPUpgradePrice = DataManager.Instance.DefaultUpgradePrice + DataManager.Instance.UpgradeIncrese * DataManager.Instance.Get_DP_Level() *
                (DataManager.Instance.Get_DP_Level() - 1) / 2;
        }
    }

    public void BackBtn()
    {
        upgradePanel.SetActive(false);
        SoundManager.Instance.PlaySound(SoundManager.Instance.button);        
    }

    public void Increase_SS_Level(int increaseAmount)
    {
        int amount;
        amount = DataManager.Instance.Get_SS_Level() + increaseAmount;

        DataManager.Instance.Set_SS_Level(amount);
    }
    public void Increase_DP_Level(int increaseAmount)
    {
        int amount;
        amount = DataManager.Instance.Get_DP_Level() + increaseAmount;

        DataManager.Instance.Set_DP_Level(amount);
    }
}
