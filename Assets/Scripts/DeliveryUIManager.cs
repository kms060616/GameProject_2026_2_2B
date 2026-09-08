using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryUIManager : MonoBehaviour
{
    [Header("UI 요소")]
    public Text statusText;
    public Text messageText;
    public Slider batterSilder;

    public DeliveryDriver driver;

    public Image batteryFill;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(driver != null)
        {
            driver.driverEvents.OnMoneyChanged.AddListener(UpdateMoney);
            driver.driverEvents.OnBetteryChanged.AddListener(UpdateBattery);
            driver.driverEvents.OnDeliveryCountChanged.AddListener(UpdateDeliveryCount);
            driver.driverEvents.OnMoveStarted.AddListener(OnMoveStarted);
            driver.driverEvents.OnMoveStoped.AddListener(OnMoveStoped);
            driver.driverEvents.OnLowBattery.AddListener(OnLowBattery);
            driver.driverEvents.OnLowBatterEmpty.AddListener(OnBatteryEmpty);
            driver.driverEvents.OnDeliveryCompleted.AddListener(OnDeliveryCompleted);

        }
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(statusText != null && driver != null)
        {
            statusText.text = driver.GetStatusText();
        }
    }

    void ShowMessage(string message, Color color)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
            StartCoroutine(ClearMessageAfterDelay(2f));
        }
    }

    IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    void UpdateMoney(float money)
    {
        ShowMessage($"돈 : {money} 원, ", Color.green);
    }
    void UpdateBattery(float battery)
    {
        if (batterSilder != null)
        {
            batterSilder.value = battery / 100f;

        }
        if (batteryFill != null)
        {
            if (battery > 50f)
            {
                batteryFill.color = Color.green;
            }
            else if (battery > 20f)
            {

                batteryFill.color = Color.green;
            }
            else
            {
                batteryFill.color= Color.red;
            }
        }
    }
    //
    void UpdateDeliveryCount(float count)
    {
        ShowMessage($"배달완료 : {count}건", Color.blue);
        
    }

    void OnMoveStarted()
    {
        ShowMessage("이동시작", Color.cyan);
    }

    void OnMoveStoped()
        {
        ShowMessage("이동정지", Color.gray);
        }
    void OnLowBattery()
    {
        ShowMessage("베터리 부족!", Color.red);
    }

    void OnBatteryEmpty()
    {
        ShowMessage("베터리 방전", Color.cyan);
    }

void OnDeliveryCompleted()
    {
        ShowMessage("배달완료", Color.green);
    }

    void UpdateUI()
    {
        if(driver != null)
        {
            UpdateMoney(driver.currentMoney);
            UpdateBattery(driver.batteryLevel);
            UpdateDeliveryCount(driver.deliveryCount);
        }
    }

    void OnDestroy()
    {
        if (driver != null)
        {
            driver.driverEvents.OnMoneyChanged.AddListener(UpdateMoney);
            driver.driverEvents.OnBetteryChanged.AddListener(UpdateBattery);
            driver.driverEvents.OnDeliveryCountChanged.AddListener(UpdateDeliveryCount);
            driver.driverEvents.OnMoveStarted.AddListener(OnMoveStarted);
            driver.driverEvents.OnMoveStoped.AddListener(OnMoveStoped);
            driver.driverEvents.OnLowBattery.AddListener(OnLowBattery);
            driver.driverEvents.OnLowBatterEmpty.AddListener(OnBatteryEmpty);
            driver.driverEvents.OnDeliveryCompleted.AddListener(OnDeliveryCompleted);

        }
    }
}
