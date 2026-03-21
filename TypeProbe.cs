using System;
using System.Linq;
using System.Reflection;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;

namespace MQAstraALT;

static class TypeProbe
{
    public static void Run()
    {
        DumpPackageMethods();
        Dump(typeof(PackageDataLocation));
        Dump(typeof(LocationTargetRadius));
        Dump(typeof(LocationTarget));
        Dump(typeof(LocationFallback));
        Dump(typeof(PackageDataTarget));
        Dump(typeof(PackageTargetSpecificReference));
        Dump(typeof(PackageTargetObjectID));
        Dump(typeof(PackageTargetAlias));
        Dump(typeof(SceneAction));
        Dump(typeof(SceneActionTypicalType));
    }

    static void DumpPackageMethods()
    {
        Console.WriteLine(new string('=', 80));
        Console.WriteLine(typeof(Package).FullName);
        Console.WriteLine("Interesting methods:");
        foreach (var method in typeof(Package).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                              .Where(m => m.Name.Contains("Copy", StringComparison.OrdinalIgnoreCase)
                                                       || m.Name.Contains("Deep", StringComparison.OrdinalIgnoreCase)
                                                       || m.Name.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"  {method}");
        }
        Console.WriteLine("Writable properties:");
        foreach (var prop in typeof(Package).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                            .Where(p => p.SetMethod != null))
        {
            Console.WriteLine($"  {prop.PropertyType.Name} {prop.Name}");
        }

        var probe = new Package(default, Fallout4Release.Fallout4);
        Console.WriteLine($"Data runtime type: {probe.Data.GetType().FullName}");
        Console.WriteLine("Data methods:");
        foreach (var method in probe.Data.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                         .Where(m => m.Name == "Add"))
        {
            Console.WriteLine($"  {method}");
        }
    }

    static void Dump(Type type)
    {
        Console.WriteLine(new string('=', 80));
        Console.WriteLine(type.FullName);
        Console.WriteLine("Constructors:");
        foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
        {
            Console.WriteLine($"  {ctor}");
        }

        Console.WriteLine("Writable properties:");
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.SetMethod != null))
        {
            Console.WriteLine($"  {prop.PropertyType.Name} {prop.Name}");
        }
    }
}
