using System;
using System.Collections.Generic;
using ThunderRoad.AI;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x02000358 RID: 856
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/SessionBlackboardTool")]
	public class SessionBlackboardTool : ThunderBehaviour
	{
		// Token: 0x14000131 RID: 305
		// (add) Token: 0x06002807 RID: 10247 RVA: 0x00112158 File Offset: 0x00110358
		// (remove) Token: 0x06002808 RID: 10248 RVA: 0x0011218C File Offset: 0x0011038C
		public static event Action onBlackboardUpdate;

		// Token: 0x06002809 RID: 10249 RVA: 0x001121C0 File Offset: 0x001103C0
		public void BoolOperation(string input)
		{
			SessionBlackboardTool.ParsedOperation parsedOperation;
			if (!this.ParseInput(input, out parsedOperation, 1))
			{
				Debug.LogError("Could not process input bool operation\nError from: " + parsedOperation.originalOperation);
				return;
			}
			if (parsedOperation.onlyIfMissing && SessionBlackboardTool.blackboard.Exist<bool>(parsedOperation.valueName))
			{
				Debug.LogWarning("Bool operation skipped: " + parsedOperation.valueName + " already has an assigned value and operation assignment contains 'only if missing' symbol '?': " + parsedOperation.originalOperation);
				return;
			}
			if (" &|#".Contains(parsedOperation.oper.ToString()))
			{
				bool lhs;
				if (!SessionBlackboardTool.StringToBool(parsedOperation.lhs, out lhs))
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
					SessionBlackboardTool.blackboard.UpdateVariable<bool>(parsedOperation.valueName, lhs);
					return;
				}
				bool rhs;
				if (!SessionBlackboardTool.StringToBool(parsedOperation.rhs, out rhs))
				{
					Debug.LogError("Type error: Second value in operation is not a valid boolean\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				char oper = parsedOperation.oper;
				if (oper != '#')
				{
					if (oper != '&')
					{
						if (oper == '|')
						{
							SessionBlackboardTool.blackboard.UpdateVariable<bool>(parsedOperation.valueName, lhs || rhs);
						}
					}
					else
					{
						SessionBlackboardTool.blackboard.UpdateVariable<bool>(parsedOperation.valueName, lhs && rhs);
					}
				}
				else
				{
					SessionBlackboardTool.blackboard.UpdateVariable<bool>(parsedOperation.valueName, (lhs || rhs) && (!lhs || !rhs));
				}
			}
			else
			{
				float lhs2;
				if (!SessionBlackboardTool.GetFloatOrIntValue(parsedOperation.lhs, true, out lhs2))
				{
					Debug.LogError("Type error: First value in comparison is not a valid number or could not be assigned\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				float rhs2;
				if (!SessionBlackboardTool.GetFloatOrIntValue(parsedOperation.rhs, true, out rhs2))
				{
					Debug.LogError("Type error: Second value in comparison is not a valid number or could not be assigned\nCould not process input bool operation\nError from: " + parsedOperation.originalOperation);
					return;
				}
				switch (parsedOperation.oper)
				{
				case '<':
					SessionBlackboardTool.blackboard.UpdateVariable<bool>(parsedOperation.valueName, lhs2 < rhs2);
					break;
				case '=':
					SessionBlackboardTool.blackboard.UpdateVariable<bool>(parsedOperation.valueName, Mathf.Approximately(lhs2, rhs2));
					break;
				case '>':
					SessionBlackboardTool.blackboard.UpdateVariable<bool>(parsedOperation.valueName, lhs2 > rhs2);
					break;
				}
			}
			Action action = SessionBlackboardTool.onBlackboardUpdate;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x0600280A RID: 10250 RVA: 0x001123FC File Offset: 0x001105FC
		public static bool StringToBool(string input, out bool value)
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
			if (cleaned != "~")
			{
				if (SessionBlackboardTool.blackboard.Exist<bool>(cleaned))
				{
					value = SessionBlackboardTool.blackboard.Find<bool>(cleaned, true);
					if (invertOutput)
					{
						value = !value;
					}
					return true;
				}
				if (SessionBlackboardTool.CheckValueTypeOfName(input) == 0)
				{
					SessionBlackboardTool.blackboard.UpdateVariable<bool>(cleaned, false);
					value = false;
					return true;
				}
			}
			value = false;
			return false;
		}

		// Token: 0x0600280B RID: 10251 RVA: 0x001124BC File Offset: 0x001106BC
		public void IntegerOperation(string input)
		{
			SessionBlackboardTool.ParsedOperation parsedOperation;
			if (!this.ParseInput(input, out parsedOperation, 2))
			{
				Debug.LogError("Could not process input integer operation\nError from: " + parsedOperation.originalOperation);
				return;
			}
			if (parsedOperation.onlyIfMissing && SessionBlackboardTool.blackboard.Exist<int>(parsedOperation.valueName))
			{
				Debug.LogWarning("Int operation skipped: " + parsedOperation.valueName + " already has an assigned value and operation assignment contains 'only if missing' symbol '?': " + parsedOperation.originalOperation);
				return;
			}
			int lhs;
			if (!SessionBlackboardTool.StringToInt(parsedOperation.lhs, out lhs))
			{
				Debug.LogError("Type error: First value in operation is not a valid integer or could not be assigned\nCould not process input integer operation\nError from: " + parsedOperation.originalOperation);
				return;
			}
			if (parsedOperation.oper == ' ')
			{
				SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, lhs);
				return;
			}
			int rhs;
			if (!SessionBlackboardTool.StringToInt(parsedOperation.rhs, out rhs))
			{
				Debug.LogError("Type error: Second value in operation is not a valid integer or could not be assigned\nCould not process input integer operation\nError from: " + parsedOperation.originalOperation);
				return;
			}
			char oper = parsedOperation.oper;
			if (oper <= '/')
			{
				if (oper != '%')
				{
					switch (oper)
					{
					case '*':
						SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, lhs * rhs);
						break;
					case '+':
						SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, lhs + rhs);
						break;
					case '-':
						SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, lhs - rhs);
						break;
					case '/':
						SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, lhs / rhs);
						break;
					}
				}
				else
				{
					SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, lhs % rhs);
				}
			}
			else if (oper != '?')
			{
				switch (oper)
				{
				case '[':
					SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, Mathf.Max(lhs, rhs));
					break;
				case ']':
					SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, Mathf.Min(lhs, rhs));
					break;
				case '^':
					SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, (int)Mathf.Pow((float)lhs, (float)rhs));
					break;
				}
			}
			else
			{
				SessionBlackboardTool.blackboard.UpdateVariable<int>(parsedOperation.valueName, UnityEngine.Random.Range(lhs, rhs));
			}
			Action action = SessionBlackboardTool.onBlackboardUpdate;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x0600280C RID: 10252 RVA: 0x001126D8 File Offset: 0x001108D8
		public static bool StringToInt(string input, out int value)
		{
			if (int.TryParse(input, out value))
			{
				return true;
			}
			float num;
			if (!float.TryParse(input, out num))
			{
				if (SessionBlackboardTool.blackboard.Exist<int>(input))
				{
					value = SessionBlackboardTool.blackboard.Find<int>(input, true);
					return true;
				}
				if (SessionBlackboardTool.CheckValueTypeOfName(input) == 0)
				{
					value = 0;
					SessionBlackboardTool.blackboard.UpdateVariable<int>(input, 0);
					return true;
				}
			}
			value = 0;
			return false;
		}

		// Token: 0x0600280D RID: 10253 RVA: 0x00112734 File Offset: 0x00110934
		public void FloatOperation(string input)
		{
			SessionBlackboardTool.ParsedOperation parsedOperation;
			if (!this.ParseInput(input, out parsedOperation, 3))
			{
				Debug.LogError("Could not process input float operation\nError from: " + parsedOperation.originalOperation);
				return;
			}
			if (parsedOperation.onlyIfMissing && SessionBlackboardTool.blackboard.Exist<float>(parsedOperation.valueName))
			{
				Debug.LogWarning("Float operation skipped: " + parsedOperation.valueName + " already has an assigned value and operation assignment contains 'only if missing' symbol '?': " + parsedOperation.originalOperation);
				return;
			}
			float lhs;
			if (!SessionBlackboardTool.GetFloatOrIntValue(parsedOperation.lhs, true, out lhs))
			{
				Debug.LogError("Type error: First value in operation is not a valid float or could not be assigned\nCould not process input float operation\nError from: " + parsedOperation.originalOperation);
				return;
			}
			if (parsedOperation.oper == ' ')
			{
				SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, lhs);
				return;
			}
			float rhs;
			if (!SessionBlackboardTool.GetFloatOrIntValue(parsedOperation.rhs, true, out rhs))
			{
				Debug.LogError("Type error: Second value in operation is not a valid float or could not be assigned\nCould not process input float operation\nError from: " + parsedOperation.originalOperation);
				return;
			}
			char oper = parsedOperation.oper;
			if (oper <= '/')
			{
				if (oper != '%')
				{
					switch (oper)
					{
					case '*':
						SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, lhs * rhs);
						break;
					case '+':
						SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, lhs + rhs);
						break;
					case '-':
						SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, lhs - rhs);
						break;
					case '/':
						SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, lhs / rhs);
						break;
					}
				}
				else
				{
					SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, lhs % rhs);
				}
			}
			else if (oper != '?')
			{
				switch (oper)
				{
				case '[':
					SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, Mathf.Max(lhs, rhs));
					break;
				case ']':
					SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, Mathf.Min(lhs, rhs));
					break;
				case '^':
					SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, Mathf.Pow(lhs, rhs));
					break;
				}
			}
			else
			{
				SessionBlackboardTool.blackboard.UpdateVariable<float>(parsedOperation.valueName, UnityEngine.Random.Range(lhs, rhs));
			}
			Action action = SessionBlackboardTool.onBlackboardUpdate;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x0600280E RID: 10254 RVA: 0x0011294C File Offset: 0x00110B4C
		public static bool StringToFloat(string input, out float value)
		{
			if (float.TryParse(input, out value))
			{
				return true;
			}
			if (SessionBlackboardTool.blackboard.Exist<float>(input))
			{
				value = SessionBlackboardTool.blackboard.Find<float>(input, true);
				return true;
			}
			if (SessionBlackboardTool.CheckValueTypeOfName(input) == 0)
			{
				value = 0f;
				SessionBlackboardTool.blackboard.UpdateVariable<float>(input, 0f);
				return true;
			}
			value = 0f;
			return false;
		}

		// Token: 0x0600280F RID: 10255 RVA: 0x001129AC File Offset: 0x00110BAC
		public bool ParseInput(string input, out SessionBlackboardTool.ParsedOperation parsedOperation, int targetType)
		{
			if (this.preParsedOperations.TryGetValue(input, out parsedOperation))
			{
				return true;
			}
			parsedOperation = new SessionBlackboardTool.ParsedOperation();
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
			if (splitAtEquals[0].Contains("?"))
			{
				splitAtEquals[0] = splitAtEquals[0].Replace("?", "");
				parsedOperation.onlyIfMissing = true;
			}
			bool correctType = false;
			switch (targetType)
			{
			case 1:
			{
				bool flag;
				correctType = SessionBlackboardTool.StringToBool(splitAtEquals[0], out flag);
				break;
			}
			case 2:
			{
				int num;
				correctType = SessionBlackboardTool.StringToInt(splitAtEquals[0], out num);
				break;
			}
			case 3:
			{
				float num2;
				correctType = SessionBlackboardTool.StringToFloat(splitAtEquals[0], out num2);
				break;
			}
			}
			if (!correctType)
			{
				Debug.LogError("Synax error in operation: Setter name in operation is stored in the blackboard as a different type");
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
					if (boolOp && targetType != 1)
					{
						Debug.LogError("Synax error in operation: Bool operator in int or float assignment");
						return false;
					}
					if (numOp && targetType == 1)
					{
						Debug.LogError("Synax error in operation: Number operator in bool assignment");
						return false;
					}
					oper = splitAtEquals[1][i];
				}
			}
			parsedOperation.valueName = splitAtEquals[0];
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

		// Token: 0x06002810 RID: 10256 RVA: 0x00112BCB File Offset: 0x00110DCB
		public static int CheckValueTypeOfName(string name)
		{
			if (SessionBlackboardTool.blackboard.Exist<bool>(name))
			{
				return 1;
			}
			if (SessionBlackboardTool.blackboard.Exist<int>(name))
			{
				return 2;
			}
			if (SessionBlackboardTool.blackboard.Exist<float>(name))
			{
				return 3;
			}
			return 0;
		}

		// Token: 0x06002811 RID: 10257 RVA: 0x00112BFC File Offset: 0x00110DFC
		public static bool GetFloatOrIntValue(string input, bool setIfUnused, out float value)
		{
			if (float.TryParse(input, out value))
			{
				return true;
			}
			int type = SessionBlackboardTool.CheckValueTypeOfName(input);
			if (type > 1)
			{
				if (type == 2)
				{
					value = (float)SessionBlackboardTool.blackboard.Find<int>(input, true);
					return true;
				}
				if (type == 3)
				{
					value = SessionBlackboardTool.blackboard.Find<float>(input, true);
					return true;
				}
			}
			else if (type == 0)
			{
				value = 0f;
				if (setIfUnused)
				{
					SessionBlackboardTool.blackboard.UpdateVariable<float>(input, 0f);
					return true;
				}
			}
			value = 0f;
			return false;
		}

		// Token: 0x06002812 RID: 10258 RVA: 0x00112C70 File Offset: 0x00110E70
		public void CheckEvaluatorAtIndex(int index)
		{
			if (index >= this.evaluators.Count)
			{
				Debug.LogError(string.Format("Input index [{0}] is out of bounds for the list! Index range: [0-{1}] (Inclusive)", index, this.evaluators.Count - 1));
				return;
			}
			this.evaluators[index].Evaluate();
		}

		// Token: 0x06002813 RID: 10259 RVA: 0x00112CC4 File Offset: 0x00110EC4
		public void CheckAllEvaluators()
		{
			this.RunAllEvaluatorsInList(this.evaluators);
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06002814 RID: 10260 RVA: 0x00112CD2 File Offset: 0x00110ED2
		// (set) Token: 0x06002815 RID: 10261 RVA: 0x00112CD9 File Offset: 0x00110ED9
		public static Blackboard blackboard { get; protected set; }

		// Token: 0x06002816 RID: 10262 RVA: 0x00112CE1 File Offset: 0x00110EE1
		private void Awake()
		{
			if (SessionBlackboardTool.blackboard == null)
			{
				SessionBlackboardTool.blackboard = new Blackboard();
			}
		}

		// Token: 0x06002817 RID: 10263 RVA: 0x00112CF4 File Offset: 0x00110EF4
		private void Start()
		{
			this.ProcessStartingOperations(this.startingBoolOperations, new Action<string>(this.BoolOperation));
			this.ProcessStartingOperations(this.startingIntegerOperations, new Action<string>(this.IntegerOperation));
			this.ProcessStartingOperations(this.startingFloatOperations, new Action<string>(this.FloatOperation));
			for (int i = 0; i < this.evaluators.Count; i++)
			{
				SessionBlackboardTool.BlackboardEvaluateEvent bbee = this.evaluators[i];
				if (bbee.evaluateOnStart)
				{
					bbee.Evaluate();
				}
				if (bbee.evaluateOnBBUpdate)
				{
					this.updateEvaluators.Add(bbee);
				}
			}
			if (this.updateEvaluators.Count > 0)
			{
				SessionBlackboardTool.onBlackboardUpdate += this.CheckUpdateEvaluators;
			}
		}

		// Token: 0x06002818 RID: 10264 RVA: 0x00112DAD File Offset: 0x00110FAD
		private void CheckUpdateEvaluators()
		{
			this.RunAllEvaluatorsInList(this.updateEvaluators);
		}

		// Token: 0x06002819 RID: 10265 RVA: 0x00112DBC File Offset: 0x00110FBC
		private void ProcessStartingOperations(List<string> operations, Action<string> method)
		{
			for (int i = 0; i < operations.Count; i++)
			{
				method(operations[i]);
			}
		}

		// Token: 0x0600281A RID: 10266 RVA: 0x00112DE8 File Offset: 0x00110FE8
		private void RunAllEvaluatorsInList(List<SessionBlackboardTool.BlackboardEvaluateEvent> bbees)
		{
			for (int i = 0; i < bbees.Count; i++)
			{
				bbees[i].Evaluate();
			}
		}

		// Token: 0x040026F0 RID: 9968
		public List<SessionBlackboardTool.BlackboardEvaluateEvent> evaluators = new List<SessionBlackboardTool.BlackboardEvaluateEvent>();

		// Token: 0x040026F1 RID: 9969
		public List<string> startingBoolOperations = new List<string>();

		// Token: 0x040026F2 RID: 9970
		public List<string> startingIntegerOperations = new List<string>();

		// Token: 0x040026F3 RID: 9971
		public List<string> startingFloatOperations = new List<string>();

		// Token: 0x040026F4 RID: 9972
		private List<SessionBlackboardTool.BlackboardEvaluateEvent> updateEvaluators = new List<SessionBlackboardTool.BlackboardEvaluateEvent>();

		// Token: 0x040026F5 RID: 9973
		private Dictionary<string, SessionBlackboardTool.ParsedOperation> preParsedOperations = new Dictionary<string, SessionBlackboardTool.ParsedOperation>();

		// Token: 0x02000A44 RID: 2628
		[Serializable]
		public class BlackboardEvaluateEvent
		{
			// Token: 0x060045AE RID: 17838 RVA: 0x0019647C File Offset: 0x0019467C
			public void Evaluate()
			{
				SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags comparison = ((this.evaluator.Contains("<") && !this.evaluator.Contains("<=")) ? SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.LessThan : SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.Null) | (this.evaluator.Contains("<=") ? SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.LessThanOrEqual : SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.Null) | (this.evaluator.Contains("==") ? SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.EqualTo : SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.Null) | (this.evaluator.Contains(">=") ? SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.GreaterThanOrEqual : SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.Null) | ((this.evaluator.Contains(">") && !this.evaluator.Contains(">=")) ? SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.GreaterThan : SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.Null);
				string[] sides = this.evaluator.Replace(" ", "").Replace("==", ":").Replace("=", "").Replace("<", ":").Replace(">", ":").Split(':', StringSplitOptions.None);
				if (sides.Length > 2)
				{
					Debug.LogError("Error in evaluator [" + this.evaluator + "]: More than one comparison operator present");
					return;
				}
				if (comparison == SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.Null || sides.Length != 2)
				{
					Debug.LogError("Error in evaluator [" + this.evaluator + "]: Couldn't find comparison, or not enough inputs to compare (Requires one on each side of a comparison)");
					return;
				}
				int lhsType = SessionBlackboardTool.CheckValueTypeOfName(sides[0]);
				if (lhsType == 0)
				{
					Debug.LogWarning("Value with name " + sides[0] + " not found! It will be created and set to the type's default value");
				}
				int rhsType = SessionBlackboardTool.CheckValueTypeOfName(sides[1]);
				if (rhsType == 0)
				{
					float num;
					int num2;
					bool flag;
					if (float.TryParse(sides[1], out num))
					{
						rhsType = 3;
					}
					else if (int.TryParse(sides[1], out num2))
					{
						rhsType = 2;
					}
					else if (bool.TryParse(sides[1], out flag))
					{
						rhsType = 1;
					}
					if (rhsType == 0)
					{
						Debug.LogError("Error in evaluator [" + this.evaluator + "]: Right hand side isn't a valid named blackboard variable or bool, int, or float value! This cannot be parsed, and the evaulator can't continue running.");
						return;
					}
				}
				if (lhsType + rhsType == 0)
				{
					Debug.LogError("Error in evaluator [" + this.evaluator + "]: Can't compare values that don't exist yet");
					return;
				}
				if (Mathf.Clamp(lhsType, 1, 2) != Mathf.Clamp(rhsType, 1, 2))
				{
					Debug.LogError("Error in evaluator [" + this.evaluator + "]: Both sides of a comparison need to be comparable (Bool vs bool, or number vs number)");
					return;
				}
				float lhsNum = 0f;
				float rhsNum = 0f;
				if (Mathf.Clamp(lhsType, 1, 2) == 2 || Mathf.Clamp(rhsType, 1, 2) == 2)
				{
					SessionBlackboardTool.GetFloatOrIntValue(sides[0], true, out lhsNum);
					SessionBlackboardTool.GetFloatOrIntValue(sides[1], true, out rhsNum);
				}
				lhsType = Mathf.Clamp(lhsType, 1, 3);
				rhsType = Mathf.Clamp(rhsType, 1, 3);
				bool evaluation;
				switch (comparison)
				{
				case SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.LessThan:
					evaluation = (lhsNum < rhsNum);
					goto IL_328;
				case SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.LessThanOrEqual:
					evaluation = (lhsNum <= rhsNum);
					goto IL_328;
				case SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.LessThan | SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.LessThanOrEqual:
					break;
				case SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.EqualTo:
					if (lhsType == 2 && rhsType == 2)
					{
						int lhsInt;
						SessionBlackboardTool.StringToInt(sides[0], out lhsInt);
						int rhsInt;
						SessionBlackboardTool.StringToInt(sides[1], out rhsInt);
						evaluation = (lhsInt == rhsInt);
						goto IL_328;
					}
					if (lhsType == 1 && rhsType == 1)
					{
						bool lhsBool;
						SessionBlackboardTool.StringToBool(sides[0], out lhsBool);
						bool rhsBool;
						SessionBlackboardTool.StringToBool(sides[1], out rhsBool);
						evaluation = (lhsBool == rhsBool);
						goto IL_328;
					}
					evaluation = Mathf.Approximately(lhsNum, rhsNum);
					goto IL_328;
				default:
					if (comparison == SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.GreaterThanOrEqual)
					{
						evaluation = (lhsNum >= rhsNum);
						goto IL_328;
					}
					if (comparison == SessionBlackboardTool.BlackboardEvaluateEvent.ComparisonFlags.GreaterThan)
					{
						evaluation = (lhsNum > rhsNum);
						goto IL_328;
					}
					break;
				}
				Debug.LogError("Error in evaluator [" + this.evaluator + "]: Unknown error occurred. All other error options should have been handled by now");
				return;
				IL_328:
				if (evaluation)
				{
					UnityEvent unityEvent = this.onTrue;
					if (unityEvent == null)
					{
						return;
					}
					unityEvent.Invoke();
					return;
				}
				else
				{
					UnityEvent unityEvent2 = this.onFalse;
					if (unityEvent2 == null)
					{
						return;
					}
					unityEvent2.Invoke();
					return;
				}
			}

			// Token: 0x04004798 RID: 18328
			public string evaluator;

			// Token: 0x04004799 RID: 18329
			public UnityEvent onTrue;

			// Token: 0x0400479A RID: 18330
			public UnityEvent onFalse;

			// Token: 0x0400479B RID: 18331
			public bool evaluateOnStart;

			// Token: 0x0400479C RID: 18332
			public bool evaluateOnBBUpdate;

			// Token: 0x02000BF2 RID: 3058
			[Flags]
			private enum ComparisonFlags
			{
				// Token: 0x04004D6C RID: 19820
				Null = 0,
				// Token: 0x04004D6D RID: 19821
				LessThan = 1,
				// Token: 0x04004D6E RID: 19822
				LessThanOrEqual = 2,
				// Token: 0x04004D6F RID: 19823
				EqualTo = 4,
				// Token: 0x04004D70 RID: 19824
				GreaterThanOrEqual = 8,
				// Token: 0x04004D71 RID: 19825
				GreaterThan = 16
			}
		}

		// Token: 0x02000A45 RID: 2629
		public class ParsedOperation
		{
			// Token: 0x0400479D RID: 18333
			public string valueName = "";

			// Token: 0x0400479E RID: 18334
			public string lhs = "";

			// Token: 0x0400479F RID: 18335
			public char oper = ' ';

			// Token: 0x040047A0 RID: 18336
			public string rhs = "";

			// Token: 0x040047A1 RID: 18337
			public string originalOperation = "";

			// Token: 0x040047A2 RID: 18338
			public bool onlyIfMissing;
		}
	}
}
