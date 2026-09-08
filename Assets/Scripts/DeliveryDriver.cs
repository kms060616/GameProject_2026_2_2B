
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DeliveryDriver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("배달원 설정")]
    public float moveSpeed = 8.0f;
    public float rotationSpeed = 10.0f;

    [Header("상태")]

    public float currentMoney = 0;
    public float batteryLevel = 100f;
    public int deliveryCount = 0;

    [System.Serializable]
    public class DriverEvents
    {
        [Header("이동 Event")]
        public UnityEvent OnMoveStarted;
        public UnityEvent OnMoveStoped;
        

        [Header("상태 변화 Event")]

        public UnityEvent<float> OnMoneyChanged;
        public UnityEvent<float> OnBetteryChanged;

        public UnityEvent<float> OnDeliveryCountChanged;

        [Header("경고Event")]

        public UnityEvent OnLowBattery;
        public UnityEvent OnLowBatterEmpty;
        public UnityEvent OnDeliveryCompleted;

    }

    public DriverEvents driverEvents;

    public bool isMoving = false;

    void Start()
    {
        driverEvents.OnMoneyChanged?.Invoke(currentMoney);
        driverEvents.OnBetteryChanged?.Invoke(batteryLevel);
        driverEvents.OnDeliveryCountChanged?.Invoke(deliveryCount);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    void ChangeBattery(float amount)
    {
        float oldBattery = batteryLevel;
        batteryLevel += amount;
        batteryLevel = Mathf.Clamp(batteryLevel, 0, 100);

        driverEvents.OnBetteryChanged?.Invoke(batteryLevel);

        if (oldBattery > 20f && batteryLevel <= 20f)
        {
            driverEvents.OnLowBattery?.Invoke();
        }

        if (oldBattery > 0f && batteryLevel <= 0f)
        {
            driverEvents.OnLowBatterEmpty?.Invoke();
        }
    }

    void HandleMovement()
    {
        Vector2 input = Keyboard.current != null ? new Vector2(
           (Keyboard.current.dKey.isPressed ? 1 : 0) -
           (Keyboard.current.aKey.isPressed ? 1 : 0),
           (Keyboard.current.wKey.isPressed ? 1 : 0) -
           (Keyboard.current.sKey.isPressed ? 1 : 0)
           ) : Vector2.zero;

        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

        if (moveDirection.magnitude > 0.1f)
        {
            if (!isMoving)
            {
                StartedMoving();
            }

            moveDirection = moveDirection.normalized;
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
            }
            ChangeBattery(-Time.deltaTime * 3.0f);
        }

        if (batteryLevel <= 0)
        {
            if(isMoving)
            {
                StopMoving();
            }
            return;
        }
    }
    void StartedMoving()
    {
        isMoving = true;
        driverEvents.OnMoveStarted?.Invoke();
    }

    void StopMoving()
    {
        isMoving = false;
        driverEvents.OnMoveStoped?.Invoke();
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
        driverEvents.OnMoneyChanged?.Invoke(currentMoney);
    }

    public void CompleteDelivery()
    {
        deliveryCount++;
        float reward = Random.Range(3000, 8000);
        AddMoney(reward);
        driverEvents.OnDeliveryCountChanged?.Invoke(deliveryCount);
        driverEvents.OnDeliveryCompleted?.Invoke();
    }
    public void ChargeBattery()
    {
        ChangeBattery(100f - batteryLevel);
    }

    public string GetStatusText()
    {
        return$"돈 : {currentMoney:F0} 원 | 베터리 : {batteryLevel:F1}% | 배달 : {deliveryCount}건";

    }

    public bool canMove()
    {

        return batteryLevel > 0;
    }

}
