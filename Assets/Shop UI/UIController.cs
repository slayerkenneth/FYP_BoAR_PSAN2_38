using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor;

public class UIController : MonoBehaviour
{
    [Header("Global Reference")] [SerializeField]
    public PlayerStatus PlayerStatus;
    public GameFlowController GameFlowCtrl;
    public PlayerSpawner playerSpawner;

    public ProgressBar HPBar;
    public Button QuitButton;
    public Button RestoreButton;
    public Button UpHealthButton;
    public Button UpWeaponButton;
    public Button UpClass1Button;
    public Button UpClass2Button;
    public Button Item1Button;
    public Button Item2Button;
    public Button Item3Button;
    public Button Item4Button;
    public Button Confirm1Button;
    public Button Confirm2Button;
    public Button Confirm3Button;
    public Button Confirm4Button;
    public Button Confirm5Button;
    public Button Cancel1Button;
    public Button Cancel2Button;
    public Button Cancel3Button;
    public Button Cancel4Button;
    public Button Cancel5Button;

    public Label MoneyText;
    public Label HealthText;
    public Label UpWeaponText;
    public Label UpClass1Text;
    public Label UpClass2Text;

    public Label UpWeaponTextHint;
    public Label UpClass1TextHint;
    public Label UpClass2TextHint;

    public Label Item1Text;
    public Label Item2Text;
    public Label Item3Text;
    public Label Item4Text;
    public Label Item5Text;

    public VisualElement MainMenu;

    [SerializeField] private int recentHealth;
    [SerializeField] private int totalHealth;
    [SerializeField] private int recentMoney;
    [SerializeField] private int recentHPLV;
    [SerializeField] private int recentWeaponLV;
    [SerializeField] private int recentClass1LV;
    [SerializeField] private int recentClass2LV;
    [SerializeField] private SceneController SceneController;
    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        Debug.Log(root);
        MainMenu = root.Q<VisualElement>("Main_Menu");
        HPBar = root.Q<ProgressBar>("HPBar");
        QuitButton = root.Q<Button>("Quit_Button");
        RestoreButton = root.Q<Button>("Restore_Button");
        UpHealthButton = root.Q<Button>("Upgrade_Health_Button");
        UpWeaponButton = root.Q<Button>("Upgrade_Weapon_Button");
        UpClass1Button = root.Q<Button>("Upgrade_Class1_Button");
        UpClass2Button = root.Q<Button>("Upgrade_Class2_Button");

        Confirm1Button = root.Q<Button>("Confirm1");
        Confirm2Button = root.Q<Button>("Confirm2");
        Confirm3Button = root.Q<Button>("Confirm3");
        Confirm4Button = root.Q<Button>("Confirm4");
        Confirm5Button = root.Q<Button>("Confirm5");

        Cancel1Button = root.Q<Button>("Cancel1");
        Cancel2Button = root.Q<Button>("Cancel2");
        Cancel3Button = root.Q<Button>("Cancel3");
        Cancel4Button = root.Q<Button>("Cancel4");
        Cancel5Button = root.Q<Button>("Cancel5");

        MoneyText = root.Q<Label>("Money_Text");
        HealthText = root.Q<Label>("Health_LV");
        UpWeaponText = root.Q<Label>("Upgrade_Weapon_LV");
        UpClass1Text = root.Q<Label>("Upgrade_Class1_LV");
        UpClass2Text = root.Q<Label>("Upgrade_Class2_LV");

        UpWeaponTextHint = root.Q<Label>("Upgrade_Weapon_Hint");
        UpClass1TextHint = root.Q<Label>("Upgrade_Class1_Hint");
        UpClass2TextHint = root.Q<Label>("Upgrade_Class2_Hint");

        Item1Text = root.Q<Label>("Item1");
        Item2Text = root.Q<Label>("Item2");
        Item3Text = root.Q<Label>("Item3");
        Item4Text = root.Q<Label>("Item4");
        Item5Text = root.Q<Label>("Item5");

        QuitButton.clicked += QuitButtonPressed;

        RestoreButton.clicked += RestoreButtonPressed;
        UpHealthButton.clicked += UpHealthButtonPressed;
        UpWeaponButton.clicked += UpWeaponButtonPressed;
        UpClass1Button.clicked += UpClass1ButtonPressed;
        UpClass2Button.clicked += UpClass2ButtonPressed;

        Confirm1Button.clicked += Confirm1ButtonPressed;
        Confirm2Button.clicked += Confirm2ButtonPressed;
        Confirm3Button.clicked += Confirm3ButtonPressed;
        Confirm4Button.clicked += Confirm4ButtonPressed;
        Confirm5Button.clicked += Confirm5ButtonPressed;

        Cancel1Button.clicked += Cancel1ButtonPressed;
        Cancel2Button.clicked += Cancel2ButtonPressed;
        Cancel3Button.clicked += Cancel3ButtonPressed;
        Cancel4Button.clicked += Cancel4ButtonPressed;
        Cancel5Button.clicked += Cancel5ButtonPressed;
        
