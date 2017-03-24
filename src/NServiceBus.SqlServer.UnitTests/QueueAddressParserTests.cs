﻿namespace NServiceBus.SqlServer.UnitTests
{
    using NUnit.Framework;
    using Transport.SQLServer;

    [TestFixture]
    public class QueueAddressParserTests
    {
        [Test]
        public void Should_not_change_canonical_address()
        {
            var parser = new QueueAddressTranslator(null, "dbo", null, new QueueSchemaAndCatalogSettings());

            var address = "my.tb[e@[my.[sch@ma]@[my.c@ta]]og]";
            var canonicalAddress = parser.Parse(address).ToString();

            Assert.AreEqual(address, canonicalAddress);
        }
    }
}