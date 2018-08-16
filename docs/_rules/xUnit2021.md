---
title: xUnit2021
description: Assert.Single should be used to test if a collection has a single item.
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Equals` is used to check if a collection has 1 item.

## Reason for rule

There are specialized assertions for checking collection sizes.

## How to fix violations

Use `Assert.Single` instead.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
    IEnumerable<string> result = GetItems();

    Assert.Equal(1, result.Count());
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
    IEnumerable<string> result = GetItems();

    Assert.Single(result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2021 // Do not use equality check to check for collection single item check.
#pragma warning restore xUnit2021 // Do not use equality check to check for collection single item check.
```
