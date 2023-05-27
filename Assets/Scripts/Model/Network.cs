using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Network {
    public List<Neuron> neurons = new List<Neuron>();
    public List<Connection> connections = new List<Connection>();
    public Dictionary<Neuron, Connection> n2c = new Dictionary<Neuron, Connection>();
}
