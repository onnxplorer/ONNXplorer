using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LineRef {
    public delegate void FlagDirty(LineRef line);
    private FlagDirty flagDirty;

    public int idx; //THINK Maybe private, or read-only

    private Vector3 _va;
    public Vector3 va {
        get => _va;
        set {
            _va = value;
            flagDirty(this);
        }
    }

    private Vector3 _vb;
    public Vector3 vb {
        get => _vb;
        set {
            _vb = value;
            flagDirty(this);
        }
    }

    private Color _ca;
    public Color ca {
        get => _ca;
        set {
            _ca = value;
            flagDirty(this);
        }
    }

    private Color _cb;
    public Color cb {
        get => _cb;
        set {
            _cb = value;
            flagDirty(this);
        }
    }

    public LineRef(FlagDirty flagDirty, int idx, Vector3 va, Color ca, Vector3 vb, Color cb) {
        this.flagDirty = flagDirty;
        this.idx = idx;
        this.va = va;
        this.ca = ca;
        this.vb = vb;
        this.cb = cb;
    }
}
