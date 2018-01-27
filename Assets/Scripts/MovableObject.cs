using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour {
    // Use this for initialization
    float startX;
    float startY;
    float xRate;
    float yRate;

    void Start () {
        startX = transform.position.x;
        startY = transform.position.y;
    }
	
    //Default is for a vertical door moving up
    public void Open(float xRate_ = 0, float yRate_ = .1f)
    {
        xRate = xRate_;
        yRate = yRate_;
        gameObject.transform.Translate(xRate, yRate, 0);
    }

	// Update is called once per frame
	void Update () {
        if (transform.position.x != startX)
            gameObject.transform.Translate(-xRate/5, 0, 0);
        if (transform.position.y != startY)
            gameObject.transform.Translate(0, -yRate/5, 0);
    }
}
