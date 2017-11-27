using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Silphid.Loadzup.Http.Caching;

namespace Silphid.Loadzup.Test.Caching
{
    [TestFixture]
    public class CacheControlTest
    {
        private readonly TimeSpan MaxAge = TimeSpan.FromDays(2);
        private readonly DateTime ExpiryDate = new DateTime(2017, 9, 16, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime DateInHeaders = new DateTime(2017, 10, 14, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateTime DateOfFileOnDisk = new DateTime(2017, 11, 12, 0, 0, 0, DateTimeKind.Utc);
        private readonly TimeSpan DefaultExpirySpan = TimeSpan.FromDays(5);
        
        [Test]
        public void MaxAge_WithDateInHeader_UsesDateInHeader()
        {
            var fixture1 = Create($"dummy, max-age={(int) MaxAge.TotalSeconds}, dummy", null, DateInHeaders);
            var fixture2 = Create($"max-age={(int) MaxAge.TotalSeconds}", null, DateInHeaders);

            var expected = DateInHeaders + MaxAge;
            Assert.That(fixture1.Expiry, Is.EqualTo(expected));
            Assert.That(fixture2.Expiry, Is.EqualTo(expected));
        }
        
        [Test]
        public void MaxAge_WithoutDateInHeader_UsesDateOfFileOnDisk()
        {
            var fixture1 = Create($"dummy, max-age={(int) MaxAge.TotalSeconds}, dummy");
            var fixture2 = Create($"max-age={(int) MaxAge.TotalSeconds}");

            var expected = DateOfFileOnDisk + MaxAge;
            Assert.That(fixture1.Expiry, Is.EqualTo(expected));
            Assert.That(fixture2.Expiry, Is.EqualTo(expected));
        }
        
        [Test]
        public void Expires_IgnoresDateInHeaders()
        {
            var fixture1 = Create(null, ExpiryDate);
            var fixture2 = Create(null, ExpiryDate, DateInHeaders);

            var expected = ExpiryDate;
            Assert.That(fixture1.Expiry, Is.EqualTo(expected));
            Assert.That(fixture2.Expiry, Is.EqualTo(expected));
        }
        
        [Test]
        public void NeitherMaxAgeNorExpires_UsesFileDatePlusDefaultExpirySpan()
        {
            var fixture1 = Create("dummy, dummy");
            var fixture2 = Create(null);

            var expected = DateOfFileOnDisk + DefaultExpirySpan;
            Assert.That(fixture1.Expiry, Is.EqualTo(expected));
            Assert.That(fixture2.Expiry, Is.EqualTo(expected));
        }

        private CacheControl Create(string cacheControl, DateTime? expires = null, DateTime? dateInHeaders = null)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (cacheControl != null)
                headers["cache-control"] = cacheControl;
            
            if (expires != null)
                headers["expires"] = FormatDate(expires.Value);

            if (dateInHeaders != null)
                headers["date"] = FormatDate(dateInHeaders.Value);
            
            return new CacheControl(headers, DateOfFileOnDisk, DefaultExpirySpan);
        }

        private static string FormatDate(DateTime date) =>
            date.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture);
    }
}