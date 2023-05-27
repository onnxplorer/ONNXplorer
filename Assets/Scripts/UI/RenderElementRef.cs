using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RenderElementRef {
    public delegate void FlagDirty(RenderElementRef line);
    public FlagDirty _flagDirty;
    public int _idx; //THINK Maybe private, or read-only

    public RenderElementRef(FlagDirty flagDirty, int idx) {
        this._flagDirty = flagDirty;
        this._idx = idx;
    }
}
