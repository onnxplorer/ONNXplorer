using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public class Timing //RAINY Export
{
    private LinkedList<Pair<String, Stopwatch>> stack = new LinkedList<Pair<String, Stopwatch>>();

    public Timing push(String name, object o = null) {
        var sw = new Stopwatch();
        sw.Start();
        stack.AddLast(new Pair<String, Stopwatch>(name, sw));

        StringBuilder s = new StringBuilder();
        s.Append("--> ");
        bool first = true;
        foreach (var p in stack) {
            if (!first) {
                s.Append("." + p.a);
            } else {
                s.Append(p.a);
                first = false;
            }
        }
        if (o != null) {
            s.Append(" " + o);
        }
        UnityEngine.MonoBehaviour.print(s.ToString());

        return this;
    }

    public Timing log(object o = null) {
        StringBuilder s = new StringBuilder();
        s.Append("--- ");
        bool first = true;
        foreach (var p in stack) {
            if (!first) {
                s.Append("." + p.a);
            } else {
                s.Append(p.a);
                first = false;
            }
        }
        if (o != null) {
            s.Append(" " + o);
        }
        UnityEngine.MonoBehaviour.print(s.ToString());

        return this;
    }

    public Timing pop(object o = null) {
        Pair<String, Stopwatch> p0 = stack.Last.Value;
        p0.b.Stop();

        StringBuilder s = new StringBuilder();
        s.Append("<-- ");
        bool first = true;
        foreach (var p in stack) {
            if (!first) {
                s.Append("." + p.a);
            } else {
                s.Append(p.a);
                first = false;
            }
        }
        TimeSpan ts = p0.b.Elapsed;
        string elapsedTime = String.Format(" [{0:00}:{1:00}:{2:00}.{3:000}]", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        s.Append(elapsedTime);
        if (o != null) {
            s.Append(" " + o);
        }
        UnityEngine.MonoBehaviour.print(s.ToString());

        stack.RemoveLast();

        return this;
    }
}
