using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    public GameObject WinUI;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerHealth>().enabled == true)
        {
            Debug.Log("Goal Touch!");

            //Open Win Menu
            Invoke("ActiveWinUI", .5f);
            //Stop controller input
            Destroy(collision.gameObject, .6f);

        }
    }

    void ActiveWinUI()
    {
        WinUI.SetActive(true);
    }
}
