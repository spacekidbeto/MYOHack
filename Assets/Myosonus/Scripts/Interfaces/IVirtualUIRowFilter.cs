using UnityEngine;
using System.Collections;

public interface IVirtualUIRowFilter {

	bool FilterRow(RepeaterRow inputRow);
}
