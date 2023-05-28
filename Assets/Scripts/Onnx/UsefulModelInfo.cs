using System.Collections.Generic;

public class UsefulModelInfo {
    public string[] OriginalOutputs;
    public Dictionary<string, int> LayerNums;
    public Dictionary<string, int> PlaceInLayer;
    public Dictionary<string, string> OpTypes;
    public Dictionary<string, string[]> OpInputs;
}
