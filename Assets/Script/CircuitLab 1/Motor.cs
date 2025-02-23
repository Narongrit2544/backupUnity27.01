using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Motor : CircuitComponent, IResistor
{
    public GameObject labelResistance;
    public TMP_Text labelResistanceText;
    public GameObject labelCurrent;
    public TMP_Text labelCurrentText;

    float speed = 0f;
    float baseCurrent = 0.005f;
    float normalSpeed = 600f;

    public double Resistance { get; private set; }

    public Motor()
    { 
        Resistance = 2000f;
    }

    protected override void Update () 
    {
        if (IsActive)
        {
            float step = speed * Time.deltaTime;
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Motor"))
                {
                    child.transform.Rotate(Vector3.forward, step);
                }
            }
        }
        labelResistance.gameObject.SetActive(IsActive && Lab.showLabels);
        labelCurrent.gameObject.SetActive(IsActive && Lab.showLabels);
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;
        labelResistanceText.text = Resistance.ToString("0.#") + "Ω";
        RotateLabel(labelResistance, LabelAlignment.Top);
        RotateLabel(labelCurrent, LabelAlignment.Bottom);
    }

    public override void SetCurrent(double current)
    {
        Current = current;
        if (!IsCurrentSignificant())
        {
            IsActive = false;
            labelResistance.gameObject.SetActive(false);
            labelCurrent.gameObject.SetActive(false);
        }
        else
        {
            labelCurrentText.text = (current * 1000f).ToString("0.#") + "mA";
            speed = normalSpeed * ((float)current / baseCurrent);
        }
    }

    // เมธอดสำหรับตั้งค่าความต้านทานจากข้อมูลที่ load เข้ามา
    public void SetResistance(double newResistance)
    {
        Resistance = newResistance;
        if (labelResistanceText != null)
        {
            labelResistanceText.text = newResistance.ToString("0.#") + "Ω";
        }
    }
}