        InitShopUI();

    }

    void QuitButtonPressed() 
    {
        MainMenu.style.opacity = 0.5f;
        Item5Text.style.display = DisplayStyle.Flex;
        Debug.Log("Exit pressed");
        GameFlowCtrl.ExitShopReEnterMap();
    }

    void RestoreButtonPressed()
    {
        //increase recent health
        Debug.Log("Restore pressed");
        if (PlayerStatus.money > 0) {
            PlayerStatus.money--;
            PlayerStatus.currentHP = PlayerStatus.maxHPStat[PlayerStatus.maxHPLv];

            UpdateText();
        }
    }

    void UpHealthButtonPressed() 
    {
        //increase recent and total health
        Debug.Log("UpHealth pressed");
        if (PlayerStatus.money > 0 && PlayerStatus.maxHPLv != 9)
        {
            PlayerStatus.money--;
            PlayerStatus.maxHPLv ++;

            UpdateText();
        }
    }

    void UpWeaponButtonPressed() 
    {
        //increase weapon lv
        Debug.Log("increase weapon lv pressed");

        if (PlayerStatus.money > 0 && PlayerStatus.weaponLv != 9)
        {
            PlayerStatus.money--;
            PlayerStatus.weaponLv++;
            UpdateText();
        }
    }

    void UpClass1ButtonPressed() 
    {
        //increase class 1 lv
        Debug.Log("increase class 1 lv pressed");
        if (PlayerStatus.money > 0 && PlayerStatus.activeClass.lv != 9)
        {
            PlayerStatus.money--;
            PlayerStatus.activeClass.lv++;
            UpdateText();
        }
    }

    void UpClass2ButtonPressed() 
    {
        //increase class 2 lv
        Debug.Log("increase class 2 lv pressed");
        if (PlayerStatus.money > 0 && PlayerStatus.passiveClass.lv != 9)
        {
            PlayerStatus.money--;
            PlayerStatus.passiveClass.lv++;
            UpdateText();
        }
    }

    void Item1ButtonPressed() 
    {
        MainMenu.style.opacity = 0.5f;
        Item1Text.style.display = DisplayStyle.Flex;
        Debug.Log("Item1ButtonPressed");

    }

    void Item2ButtonPressed() 
    {
        MainMenu.style.opacity = 0.5f;
        Item2Text.style.display = DisplayStyle.Flex;
        Debug.Log("Item2ButtonPressed");

    }

    void Item3ButtonPressed() 
    {
        MainMenu.style.opacity = 0.5f;
        Item3Text.style.display = DisplayStyle.Flex;
        Debug.Log("Item3ButtonPressed");

    }

    void Item4ButtonPressed() 
    {
        MainMenu.style.opacity = 0.5f;
        Item4Text.style.display = DisplayStyle.Flex;
        Debug.Log("Item4ButtonPressed");

    }

    void Confirm1ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item1Text.style.display = DisplayStyle.None;
        //buy something, rencent money decrease
        Debug.Log("Confirm1ButtonPressed");

    }

    void Confirm2ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item2Text.style.display = DisplayStyle.None;
        //buy something, rencent money decrease
        Debug.Log("Confirm2ButtonPressed");

    }

    void Confirm3ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item3Text.style.display = DisplayStyle.None;
        //buy something, rencent money decrease
        Debug.Log("Confirm3ButtonPressed");
    }

    void Confirm4ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item4Text.style.display = DisplayStyle.None;
        //buy something, rencent money decrease
        Debug.Log("Confirm4ButtonPressed");
    }

    void Confirm5ButtonPressed()
    {
        //change to map scene
        Debug.Log("Confirm5ButtonPressed");

    }

    void Cancel1ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item1Text.style.display = DisplayStyle.None;
        Debug.Log("Cancel1ButtonPressed");

    }

    void Cancel2ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item2Text.style.display = DisplayStyle.None;
        Debug.Log("Cancel2ButtonPressed");

    }

    void Cancel3ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item3Text.style.display = DisplayStyle.None;
        Debug.Log("Cancel3ButtonPressed");

    }

    void Cancel4ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item4Text.style.display = DisplayStyle.None;
        Debug.Log("Cancel4ButtonPressed");
    }

    void Cancel5ButtonPressed()
    {
        MainMenu.style.opacity = 1.0f;
        Item5Text.style.display = DisplayStyle.None;
        Debug.Log("Cancel5ButtonPressed");
    }

    void InitShopUI()
    {
        PlayerStatus = PlayerStatus.CurrentPlayer;
        if (PlayerStatus== null) return;
        Debug.Log(PlayerStatus);

        UpdateText();
    }

    void UpdateText() {
        recentHealth = PlayerStatus.currentHP;
        totalHealth = PlayerStatus.maxHPStat[PlayerStatus.maxHPLv];
        recentMoney = PlayerStatus.money;
        recentWeaponLV = PlayerStatus.weaponStat.lv;
        recentClass1LV = PlayerStatus.activeClass.lv;
        recentClass2LV = PlayerStatus.passiveClass.lv;
        recentHPLV = PlayerStatus.maxHPLv;

        recentMoney = PlayerStatus.money;
        MoneyText.text = recentMoney.ToString();

        HPBar.highValue = totalHealth;
        HPBar.lowValue = recentHealth;
        HPBar.value = recentHealth;
        HPBar.title = "HP:" + HPBar.value + "/" + totalHealth;

        HealthText.text = "LV: " + (recentHPLV + 1).ToString();
        UpWeaponText.text =  (recentWeaponLV+1).ToString();
        UpClass1Text.text =  (recentClass1LV + 1).ToString();
        UpClass2Text.text =  (recentClass2LV + 1).ToString();

        if (recentWeaponLV == 9) UpWeaponTextHint.text = "Max level";
        else UpWeaponTextHint.text = PlayerStatus.weaponStat.upgradeHint[recentWeaponLV];

        if (recentClass1LV == 9) UpWeaponTextHint.text = "Max level";
        else UpClass1TextHint.text = PlayerStatus.activeClass.upgradeHint[recentClass1LV];

        if (recentClass1LV == 9) UpWeaponTextHint.text = "Max level";
        else UpClass2TextHint.text = PlayerStatus.passiveClass.upgradeHint[recentClass2LV];

    }
}
