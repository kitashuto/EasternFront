using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// StageUI(ステージ数のUI/進行ボタン/街に戻るボタン)の管理
public class UIManager : MonoBehaviour
{
    public Text hpText;

    public void UpdateHP(int hp)
    {
        hpText.text = hp.ToString();
    }
}
