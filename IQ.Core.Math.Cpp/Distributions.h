// IQ.Core.Math.Cpp.h

#pragma once

#include <random>
#include <functional>
#include <chrono>
#include <array>

#pragma warning(disable : 4793)
#include <armadillo>


using namespace System;

using namespace arma;

typedef _int8 int8;
typedef unsigned _int8 uint8;
typedef short int16;
typedef unsigned short uint16;
typedef int int32;
typedef unsigned int uint32;
typedef long long int64;
typedef unsigned long long uint64;
typedef float float32;
typedef double float64;


namespace IQ { namespace Core {namespace Math { 

	
	private ref class StatsService : public IStats
	{
	private:

		template <typename T>
		static void runif_int_1(System::Array^ dst, T min, T max, int count)
		{
			std::random_device rd;
			std::default_random_engine engine(rd());
			std::uniform_int_distribution<T> src(min, max);

			for (int i = 0; i < count; i++) {
				dst->SetValue(src(engine), i);
			}
		}

		template <typename T, typename S>
		static void runif_int_2(System::Array^ dst, T min, T max, int count)
		{
			std::random_device rd;
			std::default_random_engine engine(rd());
			std::uniform_int_distribution<T> src(min, max);

			for (int i = 0; i < count; i++) {
				dst->SetValue((S)src(engine), i);
			}
		}

	public:
		generic <typename T>
			virtual array<T>^ runifd(int count, T _min, T _max)
		{
			array< T >^ samples = gcnew array< T >(count);

			if (T::typeid == uint8::typeid)
			{
				auto min = (int)System::Convert::ChangeType(_min, int32::typeid);
				auto max = (int)System::Convert::ChangeType(_max, int32::typeid);
				runif_int_2<int32, uint8>(samples, min, max, count);
			}
			else if (T::typeid == int16::typeid)
			{
				runif_int_1<int16>(samples, (int16)_min, (int16)_max, count);
			}
			else if (T::typeid == uint16::typeid)
			{
				runif_int_1<uint16>(samples, (uint16)_min, (uint16)_max, count);
			}
			else if (T::typeid == int32::typeid)
			{
				runif_int_1<int32>(samples, (int32)_min, (int32)_max, count);
			}
			else if (T::typeid == uint32::typeid)
			{
				runif_int_1<uint32>(samples, (uint32)_min, (uint32)_max, count);
			}
			else if (T::typeid == int64::typeid)
			{
				runif_int_1<int64>(samples, (int64)_min, (int64)_max, count);
			}
			else if (T::typeid == uint64::typeid)
			{
				runif_int_1<uint64>(samples, (uint64)_min, (uint64)_max, count);
			}

			return samples;
		}
	};

	private ref class CalculatorServiceInt32 : public  IVectorCalculator<int32>
	{
	public:
		virtual int Dot(Vector<int>^ x, Vector<int>^ y)
		{
			auto xcomp = x->Components;
			pin_ptr<int> pX = &xcomp[0];

			auto ycomp = y->Components;
			pin_ptr<int> pY = &ycomp[0];

			Row<int> row(pX, xcomp->Length, false, true);
			Col<int> col(pY, ycomp->Length, false, true);
			

			auto d = row*col;
			return as_scalar(d);
		}
	};



	public ref class MathServices
	{
	public:
		generic <typename T>
			static IStats^ Stats()
			{
				return gcnew StatsService();
			}
	
		generic <typename T>
			static IVectorCalculator<T>^ VectorCalcs()
			{
				if (T::typeid == int32::typeid)
				{
					auto calc = gcnew CalculatorServiceInt32();
					return (IVectorCalculator<T>^) calc;
				}
				else
				{
					throw gcnew NotSupportedException();
				}
			}
	};

}}}
