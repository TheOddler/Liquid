using System.Collections.Generic;
using System.Linq;

public class CountingDict<T>
{
	Dictionary<T, int> _counts = new Dictionary<T, int>();

	public void Increase(T elem)
	{
		if (_counts.ContainsKey(elem)) _counts[elem] += 1;
		else _counts[elem] = 1;
	}

	public void Decrease(T elem)
	{
		if (_counts.ContainsKey(elem)) _counts[elem] -= 1;
		else _counts[elem] = -1;
	}

	public int CountOf(T elem)
	{
		if (_counts.ContainsKey(elem)) return _counts[elem];
		else return 0;
	}

	public override string ToString()
	{
		var ordered = from entry in _counts orderby entry.Key ascending select entry;
		return string.Join(" ; ", ordered.Select(x => x.Key + " = " + x.Value).ToArray());
	}
}
