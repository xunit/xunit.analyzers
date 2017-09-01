## xUnit2001

<table>
<tr>
  <td>Name</td>
  <td>AssertEqualsShouldNotBeUsed</td>
</tr>
<tr>
  <td>ID</td>
  <td>xUnit2001</td>
</tr>
<tr>
  <td>Category</td>
  <td>Assertions</td>
</tr>
<tr>
  <td>Severity</td>
  <td>Hidden</td>
</tr>
</table>

## Cause

`Assert.Equals` or `Assert.ReferenceEquals` is used.

## Reason for rule

`Assert.Equals` does not assert that two objects are equal; it exists only to hide the static `Equals` method inherited from `object`. It's a similar story for `Assert.ReferenceEquals`.

## Examples

### Violates

```csharp
var o = new object();
Assert.Equals(o, o);
Assert.ReferenceEquals(o, o);
```

### Does not violate

```csharp
var o = new object();
Assert.Equal(o, o);
Assert.Same(o, o);
```

## How to fix violations

To fix a violation of this rule, use `Assert.Equal` instead of `Equals` and `Assert.Same` instead of `ReferenceEquals`.
