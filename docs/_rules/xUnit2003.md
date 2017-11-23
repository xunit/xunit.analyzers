---
title: xUnit2003
description: Do not use equality check to test for null value
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when using equality asserts like `Assert.Equal()` or `Assert.NotEqual` with `null`.

## Reason for rule

`Assert.Null` and `Assert.NotNull` are preferred when checking against `null`.

## How to fix violations

To fix a violation of this rule, replace the offending asserts by `Assert.Null` or `Assert.NotNull`.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
    string result = GetSomeValue();

    Assert.NotEqual(null, result);
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
    string result = GetSomeValue();

    Assert.NotNull(result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2003 // Do not use equality check to test for null value
#pragma warning restore xUnit2003 // Do not use equality check to test for null value
```
