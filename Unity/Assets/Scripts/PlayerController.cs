// using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public GameObject coinText;
    // Start is called before the first frame update
    private CharacterController controller;
    private Vector3 direction;
    public float forwardSpeed;
    // public PoseDetection obj=new PoseDetection();
    public int desiredLane=1;
    public float laneDistance=4;
    private bool hasHitObstacle = false;

    Thread receiveThread; //1
	UdpClient client; //2
	int port; //3
    Vector3 diff =  new Vector3(0f,5f,3f);

    void Start()
    {
        port = 5095;
        controller=GetComponent<CharacterController>();
        InitUDP(); 
    }
     void OnDisable()
    {
        StopUDP();
    }
     public void StopUDP()
    {
        if (client != null)
        {
            client.Close();
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
    }
    // 3. InitUDP
	public void InitUDP()
	{
		print ("UDP Initialized");

		receiveThread = new Thread (new ThreadStart(ReceiveData)); //1 
		receiveThread.IsBackground = true; //2
		receiveThread.Start (); //3

	}
    private void ReceiveData()
	{
		client = new UdpClient (port); //1
		while (true) //2
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), port); //3
				byte[] data = client.Receive(ref anyIP); //4

				string text = Encoding.UTF8.GetString(data); //5
				print (">> " + text);

				
                Debug.Log(text);
                if(text=="Center"){
                    desiredLane=1;
                }
                if(text=="Left"){
                    desiredLane=0;
                }
                if(text=="Right"){
                    desiredLane=2;
                }
			} catch(Exception e)
			{
				print (e.ToString()); //7
			}
		}
	}
    // Update is called once per frame
    void Update()
    {
        if(!PlayerManager.isGameStarted){
            return ;
        }
        direction.z = forwardSpeed;

        // obj= new PoseDetection();
        // desiredLane= obj.desiredLane;
        

        if(SwipeManager.swipeRight){
            desiredLane++;
            if(desiredLane==3){
                desiredLane=2;
            }
        }
         if(SwipeManager.swipeLeft){
            desiredLane--;
            if(desiredLane==-1){
                desiredLane=0;
            }
        }
        
        Vector3 targetPosition = transform.position.z*transform.forward+transform.position.y*transform.up;
        if(desiredLane==0){
            targetPosition+= Vector3.left *laneDistance;
        }
        else if(desiredLane==2){
            targetPosition+= Vector3.right *laneDistance;
        }
        if(transform.position==targetPosition){
            return;
        }Vector3 diff = targetPosition-transform.position;
        Vector3 moveDir = diff.normalized*25*Time.deltaTime;
        if(moveDir.sqrMagnitude<diff.sqrMagnitude)
            controller.Move(moveDir);
        else
            controller.Move(diff);
        // transform.position=targetPosition;
        // controller.center=controller.center;
        hasHitObstacle = false;
    }
    private void FixedUpdate()
    {
        if(!PlayerManager.isGameStarted){
            return ;
        }
        controller.Move(direction * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider hit)
    {
        if (!hasHitObstacle && hit.transform.CompareTag("Obstacle")) // Check the flag and the tag
    {
        if (PlayerManager.numberofCoins >= 3) // Check if there are enough coins to deduct
        {
            GameObject sc = Instantiate(coinText, transform.position + diff, Quaternion.identity);
            PlayerManager.numberofCoins -= 3;
            hasHitObstacle = true; // Set the flag to true to prevent further deductions in this frame
        }
    }
    }
}
