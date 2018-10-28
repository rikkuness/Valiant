using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateHUD : MonoBehaviour {
	public GameObject HUD;

	public string TitleText;
	public string MessageText;

	private Text title;
	private Text body;

	private void OnEnable() {
		title = GameObject.Find(HUD.name+"/UIHeading").GetComponent<Text>();
		body = GameObject.Find(HUD.name+"/UIBody").GetComponent<Text>();
	}

	void OnTriggerEnter(Collider other) {
		if (other.name == "HeadCollider") {
			gameObject.GetComponent<AudioSource>().Play();
			title.text = TitleText;
			body.text = MessageText;
		}
	}
}
