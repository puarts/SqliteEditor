using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteEditor.Utilities;

namespace SqliteEditorTest;

[TestClass]
public class FehDatabaseTest
{
    [TestMethod]
    public void EstimateInheritance_AnyPattern()
    {
        FehSkillUtility.EstimateInheritance("�U�������̕���", "�p�b�V�uA").Should().BeTrue();
    }
}
