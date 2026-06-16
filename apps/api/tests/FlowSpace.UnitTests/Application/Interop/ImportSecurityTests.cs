using FlowSpace.Application.Interop.Converters;
using FlowSpace.Domain.Entities;
using FluentAssertions;
using System.Xml;

namespace FlowSpace.UnitTests.Application.Interop;

public class ImportSecurityTests
{
    [Fact]
    public void FromDrawIoXml_ShouldPreventXXE()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        // XXE Payload attempting to read /etc/passwd (or local file)
        var xxePayload = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <!DOCTYPE diagram [<!ENTITY xxe SYSTEM ""file:///etc/passwd"">]>
            <mxfile>
                <diagram id=""test"" name=""&xxe;"">base64content</diagram>
            </mxfile>";

        // Act & Assert
        // The secure parser should throw an exception or ignore the DTD, preventing the entity from resolving.
        // Prohibit DTD will throw XmlException.
        var act = () => DrawIoConverter.FromDrawIoXml(boardId, xxePayload);
        
        act.Should().Throw<XmlException>().WithMessage("*DTD is prohibited*");
    }

    [Fact]
    public void FromDrawIoXml_ShouldHandleMaliciousRecursion_BillionLaughs()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var billionLaughs = @"<?xml version=""1.0""?>
            <!DOCTYPE lolz [
             <!ENTITY lol ""lol"">
             <!ENTITY lol1 ""&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;"">
             <!ENTITY lol2 ""&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;"">
             <!ENTITY lol3 ""&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;"">
            ]>
            <mxfile><diagram name=""&lol3;"">base64</diagram></mxfile>";

        // Act & Assert
        var act = () => DrawIoConverter.FromDrawIoXml(boardId, billionLaughs);
        act.Should().Throw<XmlException>();
    }
}
