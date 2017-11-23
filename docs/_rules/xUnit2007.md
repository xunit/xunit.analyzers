---
title: xUnit2007
description: Do not use typeof expression to check the type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when using the `typeof` operator with a type checking assert.

## Reason for rule

When the expected type is known at compile-time, the generic overload should be used to check against types.

## How to fix violations

Use the generic overload of `Assert.IsType`, `Assert.IsNotType`, and `Assert.IsAssignableFrom`.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
    string result = "foo bar baz";

    Assert.IsType(typeof(string), result.GetType());
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
    string result = "foo bar baz";

    Assert.IsType<string>(result.GetType());
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2007 // Do not use typeof expression to check the type
#pragma warning restore xUnit2007 // Do not use typeof expression to check the type
```
