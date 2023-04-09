using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [Header("Global Reference")] [SerializeField]
    public PlayerStatus PlayerStatus;
    public GameFlowController GameFlowCtrl;

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
    public Label RestoreText;
    public Label UpHealthText;
    public Label UpWeaponText;
    public Label UpClass1Text;
    public Label UpClass2Text;
    public Label Item1Text;
    public Label Item2Text;
    public Label Item3Text;
    public Label Item4Text;
    public Label Item5Text;

    public VisualElement MainMenu;

    [SerializeField] private int recentHealth;
    [SerializeField] private int totalHealth;
    [SerializeField] private int recentMoney;
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

        Item1Button = root.Q<Button>("Item1_Button");
        Item2Button = root.Q<Button>("Item2_Button");
        Item3Button = root.Q<Button>("Item3_Button");
        Item4Button = root.Q<Button>("Item4_Button");

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
        RestoreText = root.Q<Label>("Restore_Money");
        UpHealthText = root.Q<Label>("Upgrade_Health_Money");
        UpWeaponText = root.Q<Label>("Upgrade_Weapon_Money");
        UpClass1Text = root.Q<Label>("Upgrade_Class1_Money");
        UpClass2Text = root.Q<Label>("Upgrade_Class2_Money");
        Item1Text = root.Q<Label>("Item1");
        Item2Text = root.Q<Label>("Item2");
        Item3Text = root.Q<Label>("Item3");
        Item4Text = root.Q<Label>("Item4");
        Item5Text = root.Q<Label>("Item5");

        QuitButton.clicked += QuitButtonPressed;

        Item1Button.clicked += Item1ButtonPressed;
        Item2Button.clicked += Item2ButtonPressed;
        Item3Button.clicked += Item3ButtonPressed;
        Item4Button.clicked += Item4ButtonPressed;

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

    }

    void UpHealthButtonPressed() 
    {
        //increase recent and total health
        Debug.Log("UpHealth pressed");

    }

    void UpWeaponButtonPressed() 
    {
        //increase weapon lv
        Debug.Log("increase weapon lv pressed");

    }

    void UpClass1ButtonPressed() 
    {
        //increase class 1 lv
        Debug.Log("increase class 1 lv pressed");

    }

    void UpClass2ButtonPressed() 
    {
        //increase class 2 lv
        Debug.Log("increase class 2 lv pressed");

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
        recentHealth = PlayerStatus.currentHP;
        totalHealth = PlayerStatus.maxHP;
        recentMoney = PlayerStatus.money;
        recentWeaponLV = PlayerStatus.weaponLv;
        
        if (PlayerStatus.activeClass != null) recentClass1LV = PlayerStatus.activeClass.lv;
        if (PlayerStatus.passiveClass != null) recentClass2LV = PlayerStatus.passiveClass.lv;

        MoneyText.text = recentMoney.ToString();
        HPBar.highValue = totalHealth;
        HPBar.lowValue = recentHealth;
        HPBar.value = recentHealth;
        HPBar.title = "HP:" + HPBar.value + "/" + totalHealth;
    }
}
