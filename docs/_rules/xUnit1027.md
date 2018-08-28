---
title: xUnit1027
description: Thread blocking code invocation in xUnit tests may lead to deadlocks
category: Usage
severity: Warning
---

## Cause

A violation of this rule occurs when specific `System.Threading.Tasks.Task` members are called in unit test assembly. These members are:

* `.Wait()`
* `.Result`
* `.GetAwaiter()`
* `.Task.WaitAll(...)`
* `.Task.WaitAny(...)`

## Reason for rule

Such `Task` methods block currently executing thread which may lead to deadlocks.

## How to fix violations

To fix a violation of this rule, use `async` unit test methods where you can just `await` asynchronous `Task` code instead of blocking threads.

## Examples

### Violates

```csharp
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public int T()
    {
        return new Task<int>(() => 1).Result;
    }
}
```

### Does not violate

```csharp
using System.Threading.Tasks;
using Xunit;

public class C
{
    [Fact]
    public async Task<int> T()
    {
        return await new Task<int>(() => 1);
    }
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit1027 // Thread blocking code invocation in xUnit tests may lead to deadlocks
#pragma warning restore xUnit1027 // Thread blocking code invocation in xUnit tests may lead to deadlocks
```
