using UnityEngine.UI;
using UnityEngine;

public class ChangeText : MonoBehaviour {
	public string infoText;

	// Use this for initialization
	void Start () {
        // text.text = "Suck a fuck";
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.Space))
        {
            // text.text = "Suck a duck";
        }
	}
}
