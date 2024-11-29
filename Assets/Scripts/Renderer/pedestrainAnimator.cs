using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pedestrainAnimator : MonoBehaviour
{
	public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
	private void Awake() 
	{
		try
		{
			animator = GetComponent<Animator> ();
		}
		catch { }
		if (animator)
		{
			animator.CrossFadeInFixedTime ("walk", 0f, 0, 0f);
		}
	}
    void Update()
    {

    }
}
