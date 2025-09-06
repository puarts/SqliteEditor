using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteEditor.Utilities;

namespace SqliteEditorTest;

[TestClass]
public class FehDatabaseTest
{
    [TestMethod]
    [DataRow("UŒ‚‘¬‚³‚Ì•Ÿ‰¹")]
    [DataRow("U–‚ŒÛ•‘E•±Œƒ")]
    [DataRow("UŒ‚–‚–h‚ÌŠÅ”j")]
    public void EstimateInheritance_Passive(string name)
    {
        FehSkillUtility.EstimateInheritance(name, "ƒpƒbƒVƒuA").Should().BeTrue();
    }
}
