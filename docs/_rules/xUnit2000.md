---
title: xUnit2000
description: Expected value should be first
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when the first argument to an assert is not the expected value.

## Reason for rule

The expected value in an assert should always be the first argument. This will ensure that generated messages explaining the test failure will correctly match the situation.

## How to fix violations

To fix a violation of this rule, swap the arguments in the assert, so that the expected value is the first.

## Examples

### Violates

```csharp
[Fact]
public void AdditionExample()
{
    var result = 2 + 3;

    Assert.Equal(result, 5);
}
```

### Does not violate

```csharp
[Fact]
public void AdditionExample()
{
    var result = 2 + 3;

    Assert.Equal(5, result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2000 // Expected value should be first
#pragma warning restore xUnit2000 // Expected value should be first
```
