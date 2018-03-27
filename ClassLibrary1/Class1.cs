using Microsoft.Extensions.DependencyInjection;
using System;

namespace ClassLibrary1
{

	public interface IViewModel : IDisposable
	{ }

	public abstract class ViewModel : IViewModel
	{
		protected IDisposable Disposable { get; }
		public void Dispose()
		{
			Disposable.Dispose();
		}
	}
	public interface IVMFactory<TVM, TParam1>
		where TVM : IViewModel
	{
		TVM Create(TParam1 param1);
	}
	public interface IVMFactory<out TVM, in TParam1, in TParam2>
		where TVM : IViewModel
	{
		TVM Create(TParam1 param1, TParam2 param2);
	}

	public abstract class VMFactory<TVM, TImpl, TParam1> : IVMFactory<TVM, TParam1>
		where TVM : IViewModel
		where TImpl : TVM
	{
		IServiceProvider _provider;
		public VMFactory(IServiceProvider provider)
		{
			_provider = provider;
		}
		public TVM Create(TParam1 param1)
		{
			return (TVM)ActivatorUtilities.CreateInstance<TImpl>(_provider, param1);
		}
	}
	public abstract class VMFactory<TVM, TImpl, TParam1, TParam2> : IVMFactory<TVM, TParam1, TParam2>
		where TVM : IViewModel
		where TImpl : TVM
	{
		IServiceProvider _provider;
		public VMFactory(IServiceProvider provider)
		{
			_provider = provider;
		}
		public TVM Create(TParam1 param1, TParam2 param2)
		{
			return (TVM)ActivatorUtilities.CreateInstance<TImpl>(_provider, param1, param2);
		}
	}


}
