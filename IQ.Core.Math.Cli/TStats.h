#pragma once

#include "Math.h"

using namespace System;

namespace IQ {namespace Core {namespace Math {

	class TStats
	{
	public:
		template <typename T>
		static void runif_int_1(Array^ dst, T min, T max, int count)
		{
			std::random_device rd;
			std::default_random_engine engine(rd());
			std::uniform_int_distribution<T> src(min, max);

			for (int i = 0; i < count; i++) {
				dst->SetValue(src(engine), i);
			}
		}

		template <typename T>
		static void runif_int(T dst[], T min, T max, int count)
		{			
			std::random_device rd;
			std::default_random_engine engine(rd());
			std::uniform_int_distribution<T> src(min, max);

			for (int i = 0; i < count; i++) {
				dst[i] = src(engine);
			}

		}
		
	};
	
	private ref class StatsService : public IStats
	{
	private:

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
					auto min = (int)Convert::ChangeType(_min, int32::typeid);
					auto max = (int)System::Convert::ChangeType(_max, int32::typeid);
					runif_int_2<int32, uint8>(samples, min, max, count);
				}
				if (T::typeid == int8::typeid)
				{
					auto min = (int)Convert::ChangeType(_min, int32::typeid);
					auto max = (int)Convert::ChangeType(_max, int32::typeid);
					runif_int_2<int32, int8>(samples, min, max, count);
				}

				else if (T::typeid == int16::typeid)
				{
					TStats::runif_int_1(samples, (int16)_min, (int16)_max, count);
				}
				else if (T::typeid == uint16::typeid)
				{
					TStats::runif_int_1(samples, (uint16)_min, (uint16)_max, count);
				}
				else if (T::typeid == int32::typeid)
				{
					TStats::runif_int_1(samples, (int32)_min, (int32)_max, count);
				}
				else if (T::typeid == uint32::typeid)
				{
					TStats::runif_int_1(samples, (uint32)_min, (uint32)_max, count);
				}
				else if (T::typeid == int64::typeid)
				{
					TStats::runif_int_1(samples, (int64)_min, (int64)_max, count);
				}
				else if (T::typeid == uint64::typeid)
				{
					TStats::runif_int_1(samples, (uint64)_min, (uint64)_max, count);
				}

				return samples;
			}
	};


}}}
