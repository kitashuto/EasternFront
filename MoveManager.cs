using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    Move forcusObj;
    AudioSource aud;
    public AudioClip moveOrder;

    public bool tf = true;
    public bool attatch = true;

    void Start()
    {
        aud = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (attatch == true)
        {
            MoveMethod();
        }
    }


    public void MoveMethod()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit2d = Physics2D.Raycast(ray.origin, ray.direction);

            if (tf == true && hit2d.collider != null)//レイとコライダーの接触がnullではない時？
            {
                var s = hit2d.collider.gameObject.GetComponent<Move>();//レイが当たったgameObjectにUnitMoveをアタッチしてsに代入。おそらくsはgameObjectタイプの変数。                

                if (s != null)
                {
                    switch (s.CurrentState)
                    {
                        case Move.State.Idle:
                            aud.PlayOneShot(moveOrder);
                            forcusObj = s;
                            forcusObj.Forcus();
                            tf = false;
                            break;
                        case Move.State.Move:

                            s.FocusOut();
                            break;
                    }
                }
            }
            else if (forcusObj != null)
            {
                switch (forcusObj.CurrentState)
                {
                    case Move.State.Ready:

                        forcusObj.MoveTo(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        tf = true;
                        break;
                }
            }
        }
    }
}

/*一体目を取得後、移動先を入力せずに二体目をクリックすると一体目が永久的にready状態となり、クリック不能になるバグがあった。
 * そこでbool型のtfを宣言し、rayを飛ばして兵士オブジェクトを取得するのを一体目の移動後のみにするためfalseにし、移動後に再びtrueに。
 */ 