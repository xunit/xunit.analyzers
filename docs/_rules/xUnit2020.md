---
title: xUnit2020
description: Assert.Empty should be used to test if a collection is empty.
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Equals` or `Assert.NotEquals` are used to check if a collection is empty.

## Reason for rule

There are specialized assertions for checking collection sizes.

## How to fix violations

Use `Assert.Empty` or `Assert.NotEmpty` instead.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
    IEnumerable<string> result = GetItems();

    Assert.Equal(0, result.Count());
    Assert.NotEqual(0, result.Count());
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
    IEnumerable<string> result = GetItems();

    Assert.Empty(result);
    Assert.NotEmpty(result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2020 // Do not use equality check to check for collection empty check.
#pragma warning restore xUnit2020 // Do not use equality check to check for collection empty check.
```
