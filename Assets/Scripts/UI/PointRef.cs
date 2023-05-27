using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PointRef : RenderElementRef {
    public int idx; //THINK Maybe private, or read-only

    private Vector3 _v;
    public Vector3 v {
        get => _v;
        set {
            _v = value;
            _flagDirty(this);
        }
    }

    private Color _c;
    public Color c {
        get => _c;
        set {
            _c = value;
            _flagDirty(this);
        }
    }

    public PointRef(FlagDirty flagDirty, int idx, Vector3 v, Color c) : base(flagDirty, idx) {
        this.v = v;
        this.c = c;
    }
}
