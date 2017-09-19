---
title: xUnit1003
description: Theory methods must have test data
category: Usage
severity: Error
---

## Cause

A Theory method does not have test data.

## Reason for rule

If a Theory method does not have test data, it is never run.

## How to fix violations

- Add a data attribute such as InlineData, MemberData, or ClassData to the test method.
- Change `[Theory]` to `[Fact]` if you want a non-parameterized test.

## Examples

### Violates

```csharp
public class TestClass
{
    [Theory]
    public void TestMethod(int p1)
    {
    }
}
```

### Does not violate

```csharp
public class TestClass
{
    [Fact]
}
```

## How to suppress violations

**If the severity of your analyzer isn't _Warning_, delete this section.**

```csharp
#pragma warning disable xUnit0000 // <Rule name>
#pragma warning restore xUnit0000 // <Rule name>
```
