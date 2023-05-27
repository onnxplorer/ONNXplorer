using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PointRef : RenderElementRef {
    public const float DEFAULT_SIZE = 0.01f;

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

    private float _size = DEFAULT_SIZE;
    public float size {
        get => _size;
        set {
            _size = value;
            _flagDirty(this);
        }
    }

    public PointRef() : this(Vector3.zero, Color.white) {}

    public PointRef(Vector3 v, Color c) : base(_flagDirtyNOOP, -1) {
        this.v = v;
        this.c = c;
    }

    public PointRef(Vector3 v, Color c, float size) : base(_flagDirtyNOOP, -1) {
        this.v = v;
        this.c = c;
        this.size = size;
    }

    public PointRef(FlagDirty flagDirty, int idx, Vector3 v, Color c, float size) : base(flagDirty, idx) {
        this.v = v;
        this.c = c;
        this.size = size;
    }
}
