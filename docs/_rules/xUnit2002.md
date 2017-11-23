---
title: xUnit2002
description: Do not use null check on value type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when attempting to use `Assert.Null` or `Assert.NotNull` on a value type.

## Reason for rule

Value types, by definition, cannot be `null`. As such, it does not make sense to compare them to `null`.

## How to fix violations

To fix a violation of this rule, just remove the assert.

Violations of this rule may also be a sign that the value was not actually be supposed to be a value type but a reference type.

## Examples

### Violates

```csharp
[Fact]
public void ExampleTest()
{
    int result = GetSomeValue();

    Assert.Null(result);
    Assert.True(result > 4);
}
```

### Does not violate

```csharp
[Fact]
public void ExampleTest()
{
    int result = GetSomeValue();

    Assert.True(result > 4);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2002 // Do not use null check on value type
#pragma warning restore xUnit2002 // Do not use null check on value type
```
