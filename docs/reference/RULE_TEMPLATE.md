## xUnit0000

<table>
<tr>
  <td>Name</td>
  <td>Rule name</td>
</tr>
<tr>
  <td>ID</td>
  <td>xUnit0000</td>
</tr>
<tr>
  <td>Category</td>
  <td>Diagnostic category</td>
</tr>
<tr>
  <td>Severity</td>
  <td>Hidden, Info, Warning, or Error</td>
</tr>
</table>

## Cause

A concise-as-possible description of when this rule is violated. If there's a lot to explain, begin with "A violation of this rule occurs when..." [example 1](xUnit1000.md#cause), [example 2](xUnit2004.md#cause)

## Reason for rule

Explain why the user should care if (s)he violates the rule. [example](xUnit1000.md#reason-for-rule)

## Examples

### Violates

Example(s) of code that violates the rule. [example](xUnit1000.md#violates)

### Does not violate

Example(s) of code that does not violate the rule. [example](xUnit1000.md#does-not-violate)

## How to fix violations

To fix a violation of this rule, [describe how to fix a violation]. [example](xUnit1000.md#how-to-fix-violations)

## How to suppress violations

**If the severity of your analyzer isn't _Warning_, delete this section.**

```csharp
#pragma warning disable xUnit0000 // <Rule name>
#pragma warning restore xUnit0000 // <Rule name>
```
