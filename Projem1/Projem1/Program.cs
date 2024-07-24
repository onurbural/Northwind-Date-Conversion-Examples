using Microsoft.EntityFrameworkCore;
using Projem1.Models;
using System;
using System.Linq;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

class Program
{
    static async Task Main()
    {
        var dbContext = new NorthwindContext();
        var users = await dbContext.Employees.AsNoTracking().ToListAsync();


        await CalisanCalismaYiliHesaplama(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await CalisanYasHesaplama(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await SiparisGeciktiMi(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await GunHesaplama(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await CalismaGunu(dbContext);
    }

    static async Task CalisanCalismaYiliHesaplama(NorthwindContext dbContext)
    {
        var query = await dbContext.Employees
            .Select(x => new
            {
                Name = x.FirstName + x.LastName,
                KacYildirCalisan = EF.Functions.DateDiffYear(x.HireDate, DateTime.UtcNow),
            })
            .ToListAsync();

        foreach (var item in query)
        {
            Console.WriteLine($"{item.Name} isimli personel gün itibari ile {item.KacYildirCalisan} yıldır bizimle !");
        }
    }

    static async Task CalisanYasHesaplama(NorthwindContext dbContext)
    {
        var query = await dbContext.Employees
            .Select(x => new
            {
                Name = x.FirstName,
                KacYasinda = EF.Functions.DateDiffYear(x.BirthDate, DateTime.Now)
            }).ToListAsync();

        foreach (var item in query)
        {
            Console.WriteLine($"{item.Name} isimli personelimiz {item.KacYasinda} yaşında !");
        }
    }

    static async Task CalismaGunu(NorthwindContext dbContext)
    {
        var query = await dbContext.Employees.Select(x => new
        {
            Name = x.FirstName,
            KacGunCalismis = EF.Functions.DateDiffDay(x.HireDate, DateTime.Now),
            KacSaatCalismis = EF.Functions.DateDiffHour(x.HireDate, DateTime.Now),
        }).ToListAsync();

        foreach (var item in query)
        {
            Console.WriteLine($"{item.Name} isimli çalışanımız tam {item.KacGunCalismis} gün yani {item.KacSaatCalismis} saat çalışmış ! ");
        }
    }

    static async Task SiparisGeciktiMi(NorthwindContext dbContext)
    {
        var query = await dbContext.Orders
            .Select(x => new
            {
                MusteriAdi = x.Employee.FirstName,
                GecikmeGunu = EF.Functions.DateDiffDay(x.RequiredDate, x.ShippedDate)
            })
            .ToListAsync();

        foreach (var item in query)
        {
            if (item.GecikmeGunu < 0)
            {
                Console.WriteLine($"{item.MusteriAdi} ürününün kargosu {Math.Abs((int)item.GecikmeGunu)} kadar gecikti");
            }
        }


    }

    static async Task GunHesaplama(NorthwindContext dbContext)
    {
        string tarihStr = "2026-11-22 12:47:02.000";
        DateTime sonTeslim = DateTime.ParseExact(tarihStr, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

        var query = await dbContext.Orders.Select(x => new
        {
            Tarihi = EF.Functions.DateDiffHour(x.RequiredDate, sonTeslim)
        }).ToListAsync();

        foreach (var item in query)
        {

            int? totalHours = item.Tarihi;

            int? totalDays = totalHours / 24;
            int? remainingHours = totalHours % 24;

            int? years = totalDays / 365;
            int? remainingDaysAfterYears = totalDays % 365;

            int? months = remainingDaysAfterYears / 30;
            int? days = remainingDaysAfterYears % 30;

            Console.WriteLine($"Aradaki toplam saatsel fark {totalHours}. Parçalı olarak {years} yıl, {months} ay, {days} gün, {remainingHours} saat");
        }
    }
}
