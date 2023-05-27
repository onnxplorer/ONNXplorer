using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//RAINY Export
public class Pair<A, B> {
    public readonly A a;
    public readonly B b;

    public Pair(A a, B b) {
        this.a = a;
        this.b = b;
    }
}

public class Triple<A, B, C> { //TODO struct?
    public readonly A a;
    public readonly B b;
    public readonly C c;

    public Triple(A a, B b, C c) {
        this.a = a;
        this.b = b;
        this.c = c;
    }
}
