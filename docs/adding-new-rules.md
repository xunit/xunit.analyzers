# Adding New Rules

## Overview

This document walks you through the process of adding a new rule to xUnit.net Analyzers.

## Step 0: Open an issue

Head over to https://github.com/xunit/xunit and open an issue for the rule you have in mind. Explain clearly what constitutes a violation of the rule and give us a code sample.

If you wish, you can start working on a PR for your rule right away. However, you run the risk of wasted effort if maintainers don't like your rule. Unless you're absolutely sure they'll accept it as-is, it's better to wait until one of them leaves their feedback on your issue.

If you don't know how to write an analyzer for your rule, request that your issue be labeled up-for-grabs. If you're lucky, some other contributor will pick up the implementation for you.

## Step 1: Add a new analyzer (or modify an existing one)

In most cases, you will want to add a new analyzer to enforce your rule. However, in some cases, there may be a rule that is already implemented and very similar to the one you have in mind. If that's true, it may be easier to simply add your rule's descriptor to the analyzer's list of `SupportedDiagnostics` and modify the analyzer accordingly.

The name of your analyzer should describe the rule(s) it enforces. For example, the `TestClassMustBePublic` analyzer enforces that test classes are public.

## Step 2: Add a code fix provider (or modify an existing one)

If there is not a systematic way to fix violations of your rule, skip this step.

First, check if there is an existing code fix provider that can do the job for you. If there is, simply add your rule's ID to its list of `FixableDiagnosticIds`. Otherwise, add a new code fix provider named `<your analyzer's name>Fixer` and get hacking!

## Step 3: Add tests for your rule

Create a file named `<your analyzer's name>Tests` and add tests for your analyzer. Code fix providers are not currently unit tested-- that is tracked by https://github.com/xunit/xunit/issues/1264.

In addition to adding unit tests for your rule, you should test it out manually in Visual Studio. To do this, right-click `xunit.analyzers.vsix` in Visual Studio and click **Set as StartUp Project**. Then hit F5. An experimental instance of Visual Studio will appear, and it will have your analyzer enabled. Open any xUnit project and type code that will trigger your analyzer; if you wrote a code fix provider, press **Ctrl+.** to run it. Make sure everything works correctly and you experience no crashes.

## Step 4: Add documentation for your rule

Go to `<repo root>/docs/reference/` and create a `.md` file titled with your rule's ID, for example `xUnit1000.md`. Copy and paste the contents of [RULE_TEMPLATE.md](reference/RULE_TEMPLATE.md) into the new file, and modify the contents accordingly.

You should also add a **help link URI** to your rule's descriptor that points to your newly added documentation. A user is taken to the help link URI if (s)he sees your diagnostic in Visual Studio and clicks on the rule ID. To add one, pass in `helpLinkUri: "https://xunit.github.io/xunit.analyzers/<rule ID>"` when you create a `new DiagnosticDescriptor` for your rule.
