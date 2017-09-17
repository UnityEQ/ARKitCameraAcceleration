using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour {

	//audio
	public AudioSource fart;
	//camera location
	public Transform camTrans;
	//Test text (debug only)
	public Text timeText;
	public Text accTest;
	public Text camTest;
	public Text maxYText;
	public Text minYText;
	public Text subText;
	//is jumping?
	public bool jumpBool;
	//temp values overwritten on every jump detect
	public float tempMin;
	public float tempMax;
	public float tempSub;
	public List<float> minY;
	public List<float> maxY;
	//Lowpass accel filter
	protected Queue<Vector3> filterDataQueue = new Queue<Vector3>();
	public int filterLength = 3; //you could change it in inspector

	// Use this for initialization
	void Start () {
		//lowpass filter stuff
		for(int i=0; i<filterLength; i++)
			filterDataQueue.Enqueue(Input.acceleration); //filling the queue to requered length
	}

	// Update is called once per frame
	void Update () {
		JumpDetection ();
	}

	//lowpass filter function
	public Vector3 LowPassAccelerometer() {
		if(filterLength <= 0)
			return Input.acceleration;
		filterDataQueue.Enqueue(Input.acceleration);
		filterDataQueue.Dequeue();

		Vector3 vFiltered= Vector3.zero;
		foreach(Vector3 v in filterDataQueue)
			vFiltered += v;
		vFiltered /= filterLength;
		return vFiltered;
	}

	//called once per frame
	public void JumpDetection()
	{
		//current y
		camTest.text = "cam: " + camTrans.position.y; //debug only
		Vector3 accUpdate = LowPassAccelerometer ();
		if (accUpdate.y > 0.16) {
			//find the min cameraY
			minY.Add(camTrans.position.y);
			//find the max cameraY
			maxY.Add(camTrans.position.y);
			//store math in temp vars
			tempMax = Mathf.Max(maxY.ToArray());
			tempMin = Mathf.Min(minY.ToArray());
			//accel
			accTest.text = "acc: " + accUpdate.y; //debug only
			//max camera Y
			maxYText.text = "maxy: " + tempMax; //debug only
			//min camera Y
			minYText.text = "miny: " + tempMin; //debug only
			//difference of max and min
			tempSub = tempMax - tempMin;
			subText.text = "sub: " + tempSub; //debug only
			//if difference is large enough, register as a jump.
			if (tempSub > 0.10f) {
				jumpBool = true;
				StartCoroutine (JumpFunction ());
			} 
		}
	}

	IEnumerator JumpFunction()
	{
		//float to store time
		float timeFrame = 0;
		//loop is true when accel > 0.16 and camera Y has a difference of 0.10
		while(jumpBool)
		{
			//set time during this loop duration
			timeFrame += Time.deltaTime;
			timeText.text = "time: " + timeFrame; //debug only
			//when camera Y is less than the Y detected at jump start, do something 
			if(camTrans.position.y < tempMin)
			{
				jumpBool = false;
				fart.Play ();
				//clear lists
				maxY.Clear ();
				minY.Clear ();
			}
			//if timeframe reaches 1 second, exit loop, because a false jump was detected
			if (timeFrame > 1) 
			{
				jumpBool = false;
				//clear lists
				maxY.Clear ();
				minY.Clear ();
			}
			yield return null;
		}
	}
}
