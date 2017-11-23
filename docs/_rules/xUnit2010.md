---
title: xUnit2010
description: Do not use boolean check to check for string equality
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when using `Assert.True` or `Assert.False` to check for string equality with `string.Equals`.

## Reason for rule

There are specialized assert methods for equality checks.

## How to fix violations

Use `Assert.Equal` or `Assert.NotEqual` to check for string equality.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
    string result = "foo bar baz";

    Assert.True(string.Equals("foo bar baz", result));
    Assert.False(string.Equals("hello world", result));
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
    string result = "foo bar baz";

    Assert.Equal("foo bar baz", result);
    Assert.NotEqual("hello world", result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2010 // Do not use boolean check to check for string equality
#pragma warning restore xUnit2010 // Do not use boolean check to check for string equality
```
