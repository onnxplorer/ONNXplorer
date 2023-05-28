using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LineRef : RenderElementRef {
    private Vector3 _va;
    public Vector3 va {
        get => _va;
        set {
            _va = value;
            _flagDirty(this);
        }
    }

    private Vector3 _vb;
    public Vector3 vb {
        get => _vb;
        set {
            _vb = value;
            _flagDirty(this);
        }
    }

    private Color _ca;
    public Color ca {
        get => _ca;
        set {
            _ca = value;
            _flagDirty(this);
        }
    }

    private Color _cb;
    public Color cb {
        get => _cb;
        set {
            _cb = value;
            _flagDirty(this);
        }
    }

    public LineRef() : this(Vector3.zero, Color.grey, Vector3.zero, Color.grey) {}

    public LineRef(Vector3 va, Color ca, Vector3 vb, Color cb) : base(_flagDirtyNOOP, -1) {
        this.va = va;
        this.ca = ca;
        this.vb = vb;
        this.cb = cb;
    }

    public LineRef(FlagDirty flagDirty, int idx, Vector3 va, Color ca, Vector3 vb, Color cb) : base(flagDirty, idx) {
        this.va = va;
        this.ca = ca;
        this.vb = vb;
        this.cb = cb;
    }
}
