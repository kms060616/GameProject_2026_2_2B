using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryOrderSystem : MonoBehaviour
{

    [Header("주문설정")]
    public float ordergenrateInterval = 15f;
    public int maxActiveOrders = 8;


    [Header("게임 상태")]
    public int totalOrdersGenerated = 0;
    public int completedOrders = 0;
    public int expiredOrders = 0;

    private List<DeliveryOrder> currentOrder = new List<DeliveryOrder>();

    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();


    [System.Serializable]
    public class OrderSystemEvent
    {
        public UnityEvent<DeliveryOrder> OnNewOrderAdded;
        public UnityEvent<DeliveryOrder> OnOrderPickedUp;
        public UnityEvent<DeliveryOrder> OnOrderCompleted;
        public UnityEvent<DeliveryOrder> OnOrderExpired;
    }

    public OrderSystemEvent orderEvents;
    public DeliveryDriver driver;



    void Start()
    {

        driver = FindFirstObjectByType<DeliveryDriver>();
        FindAllBuilding();


        StartCoroutine(GenerateInitialOrder());
        StartCoroutine(orderGenerator());
        StartCoroutine(ExpiredOrderChecker());
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 1300));

        GUILayout.Label("==배달주문==");
        GUILayout.Label($"활성 주문 : {GetCurrentOrders().Count}개");
        GUILayout.Label($"픽업 대기 : {GetPickWaitingCount()}개");
        GUILayout.Label($"배달 대기 : {GetDeliveryWaitingCount()}개");
        GUILayout.Label($"완료 : {completedOrders}개 | 만ㄹ : {expiredOrders}");


        GUILayout.Space(10);

        foreach(DeliveryOrder order in currentOrder)
        {
            string status = order.state == OrderState.WaitingPickip ? "픽업 대기" : "배달 대기";
            float timeLeft = order.GetRemainingTime();

            GUILayout.Label($"{order.orderId} : {order.restaurantName} -> {order.customerName}");
            GUILayout.Label($"{status} | {timeLeft:F0}초 남음");


        }

        GUILayout.EndArea();

    }

    void FindAllBuilding()
    {
        Building[] allBuildings = FindObjectsByType<Building>(FindObjectsSortMode.None);

        foreach(Building building in allBuildings)
        {
            if (building.BuildingType == BuildingType.Restaurant)
            {
                restaurants.Add(building);
            }
            else if(building.BuildingType == BuildingType.Custmer)
            {
                customers.Add(building);
            }

        }
        Debug.Log($"음식점 {restaurants.Count}개 , 고객 {customers.Count}명 발견");

    }

    void CreateNewOrder()
    {
        if (restaurants.Count == 0 || customers.Count == 0) return;

        Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
        Building randomCustomer = customers[Random.Range(0, customers.Count)];

        if(randomRestaurant == randomCustomer)
        {
            randomCustomer = customers[Random.Range(0, customers.Count)];
        }
        float reward = Random.Range(3000f, 8000f);

        DeliveryOrder newOrder = new DeliveryOrder(++totalOrdersGenerated, randomRestaurant, randomCustomer, reward);

        currentOrder.Add(newOrder);
        orderEvents.OnNewOrderAdded?.Invoke(newOrder);

    }

    void PickupOrder(DeliveryOrder order)
    {
        order.state = OrderState.PickedUp;
        orderEvents.OnOrderPickedUp?.Invoke(order);
    }
    void CompleteOrder(DeliveryOrder order)
    {
        order.state = OrderState.Completed;
        completedOrders++;

        if(driver != null)
        {
            driver.AddMoney(order.reward);
        }
        currentOrder.Remove(order);
        orderEvents.OnOrderCompleted?.Invoke(order);
    }

    void ExpireOrder(DeliveryOrder order)
    {
        order.state = OrderState.Epired;
        expiredOrders++;    

        currentOrder.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    public List<DeliveryOrder> GetCurrentOrders()
    {
        return new List<DeliveryOrder>(currentOrder);
    }

    public int GetPickWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrder)
        {
            if (order.state == OrderState.WaitingPickip) count++;

        }
        return count;   
    }

    public int GetDeliveryWaitingCount()
    {
        int count = 0;
        foreach(DeliveryOrder order in currentOrder)
        {
            if (order.state == OrderState.PickedUp) count++;
        }
        return count;
    }


    DeliveryOrder FingOrderForPicUp(Building restaurant)
    {
        foreach(DeliveryOrder order in currentOrder)
        {
            if(order.restaurantBuilding == restaurant && order.state == OrderState.WaitingPickip)
            {
                return order;
            }
        }
        return null;
    }

    DeliveryOrder FindOrderForDelivery(Building customer)
    {
        foreach (DeliveryOrder order in currentOrder)
        {
            if(order.customerBuilding == customer && order.state == OrderState.PickedUp)
            {
                return order;
            }
        }
        return null;
    }

    public void OnDriverEnteredRestaurant(Building restaurant)
    {
        DeliveryOrder orderToPickup = FindOrderForDelivery(restaurant);

        if (orderToPickup != null)
        {
            PickupOrder(orderToPickup);
        }
    }

    public void OnDriverEnteredCustorm(Building customer)
    {
        DeliveryOrder orderToDeliver = FindOrderForDelivery(customer);

        if(orderToDeliver != null)
        {
            CompleteOrder(orderToDeliver);
        }
    }

    IEnumerator GenerateInitialOrder()
    {
        yield return new WaitForSeconds(1f);

        for(int i = 0; i < 3;  i++)
        {
            CreateNewOrder();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator orderGenerator()
    {
        while(true)
        {
            yield return new WaitForSeconds(ordergenrateInterval);

            if(currentOrder.Count < maxActiveOrders)
            {
                CreateNewOrder();
            }
        }
    }

    IEnumerator ExpiredOrderChecker()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);
            List<DeliveryOrder> expiredOrder = new List<DeliveryOrder>();

            foreach(DeliveryOrder order in currentOrder)
            {
                if (order.IsExpired() && order.state != OrderState.Completed)
                {
                    expiredOrder.Add(order);
                }
            }

            foreach (DeliveryOrder expired in expiredOrder)
            {
                ExpireOrder(expired);
            }
        }
    }

}
