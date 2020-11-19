using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    AudioSource aud;
    public AudioClip weaponAttatch;
    public AudioClip buttonClickWeapon;

    InfantryAttackController infantryAttackController;
    InfantryAnimation infantryAnimation;
    MoveManager moveManager;

    public int givenAttackPower;
    public float givenSoldierRange;
    public int givenClip;
    public float givenMinSpan;
    public float givenMaxSpan;
    public string weaponName;
    public string givenAudioClipName;
    public float givenShootEndRag;

    public string compareTagName;


    public GameObject menuButton;

    //SecondMenuButton
    public GameObject unitButton;
    public GameObject weaponButton;
    public GameObject upgradeButton;
    public GameObject commandButton;
    public GameObject trenchButton;
    public GameObject backToMenuButton;

    //UnitButton
    public GameObject infantryButton;
    public GameObject medicButton;
    public GameObject officerButton;
    public GameObject tankButton;
    public GameObject backToUnitButton;

    //WeaponButton
    public GameObject mk3Button;
    public GameObject p14Button;
    public GameObject vickersButton;
    public GameObject lewisButton;
    public GameObject webleyButton;
    public GameObject backToWeaponButton;

    //UpgradeButton
    public GameObject trainingButton;
    public GameObject ammoCarrierButton;
    public GameObject machineGunButton;
    public GameObject sniperButton;
    public GameObject backToUpgradeButton;

    //CommandButton
    public GameObject bombardmentButton;
    public GameObject backToCommandButton;

    //TrenchButton
    public GameObject ammoDumpButton;
    public GameObject machineGunPositionButton;
    public GameObject shelterButton;
    public GameObject backToTrenchButton;


    public GameObject[] secondMenu;
    public GameObject[] unitMenu;
    public GameObject[] weaponMenu;
    public GameObject[] upgradeMenu;
    public GameObject[] commandMenu;
    public GameObject[] trenchMenu;



    public bool flg = false;


    // Start is called before the first frame update
    void Start()
    {
        moveManager = gameObject.GetComponent<MoveManager>();


        secondMenu = new GameObject[] { unitButton, weaponButton, upgradeButton, commandButton, trenchButton, backToMenuButton };

        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(false);
        }


        unitMenu = new GameObject[] { infantryButton, medicButton, officerButton, tankButton, backToUnitButton };

        for (int i = 0; i < 5; i++)
        {
            unitMenu[i].SetActive(false);
        }


        weaponMenu = new GameObject[] { mk3Button, p14Button, vickersButton, lewisButton, webleyButton, backToWeaponButton };

        for (int i = 0; i < 6; i++)
        {
            weaponMenu[i].SetActive(false);
        }


        upgradeMenu = new GameObject[] { trainingButton, ammoCarrierButton, machineGunButton, sniperButton, backToUpgradeButton };

        for (int i = 0; i < 5; i++)
        {
            upgradeMenu[i].SetActive(false);
        }


        commandMenu = new GameObject[] { bombardmentButton, backToCommandButton };

        for (int i = 0; i < 2; i++)
        {
            commandMenu[i].SetActive(false);
        }


        trenchMenu = new GameObject[] { ammoDumpButton, machineGunPositionButton, shelterButton, backToTrenchButton };

        for (int i = 0; i < 4; i++)
        {
            trenchMenu[i].SetActive(false);
        }



        aud = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {       
        if(flg == true)
        {
            ClickObject();            
        }
        else
        {
            moveManager.attatch = true;
        }
    }


    //オブジェクトクリック関数
    public void ClickObject()
    {

        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit2d = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit2d.collider != null)//レイとコライダーの接触がnullではない時？
            {
                var v = hit2d.collider.gameObject.GetComponent<InfantryAnimation>();
                if (v.CompareTag(compareTagName))
                {
                    if (v != null)
                    {
                        v.course = weaponName;
                        v.t = true;
                        flg = false;

                    }

                    var s = hit2d.collider.gameObject.GetComponent<InfantryAttackController>();//レイが当たったgameObjectにUnitMoveをアタッチしてsに代入。おそらくsはgameObjectタイプの変数。                

                    if (s != null)
                    {
                        s.attackPower = givenAttackPower;
                        s.soldierRange = givenSoldierRange;
                        s.clip = givenClip;
                        s.minSpan = givenMinSpan;
                        s.maxSpan = givenMaxSpan;
                        s.audioClipName = givenAudioClipName;
                        s.shootEndRag = givenShootEndRag;

                        s.attackOrder = false;

                        aud.PlayOneShot(weaponAttatch);

                    }
                }

            }            
        }

        

    }




    //メインメニューバー
    public void MenuButton()
    {
        menuButton.SetActive(false);
        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(true);
        }
    }


    //セカンドメニューバー
    public void UnitButton()
    {
        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(false);
        }

        for (int i = 0; i < 5; i++)
        {
            unitMenu[i].SetActive(true);
        }
    }



    public void WeaponButton()
    {
        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(false);
        }

        for (int i = 0; i < 6; i++)
        {
            weaponMenu[i].SetActive(true);
        }
    }



    public void UpgradeButton()
    {
        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(false);
        }

        for (int i = 0; i < 5; i++)
        {
            upgradeMenu[i].SetActive(true);
        }
    }



    public void CommandButton()
    {
        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(false);
        }

        for (int i = 0; i < 2; i++)
        {
            commandMenu[i].SetActive(true);
        }
    }



    public void TrenchButton()
    {
        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(false);
        }

        for (int i = 0; i < 4; i++)
        {
            trenchMenu[i].SetActive(true);
        }
    }



    public void BackToMenuButton()
    {
        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(false);
        }

        menuButton.SetActive(true);
    }



    //ユニットメニュー
    public void InfantryButton()
    {
        
    }



    public void MedicButton()
    {

    }



    public void OfficerButton()
    {

    }



    public void TankButton()
    {

    }



    public void BackToUnitButton()
    {
        for (int i = 0; i < 5; i++)
        {
            unitMenu[i].SetActive(false);
        }

        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(true);
        }
    }



    //ウェポンメニュー
    public void MK3Button()
    {
        //tag; infantry || sniper   at=35, range=3.5, clip=10

        flg = true;
        aud.PlayOneShot(buttonClickWeapon);
        moveManager.attatch = false;

        compareTagName = "Infantry";
        weaponName = "Mk3";

        givenAttackPower = 35;
        givenSoldierRange = 3f;
        givenClip = 10;
        givenMinSpan = 1.5f;
        givenMaxSpan = 1.5f;
        givenAudioClipName = "Mk3SE";
        givenShootEndRag = 1.5f;
    }



    public void P14Button()
    {
        //tag; infantry || sniper   at=45, range=4, clip=5

        flg = true;
        aud.PlayOneShot(buttonClickWeapon);
        moveManager.attatch = false;

        compareTagName = "Infantry";
        weaponName = "P14";

        givenAttackPower = 50;
        givenSoldierRange = 4f;
        givenClip = 5;
        givenMinSpan = 2f;
        givenMaxSpan = 2f;
        givenAudioClipName = "P14SE";
        givenShootEndRag = 2f;

    }



    public void VickersButton()
    {
        //tag; gunner
    }



    public void LewisButton()
    {
        //tag; gunner
    }



    public void WebleyButton()
    {
        //tag; medic || officer
    }



    public void BackToWeaponButton()
    {
        for (int i = 0; i < 6; i++)
        {
            weaponMenu[i].SetActive(false);
        }

        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(true);
        }
    }



    //アップグレードメニュー
    public void TrainingButton()
    {

    }



    public void AmmoCarrierButton()
    {

    }



    public void MachineGunButton()
    {

    }



    public void SniperButton()
    {

    }



    public void BackToUpgradeButton()
    {
        for (int i = 0; i < 5; i++)
        {
            upgradeMenu[i].SetActive(false);
        }

        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(true);
        }
    }



    //コマンドメニュー
    public void BombardmentButton()
    {

    }



    public void BackToCommandButton()
    {
        for (int i = 0; i < 2; i++)
        {
            commandMenu[i].SetActive(false);
        }

        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(true);
        }
    }



    //トレンチメニュー
    public void AmmoDumpButton()
    {

    }



    public void MachineGunPositionButton()
    {

    }



    public void ShelterButton()
    {

    }



    public void BackToTrenchButton()
    {
        for (int i = 0; i < 4; i++)
        {
            trenchMenu[i].SetActive(false);
        }

        for (int i = 0; i < 6; i++)
        {
            secondMenu[i].SetActive(true);
        }
    }

}

