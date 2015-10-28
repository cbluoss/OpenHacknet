// Decompiled with JetBrains decompiler
// Type: Hacknet.UIUtils.CLinkBuffer`1
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

namespace Hacknet.UIUtils
{
    public class CLinkBuffer<T>
    {
        private int currentIndex;
        private readonly T[] data;

        public CLinkBuffer(int BufferSize = 128)
        {
            data = new T[BufferSize];
        }

        public T Get(int offset)
        {
            var index = currentIndex + offset;
            while (index < 0)
                index += data.Length;
            while (index >= data.Length)
                index -= data.Length;
            return data[index];
        }

        public void Add(T added)
        {
            currentIndex = NextIndex();
            data[currentIndex] = added;
        }

        public void AddOneAhead(T added)
        {
            data[NextIndex()] = added;
        }

        private int NextIndex()
        {
            var num = currentIndex + 1;
            if (num >= data.Length)
                num = 0;
            return num;
        }
    }
}