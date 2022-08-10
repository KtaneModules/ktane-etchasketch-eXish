using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class etchASketchScript : MonoBehaviour
{
	private Quaternion RootObjectOrientation
	{
		get
		{
			if (_rootXForm != null)
			{
				return _rootXForm.rotation;
			}
			return Quaternion.identity;
		}
	}

	private void Start()
	{
		_rootXForm = transform.root;
		_moduleId = _moduleIdCounter++;
		KMBombModule module = Module;
		module.OnActivate = (KMBombModule.KMModuleActivateEvent)Delegate.Combine(module.OnActivate, new KMBombModule.KMModuleActivateEvent(Activate));
	}

	private void Update()
	{
		ClearCheck();
	}

	private void Awake()
	{
		KMSelectable leftLeft = LeftLeft;
		leftLeft.OnInteract = (KMSelectable.OnInteractHandler)Delegate.Combine(leftLeft.OnInteract, new KMSelectable.OnInteractHandler(delegate()
		{
			StartCoroutine("MoveLeft");
			CheckEgg(0);
			return false;
		}));
		KMSelectable leftRight = LeftRight;
		leftRight.OnInteract = (KMSelectable.OnInteractHandler)Delegate.Combine(leftRight.OnInteract, new KMSelectable.OnInteractHandler(delegate()
		{
			StartCoroutine("MoveRight");
			CheckEgg(1);
			return false;
		}));
		KMSelectable rightLeft = RightLeft;
		rightLeft.OnInteract = (KMSelectable.OnInteractHandler)Delegate.Combine(rightLeft.OnInteract, new KMSelectable.OnInteractHandler(delegate()
		{
			StartCoroutine("MoveDown");
			CheckEgg(2);
			return false;
		}));
		KMSelectable rightRight = RightRight;
		rightRight.OnInteract = (KMSelectable.OnInteractHandler)Delegate.Combine(rightRight.OnInteract, new KMSelectable.OnInteractHandler(delegate()
		{
			StartCoroutine("MoveUp");
			CheckEgg(3);
			return false;
		}));
		KMSelectable leftLeft2 = LeftLeft;
		leftLeft2.OnInteractEnded = (Action)Delegate.Combine(leftLeft2.OnInteractEnded, new Action(delegate()
		{
			StopCoroutine("MoveLeft");
		}));
		KMSelectable leftRight2 = LeftRight;
		leftRight2.OnInteractEnded = (Action)Delegate.Combine(leftRight2.OnInteractEnded, new Action(delegate()
		{
			StopCoroutine("MoveRight");
		}));
		KMSelectable rightLeft2 = RightLeft;
		rightLeft2.OnInteractEnded = (Action)Delegate.Combine(rightLeft2.OnInteractEnded, new Action(delegate()
		{
			StopCoroutine("MoveDown");
		}));
		KMSelectable rightRight2 = RightRight;
		rightRight2.OnInteractEnded = (Action)Delegate.Combine(rightRight2.OnInteractEnded, new Action(delegate()
		{
			StopCoroutine("MoveUp");
		}));
		DrawNewPixel(Cursor.localPosition.x, Cursor.localPosition.z);
	}

	private IEnumerator MoveUp()
	{
		for (;;)
		{
			if (TopLeft.localPosition.z > Cursor.localPosition.z)
			{
				Cursor.Translate(new Vector3(0f, 0f, 0.001f));
			}
			DrawNewPixel(Cursor.localPosition.x, Cursor.localPosition.z);
			KnobRight.Rotate(new Vector3(0f, 0f, 2f));
			yield return new WaitForSeconds(moveDelay);
		}
	}

	private IEnumerator MoveDown()
	{
		for (;;)
		{
			if (BottomRight.localPosition.z < Cursor.localPosition.z)
			{
				Cursor.Translate(new Vector3(0f, 0f, -0.001f));
			}
			DrawNewPixel(Cursor.localPosition.x, Cursor.localPosition.z);
			KnobRight.Rotate(new Vector3(0f, 0f, -2f));
			yield return new WaitForSeconds(moveDelay);
		}
	}

	private IEnumerator MoveLeft()
	{
		for (;;)
		{
			if (TopLeft.localPosition.x < Cursor.localPosition.x)
			{
				Cursor.Translate(new Vector3(-0.001f, 0f, 0f));
			}
			DrawNewPixel(Cursor.localPosition.x, Cursor.localPosition.z);
			KnobLeft.Rotate(new Vector3(0f, 0f, -2f));
			yield return new WaitForSeconds(moveDelay);
		}
	}

	private IEnumerator MoveRight()
	{
		for (;;)
		{
			if (BottomRight.localPosition.x > Cursor.localPosition.x)
			{
				Cursor.Translate(new Vector3(0.001f, 0f, 0f));
			}
			DrawNewPixel(Cursor.localPosition.x, Cursor.localPosition.z);
			KnobLeft.Rotate(new Vector3(0f, 0f, 2f));
			yield return new WaitForSeconds(moveDelay);
		}
	}

	private void DrawNewPixel(float x, float z)
	{
		foreach (GameObject gameObject in Pixels)
		{
			if (gameObject.transform.localPosition.x == x && gameObject.transform.localPosition.z == z)
			{
				return;
			}
		}
		GameObject gameObject2 = Instantiate(Pixel);
		gameObject2.transform.SetParent(PixelContainer);
		gameObject2.transform.localPosition = new Vector3(x, Cursor.localPosition.y, z);
		Pixels.Add(gameObject2);
		totalDrawn++;
		if (totalDrawn >= 1000 && !_isSolved)
		{
			Module.HandlePass();
			_isSolved = true;
			Debug.LogFormat("[Etch-A-Sketch #{0}] You have made some wonderful art! Good job. Module solved.", new object[]
			{
				_moduleId
			});
		}
	}

	private void Activate()
	{
		_rootObjectLastOrientation = RootObjectOrientation;
	}

	private void ClearCheck()
	{
		Quaternion rootObjectOrientation = RootObjectOrientation;
		if (Mathf.Abs(Quaternion.Angle(rootObjectOrientation, _rootObjectLastOrientation)) >= MAXIMUM_ROTATION_VALUE)
		{
			GameObject[] array = new GameObject[Pixels.Count];
			Pixels.CopyTo(array);
			foreach (GameObject gameObject in array)
			{
				if (UnityEngine.Random.Range(0f, 100f) < 15f)
				{
					if (gameObject.GetComponent<PixelData>().Next())
					{
						gameObject.GetComponent<MeshRenderer>().material = PixelShaken;
					}
					else
					{
						Pixels.Remove(gameObject);
						Destroy(gameObject);
					}
				}
			}
		}
		_rootObjectLastOrientation = rootObjectOrientation;
	}

	private void CheckEgg(int value)
	{
		int[] collection = new int[]
		{
			3,
			3,
			2,
			2,
			0,
			1,
			0,
			1
		};
		int[] collection2 = new int[]
		{
			1,
			0,
			0,
			2,
			1,
			0,
			1,
			3,
			1,
			0,
			3,
			0,
			1,
			1,
			0,
			3
		};
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		list.AddRange(collection);
		list2.AddRange(collection2);
		easterEggs.Add(value);
		int[] array = new int[easterEggs.Count];
		List<int> list3 = new List<int>();
		easterEggs.CopyTo(array);
		list3.AddRange(array);
		while (list3.Count > 16)
		{
			list3.RemoveAt(0);
		}
		if (list3.Join(" ") == list2.Join(" "))
		{
			StartCoroutine("Bagels");
		}
		while (list3.Count > 8)
		{
			list3.RemoveAt(0);
		}
		if (list3.Join(" ") == list.Join(" "))
		{
			StartCoroutine("Konami");
		}
	}

	private IEnumerator Konami()
	{
		Module.GetComponentInChildren<KEgg>().Show();
		yield return new WaitForSeconds(1f);
		Module.GetComponentInChildren<KEgg>().Hide();
		yield break;
	}

	private IEnumerator Bagels()
	{
		Module.GetComponentInChildren<BEgg>().Show();
		yield return new WaitForSeconds(1f);
		Module.GetComponentInChildren<BEgg>().Hide();
		yield break;
	}

	public KMBombModule Module;

	public KMSelectable LeftLeft;

	public KMSelectable LeftRight;

	public KMSelectable RightLeft;

	public KMSelectable RightRight;

	public Transform KnobLeft;

	public Transform KnobRight;

	public Transform Cursor;

	public GameObject Pixel;

	public Material PixelShaken;

	public Transform TopLeft;

	public Transform BottomRight;

	public Transform PixelContainer;

	private List<GameObject> Pixels = new List<GameObject>();

	private static int _moduleIdCounter = 1;

	private int _moduleId;

	private float moveDelay = 0.02f;

	private int totalDrawn;

	private bool _isSolved;

	private List<int> easterEggs = new List<int>();

	private static readonly float MAXIMUM_ROTATION_VALUE = 30f;

	private Transform _rootXForm;

	private Quaternion _rootObjectLastOrientation = Quaternion.identity;
}