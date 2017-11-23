---
title: xUnit2005
description: Do not use identity check on value type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when comparing two value type objects using `Assert.Same` or `Assert.NotSame`.

## Reason for rule

`Assert.Same` and `Assert.NotSame` will use [`Object.ReferenceEquals`](https://msdn.microsoft.com/en-us/library/system.object.referenceequals.aspx) for the comparison. This check will always fail for value types since the compared value type objects will be boxed before they are being passed to the method which will always create two unequal references. As such, comparing value types based on their identity does not make any sense.

## How to fix violations

To fix a violation of this rule, use `Assert.Equal` or `Assert.NotEqual` instead.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
    DateTime result = GetDateResult();

    Assert.Same(new DateTime(2017, 01, 01), result);
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
    DateTime result = GetDateResult();

    Assert.Equal(new DateTime(2017, 01, 01), result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2005 // Do not use identity check on value type
#pragma warning restore xUnit2005 // Do not use identity check on value type
```
