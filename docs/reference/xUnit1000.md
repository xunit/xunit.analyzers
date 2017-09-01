## xUnit1000

<table>
<tr>
  <td>Name</td>
  <td>TestClassMustBePublic</td>
</tr>
<tr>
  <td>ID</td>
  <td>xUnit1000</td>
</tr>
<tr>
  <td>Category</td>
  <td>Usage</td>
</tr>
<tr>
  <td>Severity</td>
  <td>Error</td>
</tr>
</table>

## Cause

A class containing test methods is not public.

## Reason for rule

xUnit will not run the test methods in a class if the class is not public.

## Examples

### Violates

```csharp
class TestClass
{
    [Fact]
    public void TestMethod()
    {
    }
}
```

### Does not violate

```csharp
public class TestClass
{
    [Fact]
    public void TestMethod()
    {
    }
}
```

## How to fix violations

To fix a violation of this rule, make the test class public.
