using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public float speed = 8.3f;
    public float rotSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = this.transform.position;
        Vector3 rot = this.transform.rotation.eulerAngles;
        //pos.x += Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        //pos.y += Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;
        pos += this.transform.up * Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;
        rot.z -= Input.GetAxisRaw("Horizontal") * rotSpeed * Time.deltaTime;
        this.transform.position = pos;
        this.transform.rotation = Quaternion.Euler(rot);
    }
}
