using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002AB RID: 683
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Event-Linkers/AnimatorParamController.html")]
	[AddComponentMenu("ThunderRoad/Animator Param Controller")]
	[RequireComponent(typeof(Animator))]
	public class AnimatorParamController : MonoBehaviour
	{
		// Token: 0x170001EF RID: 495
		// (get) Token: 0x06001FA6 RID: 8102 RVA: 0x000D702C File Offset: 0x000D522C
		// (set) Token: 0x06001FA7 RID: 8103 RVA: 0x000D7034 File Offset: 0x000D5234
		public Animator animator { get; protected set; }

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x06001FA8 RID: 8104 RVA: 0x000D703D File Offset: 0x000D523D
		// (set) Token: 0x06001FA9 RID: 8105 RVA: 0x000D7045 File Offset: 0x000D5245
		public Item item { get; protected set; }

		// Token: 0x06001FAA RID: 8106 RVA: 0x000D7050 File Offset: 0x000D5250
		protected void Start()
		{
			this.animator = base.GetComponent<Animator>();
			if (this.animator == null)
			{
				Debug.LogError("AnimatorParamController is on GameObject \"" + base.gameObject.name + "\" and has no animator on it. This shouldn't happen. The script will not function.");
				return;
			}
			this.item = base.GetComponentInParent<Item>();
			if (this.item != null)
			{
				if (this.saveValuesOnStore)
				{
					this.item.OnContainerAddEvent += this.StoreParamValues;
				}
				if (this.loadValuesOnLoad)
				{
					ContentCustomDataAnimatorParams customData;
					this.item.TryGetCustomData<ContentCustomDataAnimatorParams>(out customData);
					if (customData != null)
					{
						using (List<ContentCustomDataAnimatorParams.AnimatorParam>.Enumerator enumerator = customData.savedParams.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								ContentCustomDataAnimatorParams.AnimatorParam param = enumerator.Current;
								switch (param.type)
								{
								case AnimatorControllerParameterType.Float:
									this.animator.SetFloat(param.name, param.floatVal);
									break;
								case AnimatorControllerParameterType.Int:
									this.animator.SetInteger(param.name, param.intVal);
									break;
								case AnimatorControllerParameterType.Bool:
									this.animator.SetBool(param.name, param.boolVal);
									break;
								}
							}
							return;
						}
					}
					this.StoreParamValues(null);
					return;
				}
			}
			else if (this.saveValuesOnStore || this.loadValuesOnLoad)
			{
				Debug.LogError("AnimatorParamController on GameObject \"" + base.gameObject.name + "\" is set to store or load values, but isn't on an item to be able to store or load values!");
			}
		}

		// Token: 0x06001FAB RID: 8107 RVA: 0x000D71D4 File Offset: 0x000D53D4
		private void StoreParamValues(Container _)
		{
			if (this.animator == null)
			{
				this.animator = base.GetComponent<Animator>();
			}
			if (this.animator == null)
			{
				Debug.LogError(base.gameObject.name + " has no Animator on it!");
			}
			if (this.item == null)
			{
				this.item = base.transform.root.GetComponentInChildren<Item>();
			}
			if (this.item == null)
			{
				Debug.LogError(base.gameObject.name + " has an Animator, but no Item on it to be able to save parameter values!");
			}
			if (this.item.HasCustomData<ContentCustomDataAnimatorParams>())
			{
				this.item.RemoveCustomData<ContentCustomDataAnimatorParams>();
			}
			this.item.AddCustomData<ContentCustomDataAnimatorParams>(new ContentCustomDataAnimatorParams(this.animator));
		}

		// Token: 0x06001FAC RID: 8108 RVA: 0x000D7295 File Offset: 0x000D5495
		public void SetTrigger(string triggerName)
		{
			this.animator.SetTrigger(Animator.StringToHash(triggerName));
		}

		// Token: 0x06001FAD RID: 8109 RVA: 0x000D72A8 File Offset: 0x000D54A8
		public void BoolOperation(string input)
		{
			if (this.animator == null)
			{
				return;
			}
			AnimatorParamController.ParsedOperation parsedOperation;
			if (!this.ParseInput(input, out parsedOperation))
			{
				Debug.LogError("Could not process input bool operation\nError from: " + parsedOperation.originalOperation);
				return;
			}
			if (" &|#".Contains(parsedOperation.oper.ToString()))
			{
				bool lhs;
				if (!this.StringToBool(parsedOperation.lhs, out lhs))
				{
					if (!(parsedOperation.lhs == "~"))
					{
						Debug.LogError("Type error: First value in operation is not a valid boolean\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation);
						return;
					}
					lhs = (UnityEngine.Random.Range(0, 2) == 0);
				}
				if (parsedOperation.oper == ' ')
				{
					this.animator.SetBool(parsedOperation.paramHash, lhs);
					return;
				}
				bool rhs;
				if (!this.StringToBool(parsedOperation.rhs, out rhs))
				{
					Debug.LogError("Type error: Second value in operation is not a valid boolean\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				char oper = parsedOperation.oper;
				if (oper == '#')
				{
					this.animator.SetBool(parsedOperation.paramHash, (lhs || rhs) && (!lhs || !rhs));
					return;
				}
				if (oper == '&')
				{
					this.animator.SetBool(parsedOperation.paramHash, lhs && rhs);
					return;
				}
				if (oper != '|')
				{
					return;
				}
				this.animator.SetBool(parsedOperation.paramHash, lhs || rhs);
				return;
			}
			else
			{
				float lhs2;
				if (!this.StringToFloat(parsedOperation.lhs, out lhs2))
				{
					Debug.LogError("Type error: First value in comparison is not a valid number\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				float rhs2;
				if (!this.StringToFloat(parsedOperation.rhs, out rhs2))
				{
					Debug.LogError("Type error: Second value in comparison is not a valid number\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				switch (parsedOperation.oper)
				{
				case '<':
					this.animator.SetBool(parsedOperation.paramHash, lhs2 < rhs2);
					return;
				case '=':
					this.animator.SetBool(parsedOperation.paramHash, Mathf.Approximately(lhs2, rhs2));
					return;
				case '>':
					this.animator.SetBool(parsedOperation.paramHash, lhs2 > rhs2);
					return;
				default:
					return;
				}
			}
		}

		// Token: 0x06001FAE RID: 8110 RVA: 0x000D74A0 File Offset: 0x000D56A0
		private bool StringToBool(string input, out bool value)
		{
			bool invertOutput = input.Contains("!");
			string cleaned = input.Replace("!", "");
			if (cleaned.ToLower() == "true" || cleaned.ToLower() == "false")
			{
				value = (cleaned.ToLower() == "true");
				if (invertOutput)
				{
					value = !value;
				}
				return true;
			}
			if (this.CheckForParam(cleaned) == 1)
			{
				value = this.animator.GetBool(Animator.StringToHash(cleaned));
				if (invertOutput)
				{
					value = !value;
				}
				return true;
			}
			value = false;
			return false;
		}

		// Token: 0x06001FAF RID: 8111 RVA: 0x000D753C File Offset: 0x000D573C
		public void IntegerOperation(string input)
		{
			if (this.animator == null)
			{
				return;
			}
			AnimatorParamController.ParsedOperation parsedOperation;
			if (this.ParseInput(input, out parsedOperation))
			{
				int lhs;
				if (!this.StringToInt(parsedOperation.lhs, out lhs))
				{
					Debug.LogError("Type error: First value in operation is not a valid integer\nCould not process input integer operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				if (parsedOperation.oper == ' ')
				{
					this.animator.SetInteger(parsedOperation.paramHash, lhs);
					return;
				}
				int rhs;
				if (!this.StringToInt(parsedOperation.rhs, out rhs))
				{
					Debug.LogError("Type error: Second value in operation is not a valid integer\nCould not process input integer operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				char oper = parsedOperation.oper;
				if (oper <= '/')
				{
					if (oper == '%')
					{
						this.animator.SetInteger(parsedOperation.paramHash, lhs % rhs);
						return;
					}
					switch (oper)
					{
					case '*':
						this.animator.SetInteger(parsedOperation.paramHash, lhs * rhs);
						return;
					case '+':
						this.animator.SetInteger(parsedOperation.paramHash, lhs + rhs);
						return;
					case ',':
					case '.':
						break;
					case '-':
						this.animator.SetInteger(parsedOperation.paramHash, lhs - rhs);
						return;
					case '/':
						this.animator.SetInteger(parsedOperation.paramHash, lhs / rhs);
						return;
					default:
						return;
					}
				}
				else
				{
					if (oper == '?')
					{
						this.animator.SetInteger(parsedOperation.paramHash, UnityEngine.Random.Range(lhs, rhs));
						return;
					}
					switch (oper)
					{
					case '[':
						this.animator.SetInteger(parsedOperation.paramHash, Mathf.Max(lhs, rhs));
						return;
					case '\\':
						break;
					case ']':
						this.animator.SetInteger(parsedOperation.paramHash, Mathf.Min(lhs, rhs));
						return;
					case '^':
						this.animator.SetInteger(parsedOperation.paramHash, (int)Mathf.Pow((float)lhs, (float)rhs));
						return;
					default:
						return;
					}
				}
			}
			else
			{
				Debug.LogError("Could not process input integer operation\nError from: " + parsedOperation.originalOperation);
			}
		}

		// Token: 0x06001FB0 RID: 8112 RVA: 0x000D770C File Offset: 0x000D590C
		private bool StringToInt(string input, out int value)
		{
			if (int.TryParse(input, out value))
			{
				return true;
			}
			if (this.CheckForParam(input) == 2)
			{
				value = this.animator.GetInteger(Animator.StringToHash(input));
				return true;
			}
			value = 0;
			return false;
		}

		// Token: 0x06001FB1 RID: 8113 RVA: 0x000D773C File Offset: 0x000D593C
		public void FloatOperation(string input)
		{
			if (this.animator == null)
			{
				return;
			}
			AnimatorParamController.ParsedOperation parsedOperation;
			if (this.ParseInput(input, out parsedOperation))
			{
				float lhs;
				if (!this.StringToFloat(parsedOperation.lhs, out lhs))
				{
					Debug.LogError("Type error: First value in operation is not a valid float\nCould not process input float operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				if (parsedOperation.oper == ' ')
				{
					this.animator.SetFloat(parsedOperation.paramHash, lhs);
					return;
				}
				float rhs;
				if (!this.StringToFloat(parsedOperation.rhs, out rhs))
				{
					Debug.LogError("Type error: Second value in operation is not a valid float\nCould not process input float operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				char oper = parsedOperation.oper;
				if (oper <= '/')
				{
					if (oper == '%')
					{
						this.animator.SetFloat(parsedOperation.paramHash, lhs % rhs);
						return;
					}
					switch (oper)
					{
					case '*':
						this.animator.SetFloat(parsedOperation.paramHash, lhs * rhs);
						return;
					case '+':
						this.animator.SetFloat(parsedOperation.paramHash, lhs + rhs);
						return;
					case ',':
					case '.':
						break;
					case '-':
						this.animator.SetFloat(parsedOperation.paramHash, lhs - rhs);
						return;
					case '/':
						this.animator.SetFloat(parsedOperation.paramHash, lhs / rhs);
						return;
					default:
						return;
					}
				}
				else
				{
					if (oper == '?')
					{
						this.animator.SetFloat(parsedOperation.paramHash, UnityEngine.Random.Range(lhs, rhs));
						return;
					}
					switch (oper)
					{
					case '[':
						this.animator.SetFloat(parsedOperation.paramHash, Mathf.Max(lhs, rhs));
						return;
					case '\\':
						break;
					case ']':
						this.animator.SetFloat(parsedOperation.paramHash, Mathf.Min(lhs, rhs));
						return;
					case '^':
						this.animator.SetFloat(parsedOperation.paramHash, Mathf.Pow(lhs, rhs));
						return;
					default:
						return;
					}
				}
			}
			else
			{
				Debug.LogError("Could not process input float operation\nError from: " + parsedOperation.originalOperation);
			}
		}

		// Token: 0x06001FB2 RID: 8114 RVA: 0x000D790C File Offset: 0x000D5B0C
		private bool StringToFloat(string input, out float value)
		{
			if (float.TryParse(input, out value))
			{
				return true;
			}
			int type = this.CheckForParam(input);
			if (type > 1)
			{
				if (type == 2)
				{
					value = (float)this.animator.GetInteger(Animator.StringToHash(input));
				}
				if (type == 3)
				{
					value = this.animator.GetFloat(Animator.StringToHash(input));
				}
				return true;
			}
			value = 0f;
			return false;
		}

		// Token: 0x06001FB3 RID: 8115 RVA: 0x000D796C File Offset: 0x000D5B6C
		public bool ParseInput(string input, out AnimatorParamController.ParsedOperation parsedOperation)
		{
			parsedOperation = new AnimatorParamController.ParsedOperation();
			if (this.animator == null)
			{
				return false;
			}
			if (this.preParsedOperations.TryGetValue(input, out parsedOperation))
			{
				return true;
			}
			parsedOperation = new AnimatorParamController.ParsedOperation();
			string operationString = input.Replace(" ", "");
			parsedOperation.originalOperation = input;
			if (!operationString.Contains("="))
			{
				Debug.LogError("Synax error in operation: Missing assignment (=)");
				return false;
			}
			string[] splitAtEquals = operationString.Split(new char[]
			{
				'='
			}, 2);
			if (splitAtEquals.Length < 2)
			{
				Debug.LogError("Synax error in operation: Assignment is missing left or right side");
				return false;
			}
			if (string.IsNullOrEmpty(splitAtEquals[0]) || string.IsNullOrEmpty(splitAtEquals[1]))
			{
				Debug.LogError("Synax error in operation: Left or right side of operation is empty");
				return false;
			}
			int type = this.CheckForParam(splitAtEquals[0]);
			if (type == 0)
			{
				Debug.LogError("Synax error in operation: Right hand side is not a valid bool, int, or float animator parameter name");
				return false;
			}
			string boolOperators = "&|#<>=";
			string numOperators = "+-*/%^[]?";
			char oper = ' ';
			for (int i = 0; i < splitAtEquals[1].Length; i++)
			{
				bool boolOp = boolOperators.Contains(splitAtEquals[1][i].ToString());
				bool numOp = numOperators.Contains(splitAtEquals[1][i].ToString());
				if (boolOp || numOp)
				{
					if (oper != ' ')
					{
						Debug.LogError("Synax error in operation: Too many operators");
						return false;
					}
					if (boolOp && type != 1)
					{
						Debug.LogError("Synax error in operation: Bool operator in int or float assignment");
						return false;
					}
					if (numOp && type == 1)
					{
						Debug.LogError("Synax error in operation: Number operator in bool assignment");
						return false;
					}
					oper = splitAtEquals[1][i];
				}
			}
			parsedOperation.paramHash = Animator.StringToHash(splitAtEquals[0]);
			parsedOperation.oper = oper;
			if (oper != ' ')
			{
				string[] splitOperation = splitAtEquals[1].Split(oper, StringSplitOptions.None);
				parsedOperation.lhs = splitOperation[0];
				parsedOperation.rhs = splitOperation[1];
			}
			else
			{
				parsedOperation.lhs = splitAtEquals[1];
			}
			this.preParsedOperations.Add(input, parsedOperation);
			return true;
		}

		// Token: 0x06001FB4 RID: 8116 RVA: 0x000D7B48 File Offset: 0x000D5D48
		public int CheckForParam(string name)
		{
			if (this.animator == null)
			{
				return 0;
			}
			if (this.animatorParams.Count == 0)
			{
				foreach (AnimatorControllerParameter param in this.animator.parameters)
				{
					if (!this.animatorParams.ContainsKey(param.name))
					{
						this.animatorParams.Add(param.name, param.type);
					}
				}
			}
			string cleaned = name.Replace("!", "");
			AnimatorControllerParameterType type;
			if (this.animatorParams.TryGetValue(cleaned, out type))
			{
				switch (type)
				{
				case AnimatorControllerParameterType.Float:
					return 3;
				case AnimatorControllerParameterType.Int:
					return 2;
				case AnimatorControllerParameterType.Bool:
					return 1;
				}
				return 0;
			}
			return 0;
		}

		// Token: 0x04001EDD RID: 7901
		[Tooltip("Only works if this controller is on an item!")]
		public bool saveValuesOnStore;

		// Token: 0x04001EDE RID: 7902
		[Tooltip("Only works if this controller is on an item!")]
		public bool loadValuesOnLoad;

		// Token: 0x04001EE1 RID: 7905
		private Dictionary<string, AnimatorParamController.ParsedOperation> preParsedOperations = new Dictionary<string, AnimatorParamController.ParsedOperation>();

		// Token: 0x04001EE2 RID: 7906
		private Dictionary<string, AnimatorControllerParameterType> animatorParams = new Dictionary<string, AnimatorControllerParameterType>();

		// Token: 0x02000940 RID: 2368
		public class ParsedOperation
		{
			// Token: 0x0400442E RID: 17454
			public int paramHash;

			// Token: 0x0400442F RID: 17455
			public string lhs = "";

			// Token: 0x04004430 RID: 17456
			public char oper = ' ';

			// Token: 0x04004431 RID: 17457
			public string rhs = "";

			// Token: 0x04004432 RID: 17458
			public string originalOperation = "";
		}
	}
}
