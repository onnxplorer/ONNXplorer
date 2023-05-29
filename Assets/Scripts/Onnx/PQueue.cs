using System;
using System.Collections.Generic;

public class PQueue<T, TPrior> where TPrior : IComparable<TPrior>
{
    private List<(T, TPrior)> heap = new List<(T, TPrior)>();

    public int Count => heap.Count;

    public void Enqueue(T item, TPrior priority)
    {
        heap.Add((item, priority));
        int i = heap.Count - 1;
        while (i > 0) {
            int parent = (i - 1) / 2;
            if (heap[parent].Item2.CompareTo(priority) <= 0) {
                break;
            }
            heap[i] = heap[parent];
            i = parent;
        }
        heap[i] = (item, priority);
    }

    public T Dequeue()
    {
        if (heap.Count == 0) {
            throw new InvalidOperationException("Priority queue is empty");
        }
        T result = heap[0].Item1;
        TPrior priority = heap[heap.Count - 1].Item2;
        heap.RemoveAt(heap.Count - 1);
        if (heap.Count > 0) {
            int i = 0;
            while (true) {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                if (left >= heap.Count) {
                    break;
                }
                int minChild = left;
                if (right < heap.Count && heap[right].Item2.CompareTo(heap[left].Item2) < 0) {
                    minChild = right;
                }
                if (heap[minChild].Item2.CompareTo(priority) >= 0) {
                    break;
                }
                heap[i] = heap[minChild];
                i = minChild;
            }
            heap[i] = (result, priority);
        }
        return result;
    }

    public T Peek()
    {
        if (heap.Count == 0) {
            throw new InvalidOperationException("Priority queue is empty");
        }
        return heap[0].Item1;
    }
}