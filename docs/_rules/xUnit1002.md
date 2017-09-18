---
title: xUnit1002
description: Test methods cannot have multiple Fact or Theory attributes
category: Usage
severity: Error
---

## Cause

A test method has multiple Fact or Theory attributes.

## Reason for rule

A test method only needs one Fact or Theory attribute.

## How to fix violations

To fix a violation of this rule, remove all but one of the Fact or Theory attributes.

## Examples

### Violates

Example(s) of code that violates the rule. [example](_rules/xUnit1000.md#violates)

### Does not violate

Example(s) of code that does not violate the rule. [example](_rules/xUnit1000.md#does-not-violate)

## How to suppress violations

**If the severity of your analyzer isn't _Warning_, delete this section.**

```csharp
#pragma warning disable xUnit0000 // <Rule name>
#pragma warning restore xUnit0000 // <Rule name>
```
