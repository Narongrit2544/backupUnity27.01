using UnityEngine;
using TMPro;
using System.Globalization;
using System.Collections.Generic;

public class Flute : CircuitComponent, IResistor
{
    [Header("UI สำหรับการตั้งค่า")]
    public GameObject infoCanvas;
    public TMP_Text labelResistanceText;
    public TMP_Text labelCurrentText;
    public TMP_Dropdown resistanceDropdown; // เปลี่ยนจาก InputField เป็น Dropdown
    public TMP_Text resistanceValueText;

    [Header("Resistor Color Bands")]
    public Renderer band1;
    public Renderer band2;
    public Renderer multiplierBand;
    public Renderer toleranceBand;

    [Header("Resistor Materials")]
    public Material[] resistorMaterials;

    [Header("เสียง")]
    public AudioClip pinchSound;   // เสียงเมื่อโดน pinch
    private AudioSource audioSource; // สำหรับเล่นเสียง

    private bool isCanvasVisible = false;
    public double resistanceValue = 220f;

    private readonly List<string> resistorValues = new List<string>
    {
        "10", "22", "47", "100", "220", "330", "470", "1k",
        "2.2k", "4.7k", "10k", "22k", "47k", "100k", "220k", "470k", "1M"
    };

    public double Resistance
    {
        get => resistanceValue;
        set
        {
            resistanceValue = value;
            UpdateResistanceLabel();
            UpdateResistorColorBands();
        }
    }

    public Flute()
    {
        Resistance = 1000f;
    }

    private void Start()
    {
        // ตรวจสอบ AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (resistanceDropdown != null)
        {
            resistanceDropdown.ClearOptions();
            resistanceDropdown.AddOptions(resistorValues);
            resistanceDropdown.onValueChanged.AddListener(delegate { UpdateResistanceFromDropdown(); });

            int defaultIndex = resistorValues.FindIndex(value => value == "220");
            if (defaultIndex != -1)
            {
                resistanceDropdown.value = defaultIndex;
                UpdateResistanceFromDropdown();
            }
        }

        UpdateResistorColorBands();
    }

    private void UpdateResistanceLabel()
    {
        if (labelResistanceText != null)
        {
            labelResistanceText.text = $"{Resistance:0.#} Ω";
        }
        if (resistanceValueText != null)
        {
            resistanceValueText.text = $"{Resistance:0.#} Ω";
        }
    }

    private void UpdateResistanceFromDropdown()
    {
        if (resistanceDropdown != null)
        {
            string selectedValue = resistanceDropdown.options[resistanceDropdown.value].text;
            Resistance = ConvertToOhms(selectedValue);
            UpdateResistanceLabel();
        }
    }

    private double ConvertToOhms(string value)
    {
        if (value.EndsWith("k"))
        {
            return double.Parse(value.Replace("k", "")) * 1000;
        }
        if (value.EndsWith("M"))
        {
            return double.Parse(value.Replace("M", "")) * 1000000;
        }
        return double.Parse(value);
    }

    private void UpdateResistorColorBands()
    {
        if (resistorMaterials == null || resistorMaterials.Length < 10)
        {
            Debug.LogWarning("⚠️ Resistor Materials ยังไม่ครบ 10 สี!"); 
            return;
        }

        int resistanceInt = Mathf.RoundToInt((float)Resistance);
        string resistanceString = resistanceInt.ToString();

        if (resistanceString.Length < 2)
        {
            resistanceString = resistanceString.PadRight(2, '0');
        }

        int digit1 = Mathf.Clamp(resistanceString[0] - '0', 0, 9);
        int digit2 = Mathf.Clamp(resistanceString[1] - '0', 0, 9);
        int multiplier = resistanceString.Length - 2;

        if (band1 != null) band1.material = resistorMaterials[digit1];
        if (band2 != null) band2.material = resistorMaterials[digit2];
        if (multiplierBand != null && multiplier >= 0 && multiplier <= 9)
        {
            multiplierBand.material = resistorMaterials[multiplier];
        }
        if (toleranceBand != null)
        {
            toleranceBand.material = resistorMaterials[1];
        }
    }

    protected override void Update()
    {
        if (Lab != null)
        {
            labelResistanceText.gameObject.SetActive(IsActive && Lab.showLabels);
            labelCurrentText.gameObject.SetActive(IsActive && Lab.showLabels);
        }
    }

    public override void SetActive(bool isActive, bool isForward)
    {
        IsActive = isActive;

        if (labelResistanceText != null)
        {
            labelResistanceText.text = $"{Resistance:0.#} Ω";
            RotateLabel(labelResistanceText.gameObject, LabelAlignment.Top);
        }

        if (labelCurrentText != null)
        {
            RotateLabel(labelCurrentText.gameObject, LabelAlignment.Bottom);
        }
    }

    public override void SetCurrent(double current)
    {
        Current = current;

        if (labelCurrentText != null)
        {
            labelCurrentText.text = $"{(current * 1000f):0.#} mA";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Pinch"))
        {
            // เล่นเสียง pinch ถ้ามีการตั้งค่า AudioClip
            if (pinchSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pinchSound);
            }
            isCanvasVisible = !isCanvasVisible;
            if (infoCanvas != null)
            {
                infoCanvas.SetActive(isCanvasVisible);
            }
        }
    }
}