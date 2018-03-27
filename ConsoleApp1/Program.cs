using ClassLibrary1;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
	// Ein Service und Implementierung

	public interface IService
	{
		void Action();
	}
	public class Service : IService
	{
		public Service()
		{ }
		public void Action()
		{ }
	}

	// Noch ein Service und Implementierung mit Abhängigkeit von IService

	public interface IFancyService
	{
		void Bäm(IBarViewModel barViewModel);
	}

	public class FancyService : IFancyService
	{
		private readonly IService _service;

		public FancyService(IService service)
		{
			_service = service;
		}
		public void Bäm(IBarViewModel barViewModel)
		{ }
	}

	// Zwei "Domain" Klassen

	public class Foo
	{ }
	public class Bar
	{ }

	// 1. ViewModel für Foo. Implementierung kriegt ein Foo und ist abhängig von der BarViewModelFactory und IFancyService

	public interface IFooViewModel : IViewModel
	{
		Foo Foo { get; }
		void MakeBäm();
	}
	public class FooViewModel : ViewModel, IFooViewModel
	{
		private IBarVMFactory _barVMFactory;
		private IFancyService _fancy;
		public FooViewModel(Foo foo, IBarVMFactory barVMFactory, IFancyService fancy)
		{
			_barVMFactory = barVMFactory;
			_fancy = fancy;

			Foo = foo;
		}
		public Foo Foo { get; }

		public void MakeBäm()
		{
			var bar = new Bar();
			// So geht die Erzeugung eines BarViewModel das ein Bar und dieses FooViewModel kriegt
			// Weitere Abhängigkeiten werden aus dem Container bedient
			// Die Parametertypen von Create werden generisch an der VMFactory definiert, weitere Abhängigkeiten nur im ctor der Implementierung
			// Ergebnis ist immer nur das Interface
			IBarViewModel barViewModel = _barVMFactory.Create(bar, this);

			_fancy.Bäm(barViewModel);
		}
	}

	// 2. VIewModel für Bar. Implementierung kriegt ein Bar und ein FooViewModel (von dem es erzeugt werden wird) und hat sonst keine Abhängigkeiten

	public interface IBarViewModel : IViewModel
	{
		Bar Bar { get; }
	}
	public class BarViewModel : ViewModel, IBarViewModel
	{
		public BarViewModel(Bar bar, IFooViewModel fooViewModel)
		{
			Bar = bar;
			FooViewModel = fooViewModel;
		}
		public Bar Bar { get; }
		public IFooViewModel FooViewModel { get; }
	}

	// Die FooViewModel Factory (die auch Abhängigkeiten haben kann was aber eher nicht vorkommen wird)
	// Der Code ist Boilerplate. Alles interessante ist in der Basisklasse
	// Generische Typen definieren die Implementierung und die Parameter bei der Erzeugung

	public interface IFooVMFactory : IVMFactory<IFooViewModel, Foo>
	{ }
	public class FooVMFactory : VMFactory<IFooViewModel, FooViewModel, Foo>, IFooVMFactory
	{
		public FooVMFactory(IServiceProvider provider)
			: base(provider)
		{ }
	}

	// Und die BarViewModel Factory

	public interface IBarVMFactory : IVMFactory<IBarViewModel, Bar, IFooViewModel>
	{ }
	public class BarVMFactory : VMFactory<IBarViewModel, BarViewModel, Bar, IFooViewModel>, IBarVMFactory
	{
		public BarVMFactory(IServiceProvider provider)
			: base(provider)
		{ }
	}

	class Program
	{
		static Program()
		{
			// Container erzeugen

			var services = new ServiceCollection();

			services.AddSingleton<IService, Service>();
			services.AddSingleton<IFancyService, FancyService>();

			services.AddSingleton<IFooVMFactory, FooVMFactory>();
			services.AddSingleton<IBarVMFactory, BarVMFactory>();

			services.AddTransient<IFooViewModel, FooViewModel>();
			services.AddTransient<IBarViewModel, BarViewModel>();

			Services = services.BuildServiceProvider();
		}

		// Das ist der Container
		public static IServiceProvider Services { get; }

		static void Main(string[] args)
		{
			// So geht die Erzeugung ohne dass etwas in den Konstruktor injected wurde (die Wurzel)
			IFooVMFactory fooVMFactory = Services.GetRequiredService<IFooVMFactory>();
			Foo foo = new Foo();

			// Die Factory kann ein FooViewModel für ein Foo erzeugen
			// Die Parameter von Create() sind die "hinteren" generischen Typen bei der Factory Definition
			IFooViewModel fooVM = fooVMFactory.Create(foo);

			fooVM.MakeBäm();
		}
	}
}
