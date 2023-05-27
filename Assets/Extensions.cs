using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

static class VectorExtension { //TODO Move to a different file
    //TODO Inline some of these?

    public static double sqr(this double d) {
        return d * d;
    }

    public static double abs(this double d) {
        return Math.Abs(d);
    }

    public static double dist2(this double[] p, double[] q) {
        double ret = 0;
        for (int i = 0; i < p.Length; i++) {
            ret += sqr(p[i] - q[i]); // HAHAHAHA, I HAD A "*" INSTEAD OF "-".  If that's the only bug...  Edit: Uh.  Wow.  There was a second bug in this very line.
        }
        return ret;
    }

    public static double dot(this double[] p, double[] q) {
        double ret = 0;
        for (int i = 0; i < p.Length; i++) {
            ret += p[i] * q[i];
        }
        return ret;
    }

    /** Only in 3d atm, sorry */
    public static double[] cross(this double[] p, double[] q) {
        return new double[] { -p[2] * q[1] + p[1] * q[2], p[2] * q[0] - p[0] * q[2], -p[1] * q[0] + p[0] * q[1] };
    }

    public static double norm(this double[] p) {
        double ret = 0;
        for (int i = 0; i < p.Length; i++) {
            ret += p[i] * p[i];
        }
        return Math.Sqrt(ret);
    }

    public static double norm2(this double[] p) {
        double ret = 0;
        for (int i = 0; i < p.Length; i++) {
            ret += p[i] * p[i];
        }
        return ret;
    }

    public static double[] normalize(this double[] p) {
        return p.div(p.norm());
    }

    public static double[] normalize2(this double[] p) {
        return p.div(p.norm2());
    }

    public static bool hasNonFinite(this double[] p) {
        for (int i = 0; i < p.Length; i++) {
            if (!Double.IsFinite(p[i])) {
                return true;
            }
        }
        return false;
    }

    public static double[] mul(this double[] p, double s) {
        double[] result = (double[])p.Clone();
        for (int i = 0; i < result.Length; i++) {
            result[i] *= s;
        }
        return result;
    }

    public static double[] div(this double[] p, double s) {
        double[] result = (double[])p.Clone();
        for (int i = 0; i < result.Length; i++) {
            result[i] /= s;
        }
        return result;
    }

    public static double[] add(this double[] p, double[] q) {
        double[] result = (double[])p.Clone();
        for (int i = 0; i < result.Length; i++) {
            result[i] += q[i];
        }
        return result;
    }

    public static double[] sub(this double[] p, double[] q) {
        double[] result = (double[])p.Clone();
        for (int i = 0; i < result.Length; i++) {
            result[i] -= q[i];
        }
        return result;
    }

    // In place
    public static double[] mulIP(this double[] p, double s) {
        for (int i = 0; i < p.Length; i++) {
            p[i] *= s;
        }
        return p;
    }

    public static double[] divIP(this double[] p, double s) {
        for (int i = 0; i < p.Length; i++) {
            p[i] /= s;
        }
        return p;
    }

    public static double[] addIP(this double[] p, double[] q) {
        for (int i = 0; i < p.Length; i++) {
            p[i] += q[i];
        }
        return p;
    }

    public static double[] subIP(this double[] p, double[] q) {
        for (int i = 0; i < p.Length; i++) {
            p[i] -= q[i];
        }
        return p;
    }

    /**
     * Swaps y and z; I'm used to z being vertical.
     */
    public static double[] flipUp0(this double[] p) {
        double[] result = (double[])p.Clone();
        result[1] = p[2];
        result[2] = p[1];
        return result;
    }

    public static double[] flipUp0IP(this double[] p) {
        double z = p[2];
        p[2] = p[1];
        p[1] = z;
        return p;
    }

    public static String toString(this double[] p) {
        return String.Join(", ", p);
    }

    public static UnityEngine.Vector3 toV3(this double[] p) {
        return new UnityEngine.Vector3((float)p[0], (float)p[1], (float)p[2]);
    }

    public static double[] copy(this double[] p) {
        return new double[] { p[0], p[1], p[2] };
    }

    public static bool equal(this double[] p, double[] q) {
        if (p.Length != q.Length) {
            return false;
        }
        for (int i = 0; i < p.Length; i++) {
            if (p[i] != q[i]) {
                return false;
            }
        }
        return true;
    }
}

static class Vector3Extension {
    public static double[] toDoubleArray(this UnityEngine.Vector3 v) {
        return new double[] { v.x, v.y, v.z };
    }

    public static double[] toDA(this UnityEngine.Vector3 v) {
        return new double[] { v.x, v.y, v.z };
    }

    public static UnityEngine.Vector3 copy(this UnityEngine.Vector3 v) {
        return new UnityEngine.Vector3(v.x, v.y, v.z);
    }
}

static class RandomExtension {
    public static Color nextColor(this System.Random rand, bool randAlpha = true) {
        return new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), randAlpha ? (float)rand.NextDouble() : 1.0f);
    }
}

public delegate void Consumer<in T>(T arg);