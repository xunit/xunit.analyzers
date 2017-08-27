## xUnit2004

<table>
<tr>
  <td>Name</td>
  <td>AssertEqualShouldNotBeUsedForBoolLiteralCheck</td>
</tr>
<tr>
  <td>ID</td>
  <td>xUnit2004</td>
</tr>
<tr>
  <td>Category</td>
  <td>Assertions</td>
</tr>
<tr>
  <td>Severity</td>
  <td>Warning</td>
</tr>
</table>

## Cause

A violation of this rule occurs when:

- `Assert.Equal`, `Assert.NotEqual`, `Assert.StrictEqual`, or `Assert.NotStrictEqual` is used
- The expected value is a `true` or `false` literal

## Reason for rule

It's more readable to use `Assert.True` or `Assert.False` instead.

## Examples

### Violates

```csharp
Assert.Equal(true, 2 + 2 == 4);
Assert.Equal(false, 2 + 2 == 5);
```

### Does not violate

```csharp
Assert.True(2 + 2 == 4);
Assert.False(2 + 2 == 5);
```

## How to fix violations

### For `Equal` and `StrictEqual`

- `Assert.Equal(true, b)` => `Assert.True(b)`
- `Assert.StrictEqual(true, b)` => `Assert.True(b)`
- `Assert.Equal(false, b)` => `Assert.False(b)`
- `Assert.StrictEqual(false, b)` => `Assert.False(b)`

### For `NotEqual` and `NotStrictEqual`

- `Assert.NotEqual(true, b)` => `Assert.False(b)`
- `Assert.NotStrictEqual(true, b)` => `Assert.False(b)`
- `Assert.NotEqual(false, b)` => `Assert.True(b)`
- `Assert.NotStrictEqual(false, b)` => `Assert.True(b)`

## How to suppress violations

```csharp
#pragma warning disable xUnit2004 // AssertEqualShouldNotBeUsedForBoolLiteralCheck
#pragma warning restore xUnit2004 // AssertEqualShouldNotBeUsedForBoolLiteralCheck
```
