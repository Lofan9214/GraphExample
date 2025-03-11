// 문제 설명
// 우선순위큐는 PriorityQueue<TElement, TPriority> 형태로 구현되어야 하며, 다음의 요구사항을 만족해야 한다.

// 요구사항
// 자료구조와 알고리즘
//- 배열 기반의 최소 힙(Min Heap)으로 구현함
//- 낮은 우선순위 값을 가진 작업이 먼저 처리되도록 함
//- 내부 배열은 필요할 때 자동으로 크기가 증가해야 함

// 필수 구현 기능
//- 작업 추가: `Enqueue(TElement element, TPriority priority)`
//- 최우선 작업 꺼내기: `TElement Dequeue()`
//- 최우선 작업 확인: `TElement Peek()`
//- 안전한 꺼내기: `bool TryDequeue(out TElement element, out TPriority priority)`
//- 안전한 확인: `bool TryPeek(out TElement element, out TPriority priority)`
//- 작업 개수 확인: `int Count { get; }`
//- 모든 작업 제거: `void Clear()`

// 배열 기반 힙 설명
// 기본 인덱스 관계

//-부모 인덱스: (자식인덱스 - 1) / 2
//-왼쪽 자식 인덱스: 부모인덱스 * 2 + 1
//-오른쪽 자식 인덱스: 부모인덱스 * 2 + 2

// 삽입(Enqueue) 과정
//1. 새 원소를 힙 배열 끝에 추가한다.
//2. 추가한 원소가 부모보다 우선순위가 낮다면(더 높은 우선 순위를 가져야 한다면), 부모와 교환하고 위로 올라간다.
//3. 더 이상 부모보다 우선순위가 낮지 않을 때까지(또는 루트에 도달할 때까지) 반복한다.

// 삭제(Dequeue) 과정
//1. 최상단(루트)에 있는 최소값(최우선 순위)을 반환한다.
//2. 힙 배열의 마지막 원소를 루트 위치로 가져온다.
//3. 새 루트 원소가 자식보다 우선순위가 높을 경우, 더 낮은 우선순위를 가진(더 작은 값인) 자식과 교환한다.
//4. 더 이상 자식보다 우선순위가 높지 않을 때까지(또는 리프 노드에 도달할 때까지) 내려간다.

// 동적 크기 확장
//- 힙 배열이 꽉 차면, 더 큰 배열을 할당하고 기존 원소들을 복사해 저장한다.
//- 이를 통해 힙 크기를 유연하게 조정할 수 있다.

using System;
using System.Linq;
using System.Collections.Generic;

public class PriorityQueue<TElement, TPriority>
{
    private struct ElementPriorityPair
    {
        public TElement Element;
        public TPriority Priority;

        public ElementPriorityPair(TElement element, TPriority priority)
        {
            Element = element;
            Priority = priority;
        }
    }

    ElementPriorityPair[] pairs;
    IComparer<TPriority> comparer;
    int count;

    public int Count => count;

    public PriorityQueue()
    {
        pairs = new ElementPriorityPair[8];
        comparer = Comparer<TPriority>.Default;
    }

    public PriorityQueue(IComparer<TPriority> comparer) : this()
    {
        this.comparer = comparer;
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        if (count > (pairs.Length / 2 + pairs.Length / 4))
        {
            Resize(pairs.Length * 2);
        }
        pairs[count] = new(element, priority);
        int current = count;
        int next = (current - 1) / 2;
        ++count;
        while (current > 0)
        {
            if (comparer.Compare(pairs[current].Priority, pairs[next].Priority) < 0)
            {
                (pairs[current], pairs[next]) = (pairs[next], pairs[current]);
                current = next;
                next = (current - 1) / 2;
                continue;
            }
            break;
        }
    }

    public TElement Dequeue()
    {
        TElement element = pairs[0].Element;
        --count;
        pairs[0] = pairs[count];
        pairs[count] = default;
        int current = 0;
        int next = current * 2 + 1;
        while (next < count)
        {
            if (next + 1 < count
                && comparer.Compare(pairs[next].Priority, pairs[next + 1].Priority) > 0)
            {
                ++next;
            }
            if (comparer.Compare(pairs[current].Priority, pairs[next].Priority) < 0)
            {
                break;
            }
            (pairs[current], pairs[next]) = (pairs[next], pairs[current]);
            current = next;
            next = current * 2 + 1;
        }
        return element;
    }

    public TElement Peek()
    {
        return pairs[0].Element;
    }

    public bool TryDequeue(out TElement element, out TPriority priority)
    {
        if (count > 0)
        {
            element = pairs[0].Element;
            priority = pairs[0].Priority;
            Dequeue();
            return true;
        }

        element = default;
        priority = default;
        return false;
    }

    public bool TryPeek(out TElement element, out TPriority priority)
    {
        if (count > 0)
        {
            element = pairs[0].Element;
            priority = pairs[0].Priority;
            return true;
        }
        element = default;
        priority = default;
        return false;
    }

    public void Clear()
    {
        Array.Clear(pairs, 0, pairs.Length);
        count = 0;
    }

    private void Resize(int size)
    {
        Array.Resize(ref pairs, size);
    }
}

