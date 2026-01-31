using UnityEngine;

public class Button : MonoBehaviour
{
	[SerializeField] private RandomPrefabSpawner randomPrefabSpawner;
	[SerializeField] private Settings settings;

	public void Yes()
	{
		MaskProperties properties = randomPrefabSpawner.spawnedInstance.GetComponent<MaskProperties>();
		if (settings == null)
		{
			Debug.LogWarning($"{nameof(Button)}: No Settings assigned.", this);
		}
		else if (properties != null)
		{
			EntryFacts facts = new EntryFacts(properties);
			if (settings.TryGetBestMatch(facts, out EntryRule rule) && rule.result == RuleResult.Deny)
			{
				Debug.LogError($"Rule broken: {DescribeRule(rule)} for mask {DescribeMask(properties)}", this);
			}
		}
		if (settings != null)
		{
			settings.RegisterDecision();
		}
		randomPrefabSpawner.ReplaceWithRandom();
	}

	public void No()
	{
		if (settings != null)
		{
			settings.RegisterDecision();
		}
	}

	private static string DescribeRule(EntryRule rule)
	{
		if (rule == null) return "<null>";
		string color = rule.useMaskColor ? rule.maskColor.ToString() : "AnyColor";
		string cracks = rule.useCrackAmounts ? rule.crackAmounts.ToString() : "AnyCracks";
		string emotion = rule.useEmotion ? rule.emotion.ToString() : "AnyEmotion";
		return $"{rule.result} [{color}, {cracks}, {emotion}]";
	}

	private static string DescribeMask(MaskProperties props)
	{
		if (props == null) return "<null>";
		return $"{props.maskColor}, {props.crackAmounts}, {props.emotion}";
	}
}
