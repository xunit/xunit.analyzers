using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ConditionalSkipPropertyValidation>;

public class X1054_ConditionalSkipPropertyValidationTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class ContainerClass {
				public static bool AlwaysFalse => false;
				public bool NonStatic => true;
				protected static bool NonPublic => true;
				public static string WrongType => "yes";
			}

			public class DerivedClass : ContainerClass { }

			public class TestClass {
				// Legal property (local type)

				[Fact(SkipUnless = nameof(AlwaysTrue))] public void Legal_Unless_Fact() { }
				[CulturedFact(new[] { "en-US" }, SkipUnless = nameof(AlwaysTrue))] public void Legal_Unless_CulturedFact() { }
				[Theory(SkipUnless = nameof(AlwaysTrue))] public void Legal_Unless_Theory() { }
				[CulturedTheory(new[] { "en-US" }, SkipUnless = nameof(AlwaysTrue))] public void Legal_Unless_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), SkipUnless = nameof(AlwaysTrue))] public void Legal_Unless_ClassData() { }
				[InlineData(SkipUnless = nameof(AlwaysTrue))] public void Legal_Unless_InlineData() { }
				[MemberData("bar", SkipUnless = nameof(AlwaysTrue))] public void Legal_Unless_MemberData() { }
				[ClassData<ContainerClass>(SkipUnless = nameof(AlwaysTrue))] public void Legal_Unless_ClassDataGeneric() { }

				[Fact(SkipWhen = nameof(AlwaysTrue))] public void Legal_When_Fact() { }
				[CulturedFact(new[] { "en-US" }, SkipWhen = nameof(AlwaysTrue))] public void Legal_When_CulturedFact() { }
				[Theory(SkipWhen = nameof(AlwaysTrue))] public void Legal_When_Theory() { }
				[CulturedTheory(new[] { "en-US" }, SkipWhen = nameof(AlwaysTrue))] public void Legal_When_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), SkipWhen = nameof(AlwaysTrue))] public void Legal_When_ClassData() { }
				[InlineData(SkipWhen = nameof(AlwaysTrue))] public void Legal_When_InlineData() { }
				[MemberData("bar", SkipWhen = nameof(AlwaysTrue))] public void Legal_When_MemberData() { }
				[ClassData<ContainerClass>(SkipWhen = nameof(AlwaysTrue))] public void Legal_When_ClassDataGeneric() { }

				// Legal property (different type)

				[Fact(SkipUnless = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_Unless_Fact_OtherType() { }
				[CulturedFact(new[] { "en-US" }, SkipUnless = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_Unless_CulturedFact_OtherType() { }
				[Theory(SkipUnless = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_Unless_Theory_OtherType() { }
				[CulturedTheory(new[] { "en-US" }, SkipUnless = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_Unless_CulturedTheory_OtherType() { }
				[ClassData(typeof(ContainerClass), SkipUnless = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_Unless_ClassData_OtherType() { }
				[InlineData(SkipUnless = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_Unless_InlineData_OtherType() { }
				[MemberData("bar", SkipUnless = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_Unless_MemberData_OtherType() { }
				[ClassData<ContainerClass>(SkipUnless = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_Unless_ClassDataGeneric_OtherType() { }

				[Fact(SkipWhen = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_When_Fact_OtherType() { }
				[CulturedFact(new[] { "en-US" }, SkipWhen = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_When_CulturedFact_OtherType() { }
				[Theory(SkipWhen = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_When_Theory_OtherType() { }
				[CulturedTheory(new[] { "en-US" }, SkipWhen = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_When_CulturedTheory_OtherType() { }
				[ClassData(typeof(ContainerClass), SkipWhen = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_When_ClassData_OtherType() { }
				[InlineData(SkipWhen = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_When_InlineData_OtherType() { }
				[MemberData("bar", SkipWhen = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_When_MemberData_OtherType() { }
				[ClassData<ContainerClass>(SkipWhen = nameof(ContainerClass.AlwaysFalse), SkipType = typeof(ContainerClass))] public void Legal_When_ClassDataGeneric_OtherType() { }

				// Legal property (intermediate type)

				[Fact(SkipUnless = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_Unless_Fact_IntermediateType() { }
				[CulturedFact(new[] { "en-US" }, SkipUnless = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_Unless_CulturedFact_IntermediateType() { }
				[Theory(SkipUnless = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_Unless_Theory_IntermediateType() { }
				[CulturedTheory(new[] { "en-US" }, SkipUnless = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_Unless_CulturedTheory_IntermediateType() { }
				[ClassData(typeof(DerivedClass), SkipUnless = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_Unless_ClassData_IntermediateType() { }
				[InlineData(SkipUnless = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_Unless_InlineData_IntermediateType() { }
				[MemberData("bar", SkipUnless = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_Unless_MemberData_IntermediateType() { }
				[ClassData<DerivedClass>(SkipUnless = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_Unless_ClassDataGeneric_IntermediateType() { }

				[Fact(SkipWhen = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_When_Fact_IntermediateType() { }
				[CulturedFact(new[] { "en-US" }, SkipWhen = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_When_CulturedFact_IntermediateType() { }
				[Theory(SkipWhen = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_When_Theory_IntermediateType() { }
				[CulturedTheory(new[] { "en-US" }, SkipWhen = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_When_CulturedTheory_IntermediateType() { }
				[ClassData(typeof(DerivedClass), SkipWhen = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_When_ClassData_IntermediateType() { }
				[InlineData(SkipWhen = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_When_InlineData_IntermediateType() { }
				[MemberData("bar", SkipWhen = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_When_MemberData_IntermediateType() { }
				[ClassData<DerivedClass>(SkipWhen = "AlwaysFalse", SkipType = typeof(DerivedClass))] public void Legal_When_ClassDataGeneric_IntermediateType() { }

				// Unknown property

				[Fact({|#0:SkipUnless = "foo"|})] public void MissingProperty_Unless_Fact() { }
				[CulturedFact(new[] { "en-US" }, {|#1:SkipUnless = "foo"|})] public void MissingProperty_Unless_CulturedFact() { }
				[Theory({|#2:SkipUnless = "foo"|})] public void MissingProperty_Unless_Theory() { }
				[CulturedTheory(new[] { "en-us" }, {|#3:SkipUnless = "foo"|})] public void MissingProperty_Unless_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), {|#4:SkipUnless = "foo"|})] public void MissingProperty_Unless_ClassData() { }
				[InlineData({|#5:SkipUnless = "foo"|})] public void MissingProperty_Unless_InlineData() { }
				[MemberData("bar", {|#6:SkipUnless = "foo"|})] public void MissingProperty_Unless_MemberData() { }
				[ClassData<ContainerClass>({|#7:SkipUnless = "foo"|})] public void MissingProperty_Unless_ClassDataGeneric() { }

				[Fact({|#10:SkipWhen = "foo"|})] public void MissingProperty_When_Fact() { }
				[CulturedFact(new[] { "en-US" }, {|#11:SkipWhen = "foo"|})] public void MissingProperty_When_CulturedFact() { }
				[Theory({|#12:SkipWhen = "foo"|})] public void MissingProperty_When_Theory() { }
				[CulturedTheory(new[] { "en-us" }, {|#13:SkipWhen = "foo"|})] public void MissingProperty_When_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), {|#14:SkipWhen = "foo"|})] public void MissingProperty_When_ClassData() { }
				[InlineData({|#15:SkipWhen = "foo"|})] public void MissingProperty_When_InlineData() { }
				[MemberData("bar", {|#16:SkipWhen = "foo"|})] public void MissingProperty_When_MemberData() { }
				[ClassData<ContainerClass>({|#17:SkipWhen = "foo"|})] public void MissingProperty_When_ClassDataGeneric() { }

				// Non-static (local type)

				[Fact({|#20:SkipUnless = nameof(NonStatic)|})] public void NonStaticProperty_Unless_Fact() { }
				[CulturedFact(new[] { "en-US" }, {|#21:SkipUnless = nameof(NonStatic)|})] public void NonStaticProperty_Unless_CulturedFact() { }
				[Theory({|#22:SkipUnless = nameof(NonStatic)|})] public void NonStaticProperty_Unless_Theory() { }
				[CulturedTheory(new[] { "en-us" }, {|#23:SkipUnless = nameof(NonStatic)|})] public void NonStaticProperty_Unless_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), {|#24:SkipUnless = nameof(NonStatic)|})] public void NonStaticProperty_Unless_ClassData() { }
				[InlineData({|#25:SkipUnless = nameof(NonStatic)|})] public void NonStaticProperty_Unless_InlineData() { }
				[MemberData("bar", {|#26:SkipUnless = nameof(NonStatic)|})] public void NonStaticProperty_Unless_MemberData() { }
				[ClassData<ContainerClass>({|#27:SkipUnless = nameof(NonStatic)|})] public void NonStaticProperty_Unless_ClassDataGeneric() { }

				[Fact({|#30:SkipWhen = nameof(NonStatic)|})] public void NonStaticProperty_When_Fact() { }
				[CulturedFact(new[] { "en-US" }, {|#31:SkipWhen = nameof(NonStatic)|})] public void NonStaticProperty_When_CulturedFact() { }
				[Theory({|#32:SkipWhen = nameof(NonStatic)|})] public void NonStaticProperty_When_Theory() { }
				[CulturedTheory(new[] { "en-us" }, {|#33:SkipWhen = nameof(NonStatic)|})] public void NonStaticProperty_When_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), {|#34:SkipWhen = nameof(NonStatic)|})] public void NonStaticProperty_When_ClassData() { }
				[InlineData({|#35:SkipWhen = nameof(NonStatic)|})] public void NonStaticProperty_When_InlineData() { }
				[MemberData("bar", {|#36:SkipWhen = nameof(NonStatic)|})] public void NonStaticProperty_When_MemberData() { }
				[ClassData<ContainerClass>({|#37:SkipWhen = nameof(NonStatic)|})] public void NonStaticProperty_When_ClassDataGeneric() { }

				// Non-static (different type)

				[Fact({|#40:SkipUnless = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_Unless_Fact_OtherType() { }
				[CulturedFact(new[] { "en-US" }, {|#41:SkipUnless = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_Unless_CulturedFact_OtherType() { }
				[Theory({|#42:SkipUnless = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_Unless_Theory_OtherType() { }
				[CulturedTheory(new[] { "en-us" }, {|#43:SkipUnless = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_Unless_CulturedTheory_OtherType() { }
				[ClassData(typeof(ContainerClass), {|#44:SkipUnless = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_Unless_ClassData_OtherType() { }
				[InlineData({|#45:SkipUnless = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_Unless_InlineData_OtherType() { }
				[MemberData("bar", {|#46:SkipUnless = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_Unless_MemberData_OtherType() { }
				[ClassData<ContainerClass>({|#47:SkipUnless = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_Unless_ClassDataGeneric_OtherType() { }

				[Fact({|#50:SkipWhen = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_When_Fact_OtherType() { }
				[CulturedFact(new[] { "en-US" }, {|#51:SkipWhen = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_When_CulturedFact_OtherType() { }
				[Theory({|#52:SkipWhen = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_When_Theory_OtherType() { }
				[CulturedTheory(new[] { "en-us" }, {|#53:SkipWhen = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_When_CulturedTheory_OtherType() { }
				[ClassData(typeof(ContainerClass), {|#54:SkipWhen = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_When_ClassData_OtherType() { }
				[InlineData({|#55:SkipWhen = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_When_InlineData_OtherType() { }
				[MemberData("bar", {|#56:SkipWhen = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_When_MemberData_OtherType() { }
				[ClassData<ContainerClass>({|#57:SkipWhen = nameof(NonStatic)|}, SkipType = typeof(ContainerClass))] public void NonStaticProperty_When_ClassDataGeneric_OtherType() { }

				// Non-public (local type)

				[Fact({|#60:SkipUnless = nameof(NonPublic)|})] public void NonPublicProperty_Unless_Fact() { }
				[CulturedFact(new[] { "en-US" }, {|#61:SkipUnless = nameof(NonPublic)|})] public void NonPublicProperty_Unless_CulturedFact() { }
				[Theory({|#62:SkipUnless = nameof(NonPublic)|})] public void NonPublicProperty_Unless_Theory() { }
				[CulturedTheory(new[] { "en-us" }, {|#63:SkipUnless = nameof(NonPublic)|})] public void NonPublicProperty_Unless_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), {|#64:SkipUnless = nameof(NonPublic)|})] public void NonPublicProperty_Unless_ClassData() { }
				[InlineData({|#65:SkipUnless = nameof(NonPublic)|})] public void NonPublicProperty_Unless_InlineData() { }
				[MemberData("bar", {|#66:SkipUnless = nameof(NonPublic)|})] public void NonPublicProperty_Unless_MemberData() { }
				[ClassData<ContainerClass>({|#67:SkipUnless = nameof(NonPublic)|})] public void NonPublicProperty_Unless_ClassDataGeneric() { }

				[Fact({|#70:SkipWhen = nameof(NonPublic)|})] public void NonPublicProperty_When_Fact() { }
				[CulturedFact(new[] { "en-US" }, {|#71:SkipWhen = nameof(NonPublic)|})] public void NonPublicProperty_When_CulturedFact() { }
				[Theory({|#72:SkipWhen = nameof(NonPublic)|})] public void NonPublicProperty_When_Theory() { }
				[CulturedTheory(new[] { "en-us" }, {|#73:SkipWhen = nameof(NonPublic)|})] public void NonPublicProperty_When_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), {|#74:SkipWhen = nameof(NonPublic)|})] public void NonPublicProperty_When_ClassData() { }
				[InlineData({|#75:SkipWhen = nameof(NonPublic)|})] public void NonPublicProperty_When_InlineData() { }
				[MemberData("bar", {|#76:SkipWhen = nameof(NonPublic)|})] public void NonPublicProperty_When_MemberData() { }
				[ClassData<ContainerClass>({|#77:SkipWhen = nameof(NonPublic)|})] public void NonPublicProperty_When_ClassDataGeneric() { }

				// Non-public (different type)

				[Fact({|#80:SkipUnless = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_Unless_Fact_OtherType() { }
				[CulturedFact(new[] { "en-US" }, {|#81:SkipUnless = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_Unless_CulturedFact_OtherType() { }
				[Theory({|#82:SkipUnless = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_Unless_Theory_OtherType() { }
				[CulturedTheory(new[] { "en-us" }, {|#83:SkipUnless = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_Unless_CulturedTheory_OtherType() { }
				[ClassData(typeof(ContainerClass), {|#84:SkipUnless = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_Unless_ClassData_OtherType() { }
				[InlineData({|#85:SkipUnless = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_Unless_InlineData_OtherType() { }
				[MemberData("bar", {|#86:SkipUnless = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_Unless_MemberData_OtherType() { }
				[ClassData<ContainerClass>({|#87:SkipUnless = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_Unless_ClassDataGeneric_OtherType() { }

				[Fact({|#90:SkipWhen = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_When_Fact_OtherType() { }
				[CulturedFact(new[] { "en-US" }, {|#91:SkipWhen = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_When_CulturedFact_OtherType() { }
				[Theory({|#92:SkipWhen = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_When_Theory_OtherType() { }
				[CulturedTheory(new[] { "en-us" }, {|#93:SkipWhen = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_When_CulturedTheory_OtherType() { }
				[ClassData(typeof(ContainerClass), {|#94:SkipWhen = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_When_ClassData_OtherType() { }
				[InlineData({|#95:SkipWhen = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_When_InlineData_OtherType() { }
				[MemberData("bar", {|#96:SkipWhen = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_When_MemberData_OtherType() { }
				[ClassData<ContainerClass>({|#97:SkipWhen = nameof(NonPublic)|}, SkipType = typeof(ContainerClass))] public void NonPublicProperty_When_ClassDataGeneric_OtherType() { }

				// Wrong type (local type)

				[Fact({|#100:SkipUnless = nameof(WrongType)|})] public void WrongTypeProperty_Unless_Fact() { }
				[CulturedFact(new[] { "en-US" }, {|#101:SkipUnless = nameof(WrongType)|})] public void WrongTypeProperty_Unless_CulturedFact() { }
				[Theory({|#102:SkipUnless = nameof(WrongType)|})] public void WrongTypeProperty_Unless_Theory() { }
				[CulturedTheory(new[] { "en-us" }, {|#103:SkipUnless = nameof(WrongType)|})] public void WrongTypeProperty_Unless_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), {|#104:SkipUnless = nameof(WrongType)|})] public void WrongTypeProperty_Unless_ClassData() { }
				[InlineData({|#105:SkipUnless = nameof(WrongType)|})] public void WrongTypeProperty_Unless_InlineData() { }
				[MemberData("bar", {|#106:SkipUnless = nameof(WrongType)|})] public void WrongTypeProperty_Unless_MemberData() { }
				[ClassData<ContainerClass>({|#107:SkipUnless = nameof(WrongType)|})] public void WrongTypeProperty_Unless_ClassDataGeneric() { }

				[Fact({|#110:SkipWhen = nameof(WrongType)|})] public void WrongTypeProperty_When_Fact() { }
				[CulturedFact(new[] { "en-US" }, {|#111:SkipWhen = nameof(WrongType)|})] public void WrongTypeProperty_When_CulturedFact() { }
				[Theory({|#112:SkipWhen = nameof(WrongType)|})] public void WrongTypeProperty_When_Theory() { }
				[CulturedTheory(new[] { "en-us" }, {|#113:SkipWhen = nameof(WrongType)|})] public void WrongTypeProperty_When_CulturedTheory() { }
				[ClassData(typeof(ContainerClass), {|#114:SkipWhen = nameof(WrongType)|})] public void WrongTypeProperty_When_ClassData() { }
				[InlineData({|#115:SkipWhen = nameof(WrongType)|})] public void WrongTypeProperty_When_InlineData() { }
				[MemberData("bar", {|#116:SkipWhen = nameof(WrongType)|})] public void WrongTypeProperty_When_MemberData() { }
				[ClassData<ContainerClass>({|#117:SkipWhen = nameof(WrongType)|})] public void WrongTypeProperty_When_ClassDataGeneric() { }

				// Wrong type (different type)

				[Fact({|#120:SkipUnless = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_Unless_Fact_OtherType() { }
				[CulturedFact(new[] { "en-US" }, {|#121:SkipUnless = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_Unless_CulturedFact_OtherType() { }
				[Theory({|#122:SkipUnless = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_Unless_Theory_OtherType() { }
				[CulturedTheory(new[] { "en-us" }, {|#123:SkipUnless = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_Unless_CulturedTheory_OtherType() { }
				[ClassData(typeof(ContainerClass), {|#124:SkipUnless = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_Unless_ClassData_OtherType() { }
				[InlineData({|#125:SkipUnless = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_Unless_InlineData_OtherType() { }
				[MemberData("bar", {|#126:SkipUnless = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_Unless_MemberData_OtherType() { }
				[ClassData<ContainerClass>({|#127:SkipUnless = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_Unless_ClassDataGeneric_OtherType() { }

				[Fact({|#130:SkipWhen = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_When_Fact_OtherType() { }
				[CulturedFact(new[] { "en-US" }, {|#131:SkipWhen = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_When_CulturedFact_OtherType() { }
				[Theory({|#132:SkipWhen = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_When_Theory_OtherType() { }
				[CulturedTheory(new[] { "en-us" }, {|#133:SkipWhen = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_When_CulturedTheory_OtherType() { }
				[ClassData(typeof(ContainerClass), {|#134:SkipWhen = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_When_ClassData_OtherType() { }
				[InlineData({|#135:SkipWhen = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_When_InlineData_OtherType() { }
				[MemberData("bar", {|#136:SkipWhen = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_When_MemberData_OtherType() { }
				[ClassData<ContainerClass>({|#137:SkipWhen = nameof(WrongType)|}, SkipType = typeof(ContainerClass))] public void WrongTypeProperty_When_ClassDataGeneric_OtherType() { }
			
				public static bool AlwaysTrue => true;
				public bool NonStatic => true;
				protected static bool NonPublic => true;
				public static string WrongType => "yes";
			}
			""";
		var expected =
			Enumerable.Range(0, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("TestClass", "foo")).Concat(
			Enumerable.Range(10, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("TestClass", "foo"))).Concat(
			Enumerable.Range(20, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("TestClass", "NonStatic"))).Concat(
			Enumerable.Range(30, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("TestClass", "NonStatic"))).Concat(
			Enumerable.Range(40, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("ContainerClass", "NonStatic"))).Concat(
			Enumerable.Range(50, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("ContainerClass", "NonStatic"))).Concat(
			Enumerable.Range(60, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("TestClass", "NonPublic"))).Concat(
			Enumerable.Range(70, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("TestClass", "NonPublic"))).Concat(
			Enumerable.Range(80, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("ContainerClass", "NonPublic"))).Concat(
			Enumerable.Range(90, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("ContainerClass", "NonPublic"))).Concat(
			Enumerable.Range(100, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("TestClass", "WrongType"))).Concat(
			Enumerable.Range(110, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("TestClass", "WrongType"))).Concat(
			Enumerable.Range(120, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("ContainerClass", "WrongType"))).Concat(
			Enumerable.Range(130, 8).Select(n => Verify.Diagnostic("xUnit1054").WithLocation(n).WithArguments("ContainerClass", "WrongType"))).ToArray();

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
	}
}
