using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUiBehaviour : MonoBehaviour
{
    public static GameUiBehaviour Instance;
    private void Awake()
    {
        Instance = this;
    }

    public Transform powerBar;

    public void UpdatePowerBar(float power, float maxPower)
    {
        powerBar.localScale = new Vector3(power / maxPower, 1, 1);
    }
}
