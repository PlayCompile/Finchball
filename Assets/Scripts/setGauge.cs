using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class setGauge : MonoBehaviour
{
    public float setValue;
    private float minScale = 0f;
    private float maxScale = 1f;
    private GameObject objInnerGauge;

    private Text txtGaugeVal;
    public InputActionAsset inputAss;
    private InputAction inputFillGauge;
    private InputAction inputEmptyGauge;

    void Start()
    {
        inputFillGauge = inputAss.FindAction("fillGauge");
        inputEmptyGauge = inputAss.FindAction("emptyGauge");
        objInnerGauge = transform.Find("innerGauge").gameObject;
        inputEmptyGauge.Disable();
        inputFillGauge.Disable();
        txtGaugeVal = GameObject.Find("Canvas/debug/txtGaugeVal").GetComponent<Text>();
    }

    void Update()
    {
        Vector3 getScale = objInnerGauge.transform.localScale;
        objInnerGauge.transform.localScale = new Vector3(getScale.x, getScale.y, setValue);

        txtGaugeVal.text = objInnerGauge.transform.localScale.z.ToString();
        if (inputEmptyGauge.IsPressed())
        {
            objInnerGauge.transform.localScale = new Vector3(getScale.x, getScale.y, getScale.z - 0.01f);
        }
        if (inputFillGauge.IsPressed())
        {
            objInnerGauge.transform.localScale = new Vector3(getScale.x, getScale.y, getScale.z + 0.01f);
        }
    }
}
