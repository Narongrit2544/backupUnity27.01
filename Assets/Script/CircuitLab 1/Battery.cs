using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Battery : CircuitComponent, IBattery
{
    public GameObject labelVoltage;
    public TMP_Text labelVoltageText;

    private double batteryVoltage = 5f;
    public double BatteryVoltage
    {
        get { return batteryVoltage; }
        set
        {
            batteryVoltage = value;
            if (labelVoltageText != null)
            {
                labelVoltageText.text = batteryVoltage.ToString("0.#") + "V";
            }
        }
    }

    public Battery()
    {
        BatteryVoltage = 5f;
    }

    protected override void Start()
    {
        labelVoltageText.text = BatteryVoltage.ToString("0.#") + "V";
    }

    protected override void Update()
    {
        labelVoltage.gameObject.SetActive(IsActive && Lab.showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;
        var rotationVoltage = labelVoltage.transform.localEulerAngles;
        var positionVoltage = labelVoltage.transform.localPosition;
        switch (Direction)
        {
            case Direction.North:
                rotationVoltage.z = 180f;
                break;
            case Direction.South:
                rotationVoltage.z = 0f;
                break;
            case Direction.East:
                rotationVoltage.z = -90f;
                break;
            case Direction.West:
                rotationVoltage.z = 90f;
                break;
            default:
                Debug.Log("Unrecognized direction!");
                break;
        }
        labelVoltage.transform.localEulerAngles = rotationVoltage;
    }
}
