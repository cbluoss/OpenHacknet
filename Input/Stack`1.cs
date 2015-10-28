// Decompiled with JetBrains decompiler
// Type: Hacknet.Input.Stack`1
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;

namespace Hacknet.Input
{
    public class Stack<T>
    {
        private T[] stack;

        public Stack()
            : this(32)
        {
        }

        public Stack(int capacity)
        {
            if (capacity < 0)
                capacity = 0;
            stack = new T[capacity];
        }

        public int Capacity
        {
            get { return stack.Length; }
        }

        public int Count { get; private set; }

        public void Push(ref T item)
        {
            if (Count == stack.Length)
            {
                var objArray = new T[stack.Length << 1];
                Array.Copy(stack, 0, objArray, 0, stack.Length);
                stack = objArray;
            }
            stack[Count] = item;
            ++Count;
        }

        public void Pop(out T item)
        {
            if (Count <= 0)
                throw new InvalidOperationException();
            item = stack[Count];
            stack[Count] = default(T);
            --Count;
        }

        public void PopSegment(out ArraySegment<T> segment)
        {
            segment = new ArraySegment<T>(stack, 0, Count);
            Count = 0;
        }
    }
}