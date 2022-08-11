using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
		gameObject2.transform.localScale = new Vector3(0.0012f, 0.0012f, 0.0012f);
		Pixels.Add(gameObject2);
		totalDrawn++;
		if (totalDrawn >= 800 && !_isSolved)
		{
			Module.HandlePass();
			_isSolved = true;
			Debug.LogFormat("[Etch-A-Sketch #{0}] You have made some {1} art! Good job. Module solved.", new object[]
			{
				_moduleId,
				new string[]{ "wonderful", "splendid", "fantastic", "exemplary", "decent", "honestly pretty bad", "mugumphrous" }.PickRandom()
			});
			if (TwitchPlaysActive)
				StopAllCoroutines();
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
					Pixels.Remove(gameObject);
					Destroy(gameObject);
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
			3,
			2,
			0,
			3,
			0,
			3,
			0,
			0,
			1,
			1,
			2
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
		while (list3.Count > 11)
		{
			list3.RemoveAt(0);
		}
		if (list3.Join(" ") == list2.Join(" "))
		{
			StartCoroutine("Blan");
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

	private IEnumerator Blan()
	{
		Module.GetComponentInChildren<BEgg>().Show();
		yield return new WaitForSeconds(1f);
		Module.GetComponentInChildren<BEgg>().Hide();
		yield break;
	}

	private IEnumerator ProcessTwitchCommand(string command)
    {
		command = Regex.Replace(command, @"\s+", " ");
		string[] parameters = command.Split(new char[]{',',';'});
		for (int i = 0; i < parameters.Length; i++)
        {
			string[] temp = parameters[i].Trim().Split(' ');
			if (temp.Length != 3)
			{
				yield return "sendtochaterror!f Invalid number of parameters for the command '" + parameters[i] + "'!";
				yield break;
			}
			if (!temp[0].ToLowerInvariant().EqualsAny("left", "right"))
            {
				yield return "sendtochaterror!f The specified knob '" + temp[0] + "' is invalid!";
				yield break;
            }
			if (!temp[1].ToLowerInvariant().EqualsAny("cw", "ccw"))
			{
				yield return "sendtochaterror!f The specified direction '" + temp[1] + "' is invalid!";
				yield break;
			}
			float time;
			if (!float.TryParse(temp[2], out time))
			{
				yield return "sendtochaterror!f The specified time '" + temp[2] + "' is invalid!";
				yield break;
			}
			if (time <= 0)
			{
				yield return "sendtochaterror The specified time '" + temp[2] + "' cannot be negative or zero!";
				yield break;
			}
		}
		yield return null;
		for (int i = 0; i < parameters.Length; i++)
		{
			string[] temp = parameters[i].ToLowerInvariant().Trim().Split(' ');
			if (temp[0] == "left" && temp[1] == "ccw")
				LeftLeft.OnInteract();
			else if (temp[0] == "left" && temp[1] == "cw")
				LeftRight.OnInteract();
			else if (temp[0] == "right" && temp[1] == "ccw")
				RightLeft.OnInteract();
			else
				RightRight.OnInteract();
			float time = float.Parse(temp[2]);
			float t = 0f;
			while (t < time)
            {
				yield return null;
				if (TwitchShouldCancelCommand)
					break;
				t += Time.deltaTime;
            }
			if (temp[0] == "left" && temp[1] == "ccw")
				LeftLeft.OnInteractEnded();
			else if (temp[0] == "left" && temp[1] == "cw")
				LeftRight.OnInteractEnded();
			else if (temp[0] == "right" && temp[1] == "ccw")
				RightLeft.OnInteractEnded();
			else
				RightRight.OnInteractEnded();
			if (TwitchShouldCancelCommand)
				yield return "cancelled";
		}
	}

	private IEnumerator TwitchHandleForcedSolve()
    {
		while (!_isSolved)
        {
			float time = UnityEngine.Random.Range(0.1f, 2f);
			int dir = UnityEngine.Random.Range(0, 4);
			float t = 0f;
			switch (dir)
            {
				case 0:
					LeftLeft.OnInteract();
					while (t < time && !_isSolved && TopLeft.localPosition.x < Cursor.localPosition.x)
                    {
						yield return null;
						t += Time.deltaTime;
					}
					LeftLeft.OnInteractEnded();
					break;
				case 1:
					LeftRight.OnInteract();
					while (t < time && !_isSolved && BottomRight.localPosition.x > Cursor.localPosition.x)
					{
						yield return null;
						t += Time.deltaTime;
					}
					LeftRight.OnInteractEnded();
					break;
				case 2:
					RightLeft.OnInteract();
					while (t < time && !_isSolved && BottomRight.localPosition.z < Cursor.localPosition.z)
					{
						yield return null;
						t += Time.deltaTime;
					}
					RightLeft.OnInteractEnded();
					break;
				default:
					RightRight.OnInteract();
					while (t < time && !_isSolved && TopLeft.localPosition.z > Cursor.localPosition.z)
					{
						yield return null;
						t += Time.deltaTime;
					}
					RightRight.OnInteractEnded();
					break;
			}
		}
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

	private bool TwitchShouldCancelCommand;

	private bool TwitchPlaysActive;

	private readonly string TwitchHelpMessage = "!{0} left/right <cw/ccw> <time> [Turns the left or right knob clockwise or counter-clockwise for a certain number of seconds] | Commands can be chained using semicolons or commas";
}