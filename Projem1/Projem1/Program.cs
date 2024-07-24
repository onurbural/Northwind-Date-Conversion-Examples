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


        await CalismaAyrilmaSaatiEkle(dbContext);
        await Console.Out.WriteLineAsync("Giriş ve Saat çıkışları eklendi");

        await CalisanCalismaYiliHesaplama(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await CalisanYasHesaplama(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await SiparisGeciktiMi(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await GunHesaplama(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await CalismaGunu(dbContext);      
        await Console.Out.WriteLineAsync("\n**********\n");
        await KacSaatCalisti(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await ToplamCalismaSaati(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await MesaiyeGecKalmaKontrolu(dbContext);
        await Console.Out.WriteLineAsync("\n**********\n");
        await CalismaSaatleriniFormatlama(dbContext);
    }

    static async Task CalismaAyrilmaSaatiEkle(NorthwindContext dbContext)
    {
        var tumCalisanlar = await dbContext.Employees.ToListAsync();

        Random gen = new Random();
        int range = 24 * 60; 

        foreach (var item in tumCalisanlar)
        {
            if (!item.EntranceTime.HasValue && !item.ExitTime.HasValue)
            {
                TimeOnly randomEntranceTime = TimeOnly.FromTimeSpan(
                    TimeSpan.FromMinutes(gen.Next(range))
                );

                TimeOnly randomExitTime = randomEntranceTime.AddMinutes(gen.Next(1, 60));

                item.EntranceTime = randomEntranceTime;
                item.ExitTime = randomExitTime;
            }
        }

        await dbContext.SaveChangesAsync();

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

    static async Task KacSaatCalisti(NorthwindContext dbContext)
    {
        var query = await dbContext.Employees.Select(x => new
        {
            Name = x.FirstName,
            CalismaDakikasi = EF.Functions.DateDiffMinute(x.EntranceTime, x.ExitTime)
        }).ToListAsync();

        foreach (var item in query)
        {
            if(item.CalismaDakikasi >= 0)
            {
                Console.WriteLine($"{item.Name} isimli çalışma arkadaşımız bugün {item.CalismaDakikasi} dakika kadar çalışmış.");
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

    static async Task ToplamCalismaSaati(NorthwindContext dbContext)
    {
        var query = await dbContext.Employees
            .Select(x => new
            {
                Name = x.FirstName,
                TotalWorkHours = x.EntranceTime.HasValue && x.ExitTime.HasValue
                    ? x.ExitTime.Value.ToTimeSpan().Subtract(x.EntranceTime.Value.ToTimeSpan()).TotalHours
                    : (double?)null
            })
            .ToListAsync();

        foreach (var item in query)
        {
            if (item.TotalWorkHours.HasValue)
            {
                Console.WriteLine($"{item.Name} isimli çalışma arkadaşımızın toplam çalışma süresi {item.TotalWorkHours:F2} saat.");
            }
        }
    }

    static async Task MesaiyeGecKalmaKontrolu(NorthwindContext dbContext)
    {
        TimeOnly mesaiBaslangicSaati = new TimeOnly(9, 0); // saat 09:00

        var query = await dbContext.Employees
            .Select(x => new
            {
                Name = x.FirstName,
                IsLate = x.EntranceTime.HasValue && x.EntranceTime.Value > mesaiBaslangicSaati,
                LateTime = x.EntranceTime.HasValue
                    ? (x.EntranceTime.Value.Hour * 60 + x.EntranceTime.Value.Minute) - (mesaiBaslangicSaati.Hour * 60 + mesaiBaslangicSaati.Minute)
                    : (int?)null
            })
            .ToListAsync();

        foreach (var item in query)
        {
            if (item.IsLate && item.LateTime.HasValue)
            {
                int lateHours = item.LateTime.Value / 60;
                int lateMinutes = item.LateTime.Value % 60;

                Console.WriteLine($"{item.Name} isimli çalışan mesaiye {lateHours} saat {lateMinutes} dakika geç kalmıştır.");
            }
            else
            {
                Console.WriteLine($"{item.Name} isimli çalışan mesaiye zamanında gelmiştir.");
            }
        }
    }


    static async Task CalismaSaatleriniFormatlama(NorthwindContext dbContext)
    {
        var query = await dbContext.Employees
            .Select(x => new
            {
                Name = x.FirstName,
                EntranceTimeFormatted = x.EntranceTime.HasValue ? x.EntranceTime.Value.ToString("HH:mm") : "N/A",
                ExitTimeFormatted = x.ExitTime.HasValue ? x.ExitTime.Value.ToString("HH:mm") : "N/A"
            })
            .ToListAsync();

        foreach (var item in query)
        {
            Console.WriteLine($"{item.Name} - Giriş Saati: {item.EntranceTimeFormatted}, Çıkış Saati: {item.ExitTimeFormatted}");
        }
    }


}
