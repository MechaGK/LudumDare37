﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserCannon : MonoBehaviour {

	public float shootInterval;
	public Sprite[] anim;
	private SpriteRenderer spriteRenderer;
	private LineRenderer lineRenderer;

	private float shootTimer;
	private float invShootInterval;


	// Use this for initialization
	void Start () {
		if (anim.Length != 3) {
			Debug.LogError("Laser cannon sprite animation array must have excactly 3 elements.");
		}
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		shootTimer = 0;
		invShootInterval = 1f/shootInterval;

		lineRenderer.SetPositions(new Vector3[8]);
	}
	
	// Update is called once per frame
	void Update () {
		shootTimer += Time.deltaTime;

		float percentReadyToShoot = invShootInterval*shootTimer;

		if (percentReadyToShoot > 0.9f) {
			lineRenderer.enabled = true;
			Shoot();
			if (spriteRenderer.sprite != anim[2]) {
				spriteRenderer.sprite = anim[2];
			}
		}
		else {
			lineRenderer.enabled = false;
			if (percentReadyToShoot > 0.7f) {
				spriteRenderer.sprite = anim[1];
			}
			else {
				if (spriteRenderer.sprite != anim[0]) {
					spriteRenderer.sprite = anim[0];
				}
			}
		}

		if (percentReadyToShoot > 1) {
			shootTimer = 0;
		}
	}

	List<Vector3> hitLocations;

	private void Shoot() {
		hitLocations = new List<Vector3>();
		Vector3[] cardinalDirections = new Vector3[] {Vector3.up, Vector3.right, Vector3.down, Vector3.left};
		for (int i = 0; i < cardinalDirections.Length; ++i) {
			Vector3 dir = cardinalDirections[i];
			int layers = 1 << LayerMask.NameToLayer("Solid") | 1 << LayerMask.NameToLayer("Player");
			RaycastHit2D hit = Physics2D.Raycast(transform.position	, (Vector2)dir, Mathf.Infinity, layers);
			if (hit.collider != null) {
				if (hit.collider.CompareTag("Player")) {
					hit.collider.GetComponent<PlayerScript>().LaserHit();
				}
				hitLocations.Add(transform.position);
				hitLocations.Add(hit.point);
			}
		}

		lineRenderer.SetPositions(hitLocations.ToArray());
	}
}
