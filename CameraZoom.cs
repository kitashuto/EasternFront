using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    Transform tf; //Main CameraのTransform
    Camera cam; //Main CameraのCamera
    private float scroll;
    private float speed;

    void Start()
    {
        speed = 6f;
        tf = this.gameObject.GetComponent<Transform>(); //Main CameraのTransformを取得する。
        cam = this.gameObject.GetComponent<Camera>(); //Main CameraのCameraを取得する。
    }

    void Update()
    {
        scroll = Input.GetAxis("Mouse ScrollWheel");
        
        cam.orthographicSize = cam.orthographicSize - scroll * speed; //ズームイン。
        if(cam.orthographicSize < 1.3f)
        {
            cam.orthographicSize = 1.3f;
        }
        else if (cam.orthographicSize > 11.5f)
        {
            cam.orthographicSize = 11.5f;
        }

        if (Input.GetKey(KeyCode.W)) //上キーが押されていれば
        {
            tf.position = tf.position + new Vector3(0.0f, 0.15f, 0.0f); //カメラを上へ移動。
        }
        else if (Input.GetKey(KeyCode.S)) //下キーが押されていれば
        {
            tf.position = tf.position + new Vector3(0.0f, -0.15f, 0.0f); //カメラを下へ移動。
        }
        if (Input.GetKey(KeyCode.A)) //左キーが押されていれば
        {
            tf.position = tf.position + new Vector3(-0.15f, 0.0f, 0.0f); //カメラを左へ移動。
        }
        else if (Input.GetKey(KeyCode.D)) //右キーが押されていれば
        {
            tf.position = tf.position + new Vector3(0.15f, 0.0f, 0.0f); //カメラを右へ移動。
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) //上キーが押されていれば
        {
            cam.orthographicSize = 5.5f;
        }

    }//-5.14, 5.14
}