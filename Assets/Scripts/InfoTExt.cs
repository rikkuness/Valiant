using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoTExt : MonoBehaviour {

	public string Heading;
	public string Body;

	void OnCollisionEnter(Collision Target) {
		Debug.Log(Target);
	}
}
