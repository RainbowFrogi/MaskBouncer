using UnityEngine;

public class Button : MonoBehaviour
{
	[SerializeField] private RandomPrefabSpawner randomPrefabSpawner;
	[SerializeField] private Settings settings;

	public void Yes()
	{
		MaskProperties properties = randomPrefabSpawner.spawnedInstance.GetComponent<MaskProperties>();
		bool correct = true;
		if (settings == null)
		{
			Debug.LogWarning($"{nameof(Button)}: No Settings assigned.", this);
		}
		else if (properties != null)
		{
			EntryFacts facts = new EntryFacts(properties);
			if (settings.TryGetBestMatch(facts, out EntryRule rule))
			{
				if (rule.result == RuleResult.Deny)
				{
					Debug.LogError($"Rule broken: {DescribeRule(rule)} for mask {DescribeMask(properties)}", this);
					correct = false;
				}
			}
		}
		if (settings != null)
		{
			settings.RegisterDecision(correct);
		}
		randomPrefabSpawner.ReplaceWithRandom();
	}

	public void No()
	{
		MaskProperties properties = randomPrefabSpawner.spawnedInstance != null
			? randomPrefabSpawner.spawnedInstance.GetComponent<MaskProperties>()
			: null;
		bool correct = true;
		if (settings == null)
		{
			Debug.LogWarning($"{nameof(Button)}: No Settings assigned.", this);
		}
		else if (properties != null)
		{
			EntryFacts facts = new EntryFacts(properties);
			if (settings.TryGetBestMatch(facts, out EntryRule rule))
			{
				if (rule.result == RuleResult.Allow)
				{
					Debug.LogError($"Rule broken: {DescribeRule(rule)} for mask {DescribeMask(properties)}", this);
					correct = false;
				}
			}
			else
			{
				Debug.LogError($"Rule broken: No matching rule (default allow) for mask {DescribeMask(properties)}", this);
				correct = false;
			}
			settings.RegisterDecision(correct);
		}
		else if (settings != null)
		{
			settings.RegisterDecision(correct);
		}
		randomPrefabSpawner.ReplaceWithRandom();
	}

	private static string DescribeRule(EntryRule rule)
	{
		if (rule == null) return "<null>";
		string color = rule.useMaskColor ? rule.maskColor.ToString() : "AnyColor";
		string emotion = rule.useEmotion ? rule.maskType.ToString() : "AnyEmotion";
		return $"{rule.result} [{color}, {emotion}]";
	}

	private static string DescribeMask(MaskProperties props)
	{
		if (props == null) return "<null>";
		return $"{props.maskColor}, {props.maskType}";
	}
}
